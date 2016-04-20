#!/usr/bin/env python
""" Make a BOM with Eagle-CAD component reference designators. """
#
# Revise the Component Reference Designators (CRDs), such as "R2" or "C3", in a Bill of Materials (BOM)
# file, to match the CRDs in the Eagle CAD files.  See MOT-445.
#
# An HTML reference-designator-mapping table is parsed to get a dictionary, mapping the
# GME CRDs to the Eagle CRDs.  Then, that mapping is used to create a BOM with Eagle CRDs,
# for use by PCB assemblers, based on a BOM with GME CRDs.
#
# The parameters that are needed are:
#   1. The path to the HTML cross reference file, with a default of "reference_designator_mapping_table.html",
#   2. The path to the input BOM file, with a default of "BomTable.csv",
#   3. An output-file path, with a default of "assemblyBom.csv".
#
# Usage:
# -x"HtmlXrefPath" -b"CsvBomPath" [-o"outputPath"]
#
# Debug parameters:
# -x"C:\Users\Henry\repos\tonkalib\designs\AstableMultivibrator\results\vwiynabo\reference_designator_mapping_table.html"
# -b"C:\Users\Henry\repos\tonkalib\designs\AstableMultivibrator\results\kl5203zz\BomTable.csv"
#
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


import os
import sys
from optparse import OptionParser
from BeautifulSoup import BeautifulSoup
import csv
import StringIO

#----------------------------------------------
g_warningCount = 0
g_errorCount = 0

m_reference_designator_key = "Reference Designator"


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


#----------------------------------------------

def get_xref_dictionary(html_path):
    """ Create a mapping of GME CRDs to Eagle CRDs. """
    xref_dictionary = {}
    soup = BeautifulSoup(open(html_path))
    # print(soup.prettify())
    for tableContents in soup.body.table.contents:
        if (len(tableContents) > 1):
            value = None
            for subContents in tableContents.contents:
                if not 'NavigableString' in str(type(subContents)):
                    if subContents.name == u'td':
                        if (subContents.attrs[0][1] == u'col1'):
                            value = str(subContents.text)
                        if (subContents.attrs[0][1] == u'col2'):
                            key = str(subContents.text)
                            if (value and not key in xref_dictionary.keys()):
                                if key == value:
                                    print("Skip adding '{0}': '{1}' to dictionary.".format(key, value))
                                else:
                                    print("Adding '{0}': '{1}' to dictionary.".format(key, value))
                                    xref_dictionary[key] = value
    return xref_dictionary
#----------------------------------------------


def divide_input_bom_csv_file(input_bom_csv_path):
    """ Reads the input BOM CSV file and separates it into three arrays of lines of text:
    1. A title that is merely informative,
    2. A sequential listing of the column headers for the table,
    3. A table that includes the header row, as well as rows of data.
    """
    title = []
    table = []
    headers = []
    now_in_table = False
    input_file = open(input_bom_csv_path, 'r')
    for line in input_file:
        if (not now_in_table) and (m_reference_designator_key in line):
            headers = [x for x in (csv.reader(StringIO.StringIO(line)))][0]
            now_in_table = True
        if now_in_table:
            table.append(line)
        else:
            title.append(line)
    return (title, headers, table)

#----------------------------------------------


def convert_bom_table_to_eagle(headers, table, xref_dict):
    """
    Routine that takes the column headers and data table of the input BOM file,
    replaces the GME CRDs in the table with Eagle CRDs,
    and returns the resulting table as a CSV-formatted string.
    """
    inmemory_file = StringIO.StringIO()
    csv.register_dialect('bomDialect', lineterminator='\n', quoting=csv.QUOTE_NONNUMERIC)
    csv_dict_writer = csv.DictWriter(inmemory_file, headers, dialect='bomDialect')
    csv_dict_reader = csv.DictReader(table)
    for table_row_dictionary in csv_dict_reader:
        if m_reference_designator_key in table_row_dictionary.keys():
            original_crd_list = [x.strip() for x in table_row_dictionary[m_reference_designator_key].split(',')]
            new_crd_list = []
            for refDes in original_crd_list:
                if refDes in xref_dict.keys():
                    print("In the BOM, should replace '{0}' with '{1}'.".format(refDes, xref_dict[refDes]))
                    new_crd_list.append(xref_dict[refDes])
                else:
                    new_crd_list.append(refDes)
            new_sorted_crd_list = sorted(new_crd_list)

            # Use spaces to separate multiple reference designators in a CSV cell
            # if the resulting string will fit OK in the cell.
            if(sum([len(x) + 2 for x in new_sorted_crd_list]) < 32):
                table_row_dictionary[m_reference_designator_key] = ', '.join(new_sorted_crd_list)
            else:
                table_row_dictionary[m_reference_designator_key] = ',\n'.join(new_sorted_crd_list)

        csv_dict_writer.writerow(table_row_dictionary)
    return inmemory_file.getvalue()

#----------------------------------------------


def main_make_eagle_bom():
    parser = OptionParser()
    parser.add_option("-x", "--xref", dest="xrefFile",
                      default='reference_designator_mapping_table.html',
                      help="path of the HTML cross reference file", metavar="FILE")
    parser.add_option("-b", "--bom", dest="bomFile", metavar="FILE",
                      default='BomTable.csv',
                      help="path of the BOM file")
    parser.add_option("-o", "--output", dest="outputFile", metavar="FILE",
                      default='assemblyBom.csv',
                      help="path of the output file")

    (options, args) = parser.parse_args()

    my_xref_path = options.xrefFile
    my_bom_path = options.bomFile
    my_output_path = options.outputFile

    # Check the my_xref_path
    if not my_xref_path:
        error("Use the -x parameter to specify the path to the HTML cross reference file.")
    else:
        if not os.path.exists(my_xref_path):
            error("The HTML cross reference file '{0}' doesn't exist.".format(my_xref_path))

    # Check the my_bom_path
    if not my_bom_path:
        error("Use the -b parameter to specify the path to the BOM (.csv) file.")
    else:
        if not os.path.exists(my_bom_path):
            error("The BOM (.csv) file '{0}' doesn't exist.".format(my_xref_path))

    # parse the HTML cross reference file to a dictionary
    xref_dictionary = {}
    if (0 == g_errorCount):
        xref_dictionary = get_xref_dictionary(my_xref_path)
        if len(xref_dictionary) <= 0:
            warning("No component reference designator changes.")
    # parse the HTML cross reference file
    if (0 == g_errorCount) and (0 == g_warningCount):
        # Split the CSV file into title, header, and table strings, where:
        # the title is just initial identifying lines of text,
        # the header is the line with the column headers, and
        # the table includes the header plus the following data rows.
        title, headers, table = divide_input_bom_csv_file(my_bom_path)
        print("======= Table Start:")
        print("".join(table))
        print("======= Table End.")
        # Convert the BOM table based on the HTML cross reference dictionary
        new_table = convert_bom_table_to_eagle(headers, table, xref_dictionary)
        print("======= New table Start:")
        print(new_table)
        print("======= New table End.")
    # Output the new BOM file
    if (0 == g_errorCount) and (0 == g_warningCount):
        try:
            output_file = open(my_output_path, 'w')
            output_file.writelines(title)
            output_file.write(",".join(headers) + '\n')
            output_file.write(new_table)
            output_file.close()
        except IOError:
            error('Unable to write file "{0}".'.format(my_output_path))
    if g_errorCount:
        print("*** Errors were detected. ***")
        return -1
    else:
        return 0


#-------------------------------------------------------------------------------------------
# Run the main program ************************************
if __name__ == "__main__":
    result = main_make_eagle_bom()
    exit(result)

#----------------------------------------------
