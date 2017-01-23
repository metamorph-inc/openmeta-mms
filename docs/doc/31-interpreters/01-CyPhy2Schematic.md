___
## CyPhy2Schematic
![CyPhy2Schematic icon](images/CyPhy2Schematic.png)

**CyPhy2Schematic** generates circuit models. It has several modes which support a number of capabilities, including:

* Generation and execution of SPICE simulation models
* Generation of EAGLE model files
* Generation of PCB Layouts, including signal routing
* "ChipFit" tests


### Workflow Parameters
_NOTE: Values of type **Boolean** are considered **false** if they are unspecified or **null**. Any other value is considered **true**._

| Key | Value Type | Description |
| :-- | :--------- | :---------- |
| doSpice | Boolean | If **true**, generate a SPICE simulation model. Ignore all other parameters.<br>If **false**, produce an EAGLE schematic model.
| doChipFit | Boolean | Run an analysis that determines whether the given components will fit into the given dimensions.<br>This mode requires the TestBench to have 2 Parameters, and produces 2 Metrics. For more details, read the **ChipFit Mode** section below.
| showChipFitVisualizer | Boolean | If **true**, a 2D viewer will be launched for the ChipFit results as part of test bench execution. Execution will be blocked until the window is closed.
| doPlaceOnly | Boolean | Generates a layout for the PCB and saves it as an EAGLE board file
| doPlaceRoute | Boolean | Same as _doPlaceOnly_, but also uses EAGLE's auto-router to route the signals on the board
| skipGUI | Boolean | If **true**, don't display a GUI to the user when the interpreter runs as part of a workflow, even on the first run over a design space.


### EDA Mode
If **doSpice** is not **true**, then the interpreter operates in EDA mode. In this mode, it will produce an EAGLE schematic model from the CyPhy model. Depending on the other options selected, it can also do a ChipFit analysis, auto-placement of components on the PCB, and auto-routing of the traces on the PCB.

#### Generated Artifacts

| Filename | Artifact Tag | Description |
| :------- | :----------- | :---------- |
| `layout-input.json` | _none_ | Input to the ChipFit and Placement tools. Provides board size and layer count constraints. For each package in the design, provides name, width, and height (in millimeters).
| `schema.sch` | _none_ | The generated EAGLE model
| `chipfit.bat` | _none_ | Runs the ChipFit analysis
| `placeonly.bat` | _none_ | Generates a board layout from `layout-input.json` and synthesizes an EAGLE board file as `schema.brd`
| `placement.bat` | _none_ | Same as `placeonly.bat`, but also uses EAGLE to auto-route the signals on the board.
| `reference_designator_mapping_table.html` | _none_ | Provides mapping from the auto-generated reference designators in the EAGLE model to the paths of components from the original CyPhy project.

#### Parameters
This mode supports the following TestBench Parameters.

| Parameter Name | Value Type | Description | Required |
| :------------- | ---------- | ----------- | -------- |
| boardWidth | real number | The width (in millimeters) of the area available for components. | Required
| boardHeight | real number | The height (in millimeters) of the area available for components. | Required
| boardTemplate | string | The relative (to the project) path of a PCB board template file | Optional
| designRules | string | The relative (to the project) path of a design rule file that specifies place and route guidelines for PCB | Optional 
| autorouterConfig | string | The relative (to the project) path of an EAGLE autorouter settings file to be used by the autorouter | Optional  
| boardEdgeSpace | string | The minimum distance (in millimeters) between the boundary of a component and the edge of the board | Optional
| interChipSpace | string | The minimum distance (in millimeters) between the boundaries of adjacent components | Optional


### ChipFit Mode
If the _doChipFit_ value is **true**, then the **ChipFit Mode** will be activated. In this mode, the test bench will run an analysis that determines whether the given components will fit into the given dimensions.

This mode produces two Metrics. If these metrics aren't provided in the TestBench, they will be added into `testbench_manifest.json` after ChipFit execution.

<i><b>Note:</b> In the Metric names that follow, <b>(x)</b> and <b>(y)</b> are replaced by the **boardWidth** and **boardHeight**, respectively, as defined above.</i>

| Metric Name | Value Type | Description |
| :---------- | ---------- | ----------- |
| fits_(x)_by_(y) | true/false | Whether the components in the design will fit within the given space
| pct_occupied_(x)_by_(y) | float | The percentage of the available space occupied by the given components.


### SPICE Mode
The **SPICE Mode** will generate and run an NGSPICE-compatible SPICE model.


#### Generated Artifacts

| Filename | Artifact Tag | Description |
| :------- | :----------- | :---------- |
| `runspice.bat` | _none_ | Runs the SPICE simulation
| `schema.cir` | _none_ | The generated SPICE model
| `siginfo.json` | _none_ | Provides mappings from SPICE signals to ports in the CyPhy model