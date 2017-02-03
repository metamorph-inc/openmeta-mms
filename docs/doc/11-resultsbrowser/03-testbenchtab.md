## Test Bench Tab

<img src="images/testbenchtab.png" alt="Test Bench Tab" style="width: 800px;"/>

### Test Bench List Pane

This pane on the left of the PET tab shows the available Test Benches.

#### Column Headers

**Dataset Types**

'TestBenchResult' is the only type of Test Bench List items.

**Name**

This name is taken from the OpenMETA model at the time of execution by the Master Interpreter

**Design**

This design name is taken from the OpenMETA model at the time of execution by the Master Interpreter

**Status**

This is the status taken from the Test Bench Manifest. The possible values are as follows:

* Unexecuted: 
* Failed:
* OK: 

**Time**

This is the time that the Test Bench execution was initiated by the Master Interpreter.

#### Action Buttons

**Open Selected in Explorer**

This button will open Windows Explorer at the location of the execution directory for this Test Bench.  

### Test Bench Details Pane

This pane shows details about the currently-highlighted dataset in the PET Dataset Pane. The displayed information in the header includes:

* Name of the Testbench
* Time of execution
* Design ID
* Design Name

Below the header is a summary of the Test Bench. Each of the five sections provides information that is encoded in the 'testbench_manifest.json' file.

* Steps:
* Parameters:
* Metrics:
* Artifacts:
* Visualization Artifacts: