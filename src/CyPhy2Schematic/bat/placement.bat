@echo off
SetLocal EnableDelayedExpansion
pushd %~dp0
%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"

SET QUERY_ERRORLEVEL=%ERRORLEVEL%

IF NOT "%1" == "--only-consider-exact-constraints" goto allconstraints
set ONLY_CONSIDER_EXACT_CONSTRAINTS=true
shift
:allconstraints

IF %QUERY_ERRORLEVEL% == 0 (
    FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO SET META_PATH=%%B)
)
IF %QUERY_ERRORLEVEL% == 1 (
    echo on
    echo "META tools not installed." >> _FAILED.txt
    echo "META tools not installed."
    exit /b %QUERY_ERRORLEVEL%
)

IF NOT DEFINED ONLY_CONSIDER_EXACT_CONSTRAINTS (
"%META_PATH%\bin\LayoutSolver.exe" layout-input.json layout.json %*
SET LAYOUT_ERRORLEVEL=!ERRORLEVEL!
IF !LAYOUT_ERRORLEVEL! == 42 (
  SET /A USING_PARTIAL_LAYOUT=1
) ELSE IF !LAYOUT_ERRORLEVEL! neq 0 (
    echo on
    echo "Layout Solver Failed. " !LAYOUT_ERRORLEVEL! >> _FAILED.txt
    echo "Layout Solver Failed."
    exit /b !LAYOUT_ERRORLEVEL!
)
)

rem Clear the error levels
VER > nul
@echo off

SET BOARDSYNTHESIS=%META_PATH%\bin\BoardSynthesis.exe
IF EXIST "%META_PATH%\src\BoardSynthesis\bin\Release\BoardSynthesis.exe" SET BOARDSYNTHESIS=%META_PATH%\src\BoardSynthesis\bin\Release\BoardSynthesis.exe

IF NOT DEFINED ONLY_CONSIDER_EXACT_CONSTRAINTS "%BOARDSYNTHESIS%" schema.sch layout.json
IF DEFINED ONLY_CONSIDER_EXACT_CONSTRAINTS "%BOARDSYNTHESIS%" schema.sch layout-input.json
SET SYNTH_ERRORLEVEL=%ERRORLEVEL%
IF %SYNTH_ERRORLEVEL% neq 0 (
	echo on
	echo "Board Synthesis Failed." >> _FAILED.txt
	echo "Board Synthesis Failed."
	exit /b %SYNTH_ERRORLEVEL%
)

"%META_PATH%\bin\Python27\Scripts\python.exe" -m get_eagle_path
SET QUERY_ERRORLEVEL=%ERRORLEVEL%

IF %QUERY_ERRORLEVEL% == 0 (
	FOR /F "tokens=*" %%i in ('"%META_PATH%\bin\Python27\Scripts\python.exe" -m get_eagle_path') DO SET EAGLE_PATH=%%i
)
IF %QUERY_ERRORLEVEL% == 1 (
	echo on
	echo "Eagle CAD tools are not installed." >> _FAILED.txt
	echo "Eagle CAD tools are not installed."
	exit /b %QUERY_ERRORLEVEL%
)

IF NOT DEFINED ONLY_CONSIDER_EXACT_CONSTRAINTS (
IF NOT DEFINED USING_PARTIAL_LAYOUT  (
  goto AutoRouting
)
)

echo "Creating a PNG of the board."

"%EAGLE_PATH%" schema.brd -C "set confirm yes; export image schema.png 800; quit;"

SET /A PNG_ERRORLEVEL=%ERRORLEVEL%

IF %PNG_ERRORLEVEL% neq 0 (
  echo on
  echo "PNG Creation Failed." >> _FAILED.txt
  echo "PNG Creation Failed."
  exit /b %PNG_ERRORLEVEL%
)
echo "PNG created OK."

IF NOT DEFINED ONLY_CONSIDER_EXACT_CONSTRAINTS (
goto :eof

:AutoRouting
if exist autoroute.ctl (
	echo Using "autoroute.ctl" Auto Routing settings ...
	"%EAGLE_PATH%" schema.brd -C "auto load autoroute.ctl; auto; set confirm yes; export image schema.png 800; write; quit;"
) else (
	echo Using default Auto Routing settings ...
	"%EAGLE_PATH%" schema.brd -C "auto; set confirm yes; export image schema.png 800; write; quit;"
)
@echo off

SET AUTOR_ERRORLEVEL=!ERRORLEVEL!
IF !AUTOR_ERRORLEVEL! neq 0 (
	echo "Auto Routing Failed." >> _FAILED.txt
	echo Auto Routing Failed.
	exit /b !AUTOR_ERRORLEVEL!
)
IF !AUTOR_ERRORLEVEL! == 0 (
	echo Auto Routing Completed OK.
)
)

popd
