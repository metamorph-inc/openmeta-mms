from win32com.client import DispatchEx
import os


def xme2mga(path_xme, path_mga):
    # use abspath, since on GME x64-only, parser will be called out-of-proc (which won't share the same cwd)
    abspath_xme = os.path.abspath(path_xme)
    abspath_mga = os.path.abspath(path_mga)

    xme = DispatchEx('Mga.MgaParser')
    (paradigm, parversion, parguid, basename, ver) = xme.GetXMLInfo(abspath_xme)

    mga = DispatchEx('Mga.MgaProject')
    resolver = DispatchEx('Mga.MgaResolver')

    try:
        resolver.IsInteractive = False
        xme.Resolver = resolver
    except AttributeError:
        # Older GME
        pass

    # create_project(mga, 'MGA=' + abspath_mga, paradigm)
    mga.Create('MGA=' + abspath_mga, paradigm)

    try:
        xme.ParseProject(mga, path_xme)
    except Exception:
        mga.Close(True)
        raise

    mga.Save('MGA=' + abspath_mga)
