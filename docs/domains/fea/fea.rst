.. _fea:

Finite-Element Analysis (FEA)
=============================

Overview
--------

OpenMETA is capable of executing an FEA analysis on a CAD model.
The workflow follows these steps:

1. Assemble a specified OpenMETA Component Assembly using PTC Creo.
2. Export a list of named points and a Parasolid file of the model.
3. Import the model into Patran.
4. Mesh the model and export images of the result.
5. Attach material properties, constraints, and loads to the model and prepare
   a deck for analysis using *Nastran Soln 101*.
6. Execute and review the Nastran analysis.


These steps are labeled in the diagram below.

.. figure:: images/fea_workflow_diagram.png

   Diagram of the OpenMETA Structural FEA Test Bench Workflow

Tutorial
--------

This rest of this section will serve as a tutorial that demonstrates the FEA
analysis capability of OpenMETA. This tutorial builds upon the skills learned
in :ref:`ledtutorial` chapter, so it may be necessary to complete that chapter
prior to attempting this one.

.. toctree::
   :maxdepth: 1
   :caption: Tutorial Parts

   tutorial_preparation
   create_openmeta_component
   build_openmeta_component_assembly
   build_fea_testbench
..   generate_results
..   perform_analysis
