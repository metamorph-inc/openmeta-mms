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

"%META_PATH%\bin\LayoutSolver.exe" layout-input.json layout.json %*
SET LAYOUT_ERRORLEVEL=%ERRORLEVEL%
IF %LAYOUT_ERRORLEVEL% == 42 (
  SET /A USING_PARTIAL_LAYOUT=1
) ELSE IF %LAYOUT_ERRORLEVEL% neq 0 (
  	echo on
  	echo "Layout Solver Failed. " %LAYOUT_ERRORLEVEL% >> _FAILED.txt
  	echo "Layout Solver Failed."
  	exit /b %LAYOUT_ERRORLEVEL%
)

rem Clear the error levels
VER > nul
@echo off

SET BOARDSYNTHESIS=%META_PATH%\bin\BoardSynthesis.exe
IF EXIST "%META_PATH%\src\BoardSynthesis\bin\Release\BoardSynthesis.exe" SET BOARDSYNTHESIS=%META_PATH%\src\BoardSynthesis\bin\Release\BoardSynthesis.exe

"%BOARDSYNTHESIS%" schema.sch layout.json
SET SYNTH_ERRORLEVEL=%ERRORLEVEL%
IF %SYNTH_ERRORLEVEL% neq 0 (
	echo on
	echo "Board Synthesis Failed." >> _FAILED.txt
	echo "Board Synthesis Failed."
	exit /b %SYNTH_ERRORLEVEL%
)

%SystemRoot%\SysWoW64\REG.exe query "HKLM\Software\Microsoft\Windows\CurrentVersion\App Paths\eagle.exe" /v "Path"
SET QUERY_ERRORLEVEL=%ERRORLEVEL%

IF %QUERY_ERRORLEVEL% == 0 (
    FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\Software\Microsoft\Windows\CurrentVersion\App Paths\eagle.exe" /v "Path"') DO SET EAGLE_PATH=%%B)
)
IF %QUERY_ERRORLEVEL% == 1 (
    echo on
    echo "Eagle CAD tools not installed." >> _FAILED.txt
    echo "Eagle CAD tools not installed."
    exit /b %QUERY_ERRORLEVEL%
)

echo "Creating a PNG of the placeonly board."

"%EAGLE_PATH%\bin\eagle.exe" schema.brd -C "set confirm yes; export image schema.png 800; quit;"

SET /A PNG_ERRORLEVEL=%ERRORLEVEL%

IF %PNG_ERRORLEVEL% neq 0 (
  echo on
  echo "PNG Creation Failed." >> _FAILED.txt
  echo "PNG Creation Failed."
  exit /b %PNG_ERRORLEVEL%
)
echo "PNG created OK."

popd
