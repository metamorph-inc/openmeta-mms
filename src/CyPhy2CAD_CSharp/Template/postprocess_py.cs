﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CyPhy2CAD_CSharp.Template
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    
    
    #line 1 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class postprocess_py : postprocess_pyBase
    {
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
        public virtual string TransformText()
        {
            this.GenerationEnvironment = null;
            this.Write(" \r\n");
            this.Write(@"import os
import sys
import logging
import subprocess

def find_python_path():
    import _winreg
    meta_python_path = """"
    try:
        uninstall_key = _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE, r""Software\META"", 0,
                                        _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY)
        val, typ = _winreg.QueryValueEx(uninstall_key, 'META_PATH')
        meta_python_path = os.path.join(val, r""bin\Python27\Scripts\Python.exe"")
    except WindowsError as e:
        meta_python_path = None
        
    return meta_python_path


def call_script(meta_python, scriptname, logger):
    return_out = 0
    try:
        script_cmd = '""{0}"" ""{1}""'\
                    .format(meta_python, scriptname)
            
        status = subprocess.check_output(script_cmd, stderr=subprocess.STDOUT, shell=True)
        logger.info('Subprocess call [{0}] ' + scriptname + ' successful')
    except subprocess.CalledProcessError as err:
        msg = ""Subprocess call failed!""
        msg += ""\n  command       : {0}"".format(err.cmd)
        msg += ""\n  return-code   : {0}"".format(err.returncode)
        if err.output:
            msg += ""\n  console output: \n\n{0}"".format(err.output)
        if err.message:
            msg +=  ""\n  error message : {0}"".format(err.message)
        logger.error(msg)
        return_out = 1

    return return_out
 
if __name__ == '__main__':
    debuglogfile = r'log/");
            
            #line 52 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(CyPhy2CAD_CSharp.TestBenchModel.TestBenchBase.SanitizePythonRawString(LogName)));
            
            #line default
            #line hidden
            this.Write(@"'
    if os.path.exists(debuglogfile):
        os.remove(debuglogfile)
    logger = logging.getLogger('debug')
    if not os.path.exists('log'):
        os.mkdir('log')
    hdlr = logging.FileHandler(debuglogfile)
    formatter = logging.Formatter('%(asctime)s %(levelname)s %(message)s')
    hdlr.setFormatter(formatter)
    logger.addHandler(hdlr)
    logger.setLevel(logging.INFO)

    if not os.path.exists('testbench_manifest.json'):
        logger.error('File does not exist: testbench_manifest.json')
        sys.exit(1)
    if (len(sys.argv) > 1):
        if not os.path.exists(sys.argv[1]):
            logger.error('Given result file does not exist: {0}'.format(sys.argv[1]))
            sys.exit(1)
    errorcnt = 0
    meta_python = find_python_path()
    if meta_python is not None:
    ");
            
            #line 74 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
 foreach (var name in ScriptNames)
    { 
            
            #line default
            #line hidden
            this.Write("    if call_script(meta_python, r\'");
            
            #line 76 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(CyPhy2CAD_CSharp.TestBenchModel.TestBenchBase.SanitizePythonRawString(name)));
            
            #line default
            #line hidden
            this.Write("\', logger) > 0:\r\n    ");
            
            #line 77 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
  } 
            
            #line default
            #line hidden
            this.Write("        logger.error(\'Can not find META Python Package!\')\r\n            errorcnt +" +
                    "=1\r\n\r\n    if errorcnt > 0:\r\n        open(\'_FAILED.txt\', \'wb\').write(\'Script erro" +
                    "r, for details see \' + debuglogfile)\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 83 "C:\Users\snyako\Desktop\META-13.17\src\CyPhy2CAD_CSharp\Template\postprocess_py.tt"
  
public List<string> ScriptNames {get;set;}
public string LogName {get;set;}

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class postprocess_pyBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
    }
    #endregion
}
