
import sys
import os
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
        #csv_filename = os.path.join(output_dir, 'parts_cost.csv')
        #csv_file = open(csv_filename,'w')
        aggregated_mass = float(0)
        aggregated_cost = float(0)
        #log("ComputeData in customProcessor.PY")	
        #print "this is a print"       
        propDict = {}
        #log("ComputeData: Initialized dict  yyyyy")
        #log("objects m,prop,para,sut="+str(len(tb_props)))
        #log("objects m,prop,para,sut="+str(len(tb_metrics))+" "+str(len(tb_props))+" "+str(len(tb_params))+" "+str(len(sut)))
        #log("scanning props")
        for pp in tb_props :
            instr = pp.Description
            #log(instr)            
            propname = pp.name
            #log("ComputeData: prop:"+instr)
            pname,classname,op = instr.split(',')
            #log("param "+pname+ " in "+ classname + " Op " + op)
            val = "0.0"
            propDict[propname] = (pname,classname,op,val)
            
        #############################################################
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
                for xx in propDict :
                    #log(xx)
                    #log("Look for:"+propDict[xx][1])
                    #log("Class is:"+obj.Classifications)
                    if propDict[xx][1] == "*" or propDict[xx][1] == "" or obj.Classifications == propDict[xx][1] :
                        #log("WOW")
                        #log("Found Class:"+obj.Classifications)
                        for child in obj.children():
                            #log(child.name+" "+child.type.name+" equal to "+propDict[xx][0])
                            if (child.type.name=="Property" or child.type.name=="Parameter") and child.name == propDict[xx][0] :
                                if propDict[xx][2] == "*" or propDict[xx][2] == "" :
                                    #log("val is: "+child.Value + " set to "+xx)
                                    propDict[xx] = (propDict[xx][0],propDict[xx][1],propDict[xx][2],child.Value)
                                    #log("add to "+xx+" child.Value")
                                if propDict[xx][2] == "+"  :
                                    #log("val is: "+child.Value + " added to "+xx)
                                    try:
                                        #log("propdict ["+propDict[xx][3]+"] and "+child.Value)
                                        newval = str(float(propDict[xx][3]) + float(child.Value))
                                        #log("Sum is "+newval)
                                        propDict[xx] = (propDict[xx][0],propDict[xx][1],propDict[xx][2],newval)
                                        #log("add to "+xx+" child.Value")
                                    except:
                                        #log("error with value/string")
                                        propDict[xx] = (propDict[xx][0],propDict[xx][1],propDict[xx][2],child.Value)
                                    
        #log("Done with parsing")
        ###############################################################################################	
        fcomp.write("import json\n")
        fcomp.write("import os\n")        
        fcomp.write("json_filename = os.path.join(\"\", 'testbench_manifest.json')\n")
        fcomp.write("json_data = {}\n")
        fcomp.write("if os.path.isfile(json_filename):\n")
        fcomp.write("    with open(json_filename, \"r\") as json_file:\n")
        fcomp.write("        json_data = json.load(json_file) \n") 
        fcomp.write("    for item in json_data['Parameters']:\n") 
        for ppp in tb_params:
            fcomp.write("        if \""+ppp.name+ "\"== item[\"Name\"]:\n")
            fcomp.write("            try:\n")
            fcomp.write("                "+ppp.name+"=float(item[\"Value\"])\n")
            fcomp.write("            except:\n")
            fcomp.write("                "+ppp.name+"=item[\"Value\"]\n")

        for xx in propDict :
            #log("Write out:"+xx)
            #log("isnum "+propDict[xx][3])
            try:
                vval = float(propDict[xx][3])
                fcomp.write(xx+"="+propDict[xx][3]+"\n")
            except:
                fcomp.write(xx+"=\""+propDict[xx][3]+"\"\n")
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

# This is the entry point    
def invoke(focusObject, rootObject, componentParameters, **kwargs):
    output_dir = componentParameters['output_dir']
    if output_dir == '':
        output_dir = os.getcwd()
        
    ComputeData().compute(focusObject, output_dir)

    ###############################################################################################				
    ##  create run_classifications.cmd
    ###############################################################################################				

    componentParameters['runCommand'] = 'run_classifications.cmd'
    outputfilename_bat_path = os.path.join(output_dir, 'run_classifications.cmd')
    
    try:
        fbat = open(outputfilename_bat_path, 'w')
    except Exception, e:
        log("Could not open  {0}: {1}".format(outputfilename_bat_path, str(e)))
    foundCommands = False
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
        fbat_contents = '''
set ERROR_CODE=0

FOR /F "skip=2 tokens=2,*" %%A IN ('C:\Windows\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO set META_PATH=%%B
%META_PATH%\\bin\\Python27\\Scripts\\python.exe compute.py

echo "DONE"
exit /b
        '''
        fbat.write(fbat_contents)

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
