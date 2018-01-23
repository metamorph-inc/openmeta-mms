.. _testbench_basics:

Test Bench Basics
=================

Introduction
------------

The *Test Bench* is the OpenMETA model object used to define an execution
analysis. The Test Bench construct allows us to define how an OpenMETA
model should be used to generate an executable job. When a Test Bench
is executed with the `Master Interpreter`_, a job is created, ready to be
processed by the :ref:`resultsbrowser`.

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

Interpreters
------------

Interpreters are used to transform OpenMETA model object into executable jobs
that are useful for evaluating, verifying, or ranking different
designs in the project.
For example, the CyPhy2Schematic Interpreter is capable of generating a
SPICE circuit file from a composed system of electronics components. 

Master Interpreter
~~~~~~~~~~~~~~~~~~

The **Master Interpreter** is used to execute a Test Bench.
Running the **Master Interpreter** results in the creation of a new output
folder in the ``results\`` folder of the project directory, the execution
of all tasks in the referenced workflow, and (optionally) the generation of
a *job* that is passed to the :ref:`resultsbrowser` and executed.
