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

exit 0
