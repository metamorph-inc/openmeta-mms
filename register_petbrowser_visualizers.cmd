reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /ve /d "Export CSV" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /v "ActionName" /d "Export as CSV" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /v "ExecutableFilePath" /d "%CD%\bin\Python27\Scripts\Python.exe" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /v "ProcessArguments" /d "\"%CD%\bin\PetBrowserExportCsv.py\" \"%%1\" \"%%4\" " /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /v "ShowConsoleWindow" /d "0" /t REG_DWORD /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\ExportCSV /f /v "WorkingDirectory" /d "%%2" /reg:32

reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /ve /d "Open With Excel" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /v "ActionName" /d "Open With Excel" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /v "ExecutableFilePath" /d "%CD%\bin\Python27\Scripts\Python.exe" /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /v "ProcessArguments" /d "\"%CD%\bin\PetBrowserOpenInExcel.py\" \"%%1\" \"%%4\" " /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /v "ShowConsoleWindow" /d "0" /t REG_DWORD /reg:32
reg add HKLM\SOFTWARE\META\PETBrowser\PETTools\OpenInExcel /f /v "WorkingDirectory" /d "%%2" /reg:32

reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /ve /d "Open With JMP" /reg:32
reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /v "ActionName" /d "Open With JMP" /reg:32
reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /v "ExecutableFilePath" /d "%CD%\bin\Python27\Scripts\Python.exe" /reg:32
reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /v "ProcessArguments" /d "\"%CD%\PetBrowserOpenInJMP.py\" \"%%1\" \"%%4\" " /reg:32
reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /v "ShowConsoleWindow" /d "0" /t REG_DWORD /reg:32
reg add HKCU\SOFTWARE\META\PETBrowser\PETTools\OpenInJMP /f /v "WorkingDirectory" /d "%%2" /reg:32
