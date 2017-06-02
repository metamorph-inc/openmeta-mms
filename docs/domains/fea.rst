.. _fea:

Finite-Element Analysis (FEA)
=============================

Overview
--------

This chapter will serve as a tutorial that demonstrates the FEA analysis aspect of OpenMETA. We will build a simple FEA TestBench using a
basic Creo model. This tutorial uses the skills learned in Chapter 3:
LED Tutorial,so you'll need to have completed that chapter prior to
attempting this one. Through this tutorial you will learn how to:

-  Build a Component for a solid model
-  Build a Component Assembly for a solid model
-  Build an FEA TestBench
-  Generate results
-  Perform solid analysis and simulation on TestBench

Downloading the Tutorial Zip File
---------------------------------

To complete this tutorial you will need to download the FEA analysis tutorial file .

Setting Up Directory
--------------------

1. Once you've downloaded the tutorial model, unzip it and double-click
   ``FEA_Tutorial.xme`` to open it in **GME**.
2. Save the project to the default directory (where your ``.xme`` file
   is located), taking note of this directory as you will need to access
   this location later in the tutorial.

*Note: Be sure all the files (e.g. "components folder", ``.xme`` file)
are in the same directory throughout the tutorial.*

Generic Process for an FEA TestBench
------------------------------------

The FEA TestBench in OpenMETA works much like all the other TestBenches do,
in that it requires a referenced component assembly. The general
structure follows the trend: **TestBench -> Component assembly ->
component -> Creo 3.0 model**. We will start our work from the bottom
and build up to the TestBench. Since the Creo.prt file is provided in
the download, we will begin with building the component.

Building a CAD Component
------------------------

In this first section, we will build a 3D component. Engineers often use
multiple types of analysis software to test their designs. This practice
takes time; you need to learn each piece of software and compose a model
for each one that will test for a parameter. However, Metamorphosys
tools allow you to run multiple types of analyses by composing just one
model.

In the Metamorphosys tools, we start a design by instantiating
components and joining their interfaces, yielding a **Component
Assembly**. We call this process *composition*. From this composed
design, we can generate new models and run analyses.

First we must create a component for the Creo model we wish to test.
This process breaks down into the following steps:

1. :ref:`create_component`
2. :ref:`reference_creo`
3. :ref:`add_objects`
4. :ref:`make_connections`

.. _create_component:

Creating a New Component
^^^^^^^^^^^^^^^^^^^^^^^^

A component houses the Creo part reference, as well as defining objects for the Model. This is the smallest piece of the FEA TestBench hierarchy, so it must be developed first.

1.  In your **GME Browser**, open the ***RootFolder*** by left-clicking
    the plus box to the left of the name.
2.  Right-click **RootFolder -> Insert folder -> Component Assembly**.
3.  Right-click **RootFolder -> Insert folder -> Components**.
4.  **Right-click** the **Components folder -> Insert Model ->
    Component**.
5.  Rename **Simple\_Cube**.
6.  Double left-click **Simple\_Cube** to open the blank component
    canvas.
7.  Located on the left is the **Part Browser**; left-click the **Solid
    Modeling** tab
8.  Locate the **Property** object, left-click the image then drag and
    drop it onto the workspace. 
	`Note: make sure you have Property, and not complex metric.`
9.  Click on the word "Property" and rename it as
    **PTC\_Material\_Name**
10. Click the empy box in the *Property* and type
    ALUMINUM\_ALLOY\_6061\_T6

.. figure:: images/IMAGE_1.png
   :alt: Solid Modeling Demo
   
This will be used later when defining the material type for the mesher and solver software.

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
points on the center of every face of the cube in the Creo model. 

.. figure:: images/IMAGE_1_5.png
   :alt: Solid Modeling Demo
   
While they are defined in the model, they have not yet been defined in our
component.
1. Connect (Ctrl+2) the **PTC_Material_Name** to the Parameter with the matching name in the `CAD Model`
2. Select the `solid modeling` tab of the **Part Browser**
3. Find the **point** object and drag and drop into the component
   workspace
4. Repeat this process 6 more times (one for every reference point on
   the model) and rename all points to match the names of the points in
   the model
5. Enter **connect mode** (Ctrl+2) and connect all these points to their
   corresponding points as **Port Composition** in the CADModel

.. figure:: images/IMAGE2.png
   :alt: Solid Modeling Demo

.. _add_objects:

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

.. figure:: images/IMAGE3.png
   :alt: Solid Modeling Demo

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

.. figure:: images/IMAGE4.png
   :alt: Solid Modeling Demo

6. Direct back to the Component, and copy and paste 6 more of these
   edited MaterialContents (one for every point in the model)
7. Rename these faces as "MaterialContents\_Front,
   MaterialContents\_Back, ... etc"

After completing these steps, your component should be ordered like the
follwoing image.

*Note: Decending order is important here as it will make later steps
much more intuitive.*

.. figure:: images/IMAGE5.png
   :alt: Solid Modeling Demo

.. _make_connections:

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
   `NOTE: All connections in the component building process will be port composition connections.`
2. Repeat this step for every *Face Reference* so that they all connect
   to the same name in the CADModel

.. figure:: images/IMAGE6.png
   :alt: Solid Modeling Demo
   
`NOTE: Make sure all the faces **Normal Direction** option is listed as **Away_Reference_point**`

.. figure:: images/IMAGE6_5.png
   :alt: Solid Modeling Demo
   
We have just assigned a reference to each face so that they connect to a
real point in the model. Now we need to assign a direction for every
point so that Patran/Nastran knows where the **normal** of each face
points. We will need to use a point in the center of the cube so that
every vector can be described as **Normal Away From** in the *Object
Inspector* under the **Attributes** tab. you could just connect the
*DirectionReferencePoint* of each face to the **Cube\_Center\_Ref**, but
this would lead to a messy model with many connections. The cleanest way
to do this is to **Chain** the *DirectionReferencePoints* together.

3. Connect the **Direction\_Reference\_Point** "Dir" of **Face\_Ref\_Front** to **Direction\_Reference\_Point** "Dir" of Face\_Ref\_Back.


.. figure:: images/IMAGE7.png
   :alt: Solid Modeling Demo

4. Repeat this process from "Dir" to "Dir" ascending to the last "Face\_Ref\_..." object.
5. Connect the **Direction\_Reference\_Point** "Dir" of **Face\_Ref\_Bottom** to **Cube\_Center\_Ref** on the CADModel

The Component should now look like this:

.. figure:: images/IMAGE8.png
   :alt: Solid Modeling Demo
   
We have completed the face reference portion of the Component, and all
that remains is connecting the MaterialContents.

Material Contents Objects
'''''''''''''''''''''''''

We will follow a lot of the same steps used to connect the *Faces Objects* but this process is slightly different.

1. Enter into Connection mode (Ctrl+2), and connect the **Start Point**
   "Sta" of **MaterialContents\_Front** to **ReferencePoint** "Ref" of
   **Face\_Ref\_Front**
2. Connect the **End Point** "End" of **MaterialContents\_Front** to
   **Start Point** "Sta" of *MaterialContents\_Back*

.. figure:: images/IMAGE9.png
   :alt: Solid Modeling Demo

We have now **Chained** the **MaterialContents\_Front** to both
**Face\_Ref\_Front** and to **MaterialContents\_Back**. Now
**MaterialContents\_Front** starts at **Face\_Ref\_Front** in the
CADModel as shown by the **Chain** from **MaterialContents\_Front** to
**Face\_Ref\_Front** to **SURF\_REF\_FRONT** in the CADModel.

3. Repeat step 2 for each material face so that they are connect as
   shown

4. Connect the **Reference\_Point** "Ref" of **Face\_Ref\_Bottom** to
   **Cube\_Center\_Ref** on the CADModel

.. figure:: images/IMAGE10.png
   :alt: Solid Modeling Demo
5. Connect **MaterialContents_Front** to **Face\_Ref\_Front**

.. figure:: images/IMAGE10_5.png
   :alt: Solid Modeling Demo

6. Repeat this step for every `MaterialContents` and its corresponding face.

.. figure:: images/IMAGE10_75.png
   :alt: Solid Modeling Demo

Now all of the MaterialContents objects are connected as needed. They
reference the same point as their corresponding face object, and point
in the direct of the previous Material Contents Object to the
**Cube\_Center\_Ref**


Building the Component Assembly
-------------------------------

We have successfully created the CyPhy Component which can be used to
edit the Creo model directly, with added parameters and internal Creo
relationships. If you recall, the TestBench requires a Referenced
**Component Assembly** and not a Component directly. This allows for the
user to test a *Design Space* (multiple configurations) of a model instead of each configuration
individually. The following steps will walk you through the construction of
a CyPhy component assembly

1. In the GME Browser, right-click **Component Assemblies -> insert model -> Component Assembly**
2. Rename This **Cube_Assembly**
3. Double left-click **Cube_Assembly**** to open the blank component canvas
4. In the Part Browser under the `All` tab, find the **ComponentRef** and drag and drop into the workspace
5. In the GME Browser, left click the **Simple_Cube** component and drop it ontop of the **ComponentRef**
6. Rename the ComponentRef **Simple_Cube_Ref**

.. figure:: images/IMAGE11.png
   :alt: Solid Modeling Demo
   
`NOTE: There must be a blue circle in the top right corner with an R inside, idicating that this is a reference to the component`

We have just created a reference to the Component **Simple\_Cube** inside of our Component Assembly. The object here can be edited by parameters or other objects linked in the assembly or in a TestBench so long as they are connected properly. Now that the component is referenced, we need to expose these surface points so that they can be used by the FEA TestBench.

7. Redirect back to the **Simple_Cube** component
8. select all the **Surf_Ref** points created earlier and copy them with `Ctrl+c`
9. Direct back into **Cube_Assembly** and Paste the points with `Ctrl+v`

We have copied over all the necessary points while also keeping the same order, saving us time in the future. 

10. Connect all the points to thier reference points in the ComponentReference

.. figure:: images/IMAGE12.png
   :alt: Solid Modeling Demo

We have now exposed the surface reference points of the Creo model through the **Component Reference** in the **Component Assembly**. This allows us to reference these points directly in our FEA TestBench. Now that both the Component and Component Assembly are built, it is time to create the FEA TestBench.

Building the TestBench
----------------------
TestBenches can be as simple as a few blocks for computation, or complex as many references that send and recieve data from different programs to compute large scale behavioral studies. The generic FEA TestBench consists of:

* Tool Features
* System Under Test
* Test Injection Points

Tool Features
^^^^^^^^^^^^^

Before we can start building the TestBench, we have to give it a home folder.

1. Right-click on **Root folder -> Insert folder ->Testing**
2. Right-click on **Testing -> Insert model ->StructuralFEATestBench**
3. Rename this **Simple_Cube_FEA**
4. Double left-click **Simple_Cube_FEA** to open the blank TestBench canvas
5. In the `Part Browser`, locate **Mesh Parameters** and drag it into the workspace
6. Select the **Mesh Parameters** and change the values in the **Attributes tab** of the Object Inspector as shown below

.. figure:: images/IMAGE12_5.png
   :alt: Solid Modeling Demo
   
7. In the `Part Browser`, locate **WorkflowRef** and drag it into the workspace
8. In the `GME Browser` Right-click on **Testing -> Insert folder -> Workflow Definitions**
9. Right-click on **Workflow Definitions -> Insert model -> Workflow**
10. Rename this `Workflow` **CADAssembly**
11. Right-click on **CADAssembly -> Insert atom -> Task -> CyPhy2CAD**

.. figure:: images/IMAGE12_75.png
   :alt: Solid Modeling Demo

12. Redirect back t0 **Simple_Cube_FEA**, locate the **WorkflowRef** in the `Part Browser` and drag it into the workspace
13. Left-click the **CADAssembly** `workflow` and drop it on top of the WorkflowRef
14. Rename the 'WorkflowRef` **CADAssembly**

.. figure:: images/IMAGE12_9.png
   :alt: Solid Modeling Demo

The added **Mesh Parameters** allows us to specify to Patran, the meshing software, how we want to generate the mesh. The **CADAssembly** WorkflowRef tells our tool what task it needs to execute to properly run the FEA TestBench.
   
System Under Test
^^^^^^^^^^^^^^^^^

A **SystemUnderTest** part tells the tool what we are trying to analyze in the TestBench. This acts much like a reference to our **Cube_Assembly** `Component Assembly`, which is in turn a reference to our **Simple_Cube** `Component`.

1. in the `Part Browser` locate the object **System Under Test**, drag and drop this into the workspace
2. In the GME Browser, left click the Cube_Assembly and drop it ontop of the **System Under Test**

.. figure:: images/IMAGE13.png
   :alt: Solid Modeling Demo
   
`NOTE: There must be a blue circle in the top right corner with an R inside, idicating that this is a reference to the Component Assembly`

We have now told the tool what to analyze but we haven't specified how to do so yet. To do that we need an **TestInjectionPoint**

Test Injection Point
^^^^^^^^^^^^^^^^^^^^

We will now specify what is happening to the CAD model we have referenced, and where these actions are taking place on the model. For our FEA analysis we care greatly about how the part reacts when loaded under certian conditions. or simplicity, the Cube will be fixed at its base, while having a downward load applied to the top. Imagine this like placing a large book over the entire top surface of a table, and analyzing how the table responds to the newly applied load. From intuition, we know it is most accurate to treat this as a pressure (Weight of book / Area of cube). 

1. Locate the **TestInjectionPoint** in the `Part Browser` and drag it into the workspace
2. Left-click **Cube_Assembly** and drop it ontop of the `TestInjectionPoint`

.. figure:: images/IMAGE14.png
   :alt: Solid Modeling Demo

`NOTE: There must be a blue circle in the top right corner with an R inside, idicating that this is a reference to the component. If the Surface_reference_points are not fully displaying thier names, change the **Port Label Length** in the preferences tab of the object inspector to 0.`

3. In the `Part Browser` locate the **Face** oject and drag it into the workspace. Change the `Face Icon name` in the `Preferences` tab of the `Object Inspector` to **Surface.png**
4. Double left-click the face to edit. Inside, drop a **ReferencePoint**
5. Direct back to **Simple_Cube_FEA** and copy this edited **Face** and paste 1 more (for the bottom and top faces of the cube)
6. Rename the faces **Face_Ref_Top** and **Face_Ref_Bottom**
6. Connect these faces to **SURF_REF_TOP** and **SURF_REF_BOTTOM**
7. In the `Part Browser` locate the **DisplacementConstraint** oject and drag it into the workspace.
8. Double left-click the **DisplacementConstraint** to edit it. add in a **Rotation** and **Translation** part
9. Select the rotation part, and in the `Attributes` tab of the`Object Inspector` change the X,Y,Z directions from **Scalar** to **Free**

.. figure:: images/IMAGE15.png
   :alt: Solid Modeling Demo

10. Direct back to the `Simple_Cube_FEA` and connect the **DisplacementConstraint** to **Face_Ref_Bottom**
11. In the `Part Browser`, drag and drop the **PressureLoadParam** into the workspace
12. Double left-click the **PressureLoadParam** and add in a **PressureLoad**
13. In the 'Object Inspector` set the `value` to **15**
14. To assign proper units: left-click the plus box next to **UnitLibrary QUDT-> TypeSpecifications-> Units**. Locate MegaPascals and drop it ontop of the **PressureLoad**

.. figure:: images/IMAGE16.png
   :alt: Solid Modeling Demo
   
15. Copy (Ctrl+C) the Pressure load and Paste it inside of **Simple_Cube_FEA**
16. Connect this to the **PressureLoadParam**

.. figure:: images/IMAGE17.png
   :alt: Solid Modeling Demo
   
We have now specified that we want to place a 15MPa pressure over the entire top surface of the cube while keeping the entire bottom surface from translating in any direction. Next we must specify how we want to solve this and what data we want to solve for.

1. Left-click on blanks space; in the `Object Inspector` change the `Solver Type` to **PATRAN_NASTRAN** and the `ElementType` to **Plate4**
2. In the `Part Browser` add a **StructuralFEAComputation**. Double left-click to edit the part.
3. Add in a **FactorOfSaftey** and **MisesStress** aspect, then redirect to `Simple_Cube_FEA`
4. Connect the **TestInjectionPoint** to the **StructuralFEAComputation** by clicking on the box border of both

.. figure:: images/IMAGE18.png
   :alt: Solid Modeling Demo
   
5. In the `Part Browser` add in 2 **Metric** parts. Rename these **FactorOfSaftey**** and **MisesStress**
6. Connect these to their **StructuralFEAComputation** counterparts

.. figure:: images/IMAGE19.png
   :alt: Solid Modeling Demo

In general, when condcuting an FEA TestBench, we are interested in simulating a load and seeing the reaction of a part. In our case, we only want to see values that do not exceed the ultimate strength of the Cube. We can set this as a **Metric Constraint** that limits values to always exceed a factor of safety of 1.0
   
7. In the `Part Browser` locate and add a **Metric Constraint**
8. Rename this **ReserveFactorRequirement**
9. set the `TargetVaule` to **1.0**
10. Connect this to the **FactorOfSaftey** metric

This does not change how the user views the data but how the TestBench Manifest sorts data. This is generally good practice as it will help debug a design space if parts continually fail the factor of saftey requirement. Our now Complete TestBench should look like this:

.. figure:: images/IMAGE20.png
   :alt: Solid Modeling Demo
