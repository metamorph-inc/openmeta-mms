## Test Benches

In the META Tools, a **Test Bench** is a virtual environment used to run experiments on a system. Test benches define a testing context for a system, providing sources of stimulus and loading elements that gather experimental data. In META, a user can dictate the test conditions for their experiment themselves or choose from a library of pre-configured test benches that represent design requirements or other criteria. In addition to the configuration of test conditions, the user can customize the data gathered through the execution of a test bench.

While most test benches are used to perform analyses, other test benches perform design services for the user. For example, a user that has completed a META design can run a test bench to auto-generate a schematic of their design. Additionally, the user can run a CAD assembly test bench to build a 3D model of their design.

A common use for test benches is the evaluation of system performance. In this application, a test bench is an executable specification of a system requirement. The parts of a Test Bench include:

- **Test Drivers:** Replicating the intended stimulus to the system.

- **Wraparound Environment:** Providing the interfaces at the periphery of the system such as the external humidity, temperature, etc.

- **Metrics Evaluation:** Measurements of the system properties converted into a value of interest. The metrics are also tied to requirements, which can convert the metric to a design “score”.

- **System Under Test:** Either a single design or a design space (many designs). In the case of a design space, the test bench can be applied over the entire set of feasible designs.

![example test bench](images/01-04-example-test-bench.png)

_An example test bench: **NewDC\_\_SimpleLEDCircuit** is the **System Under Test**, while the other **Test Components** provide the **Wraparound Environment**._


### Ara Test Benches

The following test bench types are available for Ara Module Developers:

- **Schematic Generation**: Takes an existing META design and generates an EAGLE circuit schematic.

- **Board Fit**: Runs a PCB layout tool to assess if the module design will fit on a standard Ara chip size.

- **CAD Assembly**: Using PCB layout information, this test bench assembles a CAD model of the design.

- **Thermal Test**: Uses Modelica to to assess the runtime temperatures of components. Returns temperatures and limit violations.

- **Power Usage**: Uses Modelica to estimate power usage of the module design.

- **SystemC Tests**: Executing cycle-accurate and/or transaction-level simulation of digital test benches and component assemblies.

- **RF Analysis**: Performs EM-field simulations to derive antenna parameters and estimate the maximum SAR.

- **Android Emulator**: Uses the Eclipse's Android emulator to simulate the software & hardware interaction of elements. Software crashes and other issues can be predicted this way. This is an extension of the SystemC testing framework with a special communication bridge component to the Android Emulator or to a physical phone.

- **Cost Estimation**: Generates a bill of materials (BOM) and estimates cost and lead time for designs.

- **Acoustic Analysis**: _Under development_.

- **Finite Element Analysis**: _Under development_.

- **Firmware Generation**: _Under development_.

Additionally, test benches can be used to run simple customized tests on many designs using Python post-processing "blocks". Existing test benches can be modified for the user's purposes.