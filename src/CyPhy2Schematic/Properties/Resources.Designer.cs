﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CyPhy2Schematic.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CyPhy2Schematic.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        /// 
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        /// 
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///
        ///REM TODO: check model
        ///&quot;%META_PAT [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string chipFit {
            get {
                return ResourceManager.GetString("chipFit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        /// 
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        /// 
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///	popd
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///
        ///&quot;%META_PATH%\bin\Python27\ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LaunchSpiceViewer {
            get {
                return ResourceManager.GetString("LaunchSpiceViewer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        /// 
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        /// 
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot;
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///mkdir log 2&gt;nul
        ///&quot;%META_PATH%\bin\Python27\Scripts\python.exe&quot; -E -m layout_json.reimport &gt; log\layout_json.reimport.log 2&gt;&amp;1
        ///if %ERRORLEVEL% neq 0 (type log\la [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string layoutReimport {
            get {
                return ResourceManager.GetString("layoutReimport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        ///
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        ///
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///
        ///&quot;%META_PATH%\bin\LayoutSolver.exe&quot;  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string placement {
            get {
                return ResourceManager.GetString("placement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        ///
        ///SETLOCAL EnableDelayedExpansion
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        ///
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///
        ///&quot;% [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string placeonly {
            get {
                return ResourceManager.GetString("placeonly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #!/usr/bin/env
        ///
        ///import json
        ///import string
        ///
        ///def main():
        ///    # Get Testbench Manifest Parameters
        ///    with open(&quot;testbench_manifest.json&quot;, &apos;r&apos;) as f_in:
        ///        testbench_manifest = json.load(f_in)
        ///
        ///    parameters = dict()
        ///    for param in testbench_manifest[&quot;Parameters&quot;]:
        ///        parameters[param[&quot;Name&quot;]] = str(param[&quot;Value&quot;])
        ///
        ///    with open(&quot;schema_template.cir&quot;, &apos;r&apos;) as f_in:
        ///        schema_template = f_in.read()
        ///    
        ///    # Try to generate schema file from template
        ///    try:
        ///        schema [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PopulateSchemaTemplate {
            get {
                return ResourceManager.GetString("PopulateSchemaTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo on
        ///SetLocal EnableDelayedExpansion
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        /// 
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        /// 
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///	popd
        ///    exit !QUERY_ERRORLEVEL!
        ///)        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string runspice {
            get {
                return ResourceManager.GetString("runspice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;eagle xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns=&quot;eagle&quot; version=&quot;6.5.0&quot;&gt;
        ///  &lt;drawing&gt;
        ///    &lt;settings&gt;
        ///      &lt;setting alwaysvectorfont=&quot;no&quot;/&gt;
        ///      &lt;setting verticaltext=&quot;up&quot;/&gt;
        ///    &lt;/settings&gt;
        ///    &lt;grid distance=&quot;0.01&quot; unitdist=&quot;inch&quot; unit=&quot;inch&quot; style=&quot;lines&quot; multiple=&quot;1&quot; display=&quot;yes&quot; altdistance=&quot;0.01&quot; altunitdist=&quot;inch&quot; altunit=&quot;inch&quot;/&gt;
        ///    &lt;layers&gt;
        ///      &lt;layer number=&quot;1&quot; name=&quot;Top&quot; color=&quot;4&quot; fill=&quot;1&quot; visible=&quot;n [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string schematicTemplate {
            get {
                return ResourceManager.GetString("schematicTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @echo off
        ///pushd %~dp0
        ///%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;
        /// 
        ///SET QUERY_ERRORLEVEL=%ERRORLEVEL%
        /// 
        ///IF %QUERY_ERRORLEVEL% == 0 (
        ///    FOR /F &quot;skip=2 tokens=2,*&quot; %%A IN (&apos;%SystemRoot%\SysWoW64\REG.exe query &quot;HKLM\software\META&quot; /v &quot;META_PATH&quot;&apos;) DO SET META_PATH=%%B)
        ///)
        ///IF %QUERY_ERRORLEVEL% == 1 (
        ///    echo on
        ///    echo &quot;META tools not installed.&quot; &gt;&gt; _FAILED.txt
        ///    echo &quot;META tools not installed.&quot;
        ///    exit /b %QUERY_ERRORLEVEL%
        ///)
        ///
        ///&quot;%META_PATH%\bin\python27\scripts [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string showChipFitResults {
            get {
                return ResourceManager.GetString("showChipFitResults", resourceCulture);
            }
        }
    }
}
