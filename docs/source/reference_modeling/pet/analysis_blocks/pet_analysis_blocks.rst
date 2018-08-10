.. _pet_analysis_blocks:

PET Analysis Blocks
===================

PET Analysis Blocks are Test Benches and External Tool Wrappers that can be
placed within a PET to model the system to be analyzed. Therefore, PET Analysis
Blocks serve as modular building blocks that can be combined within a single PET
to perform a full-system analysis using subanalyses from multiple domains.

.. figure:: images/AllWrappersAndTestbench.png
   :width: 764px

   An example PET with all three types of available wrappers and a Test Bench.

In addition to the complex analyses that can be performed using
Test Benches as you saw in the previous :ref:`test_benches` chapter,
virtually any external execution tool can be integrated using one of
the provided wrappers. For example, we have used the Python Wrapper to
drive proprietary executables and legacy codes written in COBAL, Fortran, and
C from the command line.

For examples of PETs with different analysis blocks see the
`Analysis Blocks <https://github.com/metamorph-inc/openmeta-examples-and-templates/tree/master/analysis-blocks>`_
project in the
`Openmeta Examples And Templates <https://github.com/metamorph-inc/openmeta-examples-and-templates>`_
repository.

.. toctree::
   :maxdepth: 1
   :caption: Analysis Blocks

   test_benches_as_analysis_blocks
   excel_wrappers
   python_wrappers
   matlab_wrappers
   constants_blocks
