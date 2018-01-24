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

.. _interpreters:

Interpreters
------------

Interpreters are used to transform OpenMETA model object into executable jobs
that are useful for evaluating, verifying, or ranking different
designs in the project.
For example, the CyPhy2Schematic Interpreter is capable of generating a
SPICE circuit file from a composed system of electronics components. 

.. _master_interpreter:

Master Interpreter
~~~~~~~~~~~~~~~~~~

Generally the **Master Interpreter** is used to execute a Test Bench.
Running the Master Interpreter results in the creation of a new output
folder in the ``results\`` folder of the project directory, the execution
of all tasks in the referenced workflow, and (optionally) the generation of
a *job* that is passed to the :ref:`resultsbrowser` and executed.

Jobs
----

An OpenMETA *job* is a single directory and associated metadata that is 
passed to the Results Browser from the Master Interpreter.
Jobs can be executed locally by the Results Browser or sent to another machine
for execution using the :ref:`remote_execution` capability of the Results
Browser.

.. _job_labels:

Job Labels
~~~~~~~~~~

When the Master Interpreter creates a job, it tags the job with labels
that specify what environment is necessary for the execution of the job.
The list below details the labels that are assigned when a Test Bench
workflow includes each of the interpreters:

-  **Default:** "Windows14.13"
-  **CyPhy2CAD:** "Creo, CADCreoParametricCreateAssembly.exev1.4,
   Windows14.13"
-  **CyPhy2Modelica:** "py_modelica14.13, OpenModelica__latest_"
-  **CyPhyCADAnalysis:** "Creo, CADCreoParametricCreateAssembly.exev1.4,
   CyPhyCADAnalysis14.13"
-  **CyPhyPET:** Takes labels from the interpreters used in each of its
   constituent test benches.
-  **CyPhyPrepareIFab:** "Creo, CADCreoParametricCreateAssembly.exev1.4,
   Windows14.13"
-  **CyPhyReliabilityAnalysis:** "Windows14.13"
-  **CyPhySOT:** Takes labels from its interpreters.
-  **CyPhy2CADPCB:** "Visualizer"
-  **CyPhy2MfgBom:** "Windows14.13"
-  **CyPhy2PCBMfg:** "Windows14.13"
-  **CyPhy2RF:** "RF"
-  **CyPhy2Schematic:** "Schematic"
-  **CyPhy2Simulink:** "Simulink"