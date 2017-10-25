# Building the Metamorphosys Tools
The Metamorphosys tools can be compiled from the source code in this repository. Compilation requires a Windows PC and access to the internet (for downloading NuGet packages).

# Build Machine Setup
Follow these configuration instructions, in order, to set up your machine to build the Metamorphosys tools from source.

## Windows x64 (7 SP3, 8.1, 10 or Server equivalent)
Install Windows updates until your version is current

## Microsoft Visual Studio 
The solution will build with Microsoft Visual Studio 2015 or 2017.

### If you have Visual Studio 2017 (Community or above)
_NOTE: Will not compile with Visual Studio Code_

When installing Visual Studio, select these packages (at minimum):
- C++
- C#

You must also install:
- Visual Studio 2015 Build Tools ([download](https://www.microsoft.com/en-us/download/details.aspx?id=48159))
- Visual C++ 2015 Build Tools ([download](http://landinghub.visualstudio.com/visual-cpp-build-tools))
  - Select the *"ATL/MFC SDK"* package (at minimum) 

### If you have Visual Studio 2015 (Community or above)
_NOTE: Will not compile under Express Edition_

When installing Visual Studio, select these packages (at minimum):
- C++
- C#

You must also install Microsoft Visual Studio 2015 Update 3
([download](https://www.visualstudio.com/en-us/news/releasenotes/vs2015-update3-vs))

## Java JDK 7 or above
[Download it here](http://www.oracle.com/technetwork/java/javase/downloads/jdk7-downloads-1880260.html)

_NOTE: Either x86 or x64 is okay_

### Set JAVA_HOME
Set environment variable JAVA_HOME to the installed directory, such as `C:\Program Files (x86)\Java\jdk1.7.0_09`

The actual name of the subdirectory depends on what version you have installed.

## GME 16.3+
[Download it here](https://forge.isis.vanderbilt.edu/gme)

GME_x64 is the best-tested (but 32-bit should work too)

## UDM x64 3.2.14+
[Download it here](http://repo.isis.vanderbilt.edu/UDM/3.2.14/)

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

## EAGLE

For the tests to pass, EAGLE must be installed.

[Download EAGLE](https://cadsoft.io/). Version 6.5 is known to work.

## OpenEMS

For the tests to pass, OpenEMS must be installed to `C:\OpenEMS`

[Download OpenEMS](http://openems.de/download/win64/) Version 0.32 is known to work.

## Git
[Download msysgit](https://msysgit.github.io/)

`git.exe` must be in your `PATH`. For `msysgit`, select `Use Git from the Windows Command Prompt` during installation.

## Clone Repo
Clone this repository to your disk.

# Build
1. Open Visual Studio Command Prompt (2015 or 2017) with ”Run as administrator”. (Do not use a Visual Studio x64 command prompt)
2. From the root repository directory, run `build_both.cmd`. This may take 30 minutes to build. _(Warnings may be ignored, but there should be no errors.)_

If you encounter errors, try to build once more. There may be some remaining race conditions in the build scripts.

# Run
For first-time users, [Walkthrough Documentation](http://www.metamorphsoftware.com/alpha/meta-walkthroughs.html) is a good introduction to using the tools. More advanced capabilities are explored in the [META Case Studies](http://www.metamorphsoftware.com/alpha/meta-case-studies.html).

# Installer
Run `bin\Python27\Scripts\python.exe deploy\build_msi.py` to build the installer.
