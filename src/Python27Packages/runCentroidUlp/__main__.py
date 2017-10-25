#!/usr/bin/env python
""" Run an Eagle-CAD centroids.ulp file. """
#
# Script to run an Eagle-CAD centroids.ulp file, by producing a Windows command line
# and executing it.
#
# Parameters:
# [-u<path of the .ulp file>] -b<path of the .brd file> [-e<path of the eaglecon.exe file>]
#
# Sample parameters:
# -u"C:\Users\Henry\repos\sandbox\Cam2Gerber\centroids.ulp"
#
# See also: MOT-444
#
# -------------------------------------------------------------------------
# The MIT License (MIT)
#
# Copyright (c) 2014 MetaMorph (http://metamorphsoftware.com)
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
#
#


__author__ = 'Henry'

from subprocess import call
import os
import sys
from optparse import OptionParser
from _winreg import *
import shutil

from get_eagle_path import find_eagle

#----------------------------------------------
g_warningCount = 0
g_errorCount = 0


def warning(args):
    global g_warningCount
    g_warningCount += 1
    sys.stdout.write("*** Warning: ")
    print(args)


def error(args):
    global g_errorCount
    g_errorCount += 1
    sys.stdout.write("*** Error: ")
    print(args)


def get_windows_command_to_run_ulp(my_ulp_path, my_board, my_eagle_path):
    """ Make a Windows command to use Eagle to run a ULP.
    For example:
    eaglecon C:\o1ul2wxq\schema.brd -C "set confirm yes; RUN centroid-screamingcircuits-smd.ulp; set confirm yes; quit;"
    """
    command_string = '"' + my_eagle_path + '" "' + my_board + \
                     '" -C "set confirm yes; RUN "' + my_ulp_path + '"; set confirm yes; quit;"'
    return command_string


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


def main_run_ulp():
    """ Main routine that runs the ULP.
    """
    module_directory = os.path.dirname(__file__)
    default_ulp_path = os.path.join(module_directory, "centroids.ulp").replace("\\", "/")
    parser = OptionParser()
    parser.add_option("-u", "--ulp", dest="ulpFile",
                      default=(default_ulp_path if os.path.exists(default_ulp_path) else None),
                      help="path of the ULP file", metavar="FILE")
    parser.add_option("-b", "--board", dest="boardFile", metavar="FILE",
                      default=r".\schema.brd",
                      help="path of the (.brd) board file")
    parser.add_option("-e", "--eagle", dest="eagleFile", metavar="FILE",
                      default=get_default_eaglecon_path(),
                      help="path of the 'eaglecon.exe' file")
    parser.add_option("-t", "--test", dest="testVector", metavar="TEST_VECTOR",
                      default="0",
                      help="sets the test vector bitmap; 1 = print a warning, 2 = print an error.")

    (options, args) = parser.parse_args()
    my_test_vector = options.testVector
    if int(my_test_vector) & 1:
        warning("The test vector is {0}.".format(my_test_vector))
    if int(my_test_vector) & 2:
        error("The test vector is {0}.".format(my_test_vector))
    my_ulp_path = options.ulpFile
    if not my_ulp_path:
        error('The path of the ULP file must be specified with the -u parameter.')
    my_board = options.boardFile
    if not os.path.exists(my_board):
        error('The board file path "{0}" does not exist.'.format(my_board))
    my_eagle_path = options.eagleFile
    if not os.path.exists(my_eagle_path):
        error('The file "{0}" does not exist.  Please specify the "eaglecon.exe" path using the -e parameter.'.format(
            my_eagle_path))
    if not os.path.exists(my_ulp_path):
        # Check if the ULP file is specified relative to the module directory, for MOT-743.
        module_ulp_path = os.path.join(module_directory, my_ulp_path).replace("\\", "/")
        if os.path.exists(module_ulp_path):
            my_ulp_path = module_ulp_path
    if (not g_errorCount):
        # Copy the ULP to the board directory
        if os.path.normpath(os.path.dirname(my_board)) != os.path.normpath(os.path.dirname(my_ulp_path)):
            shutil.copy(my_ulp_path, os.path.dirname(my_board))
        board_dir = os.path.dirname(my_board)

    if (not g_errorCount):
        copied_ulp = os.path.join(board_dir if board_dir else ".", os.path.basename(my_ulp_path))
        windows_eagle_command = get_windows_command_to_run_ulp(copied_ulp, my_board, my_eagle_path)
        print(windows_eagle_command)

        # Here's where we run the Windows command:
        return_code = call(windows_eagle_command, shell=True)

        print "return code: " + str(return_code)
        if return_code < 0:
            warning("Eagle CAD return code = {0}.".format(return_code))
        print('*** ULP job completed with {0} warnings and {1} errors. ***'.format(g_warningCount, g_errorCount))
    else:
        print('*** ULP job did not run. ***')
#    if (g_warningCount + g_errorCount) == 0:
    if g_errorCount == 0:    # MOT-543, ignore warnings
        return 0
    else:
        return -1


# Run the main program ************************************
if __name__ == "__main__":
    result = main_run_ulp()
    exit(result)


#----------------------------------------------
