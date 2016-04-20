# Building the Metamorphosys Tools
The Metamorphosys tools can be compiled from the source code in this repository. Compilation requires a Windows PC and access to the internet (for downloading NuGet packages).

# Build Machine Setup
Follow these configuration instructions, in order, to set up your machine to build the Metamorphosys tools from source.

## Windows x64 Professional (7 or 8.1)
Install Windows updates until your version is current

## .NET Framework
Install Microsoft .NET Framework, version 4.0 or higher.

Version 4.5.1 [can be found here](http://www.microsoft.com/en-us/download/details.aspx?id=30653)

## Microsoft Visual Studio 2010 (Professional or above)
_NOTE: Will not compile under Express Edition_

When installing, select these packages (at minimum):
- C++
- C#
- Office Tools

## Visual Studio Service Pack 1 (SP1)
[download it here](http://www.microsoft.com/en-us/download/details.aspx?id=23691)

## Python 2.7.x x86
Get the latest Python 2.7 (2.7.8 at the time of writing). Make sure the .py extension is associated with Python 2.7, and it is installed for All Users (NOT ”just for me”). Download it [here](http://www.python.org/download/releases/2.7.8/) _(and don’t get the 64-bit version)_.

## pywin32 for Python 2.7
Get *pywin32-219.win32-py2.7.exe* [here](http://sourceforge.net/projects/pywin32/files/pywin32/Build%20219/)

_Again, *don't* get the 64-bit version._

## WIX
[Download 3.x here](http://wixtoolset.org/releases/) (get the newest 3.x version).

3.5, 3.6, 3.7, 3.8 are detected by the build_msi.py script.

## Java JDK 7 or above
[Download it here](http://www.oracle.com/technetwork/java/javase/downloads/jdk7-downloads-1880260.html)

_NOTE: Either x86 or x64 is okay_

### Set JAVA_HOME
Set environment variable JAVA_HOME to the installed directory, such as `C:\Program Files (x86)\Java\jdk1.7.0_09`

The real name of the subdirectory depends on what version you have installed.

## GME 14.12+
[Download it here](https://forge.isis.vanderbilt.edu/gme)

GME_x64 is the best-tested (but 32-bit should work too)

## UDM x64 3.2.13+
[Download it here](http://repo.isis.vanderbilt.edu/UDM/3.2.13/)

## Android SDK
If you intend to develop Android apps on the compile machine, install the [Android SDK Bundle](http://developer.android.com/sdk/index.html). If you don't, then you will only need the [Stand-alone SDK Tools](http://developer.android.com/sdk/installing/index.html?pkg=tools).

Install the **Android 4.3 (API 18)** package (at minimum).

## Apache ANT
Download ANT version **1.9.4** [here](http://archive.apache.org/dist/ant/binaries/apache-ant-1.9.4-bin.zip)

Unzip ANT to `%APPDATA%\Local` such that its full path is `%APPDATA%\Local\apache-ant-1.9.4`

## Windows Updates
Again, install Windows updates until everything is current. Restart your computer.

## FreeCAD
FreeCAD is an open-source parametric 3D CAD modeling program. META uses FreeCAD to assemble component CAD models to provide a 3D visual representation of a user's model. It is required for the CyPhy2CADPCB visualizing test benches.

[Download FreeCAD v0.14 x64](http://sourceforge.net/projects/free-cad/files/FreeCAD%20Windows/FreeCAD%200.14/FreeCAD-0.14.3700_x64_setup.exe/download).

[Download FreeCAD v0.14 x86](http://sourceforge.net/projects/free-cad/files/FreeCAD%20Windows/FreeCAD%200.14/FreeCAD%200.14.3700_x86_setup.exe/download).

## CadQuery
CadQuery is a plugin for FreeCAD that provides a library of Python functions for the manipulation of solid geometry. These functions are used to transform the local coordinate system of a component's CAD model to the correct position and orientation in the assembly coordinate system. It is required for the CyPhy2CADPCB visualizing test benches.

[Download CadQuery](https://github.com/jmwright/cadquery-freecad-module/archive/master.zip).

To install:
* Download zip file above and extract contents.
* Copy the CadQuery folder into `<FreeCAD_Install_Dir>\Mod`

_NOTE: The CadQuery library must be copied into the `<FreeCAD_Install_Dir>\Mod` folder to work correctly with the META tools._

## Git
[Download msysgit](https://msysgit.github.io/)

`git.exe` must be in your `PATH`. For `msysgit`, select `Use Git from the Windows Command Prompt` during installation.

## Clone Repo
Clone this repository to your disk.

# Build
1. Open Visual Studio Command Prompt (2010) with ”Run as administrator”
2. From the root repository directory, run `build_both.cmd`. This may take 30 minutes to build. _(Warnings may be ignored, but there should be no errors.)_

If you encounter errors, try to build once more. There may be some remaining race conditions in the build scripts.

# Run
For first-time users, [Walkthrough Documentation](http://www.metamorphsoftware.com/alpha/meta-walkthroughs.html) is a good introduction to using the tools. More advanced capabilities are explored in the [META Case Studies](http://www.metamorphsoftware.com/alpha/meta-case-studies.html).
