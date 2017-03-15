Chapter 2: Installation and Setup {#installation-and-setup}
=================================

This chapter will help you ensure your computer has the proper resources for the Metamorphosys tools, and guide you through the process of acquiring and setting up the software.


## Verify System Requirements

In order to run the META tools your computer must have the following requirements:

1. **A 64-bit Windows 7 or 8 PC Platform**

	*How to tell if your Windows computer is 64-bit:*

	1. Open System by clicking the Start button, right-clicking Computer, and then clicking Properties.
	2. Under *System*, you can view the system type.

2. **6 GB RAM minimum (8 GB RAM recommended)**
3. **At least 10GB of spare hard disk space**

	More than 50GB is recommended

4. **Google Chrome**

	Download and install <b>[Google Chrome](https://www.google.com/intl/en/chrome/browser/)</b>

5. **Windows .NET Framework 4.0 or 4.5**

    (Windows 8 and 8.1 have .NET Framework 4.5 installed by default)

	*How to tell if your computer has Windows .NET Framework 4.0 installed:*

	1. Open your Control Panel by clicking the Start button and then clicking Control Panel.
	2. Click Uninstall a Program to view your list of installed programs.
	3. If Windows .NET Framework 4.0 is not on your list of installed programs, you can download it [here](http://www.microsoft.com/en-us/download/details.aspx?id=17851).

## Install Tools
Once you've verified that your system meets these requirements, **install the following programs in the order listed below**. The first three installers are required to complete the tutorial in the next chapter. You can install the other tools later on.

## Tools Required For Tutorial

**Note: You will need to install these tools in the order seen below**

**1) Generic Modeling Environment (GME)**

GME is a configurable toolkit for creating domain-specific models and program synthesis environments. The META tools are built on top of the GME platform.

[<b>Download GME Installer</b>](http://repo.isis.vanderbilt.edu/GME/14.7.28/GME_x64-14.7.28.msi) 

Remember to install GME **before installing the META core.**

**2) Metamorphosys Core**
 
The **Metamorphosys Core** (aka META Core) contains the primary set of interpreters and tools for assembling models and running simulations.

[<b>Download META (Windows-Beta) Installer</b>](http://docs.metamorphsoftware.com/alpha-releases/meta-tools/META_x64_beta_1994.msi)

Note: Remember to install GME **before** installing the META core.

**3) EAGLE**

EAGLE (Easily Applicable Graphical Layout Editor) is an electronic design automation tool that is commonly used to create electrical circuit schematics and printed circuit board (PCB) layouts. **META** generates schematics and circuit board layouts that are compatible with EAGLE.

[<b>Download EAGLE version 6.5.0</b>](ftp://ftp.cadsoft.de/eagle/program/6.5/eagle-win-6.5.0.exe).

## Additional Tools

### OpenModelica

OpenModelica is an open-source Modelica-based modeling and simulation environment intended for industrial and academic usage. META uses OpenModelica for physics simulations, including mechanical, thermal, and power analysis.

[Download OpenModelica](https://build.openmodelica.org/omc/builds/windows/releases/1.9.1/beta2/OpenModelica-1.9.1-Beta2-revision-19512.exe).

### Visual Studio Express

**Visual Studio Express** is a software development tool suite provided by **Microsoft**. **META** uses **Visual Studio** to compile **SystemC** simulations. The included IDE may also be useful for writing firmware to be included in a design.

[Download Visual Studio Express](http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs#DownloadFamilies_4) by selecting the ***Visual C++ 2010 Express*** option and clicking <i>"Install Now"</i>.


###Android SDK 
This optional tool is used for developing, building and deploying Android applications. The command line SDK is also bundled with the Eclipse-based Android Developer Tools (ADT), a complete IDE for these development tasks. Finally, the included Android Emulator (based on QEMU) can be used to execute Android applications which interact with SystemC peripheral models. The necessary communication bridge and SystemC simulator is included in META Core package.


###OpenEMS
OpenEMS is an open-source electromagnetic (EM) field solver based on the Finite-Difference Time-Domain (FDTD) method. META uses OpenEMS for RF simulations to estimate the antenna parameters and maximum SAR.

[Download OpenEMS v0.0.32 x64](http://openems.de/download/win64/openEMS_x64_v0.0.32.zip).

<b><i>NOTE:</b> OpenEMS must be installed under `C:\openEMS` to work correctly with the META tools.</i></b>


###FreeCAD
FreeCAD is an open-source parametric 3D CAD modeling program. META uses FreeCAD to assemble component CAD models to provide a 3D visual representation of a user's model. It is required for the CyPhy2CADPCB visualizing test benches.

[Download FreeCAD v0.14 x64] (http://sourceforge.net/projects/free-cad/files/FreeCAD%20Windows/FreeCAD%200.14/FreeCAD-0.14.3700_x64_setup.exe/download).

[Download FreeCAD v0.14 x86] (http://sourceforge.net/projects/free-cad/files/FreeCAD%20Windows/FreeCAD%200.14/FreeCAD%200.14.3700_x86_setup.exe/download).


###CadQuery
CadQuery is a plugin for FreeCAD that provides a library of Python functions for the manipulation of solid geometry. These functions are used to transform the local coordinate system of a component's CAD model to the correct position and orientation in the assembly coordinate system. It is required for the CyPhy2CADPCB visualizing test benches.

[Download CadQuery] (https://github.com/jmwright/cadquery-freecad-module/archive/master.zip).

To install:
* Download zip file above and extract contents.
* Copy the CadQuery folder into "<FreeCAD_Install_Dir>\Mod"

NOTE:</b> The CadQuery library must be copied into the "<FreeCAD_Install_Dir/>Mod" folder to work correctly with the META tools.

---
<h2>Downloads</h2>
| Name | Link | Description |
| :--- | :--- | :---------- |
| **GME Installer** | [download](http://repo.isis.vanderbilt.edu/GME/14.7.28/GME_x64-14.7.28.msi) | The modeling framework underlying the META tools. Please refer to the [Installation Instructions](@ref installation-and-setup).
| **META (Alpha) Installer** | [download](http://docs.metamorphsoftware.com/alpha-releases/meta-tools/META_x64_beta_1789.msi) | The META tools installer. Please refer to the [Installation Instructions](@ref installation-and-setup).
| **LED Tutorial Model** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/walkthrough_simple_LED_matrix_circuit.zip) | The model project used as part of [Chapter 3: LED Tutorial](@ref meta-walkthroughs)
| **Component Library** | [download](http://docs.metamorphsoftware.com/alpha-releases/component-models/all_components.zip) | A collection of components that can be imported into META projects.
| **InvertedF Antenna Example** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/inverted_f_antenna.zip) | An example project with an Inverted-F type 2.4 GHz antenna mounted onto an Ara prototype module.
| **LED Matrix Model** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/LED_matrix.zip) | A META design project capturing a simple "LED Matrix" module. This model is based on early prototypes of the Ara platform and doesn't conform to the Ara specs provided in the MDK. This model should be used for demonstration and exploration of META tool capabilities only.
| **Astable Multivibrator Model** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/astable_multivibrator.zip) | A META design project capturing a simple dual-transistor switching circuit. This model is useful for experimenting with the SPICE simulation and visualization capabilities of the META tools.
| **SCBus Example** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/scbus_example.zip) | A META design project capturing a simple microcontroller circuit. The model can be used to generate a hardware simulation that runs user-provided firmware. This example is designed to communicate with the **SystemCTest** Android app provided with the META tools.
| **Manufacturing Example** | [download](http://docs.metamorphsoftware.com/alpha-releases/design-models/manufacturing_example.zip) | A META design project used to demonstrate the generation of files for submitting a design to a circuit board fabricator.

- - -




