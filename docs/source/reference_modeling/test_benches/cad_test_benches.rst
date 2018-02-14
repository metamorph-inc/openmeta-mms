.. _cad_test_benches:

================
CAD Test Benches
================

A CAD Test Bench can be used to
build a CAD assembly from an OpenMETA Component Assembly,
calculate certain metrics from the generated assembly,
and export the complete assembly in a number of different formats
(step, igs, etc).

.. figure:: images/cad_test_bench_example.png

Build and Configure
~~~~~~~~~~~~~~~~~~~

As a minimum you must:

-  Add a *WorkflowRef* to a *Workflow* with a "CyPhy2CAD" *Task*.
-  Add the desired component assembly as the TopLevelSystemUnderTest (TLSUT).

Additionally you may:

-  Add a *CADComputationComponent*, add desired metrics within it, and
   wire those values to Metrics in the Test Bench.
-  Add Parameters that can be wired into a TestInjectionPoint reference.
