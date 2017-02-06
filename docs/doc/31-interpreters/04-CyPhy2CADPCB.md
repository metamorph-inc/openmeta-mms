___
## CyPhy2CADBPCB

The **CyPhy2CADPCB** interpreter allows a user to visualize their model in 3-D by generating either a STEP or STL file based on the `layout.JSON` file that is created with the CyPhy2Schematic interpreter. The `layout.JSON` file describes the location of components on a PCB as calculated by EAGLE. However, the origin of components in the EAGLE simulation and the origin in the CAD model may not be the same. This interpreter traverses the model searching for components that have associated CADModels. If a component also has an EDAModel connected to the CADModel, the CAD2EDATransform connection defines the transform needed to align the EDAModel and CADModel's coordinate systems. If a component contains an EDA model and no CAD model, a placeholder cube will be created in the final assembly, with the component length/width dimensions at the coordinates specified in the layout.JSON. Any components without an EDA model are excluded from the final assembly.

The assembly STEP/STL file is generated using FreeCAD. At this time, only the assembly of STEP and STL files are supported. If a component has an EDAModel that has multiple CADModels of varying format associated with it, the interpreter gives preference to the model that matches the format specified in the visualizerType parameter. If the "mix" option is chosen, STEP files are given preference over STL. If multiple CADModels of the same format point to one EDAModel, one is chosen at random. AP203 and AP214 are considered the same format.

The `layout.JSON` input file that the assembly is based on can be specified in 4 ways:
* By specifying the path to a previously generated `layout.JSON`.
* By locating a previously generated layout.JSON file using a GUI.
* By using a previously generated layout.JSON that is associated with the current **SystemUnderTest**.
* By executing a combined analysis where a PlaceRoute testbench is first executed to generate the layout file, followed by the visualizer program.

The interpreter needs to be executed from a proper testbench with one **SystemUnderTest** and a Workflow definition. Upon successful execution the interpreter:

* Generates a JSON file describing each component, including the relative path to the CAD model and CAD2EDATransform data (if applicable).
* Provides checks for conflicting interpreter workflow parameters.
* Confirms the existence of the `layout.JSON` file specified and copies it to the output directory.

If you desire the layout file to be generated by first running the PlaceRoute testbench, the Workflow definition must contain two tasks: one for CyPhy2Schematic and the other for CyPhy2CADPCB. The tasks must be connected FROM CyPhy2Schematic TO CyPhy2CADPCB.

### Workflow Parameters
_NOTE: If all parameters are left as null, the interpreter defaults to having the user select a `layout.JSON` via a file browser._
| Key | Value Type | Description |
| :--------------- | :----------: | ------------|
| launchVisualizer | String | If **true**, once the assembly script has successfully run, a web visualizer will automatically launch and load the generated STEP file.<br> *Not yet implemented - value has no effect on simulation*
| layoutFilePath | String | If specified, assembly will be created based on this `layout.JSON`.
| runLayout | String | If **true**, a CyPhy2Schematic testbench will first be created using the *doPlaceRoute* parameter setting prior to executing the assembly script.
| useSavedLayout | String | If **true**, use a previously generated layout.JSON associated with the testbench's SystemUnderTest. This file should be stored in the project directory's "designs" folder within a subfolder named after the component assembly's design ID.
| visualizerType | String | Specifies the format of the CADModels to be assembled. Supported types include STEP (all STEP models), STL (all STL models), or MIX (a mixture of STEP/STL). If parameter is left as null, the program defaults to "STEP".

### Interpreter Parameters
No parameters are used in the CyPhy2CADPCB interpreter. However, if the interpreter is executed with *runLayout* set to **true**, the following parameters are supported for the *PlaceAndRoute* analysis:

| Parameter Name | Value Type | Description | Required |
| :------------- | ---------- | ----------- | -------- |
| boardWidth | real number | The width (in millimeters) of the area available for components. | Required
| boardHeight | real number | The height (in millimeters) of the area available for components. | Required

### Generated Artifacts
| Directory/File Path | Artifact Tag | Description |
| :------- | :-----------: | :---------- |
| `CT.JSON` | - | Component information file - specifies path to CAD model and associated CAD-to-EDA-model transforms. |
| `layout.JSON` | - | File describing component locations on PCB as determined by EAGLE. |
| `BoardLayout.step` (`.stl`)| - | STEP (STL) file of component assembly |
| `BoardLayout.FCStd` | - | FreeCAD native format file of component assembly. |

*NOTE: An assembly is able to be generated with a mixture of STEP and STL files. However, if any STL files are present, the format of the generated assembly will be STL. Due to this, the launchVisualizer workflow parameter will be disregarded.*



