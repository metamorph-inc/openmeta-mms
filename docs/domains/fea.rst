.. _fea:

Finite-Element Analysis (FEA)
=============================

Overview
--------

This chapter will serve as a tutorial that demonstrates the Solid
modeling aspect of CyPhy. We will build a simple FEA testbench using a
basic Creo model. This tutorial uses the skills learned in Chapter 3:
LED Tutorial,so you'll need to have completed that chapter prior to
attempting this one. Through this tutorial you will learn how to:

-  Build Component for a solid model
-  Build Component Assembly for a solid model
-  Build an FEA testbench
-  Generate results
-  Perform solid analysis and simulation on testbench

Downloading the Tutorial Zip File
---------------------------------

To complete this tutorial you will need to download the LED Tutorial
Model .

Setting Up Directory
--------------------

1. Once you've downloaded the tutorial model, unzip it and double-click
   ``SolidModeling_Tutorial.xme`` to open it in the **GME**.
2. Save the project to the default directory (where your ``.xme`` file
   is located), taking note of this directory as you will need to access
   this location later in the tutorial.

*Note: Be sure all the files (e.g. "components folder", ``.xme`` file)
are in the same directory throughout the tutorial.*

Generic Process for an FEA testbench
------------------------------------

The FEA testbench in CyPhy works much like all the other testbenches do,
in that it requires a referenced component assembly. The general
structure follows the trend: **Testbench -> Component assembly ->
component -> Creo 3.0 model**. We will start our work from the bottom
and build up to the testbench. Since the Creo.prt file is provided in
the download, we will begin with building the component.

Building a CAD Component
------------------------

In this first section, we will build a 3D component. Engineers often use
multiple types of analysis software to test their designs. This practice
takes time; you need to learn each piece of software and compose a model
for each one that will test for a parameter. However, Metamorphosys
tools allow you to run multiple types of analyses by composing just one
model.

In the Metamorphosys tools, we build a design by instantiating
components and joining their interfaces, yielding a **Component
Assembly**. We call this process *composition*. From this composed
design, we can generate new models and run analyses.

First we must create a component for the Creo model we wish to test.
This process breaks down into the following steps:

1. :ref:`create_component`
2. :ref:`reference_creo`
3. Add all necessary objects
4. Make appropriate connections

.. _create_component:

Creating a New Component
^^^^^^^^^^^^^^^^^^^^^^^^

1.  In your **GME Browser**, open the ***RootFolder*** by left-clicking
    the plus box to the left of the name.
2.  Right-click **RootFolder -> Insert folder -> Component Assembly**.
3.  Right-click **RootFolder -> Insert folder -> Components**.
4.  **Right-click** the **Components folder -> Insert Model ->
    Component** .
5.  Rename **Simple\_Cube**.
6.  Double left-click **Simple\_Cube** to open the blank component
    canvas.
7.  Located on the left is the **Part Browser**; left-click the **Solid
    Modeling** tab
8.  Locate the **Property** object, left-click the image then drag and
    drop it onto the workspace
9.  Click on the word "Property" and rename it as
    **PTC\_Material\_Name**
10. Click the empy box in the *Property* and type
    ALUMINUM\_ALLOY\_6061\_T6

.. figure:: images/IMAGE_1.png
   :alt: Solid Modeling Demo

   Solid Modeling Demo
   
.. _reference_creo:

Reference the Creo Model
^^^^^^^^^^^^^^^^^^^^^^^^

1. Select the component authoring tool denoted by the puzzle piece from
   the top tool bar
2. Select the **Add CAD** tile.
3. Locate and select the Creo versioned file **creo\_demo.prt**.

*Note: This process may take a few seconds as it converts the key
feature of the Creo Model into objects to be used by GME.*

Now you should see a CADModel that is populated with
**SURF\_REF\_TOP,SURF\_REF\_BOTTOM, SURF\_REF\_FRONT, SURF\_REF\_BACK,
SURF\_REF\_LEFT, SURF\_REF\_RIGHT, and CUBE\_CENTER\_REF**. These are
points on the center of every face of the cube in the Creo model. While
they are defined in the model, they have not yet been defined in our
component.

1. Select the solid modeling tab of the **Part Browser**
2. Find the **point** object and drag and drop into the component
   workspace
3. Repeat this process 6 more times (one for every reference point on
   the model) and rename all points to match the names of the points in
   the model
4. Enter **connect mode** (Ctrl+2) and connect all these points to their
   corresponding points in the CADModel

***ADD IMAGE HERE***

Add Necessary Objects
^^^^^^^^^^^^^^^^^^^^^

We have now **Exposed** these points for future use. Next we need to add
objects to help Patran, the meshing software, understand what is
happening. Patran/Nastran need to know the normal directions of the
faces used and the material orientation for each face as well. This is
determined by the **Face** and **Material Contents** objects.

Face
''''

1. Find the **Face** object in the *Solid Modeling* tab of the *Part
   Browser* and drag and drop into the component workspace
2. Double click the Face object to edit it
3. Add one **Direction\_Reference\_Point** and one **ReferencePoint**
   (put the Direction point above the Reference point to make future
   steps more visible).
4. Direct back to the Component, and copy and paste 6 more of these
   edited faces (one for every point in the model)
5. Rename these faces as "Face\_Ref\_Front, Face\_Ref\_Back, ..."

After completing these steps, your component should be ordered like the
follwoing image.

*Note: Decending order is important here as it will make later steps
much more intuitive.*

***ADD IMAGE HERE***

Material Contents
'''''''''''''''''

1. Find the **MaterialContents** object in the *Solid Modeling* tab of
   the \_Part Browser\_and drag and drop into the component workspace
2. Double click the MaterialContents object to edit it
3. Add the **MaterialLayer, End\_direction,** and **Start\_Direction**
   atoms aligned below
4. Select the MaterialLayer atom, and click the **Attributes tab** in
   the *Object Inspector* on the left.
5. Set all values as shown below

***ADD IMAGE HERE***

6. Direct back to the Component, and copy and paste 6 more of these
   edited MaterialContents (one for every point in the model)
7. Rename these faces as "MaterialContents\_Front,
   MaterialContents\_Back, ... etc"

After completing these steps, your component should be ordered like the
follwoing image.

*Note: Decending order is important here as it will make later steps
much more intuitive.*

***ADD IMAGE HERE***

Making Connections
^^^^^^^^^^^^^^^^^^

Now that we have all the necessary objects for the mesher and solver to
fully define the the model, we need to make the appropriate connections
in our component. This can be done several ways, but the process
described below produces the cleanest outcome.

Face Objects
''''''''''''

1. Enter into Connection mode (Ctrl+2), and connect the
   **Reference\_Point** "Ref" of **Face\_Ref\_Front** to
   **SURF\_REF\_FRONT** exposed from the **CADModel**
2. Repeat this step for every *Face Reference* so that they all connect
   to the CADModel

We have just assigned a reference to each face so that they connect to a
real point in the model. Now we need to assign a direction for every
point so that Patran/Nastran knows where the **normal** of each face
points. We will need to use a point in the center of the cube so that
every vector can be described as **Normal Away From** in the *Object
Inspector* under the **Attributes** tab. you could just connect the
*DirectionReferencePoint* of each face to the **Cube\_Center\_Ref**, but
this would lead to a messy model with many connections. The cleanest way
to do this is to **Chain** the *DirectionReferencePoints* together. 3.
Connect the **Direction\_Reference\_Point** "Dir" of
**Face\_Ref\_Front** to **Direction\_Reference\_Point** "Dir" of
Face\_Ref\_Back. 4. Repeat this process from "Dir" to "Dir" ascending to
the last "Face\_Ref\_..." object. 5. Connect the
**Direction\_Reference\_Point** "Dir" of **Face\_Ref\_Bottom** to
**Cube\_Center\_Ref** on the CADModel

The Component should now look like this:

***ADD IMAGE HERE***

Material Contents Objects
'''''''''''''''''''''''''

We have completed the face reference portion of the Component, and all
that remains is connecting the MaterialContents. We will follow a lot of
the same steps used to connect the *Faces Objects* but this process is
slightly different.

1. Enter into Connection mode (Ctrl+2), and connect the **Start Point**
   "Sta" of **MaterialContents\_Front** to **ReferencePoint** "Ref" of
   **Face\_Ref\_Front**
2. Connect the **End Point** "End" of **MaterialContents\_Front** to
   **Start Point** "Sta" of *MaterialContents\_Back*

We have now **Chained** the **MaterialContents\_Front** to both
**Face\_Ref\_Front** and to **MaterialContents\_Back**. Now
**MaterialContents\_Front** starts at **Face\_Ref\_Front** in the
CADModel as shown by the **Chain** from **MaterialContents\_Front** to
**Face\_Ref\_Front** to **SURF\_REF\_FRONT** in the CADModel.

3. Repeat step 2 for each material face so that they are connect as
   shown

***ADD IMAGE HERE***

4. Connect the **Reference\_Point** "Ref" of **Face\_Ref\_Bottom** to
   **Cube\_Center\_Ref** on the CADModel

Now all of the MaterialContents objects are connected as needed. They
reference the same point as their corresponding face object, and point
in the direct of the previous Material Contents Object to the
**Cube\_Center\_Ref**

The final Component in CyPhy should look like this:

***ADD IMAGE HERE***

Building the Component Assembly
-------------------------------

We have successfully created the CyPhy Component which can be used to
edit the Creo model directly, with added parameters and internal Creo
relationships. If you recall, the Testbench requires a Referenced
*Component Assembly* and not a Component directly. This allows for the
user to test a *Design Space* of models instead of each model
individually. The follow steps will walk you through the construction of
a CyPhy component assembly

1. 

Building the Testbench
----------------------
