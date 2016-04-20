@ECHO OFF
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe make.msbuild /t:All /m /nodeReuse:false
