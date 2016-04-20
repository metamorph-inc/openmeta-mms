Setlocal EnableDelayedExpansion

@rem set JAVA_HOME=C:\Program Files (x86)\Java\jdk1.7.0_40
echo Using JAVA_HOME %JAVA_HOME%
set ANT_BAT=%LOCALAPPDATA%\apache-ant-1.9.4\bin\ant.bat
set ANDROID_BAT=%LOCALAPPDATA%\Android\android-sdk\tools\android.bat

@if NOT EXIST "%ANDROID_BAT%" ( echo ERROR: android.bat not found at %ANDROID_BAT%. Please install it by running //longbox/share/installers/Android/install_ant_build_deps.cmd & exit /b 2 )
@if NOT EXIST "%ANT_BAT%" ( echo ERROR: ant.bat not found at %ANT_BAT%. Please install it by running //longbox/share/installers/Android/install_ant_build_deps.cmd & exit /b 2 )

pushd %~dp0
rem workaround https://code.google.com/p/android/issues/detail?id=60496 : can't build in ant after building in Eclipse
rd /s/q SCBus\bin\res\crunch SystemCTest\bin\res\crunch XmasTree\bin\res\crunch
cd SCBus
cmd /c "%ANDROID_BAT%" update project -p . --target android-18 -n SCBus || exit /b !ERRORLEVEL!
cmd /c "%ANT_BAT%" debug || exit /b !ERRORLEVEL!
cd ..\SystemCTest
cmd /c "%ANDROID_BAT%" update project -p . --target android-18 -n SystemCTest || exit /b !ERRORLEVEL!
cmd /c "%ANT_BAT%" debug || exit /b !ERRORLEVEL!
cd ..\XmasTree
cmd /c "%ANDROID_BAT%" update project -p . --target android-18 -n XmasTree || exit /b !ERRORLEVEL!
cmd /c "%ANT_BAT%" debug || exit /b !ERRORLEVEL!
