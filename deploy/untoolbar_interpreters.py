"""
Removes from the GME CyPhyML toolbar interpreters that aren't specified to be in the toolbar in the installer
"""

import sys
import os
import os.path
import glob

from xml.etree import ElementTree

if __name__ == '__main__':
    associated_progids = []
    for filename in glob.glob(os.path.join(os.path.dirname(os.path.abspath(__file__)), '*.wxi')):
        tree = ElementTree.parse(filename).getroot()
        for progid in tree.findall(".//{http://schemas.microsoft.com/wix/2006/wi}ProgId"):
            for file in tree.findall(".//{http://schemas.microsoft.com/wix/2006/wi}RegistryKey[@Key='Associated']/{http://schemas.microsoft.com/wix/2006/wi}RegistryValue[@Name='CyPhyML']"):
                associated_progids.append(progid.get('Id', ''))
    #print associated_progids
    import win32com.client
    reg = win32com.client.DispatchEx('Mga.MgaRegistrar')
    for progid in reg.GetAssociatedComponentsDisp('CyPhyML', 7, 2):
        if "ForgePort" in progid:
            continue
        if progid not in associated_progids:
            reg.Disassociate(progid, 'CyPhyML', 2)
