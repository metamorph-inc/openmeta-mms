Visualize
---------

**Location:** ``TestBenches / AraTestBenches / Visualizer``

These test benches are designed to generate a 3-D model based on an
EAGLE schematic and board file. They assemble the CAD models of the
components in the CyPhy model by considering the coordinates of each
component's position on the PCB.

Configure
~~~~~~~~~

First, you'll need to create a copy of one of the **Visualizer** test
benches. For instructions, refer to :ref:`testbenchbasics`.

There are three available options:

-  **PlaceRouteAndVisualize**: Execute a PlaceAndRoute test bench to
   generate a layout.JSON file prior to calling the assembly program.
-  **Visualize\_Layout**: Executes a visualizer testbench by asking the
   user to point to an existing layout.JSON file using a file browser.
-  **Visualize\_SavedLayout**: Executes a visualizer testbench by using
   an existing layout.JSON file that is stored in the "designs" folder
   in the project directory under the ID of the component assembly which
   is referenced by the test bench's SystemUnderTest. This option
   reduces the need to execute a PlaceAndRoute testbench if the
   component assembly that the SystemUnderTest references has already
   had a PlaceAndRoute analysis performed (with no modifications
   between).

If the *PlaceRouteAndVisualize* testbench is used, the **boardWidth**
and **boardHeight** parameters must be set to match the size of the
board you wish to generate. Both values are in *millimeters*.

Additionally, you can run an interference analysis on the assembly to
ensure the assembly does not contain any overlapping components. The
analysis will be executed if a Parameter is placed in the test bench and
named **INTERFERENCE_CHECK**. A report (interference_report.log) is
generated describing any interferences that are calculated. Please note
that this analysis may take several minutes to complete depending on the size
of the assembly.

Metrics
~~~~~~~

This test bench produces no metrics.

Outputs
~~~~~~~

+--------------------------------------+--------------------------------------+
| Filename                             | Description                          |
+======================================+======================================+
| ``CT.JSON``                          | Component information file -         |
|                                      | specifies path to CAD model and      |
|                                      | associated CAD-to-EDA-model          |
|                                      | transforms.                          |
+--------------------------------------+--------------------------------------+
| ``layout.JSON``                      | File describing component locations  |
|                                      | on PCB as determined by EAGLE.       |
+--------------------------------------+--------------------------------------+
| ``BoardLayout.step`` (``.stl``)      | STEP (STL) file of component         |
|                                      | assembly                             |
+--------------------------------------+--------------------------------------+
| ``BoardLayout.FCStd``                | FreeCAD native format file of        |
|                                      | component assembly.                  |
+--------------------------------------+--------------------------------------+

Assumptions
~~~~~~~~~~~

This test bench supports the assembly generation of STEP and STL files.
If the assembly contains a mixture of STEP and STL files, the final
assembly will be saved as an STL file, and the launchVisualizer
parameter within the CyPhy2CADPCB interpreter will be ignored.

This test bench only considers components that have EDA Models (EAGLE
Schematics) associated with them. Any components lacking EDA Models are
skipped. Any components that have both CADModels and EDAModels, but no
connection defining the transform between them, will have a cube created
as a placeholder at the coordinates specified in the layout.JSON. The
placeholder's length/width dimensions are according to the component
dimensions specified in the layout.JSON.

If your design includes a printed circuit board (PCB) component, its
geometry is not considered in the place and route process. Instead, a
board matching the dimensions given by **boardWidth** and
**boardHeight** is assumed when building the EAGLE schematic.
