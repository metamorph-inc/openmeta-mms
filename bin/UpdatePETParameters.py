# coding: utf-8
import sys
import os
import os.path
import json
import collections
import copy
# sys.path.append(r"C:\Program Files\ISIS\Udm\bin")
# if os.environ.has_key("UDM_PATH"):
#     sys.path.append(os.path.join(os.environ["UDM_PATH"], "bin"))
import _winreg as winreg
import pywintypes
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
def invoke(focusObject, rootObject, componentParameters, udmProject, **kwargs):
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
        var_dict[desVar.attr("name")] = desVar.Range

    project_dir = os.path.dirname(focusObject.convert_udm2gme().Project.ProjectConnStr[len("MGA="):])
    try:
        pet_config_filename = componentParameters.get("pet_config_filename", os.path.join(project_dir, "results", "pet_config_refined.json"))
        with open(pet_config_filename, "rb") as input_json:
            args = json.load(input_json)
    except IOError as e:
        if e.errno == 2:
            raise CyPhyPython.ErrorMessageException("Produce the file results/pet_config_refined.json first.")
        raise

    driver_details = copy.deepcopy(args["drivers"].values()[0]["details"])
    for varName, var in args["drivers"].values()[0]["designVariables"].iteritems():
        if var.get("type") == "enum":
            var_dict[varName] = ";".join(map(json.dumps, var["items"]))
        else:
            var_dict[varName] = repr(var["RangeMin"]) + "," + repr(var["RangeMax"])
    # TODO: optimizer constraints

    testing_and_pets = {}
    queue = collections.deque()

    def get_path(folder):
        ret = tuple()
        while folder != folder.Project.RootFolder:
            ret = (folder.Name,) + ret
            folder = folder.ParentFolder
        return ret

    # create copy of PET as specified in NewPETName (if present) re-use Testing and ParametricExplorationFolder
    # FIXME: this code doesn't handle more than one missing folder
    mga_project = focusObject.convert_udm2gme().Project
    queue.append(mga_project.RootFolder)
    while queue:
        folder = queue.pop()
        testing_and_pets[get_path(folder)] = folder
        for child_folder in (f for f in folder.ChildFolders if f.MetaBase.Name in ("ParametricExplorationFolder", "Testing")):
            queue.append(child_folder)

    new_name = componentParameters.get("NewPETName", "/" + "/".join(get_path(focusObject.convert_udm2gme().ParentFolder)) + "/" + focusObject.name + "_Refined")
    new_folders = tuple(new_name.split("/")[1:-1])
    pet_folder, testing_folder = None, None
    for i in range(len(new_folders), 0, -1):
        pet_folder = testing_and_pets.get(new_folders[:i])
        if pet_folder is not None and pet_folder.MetaBase.Name == "ParametricExplorationFolder":
            break
        else:
            pet_folder = None
        testing_folder = testing_and_pets.get(new_folders[:i-1])
        if testing_folder is not None and testing_folder.MetaBase.Name in ("ParametricExplorationFolder", "Testing"):
            pet_folder = testing_folder.CreateFolder(mga_project.RootMeta.RootFolder.GetDefinedFolderByNameDisp("ParametricExplorationFolder", True))
            pet_folder.Name = new_name.split("/")[-2]
            break

    gmeCopy = pet_folder.CopyFCODisp(focusObject.convert_udm2gme())
    gmeCopy.Name = new_name.split("/")[-1]
    focusObject = udmProject.convert_gme2udm(gmeCopy)
    parameterStudy = [c for c in focusObject.children() if c.type.name == "ParameterStudy"][0]
    parameterStudy.Code = driver_details["Code"]
    parameterStudy.SurrogateType = driver_details["SurrogateType"]
    parameterStudy.DOEType = driver_details["DOEType"]
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
        parser.add_argument('--pet-config')
        parser.add_argument('--log-file')
        command_line_args = parser.parse_args()

        from win32com.client import DispatchEx
        Dispatch = DispatchEx
        import win32com.client.dynamic
        import win32com.server.util
        from pywintypes import com_error
        import json
        # with open(sys.argv[1]) as input_json:
        with open(command_line_args.pet_config) as input_json:
            args = json.load(input_json)
        project = Dispatch("Mga.MgaProject")
        project.OpenEx("MGA=" + os.path.abspath(args["MgaFilename"]), "CyPhyML", None)

        project.BeginTransactionInNewTerr()
        try:
            pet = project.RootFolder.GetObjectByPathDisp(args["PETName"].replace("/", "/@"))
            if pet is None:
                raise ValueError("Couldn't find PET '{}'".format(args["PETName"]))
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
        cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYPUT, 0, 'pet_config_filename', command_line_args.pet_config)
        cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYPUT, 0, 'NewPETName', command_line_args.new_name)
        cyPhyPython.InvokeEx(project, pet, Dispatch("Mga.MgaFCOs"), 128)

        # arg 3: @pyparm int|bResultWanted||Indicates if the result of the call should be requested
        newPETID = cyPhyPython._oleobj_.Invoke(invoke_id, 0, DISPATCH_PROPERTYGET, 1, 'NewPETID')

        project.BeginTransactionInNewTerr()
        try:
            newPET = project.GetObjectByID(newPETID)
            tbs = [tb for tb in newPET.ChildFCOs if tb.MetaBase.Name == 'TestBenchRef' and tb.Referred is not None]
            if not tbs:
                config_ids = [newPET.ID]
            else:
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

        class StatusCallback(object):
            _public_methods_ = ['SingleConfigurationProgress', 'MultipleConfigurationProgress']

            def __init__(self, log):
                self.log = log

            def SingleConfigurationProgress(self, args):
                # args = win32com.client.dynamic.Dispatch(args)
                # print (args.Context or '') + ' ' + (args.Configuration or '') + ' ' + args.Title
                pass

            def MultipleConfigurationProgress(self, args):
                args = win32com.client.dynamic.Dispatch(args)
                # print (args.Context or '') + ' ' + (args.Configuration or '') + ' ' + args.Title
                self.log.write(args.Title + '\n')
                self.log.flush()

        master = Dispatch("CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI")
        master.Initialize(project)
        logfile = None
        if command_line_args.log_file:
            logfile = open(command_line_args.log_file, 'wb')
            cb = win32com.client.dynamic.Dispatch(win32com.server.util.wrap(StatusCallback(logfile)))
            master.AddProgressCallback(cb)
        # master.Initialize(project._oleobj_.QueryInterface(pythoncom.IID_IUnknown))
        results = master.RunInTransactionWithConfigLight(config_light)
        if logfile:
            logfile.close()

        project.Save(project.ProjectConnStr + "_PET_debug.mga", True)
        try:
            project.Close(False)
        except pywintypes.com_error as e:
            if 'Access is denied' in repr(e):
                print('Could not save "{}". Is it open in GME?'.format(project.ProjectConnStr[4:]))
            else:
                raise

        # print(cyPhyPython.ComponentParameter())

    run()
