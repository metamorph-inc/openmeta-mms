@echo off
pushd %~dp0
 
SET QUERY_ERRORLEVEL=%ERRORLEVEL%
 
IF %QUERY_ERRORLEVEL% == 0 (
    FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO SET META_PATH=%%B)
)
IF %QUERY_ERRORLEVEL% == 1 (
    echo on
    echo "META tools not installed."
    exit /b %QUERY_ERRORLEVEL%
)
mkdir log 2>nul
"%META_PATH%\bin\Python27\Scripts\python.exe" -E -m layout_json.reimport > log\layout_json.reimport.log 2>&1
if %ERRORLEVEL% neq 0 (type log\layout_json.reimport.log & echo. & echo Reimport failed & pause)

popd
