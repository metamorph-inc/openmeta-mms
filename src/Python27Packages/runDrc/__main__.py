#!/usr/bin/env python
""" Run an Eagle-CAD Board Design-Rules Check. """
# *****************************************************************************************
# Execute Design-Rule Check (DRC) on an Eagle-CAD board.
# See also:  MOT-446.
#
# The parameters needed are:
#   1. The path to the Eagle-CAD eaglecon.exe file.
#   2. The path of the input board (.brd) file.
#
# This script doesn't change the design rule (DRU) file selection, which should have already been set,
#  prior to part placement and auto-routing.
#
# Usage:
# -e"EagleconPath" -b"BoardPath"
#
#   H. Forson, 25-Sept-2014
# *****************************************************************************************

import os
import sys
from optparse import OptionParser
from subprocess import call
from _winreg import *

from get_eagle_path import find_eagle


#----------------------------------------------
g_warningCount = 0
g_errorCount = 0


def warning(args):
    global g_warningCount
    g_warningCount += 1
    sys.stdout.write("*** Warning: ")
    print( args )


def error(args):
    global g_errorCount
    g_errorCount += 1
    sys.stdout.write("*** Error: ")
    print( args )


#----------------------------------------------
# eaglecon schema.brd -C "set confirm yes; DRC; set confirm yes; SHOW; DRC;"

def getEagleDrcCommand(myBoard, myEaglePath):
    cmdString = '"' + myEaglePath + '" "' + myBoard + '" -C "set confirm yes; DRC; set confirm yes; SHOW; DRC;"'
    return cmdString


#----------------------------------------------

def get_default_eaglecon_path():
    result = r"C:\Program Files (x86)\EAGLE-6.5.0\bin\eaglecon.exe"
    eagle_path = find_eagle()
    eaglecon_path = eagle_path.replace("eagle.exe", "eaglecon.exe")
    if eaglecon_path:
        result = eaglecon_path
    else:
        warning("The Eagle app's path was not found in the Windows registry.")
    return result


#----------------------------------------------

def main():
    print("Start the DRC.")
    parser = OptionParser()
    print("Add board option.")
    parser.add_option("-b", "--board", dest="boardFile", metavar="FILE",
                      default=r".\schema.brd",
                      help="path of the (.brd) board file")
    print("Add Eagle option.")
    parser.add_option("-e", "--eagle", dest="eagleFile", metavar="FILE",
                      default=get_default_eaglecon_path(),
                      help="path of the 'eaglecon.exe' file")
    print("Parse the args.")
    (options, args) = parser.parse_args()
    print("Get the board file from the options.")
    myBoard = options.boardFile
    if not os.path.exists(myBoard):
        error('The board file path "{0}" does not exist.'.format(myBoard))
    myEaglePath = options.eagleFile
    if not os.path.exists(myEaglePath):
        error('The file "{0}" does not exist.  Please specify the "eaglecon.exe" path using the -e parameter.'.format(
            myEaglePath))

    if ( not g_errorCount ):

        eagleCommand = getEagleDrcCommand(myBoard, myEaglePath)
        print( eagleCommand )
        returnCode = call(eagleCommand, shell=True)
        print "return code: " + str(returnCode)
        if returnCode < 0:
            warning("Eagle CAD return code = {0}.".format(returnCode))
        if ( g_errorCount > 0) or ( g_warningCount > 0):
            print(
                '*** Python script completed with {0} warnings and {1} errors. ***'.format(g_warningCount, g_errorCount))
        else:
            print( '*** Python script completed OK. ***')
    else:
        print( '*** Python script did not run. ***')
    if (g_warningCount + g_errorCount) == 0:
        return 0
    else:
        return -1

#----------------------------------------------
main()