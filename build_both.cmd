@Setlocal EnableDelayedExpansion

@where git.exe >nul 2>nul || (echo Must have git.exe in %%PATH%% & exit /b 1)
@if exist %windir%\Microsoft.NET\assembly\GAC_MSIL\CyPhyML ( echo You have a GACed CyPhyML.dll. Did you install the installer^? Uninstall it first. & exit /b 2 )

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe make.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe make_tonka.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!
