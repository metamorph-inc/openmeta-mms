___
## CyPhy2SystemC
![CyPhy2SystemC icon](images/CyPhy2SystemC.png)

**CyPhy2SystemC** generates and executes behavioral test benches using a SystemC discrete event simulator. The interpreter includes the *standard* SystemC v2.3.1 library from the Accellera Systems Initiative and the core components of the ARA ecosystem. The interpreter targets Visual Studio 2010 (C++) with no additional/external dependencies.

The interpreter needs to be executed from a proper testbench with at least one component/susbsytem under test and one or more test components. A successful execution involves the following steps:

* The interpreter collects all *leaf* components with SystemC models
* It builds the complete wiring across these components (using ports and/or connectors)
* Source code (```test_main.cpp```) is generated, which instantiates and wires all components and starts the SystemC simulation. Trace statements are also added for top-level signals
* A complete Visual C++ solution/project is extracted around the generated source file (```SystemCTestBench```) which includes the SystemC simulator library and the essential ARA components
* Optional/external SystemC header and c++ files are copied into the generated folder for third party components
* Special handling of Arduino-based components: a generic Arduino component is instantiated for each and configured with the specified firmware (the firmware is attached as a ```Resource`` to the component in the CyPhy model)
* Finally, the interpreter schedules the building and execution of the generates SystemC project
* The SystemC executable produces console messages, signal traces (```SystemCTestBench.vcd```) and its exit status reflects the number of failed tests

### Workflow Parameters
_NOTE: The current version does not require nor handle any special workflow parameters._

### Parameters
This interpreter supports the following Parameters.

| Parameter Name | Value Type | Units | Description |
| :------------- | ---------- | ----------- | ----------- |
| simulationTime | real number | Time | The (maximum) length of the simulation time

### Generated Artifacts
This interpreter generates a new folder for the Visual Studio Solution with subfolders containing the SystemC library and essential ARA components.

| Directory/File Path | Artifact Tag | Description |
| :------- | :----------- | :---------- |
| `SystemCTestBench.sln` | _none_ | Visual Studio solution file
| `SystemCTestBench.vcxproj` | _none_ | Visual Studio C++ project
| `SystemCTestBench.vcxproj.filters` | _none_ | Visual Studio source file folder definitions
| `include/*` | _none_ | Include files for the SystemC and ARA libraries
| `libD/*` | _none_ | Debug builds of the SystemC and ARA libraries
| `libR/*` | _none_ | Release builds of the SystemC and ARA libraries
| `test_main.cpp` | _none_ | Generated top-level SystemC source file (instantiation and wiring)
| `<other>.cpp`/`.h`/ `.ino` | _none_ | Copies of referenced SystemC sources in third party components and Arduino firmware sources

The execution of the generated test bench generates the following output file(s):

| Directory/File Path | Artifact Tag | Description |
| :------- | :----------- | :---------- |
| `SystemCTestBench.vcd` | _none_ | VCD (Value Change Dump) signal traces

