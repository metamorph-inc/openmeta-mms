@ECHO OFF
PUSHD %~dp0
%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"

SET QUERY_ERRORLEVEL=%ERRORLEVEL%

IF %QUERY_ERRORLEVEL% == 0 (
    FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO SET META_PATH=%%B)
)
IF %QUERY_ERRORLEVEL% == 1 (
    echo on
    echo "META tools not installed." >> _FAILED.txt
    echo "META tools not installed."
    exit /b %QUERY_ERRORLEVEL%
)

 REM --------------------------------
 REM Specific Absroption Ratio (SAR)
 REM --------------------------------

 REM -------
 REM OpenEMS
 REM -------
SET OPENEMS=C:\openEMS\openEMS.exe
"%OPENEMS%" openEMS_input.xml --dump-statistics
SET OPENEMS_ERRORLEVEL=%ERRORLEVEL%
IF %OPENEMS_ERRORLEVEL% NEQ 0 (
    echo on
    echo "Running %OPENEMS% failed." >> _FAILED.txt
    echo "Running %OPENEMS% failed."
    exit /b %OPENEMS_ERRORLEVEL%
)

 REM -----------
 REM Postprocess
 REM -----------
SET NF2FF=C:\openEMS\nf2ff.exe
ECHO Running %NF2FF% nf2ff_input.xml...
"%NF2FF%" nf2ff_input.xml
SET NF2FF_ERRORLEVEL=%ERRORLEVEL%
IF %NF2FF_ERRORLEVEL% NEQ 0 (
    echo on
    echo Running %NF2FF% FAILED. >> _FAILED.txt
    echo Running %NF2FF% FAILED.
    exit /b %NF2FF_ERRORLEVEL%
)

IF EXIST "%META_PATH%\src\CyPhy2RF\FDTDPostprocess\bin\Debug\FDTDPostprocess.exe" (
    SET CP2RFPP="%META_PATH%\src\CyPhy2RF\FDTDPostprocess\bin\Debug\FDTDPostprocess.exe"
) ELSE (
IF EXIST "%META_PATH%\src\CyPhy2RF\FDTDPostprocess\bin\Release\FDTDPostprocess.exe" (
    SET CP2RFPP="%META_PATH%\src\CyPhy2RF\FDTDPostprocess\bin\Release\FDTDPostprocess.exe"
) ELSE (
    SET CP2RFPP="%META_PATH%bin\FDTDPostprocess.exe"
))
ECHO Running %CP2RFPP% -sar openEMS_input.xml...
%CP2RFPP% -sar openEMS_input.xml
SET CP2RFPP_ERRORLEVEL=%ERRORLEVEL%
IF %CP2RFPP_ERRORLEVEL% NEQ 0 (
    echo on
    echo Running %CP2RFPP% FAILED. >> _FAILED.txt
    echo Running %CP2RFPP% FAILED.
    exit /b %CP2RFPP_ERRORLEVEL%
)

POPD
exit /b 0
