.. _led_running_designs:

Running Tests on Multiple Designs
---------------------------------

Now that we've created a single *Design Space* that yields 9 potential
*Design Points*, we can use test benches to evaluate them more quickly.
By configuring a test bench with a *Design Space* as the *System Under Test*,
we can easily run that test bench against any or all of the potential
*Design Points*.

We will now run three test benches on our
**NewDC__SimpleLEDCircuit** and view all the results in the OpenMETA Visualizer.
You are already familiar with configuring a test bench from the
:ref:`configuring_a_test_bench` section; the only difference
is here you will be creating a reference of **NewDC__SimpleLEDCircuit**
instead of **SimpleLEDCircuit**.

Don't forget to choose **TopLevelSystemUnderTest** as a reference
role type.

Generating Schematics
~~~~~~~~~~~~~~~~~~~~~

1. Configure the **PlaceAndRoute_1x2** Test Bench with our new design
   space, **NewDC__SimpleLEDCircuit**.

Due to Eagle licensing constraints, some failures may occur when attempting to
execute the **PlaceAndRoute_1x2** Test Bench on multiple configurations simultaneously.
To avoid this issue, we'll disable simultaneous processing in the Results Browser.

2. Open the Results Browser.

.. image:: images/launch_results_browser.png
   :alt: Launching the Results Browser

3. Change ``Simultaneous processes:`` at the bottom of the Results Browser
   window to 1 process.

.. note:: You may change this setting back to default when not
   executing the **PlaceAndRoute_1x2** Test Bench. For more information, see the
   :ref:`results_browser` reference page.


4. Back in GME, click the **Master Interpreter** icon.

5. Click **Select All** to select all configurations.

6. Make sure **Post to META Job Manager** is checked.

7. Click **OK**.

.. image:: images/03-06-design-space-eagle.png
   :alt: Place and Route test

Right-click on any job and choose Show in explorer to browse the
generated artifacts. You can open up the ``schema.sch`` file as before
to view the resulting **Eagle** file.

Performing a SPICE Analysis
~~~~~~~~~~~~~~~~~~~~~~~~~~~

Now that we've shown how to generate schematics from all *design points*
in a *design space*, let's perform some analysis. We'll perform circuit
analysis via **SPICE** as before.

1. Configure the **SPICETest** test bench with our new design space,
   **NewDC__SimpleLEDCircuit**.
2. Make the same connections as before with **VCC** and **GND**.
3. Click the **Master Interpreter** icon.
4. Click **Select All** to select all configurations.
5. Make sure **Post to META Job Manager** is checked.
6. Click **OK**.

Performing Parts Cost Estimation
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The OpenMETA tools include a test bench for estimating the parts cost of a
design. For a given design, it will check current part prices based on
the part quantity and specified number of designs.

1. Configure the **CostEstimation** test bench with our new design
   space, **NewDC__SimpleLEDCircuit**.
2. Click the **Master Interpreter** icon.
3. Click **Select All** to select all configurations.
4. Make sure **Post to META Job Manager** is checked.
5. Click **OK**.

.. |Design Space Refactorer icon| image:: images/03-03-ds-refactor-icon.png
.. |Design Space Exploration Tool| image:: images/04-design-space-exploration-tool-icon.png
.. |CONNECTMODE| image:: images/connectmode2.png
.. |DISCONNECTMODE| image:: images/disconnectmode2.png
.. |EDITMODE| image:: images/editmode2.png
