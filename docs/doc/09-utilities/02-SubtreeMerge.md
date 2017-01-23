___
##  Subtree Merge Utility
The **Subtree Merge** utility makes it possible for two or more users to collaborate on a design. Collaborators can work within designated <i>"subtrees"</i>, and their edits can be re-incorporated into a master copy of the design model.

In this section, we refer to a *Component Assembly*, and all of the objects contained within, as a *subtree*.

### Workflow
Using this method for collaboration requires identifying ways to logically divide your model. For example, consider a design with this hierarchy:

* Module_Design
  * ARA_Standard_Interface
    * EPM_Subsystem
    * MIPI_FPGA_Gateway
  * Microcontroller
    * Atmega168_Subsystem
  * Display_Circuit
  * Temperature_Sensing
    * Amp1
    * Amp2

Two designers decide to divide their work, with one collaborator editing the ***Temperature_Sensing*** subtree and the other collaborator continuing work in other areas.

They first make a copy of the modeling project, designating one copy the "master" and the other the "secondary". In the "secondary" model, one collaborator will make edits only within the ***Temperature_Sensing*** subtree. The other collaborator may make edits in any other area using the "master" model.

When the collaborators want to merge work from the "secondary" model back into the "master" model, they first open the "master" model. They open the ***Temperature_Sensing*** *Component Assembly* in the editing window, and click on the **Subtree Merge** utility. They select the "secondary" model's `.mga` file, and the **Subtree Merge** utility extracts that model's version of the subtree, replacing the original in the "master" model. Their independent work has now been fully merged into a single design project.

This process can be followed with any number of "secondary" models, provided that no two models contain edits within the same subtree.

### Limitations
Once the two models are split, don't make changes to the subtree in the "master" model until the models are merged again. Because the **Subtree Merge** creates an exact copy of the data found in the "secondary" model's version of the subtree, those changes to the "master" model's copy of the subtree will be wiped out.

If any components are added to the "secondary" model project and used in the subtree, then those components must be imported into the "master" model before the **Subtree Merge** is performed.

### Illustrated Example
Designs updated by collaborators can be re-incorporated into designs. The procedure to update the design in your project is as follows:

First, open the design you'd like to update. Here, we'll update the ARA Standard Interface.

![](images/09_01_ARA_Interface_open.png)

Then click on the SubTree Merge interpreter.

![](images/09_02_ARA_Interface_open.png)

Select the `.mga` file that contains the updated ARA Standard Interface.

![](images/09_03_ARA_Interface_open.png)

The SubTree Merge interpreter will replace the ARA Standard Interface, and should report success in the GME Console.

![](images/09_04_ARA_Interface_open.png)
