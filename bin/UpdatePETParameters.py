
import sys
import os
#sys.path.append(r"C:\Program Files\ISIS\Udm\bin")
#if os.environ.has_key("UDM_PATH"):
#    sys.path.append(os.path.join(os.environ["UDM_PATH"], "bin"))
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

    # TODO: some sort of plugin?
    for k in var_dict:
        var_dict[k] = '0,10'

    gmeCopy = focusObject.convert_udm2gme().ParentFolder.CopyFCODisp(focusObject.convert_udm2gme())
    gmeCopy.Name = gmeCopy.Name + "_Refined"
    focusObject = [c for c in focusObject.parent.children() if udm.UdmId2GmeId(c.id) == gmeCopy.ID][0]
    parameterStudy = [c for c in focusObject.children() if c.type.name == "ParameterStudy"][0]
    designVariables = [c for c in parameterStudy.children() if c.type.name == "DesignVariable"]
    for desVar in designVariables:
        desVar.Range = var_dict[desVar.attr('name')]

    gme = [c for c in gmeCopy.Project.Clients if c.Name == "GME.Application"]
    if gme:
        gme = gme[0].OLEServer
        gmeCopy.Project.CommitTransaction()
        gme.ShowFCO(gmeCopy, False)
        READ_ONLY = 1
        gmeCopy.Project.BeginTransactionInNewTerr(READ_ONLY)


# Allow calling this script with a .mga file as an argument
if __name__ == '__main__':
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
