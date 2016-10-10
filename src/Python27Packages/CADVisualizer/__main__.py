import os
import os.path
import sys
import glob
import shutil
import _winreg
import argparse
import posixpath
import subprocess
import urllib
import zipfile
import shutil
from win32com.shell import shell, shellcon
from utility_functions import setup_logger, exitwitherror

setup_logger("CADVisualizer.log")

def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('outfile', nargs='?', type=str, default='CyPhyAssembly',
                        help='Name for assembly CAD file.')
    parser.add_argument('format', nargs='?', type=str, default='step',
                        help='Type of CAD files the assembler will work with.')
    parser.add_argument('-a', '--assemble', action='store_true',
                        help='Assemble design using FreeCAD')
    parser.add_argument('-r', '--replace', action='store_true',
                        help='Swap out template stock parts for specified components.')
    parser.add_argument('-i', '--interference', action='store_true',
                        help='Perform interference analysis on final assembly.')                    
    parser.add_argument('-c', '--convert', action='store_true',
                        help='Convert STEP file to XML format.')
    parser.add_argument('-v', '--visualize', action='store_true', 
                        help='Launch visualizer.')
    return parser.parse_args()


def get_freecad_path():
    try:
        with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                             r'Software\Microsoft\Windows\CurrentVersion\Uninstall\FreeCAD 0.14',
                             0, _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY) as key:
            FREECAD_PATH = _winreg.QueryValueEx(key, 'InstallLocation')[0]
    except WindowsError:
        try:
            with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                                 r'Software\Microsoft\Windows\CurrentVersion\Uninstall\FreeCAD 0.14',
                                 0, _winreg.KEY_READ | _winreg.KEY_WOW64_64KEY) as key:
                FREECAD_PATH = _winreg.QueryValueEx(key, 'InstallLocation')[0]
        except WindowsError:
            exitwitherror("Unable to find FreeCAD registry key. Is FreeCAD installed?")

    if FREECAD_PATH is None:
        exitwitherror('FreeCAD install location not found! \n')
    FREECAD_PATH = FREECAD_PATH.strip('"').rstrip('"')

    return os.path.join(FREECAD_PATH, "bin", "FreeCAD.exe")


def assemble(freecad, outfile, fileformat, doreplace, interference_check = False):
    this_dir = os.path.dirname(os.path.abspath(__file__))
    visualizepy = os.path.join(this_dir, "BuildPCB.py")
    
    rtn = subprocess.call([freecad, visualizepy, outfile, fileformat, str(doreplace), str(interference_check)])
    if rtn != 0:
        exit(rtn)


def replace(freecad, template_file):
    this_dir = os.path.dirname(os.path.abspath(__file__))
    modulepy = os.path.join(this_dir, "BuildModule.py")

    rtn = subprocess.call([freecad, modulepy, template_file])
    if rtn != 0:
        exit(rtn)


def convert(stepfile, xmldir):
    # Check stepfile exists (double-checks assemble() exited gracefully).
    steppath = os.path.join(os.getcwd(), stepfile)
    if not os.path.exists(steppath):
        exitwitherror('STEP file ' + stepfile + ' can not be found in ' + steppath)

    # Check that StepTools converter is in expected location
    commondocs = shell.SHGetFolderPath(0, shellcon.CSIDL_COMMON_DOCUMENTS, 0, 0)
    converter = os.path.join(str(commondocs), 'StepTools', 'bin', 'export_product_asm.exe')
    if not os.path.exists(converter):
        print "export_product_asm.exe not found. Attempting to download it..."
        if not os.path.exists(os.path.dirname(converter)):
            os.makedirs(os.path.dirname(converter))
        steptoolsfile, headers = urllib.urlretrieve('http://www.steptools.com/demos/stpidx_author_win32_a7.zip')
        with zipfile.ZipFile(steptoolsfile, 'r') as steptoolszip:
            with steptoolszip.open('stpidx_author_win32/bin/export_product_asm.exe') as srcexe:
                with open(converter, 'wb') as dstexe:
                    shutil.copyfileobj(srcexe, dstexe)

    # Make directory that will contain dumped index and shell XMLs
    dumpdir = os.path.join(os.getcwd(), xmldir)
    if os.path.exists(dumpdir):
        shutil.rmtree(dumpdir)
    os.makedirs(dumpdir)

    # Launch converter
    logfile = open("log\\Step2XMLConverterLog_{0}.txt".format(stepfile), "a")
    rtn = subprocess.call([converter, steppath, '-d', '-o', dumpdir], stdout=logfile)
    logfile.close()
    if rtn != 0:
        exit(rtn)


def visualize(xmldir):
    # Find cad.js directory/app
    #commondocs = shell.SHGetFolderPath(0, shellcon.CSIDL_COMMON_DOCUMENTS, 0, 0)

    # MOT-674: cad.js moved into installer bin directory
    with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE, 
                         r'Software\META', 
                         0, _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY) as key:
        META_PATH = _winreg.QueryValueEx(key, 'META_PATH')[0]

    cadjs = os.path.join(META_PATH, 'bin', 'cad.js')
    cadjs_index = os.path.join(cadjs, 'public', 'index.html')
    if not os.path.exists(cadjs_index):
        exitwitherror('Unable to locate cad.js index.html! Is the cad.js repository ' + \
                      'in the correct location? Cad.js repository needs to be ' + \
                      'in Public Documents folder. Path searched: ' + cadjs_index)

    # Check existence of index.xml in xmldir
    step_index_dir = posixpath.join(os.getcwd(), xmldir)
    step_index = os.path.join(step_index_dir, 'index.xml')
    if not os.path.exists(step_index):
        exitwitherror('Index.XML from StepTools converter can not be found! ' + \
                      'Path searched: ' + step_index)

    """
     Launch chrome with through subprocess. The webbrowser module does not work...
     The path contains a query operator followed by XML path "...?resource_url=..."
     Launched with webbrowser, everything including and following the ? is truncated.
     Provide absolute paths to both index html and xml files.
     * Subprocess is in background, so program exits once command is executed.
    """
    try:
        with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                             r'Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe',
                             0, _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY) as key:
            CHROME_PATH = _winreg.QueryValueEx(key, 'Path')[0]
    except WindowsError:
        try:
            with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                                 r'Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe',
                                 0, _winreg.KEY_READ | _winreg.KEY_WOW64_64KEY) as key:
                CHROME_PATH = _winreg.QueryValueEx(key, 'Path')[0]
        except WindowsError:
            exitwitherror("Unable to find Google Chrome registry key!")
        
    if CHROME_PATH is None:
        exitwitherror('Google Chrome install location not found! \n')

    CHROME = os.path.join(CHROME_PATH, 'chrome.exe')

    # Use posixpath to force the index.xml path to use a forward slash
    vis_path = r'file://' + cadjs_index + r"?resource_url=" + posixpath.join(step_index_dir, "index.xml")
    subprocess.Popen([CHROME, '--allow-file-access-from-files', vis_path])


if __name__ == '__main__':
    args = get_args()

    if args.assemble:
        fc_path = get_freecad_path()
        assemble(fc_path, args.outfile, args.format, args.replace,  args.interference)

    # Build module using saved assembly. # TODO: This isn't supported in interpreter yet... should it?
    if args.replace and not args.assemble:
        template = "AraTemplateParts.json"
        fc_path = get_freecad_path()
        replace(fc_path, template)

    xmldir = 'StepXMLs_'
    if args.convert:
        [convert(x, xmldir + x.rsplit('.')[0]) for x in glob.glob('*.step')]

    if args.visualize:
        # Handles event where multiple step files have been converted to XML.
        stepdirs = [x for x in glob.glob(xmldir+'*') if os.path.isdir(x)]
        if len(stepdirs) > 1:
            vis_dir_found = False
            while not vis_dir_found:
                try:
                    vis_asm = raw_input("Which assembly would you like to visualize?: ")
                except EOFError:
                    # If called from Master Interpreter rather than a bat file, raw_input returns
                    # EOFError. In this situation, the cad.js visualizer should default to view the PCB.
                    vis_asm = args.outfile
                vis_dir = xmldir + vis_asm
                if os.path.exists(vis_dir):
                    vis_dir_found = True
                else:
                    print "{0} is not a valid assembly!".format(vis_asm)

        else:
            vis_dir = stepdirs[0]

        visualize(vis_dir)