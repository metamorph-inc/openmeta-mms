.. _master_interpreter:

Master Interpreter
==================

.. note::

   This chapter should describe:

   #. That Master Interpreter is the means by which jobs are created.
   #. What the options in the prompt mean. 


Generally the **Master Interpreter** is used to execute a Test Bench.
Running the Master Interpreter results in the creation of a new output
folder in the ``results\`` folder of the project directory, the execution
of all tasks in the referenced workflow, and (optionally) the generation of
a *job* that is passed to the :ref:`results_browser` and executed.

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