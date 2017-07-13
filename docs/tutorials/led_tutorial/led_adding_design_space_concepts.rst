.. _led_adding_design_space_concepts:

Adding Design Space Concepts
----------------------------

So far, we constructed a simple LED circuit consisting of a *single* LED
and a *single* resistor. In the OpenMETA tools, this is referred to as a
*Design Point*. In this section, we'll modify our simple LED circuit to
use a selection of resistors and LEDs in our design by creating a
**Design Space**.

This section of the tuturial builds upon the previous sections, so
you'll need to use the model that you built. Alternatively, you can open
``Walkthrough_LED_part2.xme``, which includes all of the work from the
previous sections.

Refactoring a Design Point into a Design Space
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

We must first convert our *component assembly* to a *design container*
and then convert each *component* into a *design container*.

1. Open up **SimpleLEDCircuit** and make sure nothing in the canvas is
   selected.
2. Click the **Design Space Refractorer** icon |Design Space Refactorer icon|.
3. Go to the **GME Browser** Window and locate and expand the blue
   **NewDS__SimpleLEDCircuit** folder.
4. Double-click **NewDC__SimpleLEDCircuit** to open it.

.. image:: images/03-05-new-dc.png
   :alt: New Design Space

.. |Design Space Refactorer icon| image:: images/03-03-ds-refactor-icon.png
      :alt: Design Space Refactorer icon
      :width: 18px

You'll notice that it looks *exactly* like your previous component
assembly. It has preserved the component names, port names, connections,
and layout. However, since we now have a design container, we can begin
adding variability to the design space.

Convert Component into Alternative
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Starting with the 1k resistor:

1. Select **Resistor_1k**.
2. Click the **Design Space Refactorer** tool, again.
3. When prompted, choose to convert the selected Component to a new
   **DesignContainer**.

.. image:: images/03-03-ds-refactor-prompt.png
   :align: center

4. Double-click the new **Design Container** that replaced your 1k
   resistor.
5. Drag in references of the other two resistors from the **Components**
   folder. See method 1 or 2 in :ref:`populate_the_component_assembly` above.
6. Mimic the connections of **Resistor_1k** so that each resistor in
   the design space matches the paradigm: :menuselection:`P1_Resistor_1k -->
   resistor_X --> Anode_Resistor_1k`

__ populate_the_component_assembly_

.. image:: images/03-05-new-dc-resistors.png
   :alt: Resistors in New Design Space

Go back to **NewDC__SimpleLEDCircuit** and repeat steps 1-6 for
**Led_GREEN** by creating references of the other two LEDs, Blue and
Red, inside the design container. When you are done, it should look like
this:

.. image:: images/03-05-complete-design-space.png
   :alt: Completed design space

Generating Design Points
~~~~~~~~~~~~~~~~~~~~~~~~

Since we have three alternative resistors and three alternative LEDs, we
can select up to nine configurations.

1. Select the **Design Space Exploration Tool** |Design Space Exploration Tool icon|.
2. Click **Show CFGs**.
3. Verify that there are nine configurations and select **Export All**.
4. Click **Return to CyPhy**.

It won't look like anything happened but there will be nine simulations
(one for each **Design Point**) the next time you run a test bench.

.. |Design Space Exploration Tool icon| image:: images/04-design-space-exploration-tool-icon.png
      :alt: Design Space Refactorer icon
      :width: 18px
