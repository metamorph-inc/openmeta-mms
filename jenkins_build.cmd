Setlocal EnableDelayedExpansion

pushd %~dp0

@rem BUILD SOLUTIONS
"c:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" make.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!
"c:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" make_tonka.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!

@rem RUN TESTS
pushd test
del *_result.xml *_results.xml
del results\*_result.xml
..\bin\Python27\Scripts\Python.exe run_tests_console_output_xml_parallel.py tests.xunit tests_cadcreo.xunit tests_tonka.xunit || exit /b !ERRORLEVEL!
popd

@rem BUILD INSTALLER
pushd deploy
..\bin\Python27\Scripts\python.exe build_msi.py || (popd & exit /b !ERRORLEVEL!)
popd

rem .\bin\Python27\Scripts\python.exe copy_pdbs.py
