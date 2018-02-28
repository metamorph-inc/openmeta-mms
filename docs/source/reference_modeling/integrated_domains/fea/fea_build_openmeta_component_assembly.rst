.. _fea_build_openmeta_component_assembly:

Building the Component Assembly
-------------------------------

We have successfully created the CyPhy Component which can be used to
edit the Creo model directly with added parameters and internal Creo
relationships. If you wish to check your work or start the tutorial here, open
the FEA_tutorial_part2.xme.

As we learned previously, the TestBench requires a referenced
**Component Assembly** and not a Component directly. This allows for the
user to test a *Design Space* (multiple configurations) of a model instead of each configuration
individually. The following steps will walk you through the construction of
a CyPhy component assembly.

1. In the GME Browser, right-click **Component Assemblies** and choose
   :menuselection:`Insert Model --> Component Assembly`.
2. Rename it **Cube_Assembly**.
3. Double left-click **Cube_Assembly** to open the blank component canvas.
4. In the Part Browser under the `All` tab, find the **ComponentRef** object and drag
   it into the workspace.
5. In the GME Browser, left click the **Simple_Cube** component and drop it
   on top of the **ComponentRef** object.
6. Rename the ComponentRef **Simple_Cube_Ref**.

.. figure:: images/IMAGE11.png
   :alt: Solid Modeling Demo

.. note:: There *must* be a blue circle in the top right corner of Simple_Cube_Ref
          with an R inside, indicating that this is a reference to the component.

We have just created a reference to the Component **Simple_Cube** inside of our
Component Assembly. The object here can be edited by parameters, other objects
linked in the assembly, or in a TestBench so long as they are connected properly.
Now that the component is referenced, we need to expose these surface points so
that they can be used by the FEA TestBench.

7. Redirect back to the **Simple_Cube** component canvas.
8. Select all the **Surf_Ref** points created earlier and copy them with `Ctrl+c`.
9. Direct back into **Cube_Assembly** and Paste the points with `Ctrl+v`.

We have copied over all the necessary points while also keeping the same order,
saving us time in the future.

10. Connect all of these points to their corresponding reference points in
**Simple_Cube_Ref**.

.. figure:: images/IMAGE12.png
   :alt: Solid Modeling Demo

We have now exposed the surface reference points of the Creo model through the **Component Reference**
in the **Component Assembly**. This allows us to reference these points directly in our FEA TestBench.
Now that both the Component and Component Assembly are built, it is time to create the FEA TestBench.
