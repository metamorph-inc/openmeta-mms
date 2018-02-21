.. _fea_perform_analysis:

Performing the Analysis
=======================
Once we have built a complete Test Bench, we need to run it to have results.
As instructed in the LED Tutorial we will be using the **CyPhy Master Interpreter**.

.. figure:: images/IMAGE21.png
   :alt: Solid Modeling Demo

1. Make sure you are viewing the Simple_Cube_FEA Test Bench
2. Double-click the **Master Interpreter** and select **okay** at the bottom of
   the new window.
3. Unselect both of the preselected AP203 part files as they will not be necessary.
4. Click **Okay** in the CAD Options window

The results browser should have automatically opened.
In here you can check the progress of the current tests as well as the results of
previous tests. A complete history of tests stored in the .xme can be found under
the **Test Benches** tab.

5. In the **Active Jobs** tab, right click on the current Test Bench and select
   **Show in explorer**
6. To view the results of the successful job navigate to :menuselection:`
   Analysis --> Patran_Nastran` and open the ``Nastran_mod_3_1_VM_D_iso_1.png``
   file.

.. figure:: images/IMAGE22.png
   :alt: Solid Modeling Demo

Here you can see the results of the Nastran Analysis without opening Patran.
If you wish to view the results with Patran, open the ``Nastran_mod.db`` file.
