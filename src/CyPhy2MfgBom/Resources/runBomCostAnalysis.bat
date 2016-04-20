@echo off
pushd %~dp0
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

rem =====
rem Look for the BomCostAnalysis executable in the location for installed machines.
rem Then, test if it exists in the location for development machines. If yes, use that location instead.
rem =====
SET BOMCOSTANALYSIS=%META_PATH%\bin\BomCostAnalysis.exe
IF EXIST "%META_PATH%\src\BomCostAnalysis\bin\Release\BomCostAnalysis.exe" SET BOMCOSTANALYSIS=%META_PATH%\src\BomCostAnalysis\bin\Release\BomCostAnalysis.exe

rem Run BomCostAnalysis.exe and pass in the path to the manifest file
"%BOMCOSTANALYSIS%" testbench_manifest.json

SET BCA_ERRORLEVEL=%ERRORLEVEL%
IF %BCA_ERRORLEVEL% neq 0 (
	echo on
	echo "BomCostAnalysis.exe failed." >> _FAILED.txt
	echo "BomCostAnalysis.exe failed."
	exit /b %BCA_ERRORLEVEL%
)

popd