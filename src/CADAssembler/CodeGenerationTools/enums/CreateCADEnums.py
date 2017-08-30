#	This program creates .h and .cpp files defining enums and functions to 
#	convert between the eunums and strings.
#	The input to this program is a file (e.g.CreateCADEnums_InputFile.txt) that
#	describes the enums and output files that will be created.  The output files
#   are described at the top of the input file.
#
# 	R. Owens, 5/16/2017 
#
################################################################################
class CADExcep(Exception):
    """
    Base class for all CAD exceptions
    """

    def __init__(self, message):
        self.message = message

################################################################################

class	ValueToList:
    def __init__(self, in_Value, in_List):
        self.value = in_Value
        self.list = in_List

    def __str__(self):
        return \
        "\nValue:  " + str(self.value) + \
        "\nList:   " + str(self.list)

################################################################################
class ValueToValue:
    def __init__(self, in_Value_1, in_Value_2):
        self.value_1 = in_Value_1
        self.value_2 = in_Value_2

    def __str__(self):
        return \
            "Value_1: " + str(self.value_1) + "    " + \
            "Value_2: " + str(self.value_2)

################################################################################
class  EnumData:
    def __init__(self):
        self.inputFileLineNum = 0
        self.writtenToFile =    False			# bool
        self.createEnum =       False			# bool
        self.fileID  =          ""	            # CAD, Creo, SolidWorks
        self.functionName =     "" 		        # CADAssemblyConstraintType
        self.enumType 	=       ""		        # e_CADAssemblyConstraintType
        self.enumToSrings = 	[]    # ValueToList e.g. CAD_ASM_MATE  MATE MATE_Z

    def __str__(self):
        temp_string = \
            'inputFileLineNum: ' + str(self.inputFileLineNum) + \
            '\nwrittenToFile:    ' + str(self.writtenToFile) + \
            '\ncreateEnum:       ' + str(self.createEnum)  + \
            '\nfileID"           ' + self.fileID   + \
            '\nfunctionName:     ' + self.functionName  + \
            '\nenumType:         ' +  self.enumType
        for  i in self.enumToSrings:
            temp_string += str(i)
        return temp_string

################################################################################
class EnumLinked:
    def __init__(self):
        self.inputFileLineNum = 0
        self.writtenToFile =    False			# bool
        self.fileID  =          ""	            # Creo, SolidWorks,  NOT CAD
        self.functionName =     "" 		        # ProAsmcompConstrType
        self.enumInputArgumentType = ""         # e_CADAssemblyConstraintType
        self.enumReturnType =   ""              # ProAsmcompConstrType
        self.commoneEnumToSpecificEnum = 	[]  # ValueToValue e.g. CAD_ASM_MATE  PRO_ASM_MATE

    def __str__(self):
        temp_string = \
            'inputFileLineNum:        ' + str(self.inputFileLineNum) + \
            '\nwrittenToFile:         ' + str(self.writtenToFile) + \
            '\nfileID                 ' + self.fileID   + \
            '\nfunctionName:          ' + self.functionName  + \
            '\nenumInputArgumentType: ' + self.enumInputArgumentType + \
            '\nenumReturnType:        ' + self.enumReturnType
        for i in self.commoneEnumToSpecificEnum:
            temp_string += "\n" + str(i)
        return temp_string

################################################################################
def WriteDoNotEditMsg(in_FileHandle):
    f = in_FileHandle
    f.write('\n// WARNING - DO NOT EDIT THIS FILE')
    f.write('\n// This file was auto generated by src\CADAssembler\CodeGenerationTools\enums\CreateCADEnums.bat.')
    f.write('\n// To edit, modify src\CADAssembler\CodeGenerationTools\enums\CreateCADEnums_InputFile.txt and run CreateCADEnums.bat.')
    f.write('\n\n')

################################################################################
def WriteCAD_h_Header(in_FileHandle):
    f = in_FileHandle
    WriteDoNotEditMsg(f)
    f.write('#ifndef CAD_STRING_TO_ENUM_CONVERSIONS_H')
    f.write('\n#define	CAD_STRING_TO_ENUM_CONVERSIONS_H')
    f.write('\n')
    f.write('\n#include "isis_application_exception.h"')
    f.write('\n#include <string>')
    f.write('\n')
    f.write('\n#pragma warning( disable : 4290 )  // a future feature : exception specification, i.e. throw')
    f.write('\n')
    f.write('\nnamespace isis')
    f.write('\n{')

################################################################################
def WriteCAD_cpp_Header(in_FileHandle):
    f = in_FileHandle
    WriteDoNotEditMsg(f)
    f.write('#include "CADStringToEnumConversions.h"')
    f.write('\n#include <boost/algorithm/string/case_conv.hpp>')
    f.write('\n#include "CADCommonConstants.h"')
    f.write('\n')
    f.write('\nusing namespace std;')
    f.write('\n')
    f.write('\nnamespace isis')
    f.write('\n{')

################################################################################
def WriteCreo_h_Header (in_FileHandle):
    f = in_FileHandle
    WriteDoNotEditMsg(f)
    f.write('/*! \\file CreoStringToEnumConversions.h')
    f.write('\n   \\brief  Functions to convert between strings and enums.')
    f.write('\n')
    f.write('\n	The input/output XML files that are used by the CAD applications')
    f.write('\n	do not persist enums for known values.  They instead persist strings,')
    f.write('\n	which for efficiency reasons are converted to enums.  This file provides')
    f.write('\n	the conversion routines.')
    f.write('\n */')
    f.write('\n#ifndef CREO_STRING_TO_ENUM_CONVERSIONS_H')
    f.write('\n#define CREO_STRING_TO_ENUM_CONVERSIONS_H')
    f.write('\n')
    f.write('\n#pragma warning( disable : 4290 )  // a future feature : exception specification, i.e. throw')
    f.write('\n')
    f.write('\n#include <isis_application_exception.h>')
    f.write('\n#include <isis_include_ptc_headers.h>')
    f.write('\n#include <string>')
    f.write('\n#include <iostream>')
    f.write('\n#include <CADStringToEnumConversions.h>')
    f.write('\n')
    f.write('\nusing namespace std;')
    f.write('\n')
    f.write('\nnamespace isis')
    f.write('\n{')

################################################################################
def WriteCreo_cpp_Header(in_FileHandle):
    f = in_FileHandle
    WriteDoNotEditMsg(f)
    f.write('#include <CreoStringToEnumConversions.h>')
    f.write('\n#include <CommonUtilities.h>')
    f.write('\n#include <CADCommonConstants.h>')
    f.write('\n#include <sstream>')
    f.write('\n#include <boost/algorithm/string.hpp>')	
    f.write('\n')
    f.write('\nnamespace isis')
    f.write('\n{')

################################################################################
def Write_h_Footer (in_FileHandle):
    f = in_FileHandle
    f.write('\n\n} // End Namespace')
    f.write('\n#endif')
################################################################################
def Write_cpp_Footer (in_FileHandle):
    f = in_FileHandle
    f.write('\n\n} // End Namespace')

################################################################################
def WriteFileHeaders( in_FileHandles_h_dict, in_FileHandles_cpp_dict):
    if  'CREO' in  in_FileHandles_h_dict:
        WriteCreo_h_Header(in_FileHandles_h_dict['CREO'])
        WriteCreo_cpp_Header(in_FileHandles_cpp_dict['CREO'])

    if  'CAD' in  in_FileHandles_h_dict:
        WriteCAD_h_Header(in_FileHandles_h_dict['CAD'])
        WriteCAD_cpp_Header(in_FileHandles_cpp_dict['CAD'])

################################################################################
def WriteFileFooters(in_FileHandles_h_dict, in_FileHandles_cpp_dict):
    if 'CREO' in in_FileHandles_h_dict:
        Write_h_Footer(in_FileHandles_h_dict['CREO'])
        Write_cpp_Footer(in_FileHandles_cpp_dict['CREO'])

    if 'CAD' in in_FileHandles_h_dict:
        Write_h_Footer(in_FileHandles_h_dict['CAD'])
        Write_cpp_Footer(in_FileHandles_cpp_dict['CAD'])

################################################################################
# return -1 if LinkedValue not found
def FindIndexOfLinkedValue(in_EnumsLinked_list, in_FunctionName):
    index = 0
    for enumLinked_itr in in_EnumsLinked_list:
        if enumLinked_itr.functionName == in_FunctionName:
            return index
        index += 1

    return -1
################################################################################
def WriteEnums(in_FileHandles_h_dict, in_FileHandles_cpp_dict, in_Enums_list, in_out_EnumsLinked_list):

    function_name = "WriteEnums"

    for enumData_itr in in_Enums_list:
        f_h =   in_FileHandles_h_dict[enumData_itr.fileID]
        f_cpp = in_FileHandles_cpp_dict[enumData_itr.fileID]
        f_h.write('\n\n\t////////////////////////////////////////////////////////////////////////////////////////')

        if enumData_itr.createEnum:
            f_h.write('\n')
            f_h.write('\n\tenum' + " " + enumData_itr.enumType )
            f_h.write('\n\t{')
            for enumToSrings_itr in enumData_itr.enumToSrings:
                f_h.write('\n\t\t' + enumToSrings_itr.value + ',')
            f_h.write('\n\t};')

        ############
        # h file
        ############

        # ProAsmcompConstrType ProAsmcompConstrType_enum(const std::string & in_String)

        f_h.write('\n')
        f_h.write('\n\t' + enumData_itr.enumType + ' ' + enumData_itr.functionName + '_enum( const std::string &in_String)')
        f_h.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception);')
        f_h.write('\n')

        # std::string ProAsmcompConstrType_string(ProAsmcompConstrType in_Enum )

        f_h.write('\n\tstd::string ' + enumData_itr.functionName + '_string( ' + enumData_itr.enumType + ' in_Enum )')
        f_h.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception);')


        ############
        # cpp file
        ############

        # ProAsmcompConstrType ProAsmcompConstrType_enum(const std::string & in_String)

        f_cpp.write('\n')
        f_cpp.write('\n\t////////////////////////////////////////////////////////////////////////////////////////')
        f_cpp.write('\n\t' + enumData_itr.enumType + ' ' + enumData_itr.functionName + '_enum( const std::string &in_String)')
        f_cpp.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception)')
        f_cpp.write('\n\t{')
        f_cpp.write('\n\t\tstd::string strUpper = boost::to_upper_copy<std::string>(in_String);\n')
        ifWord = 'if    '
        acceptableStrings = ""
        for enumToSrings_itr in enumData_itr.enumToSrings:
            for  stringLiteral in enumToSrings_itr.list:
                if stringLiteral != '""':
                    f_cpp.write('\n\t\t' + ifWord + '\t(strUpper.compare("' + stringLiteral.upper() + '") == 0 ) return ' +  enumToSrings_itr.value + ';')
                    acceptableStrings += stringLiteral + '   '
                else:
                    f_cpp.write('\n\t\t' + ifWord + '\t(strUpper.compare("") == 0 ) return ' +  enumToSrings_itr.value + ';')
                    acceptableStrings += 'null_string'  + '   '
                ifWord = 'else if'

        f_cpp.write('\n\n\t\tstd::stringstream errorString;')
        f_cpp.write('\n\t\terrorString << "Function - " << __FUNCTION__ << ", was passed: " << in_String <<')
        f_cpp.write('\n\t\t\t", which is an erroneous value. Allowed values are: " <<')
        f_cpp.write('\n\t\t\t"' + acceptableStrings.strip() + '";' )
        f_cpp.write('\n\t\tthrow isis::application_exception(errorString);')
        f_cpp.write('\n\t}')

        # std::string ProAsmcompConstrType_string(ProAsmcompConstrType in_Enum )

        f_cpp.write('\n')
        f_cpp.write('\n\tstd::string ' + enumData_itr.functionName + '_string( ' + enumData_itr.enumType + ' in_Enum )')
        f_cpp.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception)')
        f_cpp.write('\n\t{')
        f_cpp.write('\n\t\tswitch ( in_Enum )')
        f_cpp.write('\n\t\t{')
        acceptableEnums = ""
        for enumToSrings_itr in enumData_itr.enumToSrings:
            f_cpp.write('\n\t\t\tcase ' + enumToSrings_itr.value + ':')
            if enumToSrings_itr.list[0] != '""':
                f_cpp.write('\n\t\t\t\treturn "' + enumToSrings_itr.list[0] + '";')
            else:
                f_cpp.write('\n\t\t\t\treturn "";')
            f_cpp.write('\n\t\t\t\tbreak;')
            acceptableEnums += enumToSrings_itr.value + '   '
        f_cpp.write('\n\t\t\tdefault:')
        f_cpp.write('\n\t\t\t\tstd::stringstream errorString;')
        f_cpp.write('\n\t\t\t\terrorString << "Function - " << __FUNCTION__ << ", was passed: " << in_Enum <<')
        f_cpp.write('\n\t\t\t\t\t", which is an erroneous value. Allowed values are: " <<')
        f_cpp.write('\n\t\t\t\t\t"' + acceptableEnums.strip() + '";' )
        f_cpp.write('\n\t\t\t\tthrow isis::application_exception(errorString);')
        f_cpp.write('\n\t\t}')
        f_cpp.write('\n\t}')

        #################
        # Linked Fields
        #################
        index_linked = FindIndexOfLinkedValue(in_out_EnumsLinked_list, enumData_itr.functionName)

        if index_linked >= 0:
            f_h.write('\n')
            f_h.write(
                '\n\t' + in_out_EnumsLinked_list[index_linked].enumReturnType + ' ' + enumData_itr.functionName + '_enum( ' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' in_Enum )')
            f_h.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception);')
            f_h.write('\n')
            f_h.write(
                '\n\tstd::string ' + enumData_itr.functionName + '_string( ' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' in_Enum )')
            f_h.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception);')
            f_h.write('\n')


            # _CADFeatureGeometryType CADFeatureGeometryType_enum( ProType in_Enum )
            functionName_temp = in_out_EnumsLinked_list[index_linked].enumInputArgumentType.lstrip('e_')
            f_h.write(
                '\n\t' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' ' + functionName_temp + '_enum( ' + in_out_EnumsLinked_list[index_linked].enumReturnType + ' in_Enum )')
            f_h.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception);')


            # ProAsmcompConstrType ProAsmcompConstrType_enum( e_CADAssemblyConstraintType in_Enum )

            f_cpp.write('\n')
            f_cpp.write(
                '\n\t' + in_out_EnumsLinked_list[index_linked].enumReturnType + ' ' + enumData_itr.functionName + '_enum( ' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' in_Enum )')
            f_cpp.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception)')
            f_cpp.write('\n\t{')
            f_cpp.write('\n\t\tswitch ( in_Enum )')
            f_cpp.write('\n\t\t{')
            acceptableEnums = ""
            for enumToSrings_itr in in_out_EnumsLinked_list[index_linked].commoneEnumToSpecificEnum:
                f_cpp.write('\n\t\t\tcase ' + enumToSrings_itr.value_1 + ':')
                f_cpp.write('\n\t\t\t\treturn ' + enumToSrings_itr.value_2 + ';')
                f_cpp.write('\n\t\t\t\tbreak;')
                acceptableEnums += enumToSrings_itr.value_1 + '   '
            f_cpp.write('\n\t\t\tdefault:')
            f_cpp.write('\n\t\t\t\tstd::stringstream errorString;')
            f_cpp.write('\n\t\t\t\terrorString << "Function - " << __FUNCTION__ << ", was passed: " << in_Enum <<')
            f_cpp.write('\n\t\t\t\t\t", which is an erroneous value. Allowed values are: " <<')
            f_cpp.write('\n\t\t\t\t\t"' + acceptableEnums.strip() + '";')
            f_cpp.write('\n\t\t\t\tthrow isis::application_exception(errorString);')
            f_cpp.write('\n\t\t}')
            f_cpp.write('\n\t}')

            # std::string ProAsmcompConstrType_string(e_CADAssemblyConstraintType in_CADAssemblyConstraintType_enum)

            f_cpp.write('\n')
            f_cpp.write('\n\tstd::string ' + enumData_itr.functionName + '_string( ' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' in_Enum )')
            f_cpp.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception)')
            f_cpp.write('\n\t{')
            f_cpp.write('\n\t\ttry')
            f_cpp.write('\n\t\t{')
            f_cpp.write('\n\t\t\t' + in_out_EnumsLinked_list[index_linked].enumReturnType + ' tempType;')
            f_cpp.write('\n\t\t\ttempType = ' + in_out_EnumsLinked_list[index_linked].functionName + '_enum( in_Enum );')
            f_cpp.write('\n\t\t\treturn ' + in_out_EnumsLinked_list[index_linked].functionName + '_string( tempType );')
            f_cpp.write('\n\t\t}')
            f_cpp.write('\n\t\tcatch ( isis::application_exception ex )')
            f_cpp.write('\n\t\t{')
            f_cpp.write('\n\t\t\tstd::stringstream errorString;')
            f_cpp.write('\n\t\t\terrorString << ex.tostring() << std::endl << "Function - " << __FUNCTION__ << ')
            f_cpp.write('\n\t\t\t\t\t", failed to convert ' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' to ' + in_out_EnumsLinked_list[index_linked].functionName + '_string";')
            f_cpp.write('\n\t\t\tthrow isis::application_exception(errorString);	')
            f_cpp.write('\n\t\t}')
            f_cpp.write('\n\t}')


            # _CADFeatureGeometryType CADFeatureGeometryType_enum( ProType in_Enum )

            f_cpp.write('\n')
            f_cpp.write(
                '\n\t' + in_out_EnumsLinked_list[index_linked].enumInputArgumentType + ' ' + functionName_temp + '_enum( ' + in_out_EnumsLinked_list[index_linked].enumReturnType + ' in_Enum )')
            f_cpp.write('\n\t\t\t\t\t\t\t\t\t\tthrow (isis::application_exception)')
            f_cpp.write('\n\t{')
            f_cpp.write('\n\t\tswitch ( in_Enum )')
            f_cpp.write('\n\t\t{')
            acceptableEnums = ""
            for enumToSrings_itr in in_out_EnumsLinked_list[index_linked].commoneEnumToSpecificEnum:
                f_cpp.write('\n\t\t\tcase ' + enumToSrings_itr.value_2 + ':')
                f_cpp.write('\n\t\t\t\treturn ' + enumToSrings_itr.value_1 + ';')
                f_cpp.write('\n\t\t\t\tbreak;')
                acceptableEnums += enumToSrings_itr.value_2 + '   '
            f_cpp.write('\n\t\t\tdefault:')
            f_cpp.write('\n\t\t\t\tstd::stringstream errorString;')
            f_cpp.write('\n\t\t\t\terrorString << "Function - " << __FUNCTION__ << ", was passed: " << in_Enum <<')
            f_cpp.write('\n\t\t\t\t\t", which is an erroneous value. Allowed values are: " <<')
            f_cpp.write('\n\t\t\t\t\t"' + acceptableEnums.strip() + '";')
            f_cpp.write('\n\t\t\t\tthrow isis::application_exception(errorString);')
            f_cpp.write('\n\t\t}')
            f_cpp.write('\n\t}')


            in_out_EnumsLinked_list[index_linked].writtenToFile = True

    ################################################################################


################################################################################
################################# Main #########################################
################################################################################
def main():
    function_name = "main"
    enumData_temp = EnumData()
    enumLinked_temp = EnumLinked()
    enums_list = []
    enumsLinked_list = []
    currentArgumentType = ""
    state = 'START'
    line_count = 0
    fileID_Name_dict = {}
    input_file_name = '.\CreateCADEnums_InputFile.txt'
    inputFile = open(input_file_name, 'r')
    try:
        for line in iter(inputFile):
            #print line

            line_count += 1
            if len(line) == 0:
                continue

            line = line.replace('\t', ' ')

            comment_index = line.find("//")
            if comment_index >= 0:
                line = line[0:comment_index]

            comment_index = line.find("#")
            if comment_index >= 0:
                line = line[0:comment_index]

            line = line.strip()
            if len(line) == 0:
                continue
            words = line.split()
            #print len(words)
            #print words
            #print "---------------"

            #for word in words:
            #    print word

            START = 1
            FILE  = 2
            ENUM_EXISTING = 3
            ENUM_NEW = 4
            ENUM_LINKED =5

            definedEnums_list = []

            if words[0].upper() == 'FILE':
                fileID_Name_dict[words[1].upper()] = words[2]
                state = 'FILE'
            elif words[0].upper() == 'ENUM_EXISTING':
                if state == "ENUM_LINKED":
                    enumsLinked_list.append(enumLinked_temp)
                    enumLinked_temp = EnumLinked()
                #if state == 'ENUM_EXISTING' or state == 'ENUM_NEW' or state == 'ENUM_LINKED':
                if state == 'ENUM_EXISTING' or state == 'ENUM_NEW':
                    enums_list.append(enumData_temp )
                    enumData_temp = EnumData()
                state = 'ENUM_EXISTING'
            elif words[0].upper() == 'ENUM_NEW':
                if state == "ENUM_LINKED":
                    enumsLinked_list.append(enumLinked_temp)
                    enumLinked_temp = EnumLinked()
                # if state == 'ENUM_EXISTING' or state == 'ENUM_NEW' or state == 'ENUM_LINKED':
                if state == 'ENUM_EXISTING' or state == 'ENUM_NEW':
                    enums_list.append(enumData_temp )
                    enumData_temp = EnumData()
                state = 'ENUM_NEW'
            elif words[0].upper() == 'ENUM_LINKED':
                if state == "ENUM_LINKED":
                    enumsLinked_list.append(enumLinked_temp)
                    enumLinked_temp = EnumLinked()
                if state == 'ENUM_EXISTING' or state == 'ENUM_NEW':
                    enums_list.append(enumData_temp )
                    enumData_temp = EnumData()
                state ='ENUM_LINKED'

            ##################
            # ENUM_EXISTING
            ##################

            if state == 'ENUM_EXISTING':
                if words[0].upper() == 'ENUM_EXISTING':
                    print "ENUM_EXISTING"
                    if len(words) != 4:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 4 tokens. \n   Line: " + line)
                    enumData_temp.inputFileLineNum = line_count
                    enumData_temp.writtenToFile =   False
                    enumData_temp.createEnum =      False
                    enumData_temp.enumType 	=   words[1]        # e.g. e_CADAssemblyConstraintType
                    enumData_temp.functionName = words[2]	    # e.g. CADAssemblyConstraintType
                    enumData_temp.fileID  =      words[3].upper()        # e.g CAD, Creo, SolidWorks
                    currentArgumentType =  words[1]
                else:
                    enumStrings = []
                    if len(words) < 2:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 2 or more tokens. \n   Line: " + line)
                    for i in range(1, len(words) ):
                        #print '----> range: ' + str(i)
                        # Some strings might have a % which indicates a space
                        enumStrings.append(words[i].replace('%', ' '))

                    enumData_temp.enumToSrings.append(ValueToList(words[0],enumStrings ))

            if state == 'ENUM_NEW':
                if words[0].upper() == 'ENUM_NEW':
                    print "ENUM_NEW"
                    if len(words) != 4:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 4 tokens. \n   Line: " + line)
                    enumData_temp.inputFileLineNum = line_count
                    enumData_temp.writtenToFile = False
                    enumData_temp.createEnum = True
                    enumData_temp.enumType = words[1]  # e.g. e_CADAssemblyConstraintType
                    enumData_temp.functionName = words[2]  # e.g. CADAssemblyConstraintType
                    enumData_temp.fileID = words[3].upper()  # e.g CAD, Creo, SolidWorks
                    currentArgumentType =  words[1]
                else:
                    enumStrings = []
                    if len(words) < 2:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 2 or more tokens. \n   Line: " + line)
                    for i in range(1, len(words)):
                        #print '----> range: ' + str(i)
                        # Some strings might have a % which indicates a space
                        enumStrings.append(words[i].replace('%', ' '))
                    enumData_temp.enumToSrings.append(ValueToList(words[0], enumStrings))

            if state == 'ENUM_LINKED':
                if words[0].upper() == 'ENUM_LINKED':
                    if len(words) != 4:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 4 tokens. \n   Line: " + line)
                    print "ENUM_LINKED"
                    enumLinked_temp.inputFileLineNum = line_count
                    enumLinked_temp.writtenToFile = False
                    enumLinked_temp.fileID = words[3].upper()           # e.g Creo, SolidWorks,  NOT CAD
                    enumLinked_temp.functionName = words[2]     # e.g. ProAsmcompConstrType  NoT CAD...
                    enumLinked_temp.enumInputArgumentType  = currentArgumentType   # e.g. e_CADAssemblyConstraintType
                    enumLinked_temp.enumReturnType =  words[1]  # ProAsmcompConstrType
                else:
                    if len(words) != 2:
                        raise CADExcep("Error, Function: " + function_name + "\n   " + input_file_name +
                            " line number: " + str(line_count) + "\n   Expected 2 tokens. \n   Line: " + line)
                    enumLinked_temp.commoneEnumToSpecificEnum.append(ValueToValue(words[0], words[1]))



                            #if len(words) > 1 :
            #    print (words[1])

        if state == 'ENUM_EXISTING' or state == 'ENUM_NEW':
            enums_list.append(enumData_temp )
            enumData_temp = EnumData()

        if state == "ENUM_LINKED":
            enumsLinked_list.append(enumLinked_temp)
            enumLinked_temp = EnumLinked()


        print '******************** File IDs to File Name *****************************'
        print 'fileID_Name_dict length: ' + str(len(fileID_Name_dict))
        print '----------------------------'
        for key, value in fileID_Name_dict.iteritems():
            print  key + '  ' + value

        print '************************** Enum Data ***********************************'
        print 'enum_list length length: ' + str(len(enums_list))
        for enumData_itr in enums_list:
            print '----------------------------'
            print str(enumData_itr)

        print '*********************** Enum Linked Data *******************************'
        print 'enumsLinked_list length: ' + str(len(enumsLinked_list))
        for enumLinked in enumsLinked_list:
            print '----------------------------'
            print str(enumLinked)

        #### Close Input File ######
        inputFile.close()

        ###############################################
        ######### Open .h and .cpp Files ##############
        ###############################################
        fileHandles_h_dict = {}
        fileHandles_cpp_dict = {}

        for key, value in fileID_Name_dict.iteritems():
            fileHandles_h_dict[key] = open(value + '.h', 'w')
            fileHandles_cpp_dict[key] = open(value + '.cpp', 'w')

        ###############################################
        ######### Write File Headers ##################
        ###############################################
        WriteFileHeaders ( fileHandles_h_dict, fileHandles_cpp_dict )

        ###############################################
        ######### Write Enums ##################
        ###############################################
        WriteEnums(fileHandles_h_dict, fileHandles_cpp_dict, enums_list, enumsLinked_list )


        ###############################################
        ######### Write File Footers ##################
        ###############################################
        WriteFileFooters( fileHandles_h_dict, fileHandles_cpp_dict)

        ###############################################
        #########Close .h and .cpp Files ##############
        ###############################################
        for key, value in fileID_Name_dict.iteritems():
            fileHandles_h_dict[key].close()
            fileHandles_cpp_dict[key].close()


        print "\nCompleted Successfully."


    except Exception as e:
        print "\nFailed."
        print "\n" + e.message

if __name__ == '__main__':
    main()