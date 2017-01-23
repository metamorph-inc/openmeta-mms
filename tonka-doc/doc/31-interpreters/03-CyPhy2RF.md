___
## CyPhy2RF
![CyPhy2RF icon](images/CyPhy2RF.png)

The **CyPhy2RF** interpreter builds up the EM simulation model of the Ara module, generates the input files for OpenEMS and executes the simulator. It imports and generates the models described with [CSXCAD](http://openems.de/index.php/CSXCAD) XML scheme of OpenEMS.

The interpreter needs to be executed from a proper testbench with one **System Under Test** and one *Excitation* test component. Upon successful execution the interpreter:

* Instantiates an Ara module with the size of the requested Endo slot (currently 1x2 and 2x2 are supported)
* Imports the antenna component and uses it to populate the PCB of the Ara module
* Configures the excitation for the antenna
* Instantiates an Ara Endo and inserts the Ara module into the requested Endo slot
* Creates a rectilinear FDTD grid for the Ara Endo with the requested resolution and aligned with the antenna feed point
* Exports the simulation scenario for OpenEMS
* Schedules the execution of the OpenEMS simulation and the associated directivity or SAR post-processing steps

_NOTE: The current version assumes that OpenEMS is installed under the `C:\OpenEMS\` directory._

### Workflow Parameters
The current version does not require nor handle any special workflow parameters.

### RF Interpreter Parameters
This interpreter supports the following parameters:

| Parameter Name | Value Type | Units | Description |
| :------------- | :--------: | :---: | ----------- |
| Frequency | real number | Hz | The center frequency of the excitation signal
| Resolution | real number | mm | The target resolution in the Ara Endo vicinity
| Slot | positive integer | - | The Ara Endo slot index to be used (see the figure below)

![Ara Endo slot indexes](images/endo-mdk-labeled.png)
<center><i>Ara 6x3 Endo slot indeces</i></center>

### Generated Artifacts (Directivity)
| Directory/File Path | Artifact Tag | Description |
| :------- | :-----------: | :---------- |
| `run_dir_simulation.cmd` | - | Batch file that runs the OpenEMS simulation and postprocessing |
| `openEMS_input.xml` | - | Simulation description for the OpenEMS simulation |
| `nf2ff_input.xml` | - | Simulation description for the postprocessing steps |

### Generated Artifacts (SAR)
| Directory/File Path | Artifact Tag | Description |
| :------- | :-----------: | :---------- |
| `run_sar_simulation.cmd` | - | Batch file that runs the OpenEMS simulation and postprocessing |
| `openEMS_input.xml` | - | Simulation description for the OpenEMS simulation |
| `nf2ff_input.xml` | - | Simulation description for the postprocessing steps |

The execution of the SAR postprocessing generates the following output files:

| Directory/File Path | Artifact Tag | Description |
| :------- | :-----------: | :---------- |
| `SAR-X.png` | CyPhy2RF:SAR:X | PNG image with the Y-Z plane cross-sectional cut of the head phantom at the SAR maximum location |
| `SAR-Y.png` | CyPhy2RF:SAR:Y | PNG image with the X-Z plane cross-sectional cut of the head phantom at the SAR maximum location |
| `SAR-Z.png` | CyPhy2RF:SAR:Z | PNG image with the X-Y plane cross-sectional cut of the head phantom at the SAR maximum location |
