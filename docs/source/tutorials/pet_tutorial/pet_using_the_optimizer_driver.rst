.. _pet_using_the_optimizer_driver:

Using the Optimizer PET Driver
==============================

We will now use an **Optimizer** Driver to find
the **x** and **y** values that minimize **f_xy**.

In the previous :ref:`pet_adding_a_driver` section, we used the Parameter Study
driver to obtain the **x** and **y** values needed to minimize **f_xy**. That method
was rather ineffecient as it relied on a brute force sampling (961 samples)
of the design space in order to obtain a reasonable estimate of the optimal
x and y values.

In this section, we will introduce the :ref:`pet_drivers_optimizer`. The Optimizer driver
is better suited for optimization/minimization problems.

.. note:: This section of the tutorial builds on the preceding Parameter Study sections.
   You will need to have completed the :ref:`pet_getting_started` and
   :ref:`pet_adding_an_analysis_block` sections before you start this section.

Open an existing OpenMETA Project
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

If the **parameterstudy-tutorial.mga** GME project is still open,
then you can skip Steps 1-3.

1. Start GME.
2. Within GME, open the **File** menu and select **Open Project...**.
3. Left-click on the **parameterstudy-tutorial.mga** file that you created
   in the last tutorial then select **Open**.

.. figure:: images/optimizer_tutorial_1.png
   :alt: text

Create a new PET within the Project
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

4. Inside the **GME Browser** window, right-click on the
   :menuselection:`RootFolder --> Testing --> Parametric Exploration` folder
   and select :menuselection:`Insert Model --> Parametric Exploration`.

.. image:: images/optimizer_tutorial_1_a.png

5. Change the name of the newly created **ParametricExploration** model to
"**optimizer-tutorial**".

.. figure:: images/optimizer_tutorial_2.png
   :alt: text

6. Double-click on **optimizer-tutorial** to open it in the main GME window.
It should appear as a blank canvas.

.. figure:: images/optimizer_tutorial_2_a.png
   :alt: text

Instead of redoing work, let's copy our existing work from the Parameter Study tutorial.

7. Inside the **GME Browser** window, double-click on the :menuselection:`
RootFolder --> Testing --> Parametric Exploration --> parameterstudy-tutorial`
to open it in a window.

.. image:: images/optimizer_tutorial_3.png

8. Left-click and drag within **parameterstudy-tutorial**'s canvas to select everything.
9. Press :kbd:`(Control-C)` to copy the selected area.

.. image:: images/optimizer_tutorial_4.png

10. Return to the **optimizer-tutorial** canvas and press :kbd:`(Control-V)`
to paste **ParameterStudy** and **Paraboloid** into **optimizer-tutorial**.

.. image:: images/optimizer_tutorial_5.png

Now, we don't actually need **ParameterStudy** since the plan is to use
an Optimizer driver instead.

11. Left-click on **ParameterStudy** and press :kbd:`(Delete)`.

.. figure:: images/optimizer_tutorial_6.png
   :alt: text

Adding an Optimizer Driver to the PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

12. Left-click on the **Optimizer** icon in the **Part Browser** and drag it onto the PET canvas.

.. figure:: images/optimizer_tutorial_7.png
   :alt: text

.. figure:: images/optimizer_tutorial_7_a.png
   :alt: text

13. Double-click on the **Optimizer** model.

A window with a blank canvas will open up.

.. figure:: images/optimizer_tutorial_8.png
   :alt: text

14. Left-click on the **Design Variable** icon in the **Part Browser**
and drag it onto the Optimizer canvas.

.. figure:: images/optimizer_tutorial_9.png
   :alt: text

15. Left-click the newly added **DesignVariable** to select it.
16. Left-click on the “DesignVariable” label and change it to “x”.

.. figure:: images/optimizer_tutorial_10_a.png
   :alt: text

17. Left-click on the Design Variable **x** to select it.
18. Locate the **Range** field under **Attributes** in the **Object Inspector** window.
19. Set **x**’s range by entering “**-50,+50**” in the Range field.

.. figure:: images/optimizer_tutorial_10.png
   :alt: text

20. Repeat Steps 14-19 to add a second Design Variable **y** with a range of **-50,+50** as well.

.. figure:: images/optimizer_tutorial_11.png
   :alt: text

21. Left-click on the **Objective** icon in the **Part Browser** and drag it onto the Optimizer canvas.
22. Change **Objectives**'s name to "**f_xy**".

.. figure:: images/optimizer_tutorial_12.png
   :alt: text

23. Left-click on the **Optimizer Constraint** icon in the **Part Browser** and drag it onto the Optimizer canvas.
24. Change **Optimizer Constraint**'s name to "**x_con**".

.. figure:: images/optimizer_tutorial_13_a.png
   :alt: text

25. Left-click on the Optimizer Constraint **x_con** to select it.
26. Locate the **MaxValue** and **MinValue** fields under **Attributes** in the **Objective Inspector** window.
27. Enter "**+50**" and "**-50**" in **MaxValue** and **MinValue**'s respective fields.

.. figure:: images/optimizer_tutorial_13.png
   :alt: text

28. Repeat Steps 23-27 to add a second Optimizer Constraint **y_con** with
a MaxValue of **+50** and a MinValue of **-50**.

.. figure:: images/optimizer_tutorial_14.png
   :alt: text

29. Left-click on the **Optimizer** canvas to select it.
30. Select **COBYLA** for the **Function** field.

.. note:: **COBYLA** stands for Constrained Optimization BY Linear Approximation and
  is the default Optimizer function in OpenMETA since it does not require defined
  gradients / Jacobian matrices in order to work.

.. figure:: images/optimizer_tutorial_15.png
   :alt: text

31. Open the **optimizer-tutorial** window

Notice that Design Variables **x** and **y**, Optimizer Constraints **x_con**
and **y_con**, and the Objective **f_xy** are now exposed as ports on the
outside of the Optimizer model.

.. figure:: images/optimizer_tutorial_16.png
   :alt: text

Making connections within the PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

32. Left-click the **Connect Mode** icon on the **Modeling** toolbar.

.. figure:: images/optimizer_tutorial_16_a.png
   :alt: text

33. Using **Connect Mode**, connect **Optimizer**'s Design Variables
**x** and **y** to **Paraboloid**'s Parameters **x** and **y**.

.. figure:: images/optimizer_tutorial_17.png
   :alt: text

34. Connect **Optimizer**'s Design Variables **x** and **y** to
**Optimizer**'s Optimizer Constraints **x_con** and **y_con**.

.. figure:: images/optimizer_tutorial_18.png
   :alt: text

35. Connect **Paraboloid**'s Metric **f_xy** to **Optimizer**'s
Objective **f_xy**.

.. figure:: images/optimizer_tutorial_19.png
   :alt: text

Now everything is connected!

Running a PET Analysis
~~~~~~~~~~~~~~~~~~~~~~

Now that the PET has been set up, it is time to run it.

36. Left-click on the **CyPhy Master Interpreter** icon on the **Components** toolbar.

.. figure:: images/optimizer_tutorial_20.png
   :alt: text

The **CyPhy Master Interpreter** window will open up.

37. Make sure the **Post to META Job Manager** checkbox is selected.
38. Select **OK**.

.. figure:: images/optimizer_tutorial_21.png
   :alt: text

The **Results Browser** will open up.

.. figure:: images/optimizer_tutorial_22.png
   :alt: text

39. Left-click on the **PET** tab within the **Results Browser**.

.. figure:: images/optimizer_tutorial_23.png
   :alt: text


40. Left-click **optimizer-tutorial** to display run information on the right pane.

.. figure:: images/optimizer_tutorial_23_a.png
   :alt: text

You will notice that **optimizer-tutorial** generated **58** records, meaning
that it converged in **58** iterations. As you can see it discovered the correct
global minimum of **f_xy** at value of **-27.33**.

Compared to **parameterstudy-tutorial**, **optimizer-tutorial** found
**f_xy**'s minimum much more efficiently and accurately.

.. note:: The (dis)advantage of using an Optimizer Driver
   is that it will not explore nearly as much of the design space as a
   Parameter Study Driver will.

Visualizer Analysis
~~~~~~~~~~~~~~~~~~~

41. Left-click **Launch in Visualizer** in the bottom-right corner of the **Results Browser**.

A browser window will open with the Visualizer.

42. Navigate to the **Pairs Plot** tab of the **Explore** tab.
43. Clear the default contents of the **Design Variables:** field in the **Variables** section.
44. Add **x**, **y**, and **f_xy** to the **Design Variables:** field.

.. figure:: images/optimizer_tutorial_24_a.png
   :alt: text

The graphs show how **x** and **y** had their values changed by
the Optimizer Driver as **f_xy**'s value was minimized.

.. figure:: images/optimizer_tutorial_24.png
   :alt: text

45. Left-click on the **Data Table** tab of the Visualizer.

This will display the result records in a table format.

.. figure:: images/optimizer_tutorial_25.png
   :alt: text

By default, the results are sorted in ascending order by iteration.

.. figure:: images/optimizer_tutorial_25_a.png
   :alt: text

46. Left-click on the **f_xy** column header to sort the results in ascending order.

The Optimizer found a minimum value of **-27.33** for **f_xy** at **x = 6.67**
and **y = -7.33**.

.. figure:: images/optimizer_tutorial_26_a.png
   :alt: text

**Congratulations!** You have successfully completed the **PET Tutorial**.

For more information on PETs, Analysis Blocks, and Drivers, check out the
:ref:`pet` chapter of the OpenMETA Documentation.

For more information on the Optimizer PET Driver specifically, check out the
:ref:`pet_drivers_optimizer` section as well as the Optimization section of
:ref:`pet_advanced_topics`.
