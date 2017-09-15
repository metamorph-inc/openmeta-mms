.. _pet_refinement:

PET Refinement Tab
==================

The PET Refinement Tab allows for the generation of new results directly from
the Visualizer. You can configure any of the design variable ranges as well as
the design configurations upon which you want to re-execute the PET.

.. figure:: images/petrefinement.png
   :alt: PET Refinement Tab

   User Interface of the PET Refinement Tab

Driver Configuration
~~~~~~~~~~~~~~~~~~~~

-  **New Sampling Method:** 'FullFactorial' or 'Uniform'

-  **New Number of Samples:** An argument that specifies the number of
   samples.

Design Configuration
~~~~~~~~~~~~~~~~~~~~

This section allows the user to choose only certain configurations to
run for the next PET.

Numeric Ranges
~~~~~~~~~~~~~~

-  **Original Numeric Ranges:** This section displays the min and max
   value, for each variable, before any filtering has been applied.
   Clicking ‘Original’ inserts all these value to the New Ranges
   (whereas clicking 1 apply adds the ‘new’ value for just a single
   row).

-  **Refined Numeric Ranges:** This section displays the min and max
   value, for each variable, post-filtering. Clicking ‘Refined’ inserts
   all these value to the New Ranges (whereas clicking 1 apply adds the
   ‘new’ value for just a single row).

-  **New Numeric Ranges:** This section can be populated with the listed
   values using the appropriate 'Apply' button or new min and max values
   for each variable can be set manually.

PET Details
~~~~~~~~~~~

-  **New PET Name:** A copy of the original PET will be generated and
   given this name.

Execute New PET
~~~~~~~~~~~~~~~

Clicking the 'Execute New PET' button will cause a new PET to be started
with the Master Interpreter and dispatched to the Active Jobs tab in the
Results Browser.
