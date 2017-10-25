__author__ = 'Henry'

#!/usr/bin/env python
#
# Script to parse an Eagle-CAD CAM file, to produce Windows command lines
# that are executed to produce Gerber plots and/or an Excellon drill file.
#
# Parameters:
# -c<path of the .cam file> -b<path of the .brd file> [-e<path of the eaglecon.exe file>]
#
# Sample parameters:
# -c"C:\Users\MyPie\Seeed_Gerber_Generator_4-layer_1-2-15-16.cam" -b"C:\Users\MyPie\schema.brd"
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
from subprocess import call
import re
import os
import sys
from optparse import OptionParser
from _winreg import *

from get_eagle_path import find_eagle

#----------------------------------------------
r"""
Cam2Gerber
==========

**Cam2Gerber** contains a Python script for EAGLE PCB software, to run a ".cam" file from a Windows command line,
producing Gerber files.

Parameters
----------
 -c<path of the .cam file>
 -b<path of the .brd file>
 -e<path of the eaglecon.exe file> (defaults to "C:\Program Files (x86)\EAGLE-6.5.0\bin\eaglecon.exe")

Outputs
-------
The outputs are extended Gerber files and/or Excellon drill files, as specified by the ".cam" file.

Background
----------
EAGLE software  is used for designing printed circuit boards.  It can produce Gerber files and Excellon drill files,
used to make printed circuit boards, in two ways:

1. EAGLE's PCB layout editor includes a CAM GUI that lets users configure the mapping of EAGLE CAD layers to Gerber
files and Excellon drill files, save or load settings in ".cam" files, and run a single job that produces multiple
output files for PCB manufacturing.

2. EAGLE has a Windows command that can generate a single output file at a time. It doesn't use a ".cam" file; instead,
everything is specified by parameters.

The Problem
-----------
Sometimes you may want an easy way to produce all the output for a board automatically, without needing a person to
click through a CAM GUI.  Method (1), using the CAM GUI, needs human intervention to click through the GUI.  Method (2),
 generating each output file with a separate windows command having detailed parameters, is complex, not well explained,
 and not easy to maintain.

The Solution
------------
The **Cam2Gerber** Python script takes a ".cam" file as input to specify what outputs are needed, and then executes the
Windows commands needed to make that output.  It allows a board's output files to be generated from a script, without
human intervention.  It also allows users to customize the outputs via a modified ".cam" file, instead of changing
complicated command-line parameters.

Limitations
-----------
The Python script currently only supports the output of Extended Gerber (RS-274X) files and Excellon drill files.  It
was written for EAGLE version 6.5.0 and Python version 2.7.5.

"""
#----------------------------------------------

g_warningCount = 0
g_errorCount = 0


def warning(args):
    """
     Print a warning message to stdout, and increment a global warning-message counter.

     args -- A string containing the message to print.
    """
    global g_warningCount
    g_warningCount += 1
    sys.stdout.write("*** Warning: ")
    print(args)


def error(args):
    """
     Print an error message to stdout, and increment a global error-message counter.

     args -- A string containing the message to print.
    """
    global g_errorCount
    g_errorCount += 1
    sys.stdout.write("*** Error: ")
    print(args)


#----------------------------------------------

class CamFile:
    def __init__(self, cam_file_path):
        """ Initializes the CamFile class with the CAM file's path. """
        if not os.path.exists(cam_file_path):
            msg = ('A copy of the CAM file, which would be renamed as "{0}", does not exist.\n'
                   'Please check that there is a parameter named "CAM file" in the PCB Mfg test bench, with a value\n'
                   'set to the path of the existing Eagle-CAD CAM file you want to use.')
            error(msg.format(cam_file_path))
        else:
            handle = open(cam_file_path, 'r')
            self.all_lines = handle.readlines()
            handle.close()
            self.line_index = 0
            self.max_lines = len(self.all_lines)
            self.current_line_string = ""

    def get_next_line(self):
        """ Returns the next line from the CAM file, with trailing whitespace trimmed. """
        result = None
        if self.line_index < self.max_lines:
            result = self.all_lines[self.line_index].rstrip()
            self.line_index += 1
        self.current_line_string = result
        return result

    def is_line_prefix(self, prefix):
        """ Check if the current line starts with a prefix string. """
        result = False
        if self.current_line_string[:len(prefix)] == prefix:
            result = True
        return result

    def get_key_eq_value(self, key):
        """ Check if a line starts with <key>=<value>, and if so
         advances to the next CAM line and return the value string.
        """
        value = None
        pattern = key + "="
        if self.is_line_prefix(pattern):
            value = self.current_line_string[len(pattern):]
            self.get_next_line()
        return value

    def eof(self):
        """ Return True if all CAM file lines have been processed."""
        return self.line_index >= self.max_lines

    def skip_blank_line(self):
        """ Skip the current CAM file line if it is blank. """
        result = False
        if not self.current_line_string:
            self.get_next_line()
            result = True
        return result

    def get_key_lang_eq_quoted_val(self, key):
        """ If the current line matches <key>[<languageCode>]="<value>",
        then return a tuple with the languageCode and value, and
        advance to the next line
        """
        result = []
        pattern = key + r'\[(.+)\]="(.*)"'
        match = re.match(pattern, self.current_line_string)
        if match:
            result.append(match.group(1))
            result.append(match.group(2))
            self.get_next_line()
        return result

    def get_multiple_key_lang_eq_quoted_val(self, key):
        """ Match one or more lines matching <key>[<languageCode>]="<value>",
        and if found, advance past them and return a dictionary with
        languageCodes as keys mapping to the values.
        """
        matching_done = False
        result_dict = {}
        while not matching_done:
            parts = self.get_key_lang_eq_quoted_val(key)
            if parts:
                result_dict[parts[0]] = parts[1]
            else:
                matching_done = True
        return result_dict

    def get_val_in_sq_brackets(self):
        """ Check if the line starts with text in square brackets, and if so,
        advance to the next line and return the string found in the brackets.
        """
        result = None
        pattern = r'\[(.+)\]'
        match = re.match(pattern, self.current_line_string)
        if match:
            result = match.group(1)
            self.get_next_line()
        return result

    def get_key_eq_quoted_val(self, key):
        """ Check if a line starts with <key>="<value>", and if so
         advance to the next line and return the value.
        """
        result = None
        pattern = key + r'="(.*)"'
        match = re.match(pattern, self.current_line_string)
        if match:
            result = match.group(1)
            self.get_next_line()
        return result

    # generic parse of cam name/value pair
    def get_key_value_pairs(self, key):
        """ Checks if there are one or more lines of:
         - <key>[<languageCode]="<value>",
         or a single line of:
         - <key>="<value">,
         or a single line of:
         - <key>=<value>
         and advances past these lines.

         In the first case, a dictionary is returned mapping
         languages to values.  In the second and third case,
         the value string is returned.
        """
        result = self.get_multiple_key_lang_eq_quoted_val(key)
        if not result:
            result = self.get_key_eq_quoted_val(key)
        if not result:
            result = self.get_key_eq_value(key)
        return result


#---------------------------------------

def parse_cam_file(cam_file_path):
    """ Parses a CAM file using the CamFile class, returning a dictionary with
    a description of the CAM job and sections describing each output file to be generated.
    """
    done = False
    cam = CamFile(cam_file_path)
    big_result = {}
    section_list = []
    if not cam.max_lines:
        done = True
        error("Unable to open '{0}'.".format(cam_file_path))
    if not done:
        cam.get_next_line()
        val = cam.get_val_in_sq_brackets()
        if not ('CAM Processor Job' == val):
            done = True
            error("File '{0}' was not a CAM processor job.".format(cam_file_path))
    if not done:
        big_result['Description'] = cam.get_multiple_key_lang_eq_quoted_val('Description')

    if not done:
        maybe_section = True
        while maybe_section:
            section_value = cam.get_key_eq_value('Section')
            if None != section_value:
                section_list.append(section_value)
            else:
                maybe_section = False
        if len(section_list) == 0:
            done = True
            error("No sections found in the CAM file.")
    if not done:
        big_result['Sections'] = []
    # parse multiple sections
    while (not done) and (not cam.eof()):
        section_results = {}
        done = not cam.skip_blank_line()
        if not done:
            this_section = cam.get_val_in_sq_brackets()
            if not (this_section and this_section in section_list):
                done = True
                warning("Section not found.")
            else:
                section_results['tag'] = this_section
        if not done:
            section_results['name'] = cam.get_key_value_pairs('Name')
            section_results['prompt'] = cam.get_key_value_pairs('Prompt')
            section_results['device'] = cam.get_key_value_pairs('Device')
            if not section_results['device']:
                done = True
                error("Device specification not found.")
        if not done:
            section_results['wheel'] = cam.get_key_value_pairs('Wheel')
            section_results['rack'] = cam.get_key_value_pairs('Rack')
            section_results['scale'] = cam.get_key_value_pairs('Scale')
            section_results['output'] = cam.get_key_value_pairs('Output')
            if not section_results['output']:
                done = True
                error("Output file name not found.")
        if not done:
            section_results['flags'] = cam.get_key_value_pairs('Flags')
            if not section_results['flags']:
                done = True
                error("Flags not found.")
        if not done:
            section_results['emulate'] = cam.get_key_value_pairs('Emulate')
            if not section_results['emulate']:
                done = True
                error("Emulate not found.")
        if not done:
            section_results['offset'] = cam.get_key_value_pairs('Offset')
            if not section_results['offset']:
                done = True
                error("Offset not found.")
        if not done:
            section_results['sheet'] = cam.get_key_value_pairs('Sheet')
            section_results['tolerance'] = cam.get_key_value_pairs('Tolerance')
            section_results['pen'] = cam.get_key_value_pairs('Pen')
            section_results['page'] = cam.get_key_value_pairs('Page')

            section_results['layers'] = cam.get_key_value_pairs('Layers')
            if not section_results['layers']:
                done = True
                error("Layers not found")
        if not done:
            section_results['colors'] = cam.get_key_value_pairs('Colors')
        if not done:
            # Add section info to results
            big_result['Sections'].append(section_results)
    return big_result


#----------------------------------------------

def get_output_name(name_template, board_path):
    """ Returns a file name string without CAM-name placeholders, from a file name string that may contain them.

     nameTemplate is a string possibly containing placeholders, such as "%N.cmp".
     boardPath is the path to the Eagle ".brd" file.
    """
    result = None
    if name_template and board_path:
        replacement_dict = {'%N': os.path.splitext(os.path.basename(board_path))[0],
                            '%E': os.path.splitext(os.path.basename(board_path))[1][1:],
                            '%P': os.path.dirname(board_path), '%H': os.path.expanduser('~'), '%%': '%'}

        #do the replacements here in one pass
        rep = dict((re.escape(k), v) for k, v in replacement_dict.iteritems())
        pattern = re.compile("|".join(rep.keys()))
        result = pattern.sub(lambda m: rep[re.escape(m.group(0))], name_template)
    return result

#----------------------------------------------
g_boardLayerNumberToNameMap = {}


def get_board_layer_number_to_name_map(board_path):
    """ Returns a map of an Eagle board's layer numbers to layer names. """
    if g_boardLayerNumberToNameMap:
        # TODO: We might check that the board path hasn't changed before returning the previous map.
        return g_boardLayerNumberToNameMap
    if not os.path.exists(board_path):
        error('Unable to open the board file "{0}".'.format(board_path))
        return g_boardLayerNumberToNameMap
    # TODO: We should ideally parse the board file as XML instead of using regular expressions to find layers.
    pattern = r'<layer number="([0-9]+)" name="([^"]+)"'
    handle = open(board_path, 'r')
    all_lines = handle.readlines()
    for line in all_lines:
        match = re.match(pattern, line)
        if match:
            g_boardLayerNumberToNameMap[match.group(1)] = match.group(2)
    return g_boardLayerNumberToNameMap


#----------------------------------------------

def get_valid_layers(layer_string, board_path, section_name):
    """ Generate a string with a space-separated list of board layers that are
    both in the board and in the layerString input.
    """
    valid_list = []
    board_layer_number_to_name_map = get_board_layer_number_to_name_map(board_path)
    layer_list = layer_string.split()
    for layer in layer_list:
        if (layer in board_layer_number_to_name_map) or (layer in board_layer_number_to_name_map.values()):
            valid_list.append(layer)
        else:
            msg_string = "Eagle layer {0} in the CAM tab named '{1}' is not a layer listed in the board file."
            warning(msg_string.format(layer, section_name))
    valid_layer_string = " ".join(valid_list)
    return " " + valid_layer_string


#----------------------------------------------


# Convert CAM-file flag parameter to CAM-processor options:
def get_flag_string(cam_section):
    """ Convert a space-separated string of zeroes and ones from a CAM-file flag value to
    a sequence of CAM-processor parameters, for parameters different from defaults.

    Here are the flag CAM-Processor parameters in sequence, from the Eagle manual:
    -m- Mirror output
    -r- Rotate output 90 degrees
    -u- Rotate output 180 degrees
    -c+ Positive coordinates
    -q- Quick plot
    -O+ Optimize pen movement
    -f+ Fill pads

    A trailing '+" means the option's default is on, and '-' means it's off.
    """
    result_string = ''
    default_value = ' 0 0 0 1 0 1 1'.split()
    flag_letters = 'm r u c q O f'.split()
    raw_flag_list = cam_section['flags'].split()
    for index, value in enumerate(raw_flag_list):
        if value != default_value[index]:
            result_string += ' -' + flag_letters[index] + ('+' if '1' == value else '-')
    return result_string


#----------------------------------------------
def get_offsets(cam_section):
    """  Convert a CAM string containing a number, a linear-units string, a space, another
    number, and another linear-units string, to a tuple of two floats representing inches. """
    units = {
        'mil': (1 / 1000.0),
        'cm': (1 / 25.4),
        'mm': (1 / 25.4),
        'inch': 1.0
    }
    x_result = 0.0
    y_result = 0.0
    pattern = r'^([-+]?(\d+(\.\d*)?|\.\d+)([eE][-+]?\d+)?)(\S+)\s([-+]?(\d+(\.\d*)?|\.\d+)([eE][-+]?\d+)?)(\S+)$'
    match = re.match(pattern, cam_section['offset'])
    if match:
        x_result = float(match.group(1)) * float(units.get(match.group(5), 0))
        y_result = float(match.group(6)) * float(units.get(match.group(10), 0))
    return x_result, y_result


#----------------------------------------------

def get_eagle_command_from_cam_section(cam_section, board_path, eagle_path):
    """Produce a Windows command to run Eagle to generate a CAM output file.

    camSection is a dictionary containing parameter info.
    boardpath is the path to the Eagle board file.
    eaglePath is the path to the eaglecon.exe file.
    """
    output_name = get_output_name(cam_section['output'], board_path)
    # Check if we need to create a subdirectory for the output file.
    if not os.path.isabs(output_name):
        subdirectory = os.path.dirname( output_name )
        if (len(subdirectory) > 0):
            targetSubdirectoryPath = os.path.join( '.', subdirectory)
            if not os.path.exists( targetSubdirectoryPath ):
                os.makedirs( targetSubdirectoryPath )
    wheel_name = get_output_name(cam_section['wheel'], board_path)
    in_name = cam_section['name']
    if isinstance(in_name, dict) and ('en' in in_name):
        section_name = in_name['en']
    else:
        section_name = str(in_name)
    valid_layers = get_valid_layers(cam_section['layers'], board_path, section_name)
    flag_string = get_flag_string(cam_section)

    x_flag = ''
    y_flag = ''
    x_offset, y_offset = get_offsets(cam_section)
    if x_offset:
        x_flag = ' -x' + str(x_offset)
    if y_offset:
        y_flag = ' -y' + str(y_offset)

    command_string = ('"' + eagle_path + '"' + flag_string + ' -X -d"' + cam_section['device'] + '" -o"' + output_name +
                      (('" -W"' + wheel_name) if wheel_name else '') +
                      '"' + x_flag + y_flag +
                      ' "' + board_path + '" ' + valid_layers)
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


def main_cam_routine():
    """ Main routine that parses the .cam file, generates Windows commands, and executes them,
    to produce extended Gerber files and/or Excellon drill files.
    """
    parse_result = {}
    parser = OptionParser()
    parser.add_option("-c", "--cam", dest="camFile",
                      help="path of the (.cam) CAM job file", metavar="FILE")
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
    my_cam_path = options.camFile
    if not my_cam_path:
        error('The path of the CAM file must be specified with the -c parameter.\n' +
              'Check "Command to execute" in the testbench workflow execution-task attributes.')
    my_board = options.boardFile
    if not os.path.exists(my_board):
        error(('The board file path "{0}" does not exist.\n' +
               'Please run the PickAndPlace testbench first.').format(my_board))
    my_eagle_path = options.eagleFile
    if not os.path.exists(my_eagle_path):
        msg = ('The file "{0}" does not exist.  Please specify the "eaglecon.exe" path using the -e parameter,\n' +
               'in the "Command to execute" attribute of the testbench\'s workflow execution task.')
        error(msg.format(my_eagle_path))

    if not g_errorCount:
        parse_result = parse_cam_file(my_cam_path)

    expected_devices = ["EXCELLON", "GERBER_RS274X", "GERBER_RS274X_25"]

    if 'Sections' in parse_result:
        for camSection in parse_result['Sections']:
            eagle_command = get_eagle_command_from_cam_section(camSection, my_board, my_eagle_path)
            print(eagle_command)
            if not camSection['device'] in expected_devices:
                warning(
                    ('Device "{0}" is not supported, and the generated command line may be missing parameters. '
                     'Only "GERBER_RS274X", "GERBER_RS274X_25", and "EXCELLON" '
                     'are supported.').format(camSection['device']))
            return_code = call(eagle_command, shell=True)
            print "return code: " + str(return_code)
            if return_code < 0:
                warning("Eagle CAD return code = {0}.".format(return_code))
                # call('DIR /A-D /OD /TW "' + os.path.dirname(myBoard) + '"', shell=True)
        print('*** CAM job completed with {0} warnings and {1} errors. ***'.format(g_warningCount, g_errorCount))
    else:
        print('*** CAM job did not run. ***')
#    if g_warningCount + g_errorCount == 0:
    if g_errorCount == 0:  # Ignore warnings, MOT-543
        return 0
    else:
        return -1

# Run the main program ************************************
if __name__ == "__main__":
    result = main_cam_routine()
    exit(result)

#----------------------------------------------
