.. _testbench_basics:

Test Bench Basics
=================

Introduction
------------

The *Test Bench* is the OpenMETA model object used to define how an
OpenMETA system model should be translated into an executable
domain-specific model, analysis, or simulation.
When a Test Bench is executed with the :ref:`master_interpreter`, a job is
created, ready to be processed by the :ref:`resultsbrowser`.

For example, a Modelica Test Bench defines how a system model composed of
OpenMETA components, each containing a Modelica model, should be translated
into a Modelica system model that includes the complete set of equations.
When the job is passed to the Results Browser and executed, the Modelica
solver consumes the system model and returns the results.

In the OpenMETA Tools, a **Test Bench** is a virtual environment used to run
experiments on a system. Test benches define a testing context for a
system, providing sources of stimulus and loading elements that gather
experimental data. In OpenMETA, a user can dictate the test conditions for
their experiment themselves or choose from a library of pre-configured
test benches that represent design requirements or other criteria. In
addition to the configuration of test conditions, the user can customize
the data gathered through the execution of a test bench.

While most test benches are used to perform analyses, other test benches
perform design services for the user. For example, a user that has
completed a OpenMETA design can run a test bench to auto-generate a
schematic of their design. Additionally, the user can run a CAD assembly
test bench to build a 3D model of their design.

A common use for test benches is the evaluation of system performance.
In this application, a test bench is an executable specification of a
system requirement. The parts of a Test Bench include:

-  **Test Drivers:** Replicating the intended stimulus to the system.

-  **Wraparound Environment:** Providing the interfaces at the periphery
   of the system such as the external humidity, temperature, etc.

-  **Metrics Evaluation:** Measurements of the system properties
   converted into a value of interest. The metrics are also tied to
   requirements, which can convert the metric to a design “score”.

-  **System Under Test:** Either a single design or a design space (many
   designs). In the case of a design space, the test bench can be
   applied over the entire set of feasible designs.

.. figure:: images/01-04-example-test-bench.png
   :alt: example test bench

   *An Example Test Bench*

   **NewDC\_\_SimpleLEDCircuit** is the **System Under Test**, while the other
   **Test Components** provide the **Wraparound Environment**.


Systems Under Test
------------------

Test Benches always include a *TopLevelSystemUnderTest*, which references
the model in our project that is the object of testing. If there are
parameters exposed from this assembly, they will be visible in the Test
Bench. You can in turn create Parameters in the Test Bench itself and use
these to drive the parameters of the *TopLevelSystemUnderTest*.

Workflows
---------

A *Workflow* is an OpenMETA model object used to define steps necessary to
properly run an analysis. These steps may include *Tasks* to call 
model *Interpreters* at the time of job creation, *Executions Tasks* to
define scripts to be run at the time of job execution, or both.
