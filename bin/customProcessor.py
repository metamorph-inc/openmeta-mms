
import sys
import os
import parser
#sys.path.append(r"C:\Program Files\ISIS\Udm\bin")
#if os.environ.has_key("UDM_PATH"):
#    sys.path.append(os.path.join(os.environ["UDM_PATH"], "bin"))
import _winreg as winreg
with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"Software\META") as software_meta:
    meta_path, _ = winreg.QueryValueEx(software_meta, "META_PATH")
sys.path.append(os.path.join(meta_path, 'bin'))
import udm


def log(s):
    dummmy=1
    #print s
try:
    import CyPhyPython # will fail if not running under CyPhyPython
    import cgi
    def log(s):
        CyPhyPython.log(cgi.escape(s))
except ImportError:
    pass

def log_formatted(s):
    print s
try:
    import CyPhyPython # will fail if not running under CyPhyPython
    import cgi
    def log(s):
        CyPhyPython.log(s)
except ImportError:
    pass


def is_number(s):
    try:
        float(s)
        return True
    except ValueError:
        return False


def start_pdb():
    ''' Starts pdb, the Python debugger, in a console window
    '''
    import ctypes
    ctypes.windll.kernel32.AllocConsole()
    import sys
    sys.stdout = open('CONOUT$', 'wt')
    sys.stdin = open('CONIN$', 'rt')
    import pdb; pdb.set_trace()
    
supportedOperators = ["*", "+", ""]

class ComputeData(object):
    def compute(self, currentobj, output_dir):

        json_filename = os.path.join(output_dir, 'testbench_manifest.json')
        pyprog_filename = os.path.join(output_dir, 'compute.py')
        fcomp = open(pyprog_filename, 'w')
        classifications = {}
        tb_metrics = [child for child in currentobj.children() if child.type.name=="Metric"]
        tb_props = [child for child in currentobj.children() if child.type.name=="Property"]
        tb_params = [child for child in currentobj.children() if child.type.name=="Parameter"]
        sut = [child for child in currentobj.children() if child.type.name=="TopLevelSystemUnderTest"]
        if len(sut) == 0:
            raise Exception(currentobj.name + ' has no TopLevelSystemUnderTest')
        sut = sut[0]
        import collections
        que = collections.deque()
        que.extend(sut.ref.children())
        
        
        ################################################
        # Gather instructions from TB-level Properties #
        ################################################
        
        propDict = {}
        log("Test Bench Properties:")
        for tb_prop in tb_props :
            name = tb_prop.name
            instruction = tb_prop.Description
            log("Name: "+name+", Instruction: "+instruction)
            if instruction.count(',') != 2:
                raise ValueError('Property {} has invalid description format. It should contain 3 values, comma-separated'.format(name))
            pname,classname,op = instruction.split(',')
            if op not in ('*', '+', ''):
                raise ValueError('Property {} has invalid operator \'{}\'. If should be * or +'.format(name, op))
            propDict[name] = {}
            propDict[name]["pname"] = pname
            propDict[name]["classname"] = classname
            propDict[name]["op"] = op
            propDict[name]["val"] = "0.0"

        ###############################################
        # Perform the desired operations on the model #
        ###############################################
        
        while que:
            obj = que.pop()
            if obj.type.name == "ComponentRef":
                obj = obj.ref
            que.extend(obj.children())
            def add_classification(component):
                for class_ in (s for s in component.Classifications.replace("\r", "").split("\n") if not s.isspace() and s != ""):
                    classifications.setdefault(class_, [])
                    classifications[class_].append(component)
            if obj.type.name == "Component":
                #log("Visit:"+obj.name)
                add_classification(obj)
                for tb_prop in propDict :
                    #log(tb_prop)
                    #log("Look for:"+propDict[tb_prop]["classname"])
                    #log("Class is:"+obj.Classifications)
                    if propDict[tb_prop]["classname"] == "*" or propDict[tb_prop]["classname"] == "" or obj.Classifications == propDict[tb_prop]["classname"] :
                        #log("Found Class:"+obj.Classifications)
                        for child in obj.children():
                            #log(child.name+" "+child.type.name+" equal to "+propDict[tb_prop]["pname"])
                            if (child.type.name=="Property" or child.type.name=="Parameter") and child.name == propDict[tb_prop]["pname"] :
                                if propDict[tb_prop]["op"] == "*" or propDict[tb_prop]["op"] == "" :
                                    #log("val is: "+tb_prop+ " set to "+child.Value)
                                    propDict[tb_prop]["val"] = child.Value
                                    #log("add to "+tb_prop+" child.Value")
                                if propDict[tb_prop]["op"] == "+"  :
                                    #log("val is: "+child.Value + " added to "+tb_prop)
                                    try:
                                        #log("Adding "+child.Value+" to running sum of "+propDict[tb_prop]["val"]+".")
                                        propDict[tb_prop]["val"] = str(float(propDict[tb_prop]["val"]) + float(child.Value))
                                    except:
                                        #log("Error with value/string")
                                        propDict[tb_prop]["val"] = child.Value
        #log("Done with parsing")
        
        #####################################
        # Generate contents of 'compute.py' #
        #####################################
        
        fcomp.write("import json\n")
        fcomp.write("import os\n")
        fcomp.write("import math\n")
        fcomp.write("json_filename = os.path.join(\"\", 'testbench_manifest.json')\n")
        fcomp.write("json_data = {}\n")
        fcomp.write("if os.path.isfile(json_filename):\n")
        fcomp.write("    with open(json_filename, \"r\") as json_file:\n")
        fcomp.write("        json_data = json.load(json_file) \n")
        fcomp.write("    for item in json_data['Parameters']:\n")
        if len(tb_params) == 0:
            fcomp.write("        pass\n")
        for tb_param in tb_params:
            fcomp.write("        if \""+tb_param.name+ "\"== item[\"Name\"]:\n")
            fcomp.write("            try:\n")
            fcomp.write("                "+tb_param.name+"=float(item[\"Value\"])\n")
            fcomp.write("            except:\n")
            fcomp.write("                "+tb_param.name+"=item[\"Value\"]\n")

        for tb_prop in propDict :
            #log("Write out:"+tb_prop)
            #log("isnum "+propDict[tb_prop]["val"])
            try:
                vval = float(propDict[tb_prop]["val"])
                fcomp.write(tb_prop+"="+propDict[tb_prop]["val"]+"\n")
            except:
                fcomp.write(tb_prop+"=\""+propDict[tb_prop]["val"]+"\"\n")
        import json
        json_data = {}
        #log('json filename %s:' % json_filename)
        if os.path.isfile(json_filename):
            with open(json_filename, "r") as json_file:
                json_data = json.load(json_file)
        json_data.setdefault('Metrics', [])
        #log("metrics code gen")
        for item in json_data['Metrics']:
            if len(item["Description"]) > 0 :
                #log(item["Description"])
                parser.suite(item["Description"])  # fail fast if it doesn't parse
                fcomp.write(item["Description"]+"\n")
        #log("done metrics code gen")
        fcomp.write("\n##############################\n")
        #log("creating json handling code")
        fcomp.write("for metric in json_data['Metrics']: \n")
        for item in json_data['Metrics']:
            #log(item["Name"])
            fcomp.write("    if metric[\"Name\"] == \""+item["Name"]+"\":\n")
            fcomp.write("        metric[\"Value\"] = "+item["Name"]+"\n")
        fcomp.write("with open(json_filename, \"w\") as json_file:\n")
        fcomp.write("    json.dump(json_data, json_file, indent=4)\n")

        fcomp.write("# json handler done\n")
        fcomp.close()

        #log(str(json_data))
        #log('- Saving to: %s' % json_filename)


# This is the entry point
def invoke(focusObject, rootObject, componentParameters, **kwargs):
    #log(rootObject.name)
    #print repr(rootObject.name)
    output_dir = componentParameters['output_dir']
    if output_dir == '':
        output_dir = os.getcwd()

    ComputeData().compute(focusObject, output_dir)
    #log_formatted("Output files are <a href=\"file:///{0}\" target=\"_blank\">{0}</a>.".format(output_dir))
    #log("Output files are <a href=\"file:///{0}\" target=\"_blank\">{0}</a>.".format(output_dir))
    #log('done')

    ##################################
    # Create run_classifications.cmd #
    ##################################

    #with open(os.path.join(output_dir, 'run_classifications.cmd'), 'w'): pass
    componentParameters['runCommand'] = 'run_classifications.cmd'
    ##componentParameters['labels'] = 'Windows'

    outputfilename_bat_path = os.path.join(output_dir, 'run_classifications.cmd')

    #start_pdb()
    #print "outputfilename_bat_path: " + outputfilename_bat_path

    try:
        fbat = open(outputfilename_bat_path, 'w')
    except Exception, e:
        log("Could not open  {0}: {1}".format(outputfilename_bat_path, str(e)))
    foundCommands = False
    fbat.write('Setlocal EnableDelayedExpansion\n')
    for child in focusObject.children():
         if child.type.name=="Property" and child.name =="preExec":
            fbat.write(child.Description+'\n\n')
            foundCommands=True
    for child in focusObject.children():
         if child.type.name=="Property" and child.name =="Exec":
            fbat.write(child.Description+'\n\n')
            foundCommands=True
    for child in focusObject.children():
         if child.type.name=="Property" and child.name =="postExec":
            fbat.write(child.Description+'\n\n')
            foundCommands=True

    if foundCommands == False:
        fbat.write('set ERROR_CODE=0\n')
        fbat.write('''FOR /F "skip=2 tokens=2,*" %%A IN ('C:\Windows\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO set META_PATH=%%B\n''')
        fbat.write('"%META_PATH%\\bin\\Python27\\Scripts\\python.exe" compute.py || exit /b !ERRORLEVEL!\n')
        fbat.write('echo "DONE"	\n')
        fbat.write('\n')
        fbat.write('''pause  2 exit 0\n''')

    fbat.close()

# Allow calling this script with a .mga file as an argument
if __name__=='__main__':
    import _winreg as winreg
    with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"Software\META") as software_meta:
        meta_path, _ = winreg.QueryValueEx(software_meta, "META_PATH")

    # need to open meta DN since it isn't compiled in
    uml_diagram = udm.uml_diagram()
    meta_dn = udm.SmartDataNetwork(uml_diagram)
    import os.path
    CyPhyML_udm = os.path.join(meta_path, r"generated\CyPhyML\models\CyPhyML_udm.xml")
    if not os.path.isfile(CyPhyML_udm):
        CyPhyML_udm = os.path.join(meta_path, r"meta\CyPhyML_udm.xml")
    meta_dn.open(CyPhyML_udm, "")

    dn = udm.SmartDataNetwork(meta_dn.root)
    dn.open(sys.argv[1], "")
    # TODO: what should focusObject be
    # invoke(None, dn.root);
    dn.close_no_update()
    meta_dn.close_no_update()
