Setlocal EnableDelayedExpansion

@rem BUILD SOLUTIONS
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild make.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild make_tonka.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!

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