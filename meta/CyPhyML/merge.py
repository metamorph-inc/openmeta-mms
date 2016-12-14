# -*- coding: utf-8 -*-
''''set -e
# dirty hack so `bash merge.py` and `python merge.py` both do something
set -x
git checkout metarefs
../../bin/Python27/Scripts/python merge.py "$@"
../../bin/Python27/Scripts/python add_folder_treeIcons.py
../../bin/Python27/Scripts/python add_folder_treeIcons.py
cat metarefs_new > metarefs
exit 0

bash merge.py --version 14.13-t12
'''
import argparse
from win32com.client.dynamic import Dispatch
import subprocess
import contextlib

OBJTYPE_MODEL = 1
OBJTYPE_ATOM = 2
OBJTYPE_REFERENCE = 3
OBJTYPE_CONNECTION = 4
OBJTYPE_SET = 5
OBJTYPE_FOLDER = 6

vu_rev = '4fbabdd2ecfc24cbe3c452b716a14f7989c4ce2a'
# vu_rev = open(subprocess.check_output('git rev-parse --show-toplevel').strip() + '/.git/MERGE_HEAD', 'rb').read().strip()
# subprocess.check_call('git show {}:./CyPhyML-core.xme > CyPhyML-core.xme'.format(vu_rev), shell=True)
# subprocess.check_call('git show {}:./CyPhyML.xme > CyPhyML.xme'.format(vu_rev), shell=True)




@contextlib.contextmanager
def in_tx(project):
    project.BeginTransactionInNewTerr()
    try:
        yield
    finally:
        project.CommitTransaction()

core_mods = {
    "/@1_Component/@1x_Component/@Resource":
        {"Icon": "resource.png"},
    "/@1_Component/@4x_Port/@Connector|kind=Model":
        {"Decorator": "Mga.CPMDecorator",
            "GeneralPreferences": "fillColor = 0xdddddd\nportLabelLength=0\ntreeIcon=connector_port.png\nexpandedTreeIcon=connector_port.png",
            "Icon": "connector.png",
            "IsTypeInfoShown": False,
            "PortIcon": "connector_port.png"},
    "/@1_Component/@3_Properties_Parameters/@IsProminent":
        {"BooleanDefault": False},


    "/@1_Component|kind=SheetFolder|relpos=0/@3_Properties_Parameters/@OnlyTestComponentsAndTestBenchesAndMobilitySimsCanContainMetric":
        {"ConstraintEqn": """self.parent().kindName() = "TestComponent"
or self.parent().kindName() = "TestBench"
or self.parent().kindName() = "MobilitySim"
or self.parent().kindName() = "TestBenchSuite"
or self.parent().kindName() = "BallisticTestBench"
or self.parent().kindName() = "CFDTestBench"
or self.parent().kindName() = "BlastTestBench"
or self.parent().kindName() = "CADTestBench"
or self.parent().kindName() = "CarTestBench"
or self.parent().kindName() = "ParametricTestBench"
or self.parent().kindName() = "ExcelWrapper"
or self.parent().kindName() = "MATLABWrapper"
or self.parent().kindName() = "PythonWrapper"
"""
        },

    "/@6_Testing|kind=SheetFolder|relpos=0/@3_PET_TBS|kind=ParadigmSheet|relpos=0/@Range":
        {"Help": """A single number (e.g. 1.5), a comma-separated range (e.g. 1.5,3), or a list of enum values (e.g. "item one";"item two" or 1.5;3)""",
         "FieldDefault": ""},
    }

cyphy_mods = {
    "/@Dynamics|kind=SheetFolder/@ModelicaModel|kind=ParadigmSheet/@ModelicaConnector|kind=Model":
        {"Decorator": "Mga.CPMDecorator",
            "GeneralPreferences": "fillColor = 0xdddddd\nportLabelLength=0",
            "Icon": "modelica_connector.png",
            "PortIcon": "modelica_connector_port.png",
            "IsTypeInfoShown": False},
    "/@CAD|kind=SheetFolder/@3_SolidModeling|kind=ParadigmSheet/@CADModel|kind=Model":
        {"Decorator": "Mga.CPMDecorator",
            "GeneralPreferences": "fillColor = 0xdddddd\nhelp=$META_DOCROOT$34d163d3-f7d6-4178-bcae-6c469f52be14.html\nportLabelLength=0",
            "Icon": "cad_model.png"},
    "/@CAD|kind=SheetFolder/@3_SolidModeling|kind=ParadigmSheet/@FileFormat|kind=EnumAttribute":
        {"MenuItems": """Creo
AP 203
AP 214
Solidworks
NX
STL
"""},
    "/@Dynamics|kind=SheetFolder/@ModelicaModel|kind=ParadigmSheet/@ModelicaModel|kind=Model":
        {"Decorator": "Mga.CPMDecorator"}

    }


def do_mods(project, mods, extra=None):
    with in_tx(project):
        for kind, attrs in mods.iteritems():
            model = project.RootFolder.GetObjectByPathDisp(kind)
            print model.AbsPath + " " + kind
            for attrname, attrvalue in attrs.iteritems():
                # print model.Meta.Name
                # print [a.Name for a in model.Meta.DefinedAttributes]
                if isinstance(attrvalue, basestring):
                    model.SetStrAttrByNameDisp(attrname, attrvalue)
                    print "  " + attrname + "=" + attrvalue
                else:
                    print "  " + attrname
                    model.SetBoolAttrByNameDisp(attrname, attrvalue)

        if extra:
            extra(project)


def import_xme(project, filename):
    xme = Dispatch("Mga.MgaParser")
    resolver = Dispatch("Mga.MgaResolver")
    resolver.IsInteractive = False
    xme.Resolver = resolver
    xme.ParseProject(project, filename)


def update_core():
    subprocess.check_call('git checkout --theirs CyPhyML-core.xme', shell=True)
    project = Dispatch("Mga.MgaProject")
    project.Create("MGA=" + "CyPhyML-core.mga", "MetaGME")
    import_xme(project, "CyPhyML-core.xme")


    do_mods(project, core_mods)

    project.Save()

    dumper = Dispatch("Mga.MgaDumper")
    dumper.DumpProject(project, "CyPhyML-core.xme")
    project.Close(True)


def switch_lib(from_, to):
    old_path = from_.AbsPath
    new_path = to.AbsPath
    project = to.Project

    def log(msg):
        raise Exception(msg)

    def switch_children(parent):
        if parent.IsLibObject:
            return
        for child in parent.ChildObjects:
            if child.ObjType == OBJTYPE_MODEL or child.ObjType == OBJTYPE_FOLDER:
                switch_children(child)
            if child.ObjType == OBJTYPE_REFERENCE:
                if not child.Referred:
                    log('Warning: null reference <a href="mga:{}">{}</a>'.format(child.ID, child.Name))
                path = child.Referred.AbsPath
                if path.startswith(old_path):
                    new_referred_path = new_path + path[len(old_path):]
                    try:
                        child.Referred = project.RootFolder.GetObjectByPathDisp(new_referred_path)
                    except:
                        log('Cannot switch <a href="mga:{}">{}</a> to {}'.format(child.ID, child.AbsPath, new_referred_path))

    switch_children(project.RootFolder)


def update_cyphy(version):
    subprocess.check_call('git checkout --theirs ./CyPhyML.xme', shell=True)
    project = Dispatch("Mga.MgaProject")
    project.Create("MGA=" + "CyPhyML.mga", "MetaGME")
    import_xme(project, "CyPhyML.xme")

    do_mods(project, cyphy_mods)

    import_xme(project, "CyPhyML-tonka.xme")

    with in_tx(project):
        project.RootFolder.GetObjectByPathDisp("/@Schematic|kind=SheetFolder/@EDACad|kind=ParadigmSheet/@CADModel|kind=ModelProxy").Referred = \
            project.RootFolder.GetObjectByPathDisp("/@CAD|kind=SheetFolder/@3_SolidModeling|kind=ParadigmSheet/@CADModel|kind=Model")

        libs = [lib for lib in project.RootFolder.ChildFolders if lib.LibraryName]
        assert len(libs) == 2
        libs.sort(key=lambda lib: lib.ID)
        original_lib, copy_lib = libs

        # RefreshLibrary fails because xme import reassigns the GUIDs
        # original_lib_relid = original_lib.RelID
        # original_lib.RefreshLibrary(original_lib.LibraryName)
        # original_lib = project.RootFolder.ChildObjectByRelID(original_lib_relid)
        switch_lib(from_=copy_lib, to=original_lib)
        copy_lib.DestroyObject()
        metaint_currentobj = project.RootFolder.GetObjectByPathDisp("/@Ports")

        project.Version = version

    project.Save()

    metaint = Dispatch("MGA.Interpreter.MetaInterpreter")
    metaint.InvokeEx(project, metaint_currentobj, Dispatch("Mga.MgaFCOs"), 128)
    # launcher = win32com.client.DispatchEx("Mga.MgaLauncher")
    # launcher.RunComponent("MGA.Interpreter.MetaInterpreter", self.project, N)

    project.Save()

    dumper = Dispatch("Mga.MgaDumper")
    dumper.DumpProject(project, "CyPhyML.xme")
    project.Close(True)


if __name__ == '__main__':
    def test():
        project = Dispatch("Mga.MgaProject")
        project.Open("MGA=" + "CyPhyML.mga")
        import sys
        sys.stdin.readline()
        metaint = Dispatch("MGA.Interpreter.MetaInterpreter")
        metaint.InvokeEx(project, None, Dispatch("Mga.MgaFCOs"), 128)
        exit(1)

    parser = argparse.ArgumentParser(description='Updates CyPhyML-core.xme and CyPhyML.xme during a merge.')
    parser.add_argument('--version', required=True, help='version string for CyPhyML')
    args = parser.parse_args()

    update_core()
    update_cyphy(version=args.version)
