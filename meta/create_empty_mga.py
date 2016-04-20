"""
Creates an EmptyCyPhyML.mga, an empty file + libraries
"""

import pythoncom
import sys
import win32com.client

mga = win32com.client.DispatchEx("Mga.MgaProject")
mga.Create("MGA=" + "EmptyCyPhyML.mga", "CyPhyML")
sbAddon = win32com.client.DispatchEx("MGA.Addon.CyPhySignalBlocksAddOn")
addon = mga.CreateAddOn(sbAddon)
sbAddon.Initialize(mga)
mga.Notify(11) # GLOBALEVENT_OPEN_PROJECT_FINISHED
pythoncom.PumpWaitingMessages()
pythoncom.PumpWaitingMessages()
mga.Save("", True)

mga.Close(True)
