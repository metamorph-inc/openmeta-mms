@rem BUILD SOLUTIONS
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild make.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild make_tonka.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!

@rem BUILD INSTALLER
pushd deploy
..\bin\Python27\Scripts\python.exe build_msi.py || (popd & exit /b !ERRORLEVEL!)
popd