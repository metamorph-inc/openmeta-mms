.. _build_fea_testbench:

Building the Test Bench
-----------------------
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
14. Rename the 'WorkflowRef' **CADAssembly**

.. figure:: images/IMAGE12_9.png
   :alt: Solid Modeling Demo

   Text

The added **Mesh Parameters** allows us to specify to Patran, the meshing software, how we want to generate the mesh. The **CADAssembly** WorkflowRef tells our tool what task it needs to execute to properly run the FEA TestBench.

System Under Test
^^^^^^^^^^^^^^^^^

A **SystemUnderTest** part tells the tool what we are trying to analyze in the TestBench. This acts much like a reference to our **Cube_Assembly** `Component Assembly`, which is in turn a reference to our **Simple_Cube** `Component`.

1. in the `Part Browser` locate the object **System Under Test**, drag and drop this into the workspace
2. In the GME Browser, left click the Cube_Assembly and drop it ontop of the **System Under Test**

.. figure:: images/IMAGE13.png
   :alt: Solid Modeling Demo

.. note:: There must be a blue circle in the top right corner with an R inside, indicating that this is a reference to the Component Assembly`

We have now told the tool what to analyze but we haven't specified how to do so yet. To do that we need an **TestInjectionPoint**

Test Injection Point
^^^^^^^^^^^^^^^^^^^^

We will now specify what is happening to the CAD model we have referenced, and where these actions are taking place on the model. For our FEA analysis we care greatly about how the part reacts when loaded under certian conditions. or simplicity, the Cube will be fixed at its base, while having a downward load applied to the top. Imagine this like placing a large book over the entire top surface of a table, and analyzing how the table responds to the newly applied load. From intuition, we know it is most accurate to treat this as a pressure (Weight of book / Area of cube).

1. Locate the **TestInjectionPoint** in the `Part Browser` and drag it into the workspace
2. Left-click **Cube_Assembly** and drop it ontop of the `TestInjectionPoint`

.. figure:: images/IMAGE14.png
   :alt: Solid Modeling Demo

.. note:: There must be a blue circle in the top right corner with an R inside, indicating that this is a reference to the component. If the Surface_reference_points are not fully displaying thier names, change the **Port Label Length** in the preferences tab of the object inspector to 0.`

3. In the `Part Browser` locate the **Face** oject and drag it into the workspace. Change the `Face Icon name` in the `Preferences` tab of the `Object Inspector` to **Surface.png**
4. Double left-click the face to edit. Inside, drop a **ReferencePoint**
5. Direct back to **Simple_Cube_FEA** and copy this edited **Face** and paste 1 more (for the bottom and top faces of the cube)
6. Rename the faces **Face_Ref_Top** and **Face_Ref_Bottom**
7. Connect these faces to **SURF_REF_TOP** and **SURF_REF_BOTTOM**
8. In the `Part Browser` locate the **DisplacementConstraint** oject and drag it into the workspace.
9. Double left-click the **DisplacementConstraint** to edit it. add in a **Rotation** and **Translation** part
10. Select the rotation part, and in the `Attributes` tab of the`Object Inspector` change the X,Y,Z directions from **Scalar** to **Free**

.. figure:: images/IMAGE15.png
   :alt: Solid Modeling Demo

11. Direct back to the `Simple_Cube_FEA` and connect the **DisplacementConstraint** to **Face_Ref_Bottom**
12. In the `Part Browser`, drag and drop the **PressureLoadParam** into the workspace
13. Double left-click the **PressureLoadParam** and add in a **PressureLoad**
14. In the 'Object Inspector' set the ``value`` to **15**
15. To assign proper units: left-click the plus box next to **UnitLibrary QUDT-> TypeSpecifications-> Units**. Locate MegaPascals and drop it ontop of the **PressureLoad**

.. figure:: images/IMAGE16.png
   :alt: Solid Modeling Demo

16. Copy (Ctrl+C) the Pressure load and Paste it inside of **Simple_Cube_FEA**
17. Connect this to the **PressureLoadParam**

.. figure:: images/IMAGE17.png
   :alt: Solid Modeling Demo

We have now specified that we want to place a 15MPa pressure over the entire top surface of the cube while keeping the entire bottom surface from translating in any direction. Next we must specify how we want to solve this and what data we want to solve for.

1. Left-click on blanks space; in the `Object Inspector` change the `Solver Type` to **PATRAN_NASTRAN** and the `ElementType` to **Plate4**
2. In the 'Part Browser' add a **StructuralFEAComputation**. Double left-click to edit the part.
3. Add in a **FactorOfSaftey** and **MisesStress** aspect, then redirect to ``Simple_Cube_FEA``
4. Connect the **TestInjectionPoint** to the **StructuralFEAComputation** by clicking on the box border of both

.. figure:: images/IMAGE18.png
   :alt: Solid Modeling Demo

5. In the Part Browser add two (2) **Metric** parts. Rename these **FactorOfSaftey** and **MisesStress**
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
