# -*- coding: utf-8 -*-
''''set -e
# dirty hack so `bash merge.py` and `python merge.py` both do something
set -x
git checkout metarefs
../../bin/Python27/Scripts/python merge.py "$@"
../../bin/Python27/Scripts/python add_folder_treeIcons.py
../../bin/Python27/Scripts/python add_folder_treeIcons.py
# cat metarefs_new > metarefs
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
CATIA
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


def update_core(upstream_rev):
    subprocess.check_call('git checkout {} CyPhyML-core.xme'.format(upstream_rev), shell=True)
    project = Dispatch("Mga.MgaProject")
    project.Create("MGA=" + "CyPhyML-core.mga", "MetaGME")
    import_xme(project, "CyPhyML-core.xme")


    do_mods(project, core_mods)

    project.Save()

    dumper = Dispatch("Mga.MgaDumper")
    dumper.DumpProject(project, "CyPhyML-core.xme")
    project.Close(True)

def export_core():
    project = Dispatch("Mga.MgaProject")
    project.Create("MGA=" + "CyPhyML-core.mga", "MetaGME")
    import_xme(project, "CyPhyML-core.xme")
    project.Save()

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
                        raise

    switch_children(project.RootFolder)


def update_cyphy(version, upstream_rev):
    subprocess.check_call('git checkout {} ./CyPhyML.xme'.format(upstream_rev), shell=True)
    project = Dispatch("Mga.MgaProject")
    project.Create("MGA=" + "CyPhyML.mga", "MetaGME")
    import_xme(project, "CyPhyML.xme")

    do_mods(project, cyphy_mods)

    with in_tx(project):
        for lib in (l for l in project.RootFolder.ChildFolders if l.LibraryName):
            original_lib_relid = lib.RelID
            lib.RefreshLibrary(lib.LibraryName)
            lib = project.RootFolder.ChildObjectByRelID(original_lib_relid)

    import_xme(project, "CyPhyML-tonka.xme")

    with in_tx(project):
        project.RootFolder.GetObjectByPathDisp("/@Schematic|kind=SheetFolder/@EDACad|kind=ParadigmSheet/@CADModel|kind=ModelProxy").Referred = \
            project.RootFolder.GetObjectByPathDisp("/@CAD|kind=SheetFolder/@3_SolidModeling|kind=ParadigmSheet/@CADModel|kind=Model")

        libs = [lib for lib in project.RootFolder.ChildFolders if lib.LibraryName]
        assert len(libs) == 2
        libs.sort(key=lambda lib: lib.ID)
        original_lib, copy_lib = libs
        print(original_lib.AbsPath)
        print(copy_lib.AbsPath)

        switch_lib(from_=copy_lib, to=original_lib)
        copy_lib.DestroyObject()
        metaint_currentobj = project.RootFolder.GetObjectByPathDisp("/@Ports")

        project.Version = version

    project.Save()

    with in_tx(project):
        for fco in project.AllFCOs(project.CreateFilter()):
            if fco.ObjType == OBJTYPE_REFERENCE and fco.Referred is None:
                raise ValueError('Null reference {}'.format(fco.AbsPath))

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
    parser.add_argument('--upstream_rev', default='MERGE_HEAD', help='specify meta-core/master to skip CyPhyML-core.xme update checkout upstream_rev\'s CyPhyML.xme')
    args = parser.parse_args()

    if args.upstream_rev != "meta-core/master":
        update_core(args.upstream_rev)
    else:
        export_core()
    update_cyphy(version=args.version, upstream_rev=args.upstream_rev)
