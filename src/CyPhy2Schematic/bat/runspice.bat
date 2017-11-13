@echo on
SetLocal EnableDelayedExpansion
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
	popd
    exit !QUERY_ERRORLEVEL!
)

if exist "schema.cir.template" (
	"%META_PATH%\bin\python27\scripts\python.exe" PopulateSchemaTemplate.py

	IF !ERRORLEVEL! neq 0 (
		echo on
		echo "Template Population Failed" >> _FAILED.txt
		echo "Template Population Failed."
		popd
		exit !ERRORLEVEL!
	)
)

REM ------------------------
REM Check if SPICE installed
REM ------------------------

if exist "%META_PATH%\bin\spice\bin\ngspice.exe" (
	REM NGSpice
	REM TODO: check model
	for %%f in ("%META_PATH%\bin\spice\share\ngspice") do set SPICE_LIB_DIR= %%~sf

	"%META_PATH%\bin\spice\bin\ngspice.exe" -b -r schema.raw -o schema.log schema.cir
	IF !ERRORLEVEL! neq 0 (
		echo on
		echo "Spice Simulation Failed" >> _FAILED.txt
		echo "Spice Simulation Failed."
		popd
		exit !ERRORLEVEL!
	)

    %windir%\System32\find "run simulation(s) aborted" schema.log >nul && (echo ERROR: Spice Simulation was aborted & exit /b 5)
) else (
	echo "ERROR: NgSPICE not found! Looked for:  %META_PATH%\bin\spice\bin\ngspice.exe" >> _FAILED.txt
	echo "ERROR: NgSPICE not found! Looked for:  %META_PATH%\bin\spice\bin\ngspice.exe"
	popd
	exit 1
)
popd
if exist "schema.raw" (
	"%META_PATH%\bin\python27\scripts\python.exe" -E -m SpiceVisualizer.post_process -m PowerReference schema.raw
)

exit 0
