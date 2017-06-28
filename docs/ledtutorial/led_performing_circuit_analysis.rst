.. _led_performing_circuit_analysis:

Performing Circuit Analysis
---------------------------

Next we will perform a circuit analysis via **SPICE**, a circuit simulation
software, on our **SimpleLEDCircuit** component assembly.

1. Open up the **SPICETest** test bench.
2. Create a reference of **SimpleLEDCircuit** using the `same method as
   before`__.
3. Connect the corresponding pins, **VCC** and **GND**, of the
   **SimpleLEDCircuit** to those of the **Test Components**.
   (Connecting GND to either Test Component will work)

   __ populate_the_component_assembly_

Now we are going to run a simulation that provides 5V DC to **VCC**
while establishing an electrical ground for **GND**.

Generating Results
~~~~~~~~~~~~~~~~~~

1. Locate the **Master Interpreter** icon near the top and click it.
2. Make sure **Post to META Job Manager** is checked.
3. Click **OK**.

.. image:: images/03-04-SPICE-Test-bench.png
   :alt: SPICE Test

The :ref:`resultsbrowser` will launch if it is not already open and begin
running your simulation. After a few more moments, the new job
should change from blue to green. If there is an
error and the test bench fails, it will turn red.

Viewing Results in Spice Viewer
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

1. Once your job successfully completes and turns green, right-click it
   and select **Show in explorer**.
2. Double-click the ``LaunchSpiceViewer.bat`` file.

As you can see, this circuit draws about **23.5 milliWatts** of power
with about **4.5 milliAmps** of current.
