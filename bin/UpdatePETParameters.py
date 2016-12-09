# coding: utf-8
import sys
import os
import os.path
import json
import imp
# taken from CyPhyPython:
#  pythoncom.py calls LoadLibrary("pythoncom27.dll"), which will load via %PATH%
#  Anaconda's pythoncom27.dll (for one) doesn't include the correct SxS activation info, so trying to load it results in "An application has made an attempt to load the C runtime library incorrectly."
#  load our pythoncom27.dll (which we know works) with an explicit path
import os.path
import afxres
# FIXME: would this be better : pkg_resources.resource_filename('win32api', 'pythoncom27.dll')
imp.load_dynamic('pythoncom', os.path.join(os.path.dirname(afxres.__file__), 'pythoncom%d%d.dll' % sys.version_info[0:2]))
import pythoncom
# sys.path.append(r"C:\Program Files\ISIS\Udm\bin")
# if os.environ.has_key("UDM_PATH"):
#     sys.path.append(os.path.join(os.environ["UDM_PATH"], "bin"))
import _winreg as winreg
with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"Software\META") as software_meta:
    meta_path, _ = winreg.QueryValueEx(software_meta, "META_PATH")
sys.path.append(os.path.join(meta_path, 'bin'))
import udm


def log(s):
    print s


def log_formatted(s):
    print s
try:
    import CyPhyPython  # will fail if not running under CyPhyPython
    import cgi

    def log_formatted(s):
        CyPhyPython.log(s)

    def log(s):
        CyPhyPython.log(cgi.escape(s))
except ImportError:
    pass


def start_pdb():
    """Start pdb, the Python debugger, in a console window."""
    import ctypes
    ctypes.windll.kernel32.AllocConsole()
    import sys
    sys.stdout = open('CONOUT$', 'wt')
    sys.stdin = open('CONIN$', 'rt')
    import pdb
    pdb.set_trace()


# This is the entry point
def invoke(focusObject, rootObject, componentParameters, **kwargs):
    if focusObject is not None and focusObject.type.name == "ParameterStudy":
        focusObject = focusObject.parent
    if focusObject is None or focusObject.type.name != "ParametricExploration":
        raise CyPhyPython.ErrorMessageException("Run on ParametricExploration")

    parameterStudy = [c for c in focusObject.children() if c.type.name == "ParameterStudy"]
    if not parameterStudy:
        raise CyPhyPython.ErrorMessageException("Must contain a ParameterStudy")
    parameterStudy = parameterStudy[0]
    designVariables = [c for c in parameterStudy.children() if c.type.name == "DesignVariable"]
    var_dict = {}
    for desVar in designVariables:
        var_dict[desVar.attr('name')] = desVar.Range

    project_dir = os.path.dirname(focusObject.convert_udm2gme().Project.ProjectConnStr[len("MGA="):])
    try:
        with open(os.path.join(project_dir, "results", "pet_config.json"), "rb") as input_json:
            args = json.load(input_json)
    except IOError as e:
        if e.errno == 2:
            raise CyPhyPython.ErrorMessageException("Produce the file results/pet_config.json first.")
        raise

    for varName, var in args["drivers"].values()[0]["designVariables"].iteritems():
        if var.get("type") == "enum":
            var_dict[varName] = ';'.join(map(json.dumps, var["items"]))
        else:
            var_dict[varName] = repr(var["RangeMin"]) + ',' + repr(var["RangeMax"])
    # TODO: optimizer constraints

    gmeCopy = focusObject.convert_udm2gme().ParentFolder.CopyFCODisp(focusObject.convert_udm2gme())
    gmeCopy.Name = componentParameters.get("NewPETName", gmeCopy.Name + "_Refined")
    focusObject = [c for c in focusObject.parent.children() if udm.UdmId2GmeId(c.id) == gmeCopy.ID][0]
    parameterStudy = [c for c in focusObject.children() if c.type.name == "ParameterStudy"][0]
    designVariables = [c for c in parameterStudy.children() if c.type.name == "DesignVariable"]
    for desVar in designVariables:
        if desVar.name not in var_dict:
            # probably it is an output
            # log("Couldn't find Design Variable '{}' in model".format(desVar.name))
            continue
        # log("{}.Range = {}".format(desVar.name, var_dict[desVar.name]))
        desVar.Range = var_dict[desVar.name]

    componentParameters["NewPETID"] = gmeCopy.ID

    gme = [c for c in gmeCopy.Project.Clients if c.Name == "GME.Application"]
    if gme:
        gme = gme[0].OLEServer
        gmeCopy.Project.CommitTransaction()
        gme.ShowFCO(gmeCopy, False)
        READ_ONLY = 1
        gmeCopy.Project.BeginTransactionInNewTerr(READ_ONLY)


# Allow calling this script with a .mga file as an argument
if __name__ == '__main__':
    def run():
        import argparse

        parser = argparse.ArgumentParser(description='Re-run a PET with updated parameters.')
        parser.add_argument('--new-name')
        command_line_args = parser.parse_args()

        from win32com.client import DispatchEx
        Dispatch = DispatchEx
        from pywintypes import com_error
        import json
        # with open(sys.argv[1]) as input_json:
        with open("results/pet_config.json") as input_json:
            args = json.load(input_json)
        project = Dispatch("Mga.MgaProject")
        project.Open("MGA=" + os.path.abspath(args["MgaFilename"]))

        project.BeginTransactionInNewTerr()
        try:
            pet = project.RootFolder.GetObjectByPathDisp(args["PETName"].replace("/", "/@"))
        finally:
            project.CommitTransaction()

        cyPhyPython = Dispatch("Mga.Interpreter.CyPhyPython")

        try:
            invoke_id = cyPhyPython._oleobj_.GetIDsOfNames(u'ComponentParameter')
        except com_error as e:
            if e.hresult & 0xffffffff == 0x80020006L:
                # 'Unknown name': old GME components don't do IDispatch
                sys.stderr.write('GME is too old. Please upgrade it\n')
                sys.exit(5)
            raise
        DISPATCH_PROPERTYGET = 0x2
        DISPATCH_PROPERTYPUT = 0x4
        cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYPUT, 0, 'script_file', os.path.basename(__file__))
        cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYPUT, 0, 'NewPETName', command_line_args.new_name)
        cyPhyPython.InvokeEx(project, pet, Dispatch("Mga.MgaFCOs"), 128)

        # arg 3: @pyparm int|bResultWanted||Indicates if the result of the call should be requested
        newPETID = cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYGET, 1, 'NewPETID')

        project.BeginTransactionInNewTerr()
        try:
            newPET = project.GetObjectByID(newPETID)
            tbs = [tb for tb in newPET.ChildFCOs if tb.MetaBase.Name == 'TestBenchRef' and tb.Referred is not None]
            if not tbs:
                raise ValueError('Error: PET does not have a TestBenchRef')
            tb = tbs[0]
            suts = [sut for sut in tb.Referred.ChildFCOs if sut.MetaRole.Name == 'TopLevelSystemUnderTest']
            if len(suts) == 0:
                raise ValueError('Error: TestBench "{}" has no TopLevelSystemUnderTest'.format(tb.Name))
            if len(suts) > 1:
                raise ValueError('Error: TestBench "{}" has more than one TopLevelSystemUnderTest'.format(tb.Name))
            sut = suts[0]
            if sut.Referred.MetaBase.Name == 'ComponentAssembly':
                config_ids = [sut.Referred.ID]
            else:
                configurations = [config for config in sut.Referred.ChildFCOs if config.MetaBase.Name == 'Configurations' and config.Name == args["GeneratedConfigurationModel"]]
                if not configurations:
                    raise ValueError('Error: design has no Configurations model "{}"'.format(args["GeneratedConfigurationModel"]))
                configurations = configurations[0]
                cwcs = [cwc for cwc in configurations.ChildFCOs if cwc.MetaBase.Name == 'CWC' and cwc.Name in args["SelectedConfigurations"]]
                if len(cwcs) != len(args["SelectedConfigurations"]):
                    raise ValueError('Error: could not find all CWCs "{!r}" in "{}"'.format(args["SelectedConfigurations"], args["GeneratedConfigurationModel"]))
                config_ids = [cwc.ID for cwc in cwcs]
        finally:
            project.CommitTransaction()

        config_light = Dispatch("CyPhyMasterInterpreter.ConfigurationSelectionLight")

        # GME id, or guid, or abs path or path to Test bench or SoT or PET
        config_light.ContextId = newPETID

        config_light.SetSelectedConfigurationIds(config_ids)

        # config_light.KeepTemporaryModels = True
        config_light.PostToJobManager = True

        master = Dispatch("CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI")
        master.Initialize(project)
        # master.Initialize(project._oleobj_.QueryInterface(pythoncom.IID_IUnknown))
        results = master.RunInTransactionWithConfigLight(config_light)

        project.Save("MGA=debug.mga", True)
        project.Close(True)

        # print(cyPhyPython.ComponentParameter())

    run()
