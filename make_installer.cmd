@rem BUILD SOLUTIONS
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" make.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" make_tonka.msbuild /t:All /m /nodeReuse:false || exit /b !ERRORLEVEL!

@rem BUILD INSTALLER
pushd deploy
..\bin\Python27\Scripts\python.exe build_msi.py || (popd & exit /b !ERRORLEVEL!)
popd