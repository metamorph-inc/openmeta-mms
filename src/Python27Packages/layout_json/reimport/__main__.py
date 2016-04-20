from __future__ import print_function
import os
import os.path
import errno
import shutil
import json
import win32com.client
import win32com.client.dynamic
import subprocess
import _winreg as winreg
import sys

def checkNotInUse( file ):
    if os.path.exists(file):
        try:
            os.rename(file,file+"_")
            os.rename(file+"_",file)
        except OSError as e:
            sys.exit( "Access-error on file \"" + str(file) + "\". Please close GME, and then try again.\n" + str(e) )

with open("layoutReimportMetadata.json") as metadataFile:
    metadata = json.loads(metadataFile.read())
    mgaFile = metadata["mgaFile"]
    currentFCO = metadata["currentFCO"]
    # layoutBox = metadata["layoutBox"]

checkNotInUse(mgaFile)

with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r'Software\meta', 0, winreg.KEY_READ) as key:
    meta_path = winreg.QueryValueEx(key, 'META_PATH')[0]

boardSynthesis = [p for p in (os.path.join(meta_path, path) for path in ('bin/BoardSynthesis.exe', 'src/BoardSynthesis/bin/Release/BoardSynthesis.exe')) if os.path.isfile(p)][0]
subprocess.check_call([boardSynthesis, "schema.sch", "layout.json", "-r"])

# Read the board width and height from the layout.json file, to make the layout boxes. MOT-788
with open("layout.json") as layoutFile:
    layoutData = json.loads(layoutFile.read())
    boardHeight = str(layoutData["boardHeight"])
    boardWidth = str(layoutData["boardWidth"])
    layoutBox = "0,0," + boardWidth + "," + boardHeight + ",0;0,0," + boardWidth + "," + boardHeight + ",1"

project = win32com.client.dynamic.Dispatch("Mga.MgaProject")
# KMS: this is better, but requires a GME release post 8/3/2015
# project.OpenEx(u"MGA=" + mgaFile, "CyPhyML", None)
project.Open(u"MGA=" + mgaFile)

try:
    project.BeginTransactionInNewTerr()
    try:
        tb = project.RootFolder.GetObjectByPathDisp(currentFCO)
        tlsut = [child for child in tb.ChildFCOs if child.MetaRole.Name == 'TopLevelSystemUnderTest'][0]
        ca = tlsut.Referred
        ca.SetRegistryValueDisp("layoutFile", "layout.json")
        ca.SetRegistryValueDisp("layoutBox", layoutBox)
        designPath = ca.GetStrAttrByNameDisp("Path")
        if len(designPath) == 0:
            raise Exception("ComponentAssembly {} has no Path attribute".format(ca.Name))
        designPath = os.path.join(os.path.dirname(mgaFile), designPath)
        try:
            os.makedirs(designPath)
        except OSError as exception:
            if exception.errno != errno.EEXIST:
                raise
        shutil.copy("layout.json", designPath)
        print("Copied layout.json to " + designPath)
    finally:
        project.CommitTransaction()
    project.Save("", True)
finally:
    project.Close(True)
