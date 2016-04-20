//--------------------------------------------------------------------
//
//  Uncomment.cs
//
//  This file defines a class used to remove comments from a SystemC file,
//  to help the SystemC CAT create ports from a SystemC model.
//
//  See also: MOT-419 "CAT module for SystemC"
//
//  Henry Forson, 11/11/2014
//
//--------------------------------------------------------------------

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemCParser
{
    public class Uncomment
    {
        public string result;

        public Uncomment(string fileName)
        {
            result = "";

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string rawInput = "";

            // Read the file
            try
            {
                if ((fileName == null) || (!File.Exists(fileName)))
                {
                    throw new Exception(string.Format("Error: File '{0}' does not exist.", fileName));
                }

                rawInput = System.IO.File.ReadAllText(fileName);

            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                string msg = string.Format("Exception while reading file '{0}':\n {1}",
                            fileName, e.Message);
                throw new Exception(msg, e);
            }

            // Now, "input" has the raw file text as a big string.

            // Trim the comments.  See: http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
            result = Regex.Replace(rawInput,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);
        }
    }
}
