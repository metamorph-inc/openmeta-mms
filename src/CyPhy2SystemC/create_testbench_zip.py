import zipfile
import os
import os.path
import glob
import subprocess
import traceback
import sys

os.chdir(os.path.dirname(os.path.abspath(__file__)))

def testbench_zip():
    with zipfile.ZipFile('SystemCTestBench.zip', 'w') as zf:
        zf.write('../../models/SystemC/Release/systemc-2.3.0.lib', 'libR/systemc-2.3.0.lib', zipfile.ZIP_DEFLATED)
        zf.write('../../models/SystemC/Debug/systemc-2.3.0.lib', 'libD/systemc-2.3.0.lib', zipfile.ZIP_DEFLATED)
        zf.write('../../models/SystemC/Release/TonkaSCLib.lib', 'libR/TonkaSCLib.lib', zipfile.ZIP_DEFLATED)
        zf.write('../../models/SystemC/Debug/TonkaSCLib.lib', 'libD/TonkaSCLib.lib', zipfile.ZIP_DEFLATED)

        zf.write('TestBenchFiles/SystemCTestBench.sln', 'SystemCTestBench.sln')
        zf.write('TestBenchFiles/SystemCTestBench.vcxproj', 'SystemCTestBench.vcxproj')
        zf.write('TestBenchFiles/SystemCTestBench.vcxproj.filters', 'SystemCTestBench.vcxproj.filters')
        def filt(fn):
            basename = os.path.basename(fn)
            for ext in (".h", ".hpp", ".ino", ".inc"):
                if basename.endswith(ext):
                    return True
            if basename.find(".") == -1:
                return True
            return

        def zip_r(starting, prepend):
            #with open(os.devnull, 'wb') as nul:
            for fn in subprocess.check_output(['git.exe', 'ls-files', '.'], cwd=starting).split():
                if fn != "" and not fn.isspace() and filt(fn):
                    #print repr(fn)
                    zf.write(starting + '/' + fn, prepend + '/' + fn, zipfile.ZIP_DEFLATED)

        zip_r('../../models/SystemC/systemc-2.3.0/src', 'include/systemc-2.3.0')

        # Zip up everything under '../../models/SystemC/*/', except those directories
        # listed in the excludeList. Excluding 'systemc-2.3.0' is okay here because
        # the elements needed from that directory were included earlier in this process.
        excludeList = ["systemc-2.3.0", "Debug", "ipch", "Release", "usb11", "VCDSource"]
        targetPathList = glob.glob( "../../models/SystemC/*/" )
        for path in targetPathList:
            targetDirectory = os.path.normpath( os.path.dirname( path ))
            tonkasclib = os.path.basename( targetDirectory )
            if not tonkasclib in excludeList:
                zip_r(targetDirectory, 'include/TonkaSCLib/' + tonkasclib)

if __name__ == '__main__':
    try:
        testbench_zip()
    except Exception as e:
        error = traceback.format_exc()
        # print MSBuild-formatted message
        print(('\n' + error.rstrip()).replace('\n', '\nERROR: ')[1:])
        sys.exit(3)
