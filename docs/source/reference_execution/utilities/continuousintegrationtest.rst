.. _continuousintegrationtest:

Continuous Integration and Testing
==================================
OpenMETA projects can be integrated with Continuous Integration (CI) methods to provide automatic testing for regressions. OpenMETA includes a utility script that automatically runs Test Benches and PETs and checks the resulting Metric values against target values provided by the model author in the form of Metric Constraints.

Configuring the Jenkins job
~~~~~~~~~~~~~~~~~~~~~~~~~~~
For this example, we'll use the Jenkins CI platform. We'll create a new Freestyle project that polls the repository for changes. Our test model will be the open-source `Spacecraft Study <https://github.com/metamorph-inc/openmeta-spacecraft-study>`_ model project available on GitHub.

.. image:: images/ci-job-name.png
   :alt: Job Name
   :width: 600px

In the Source Code Management section, we'll configure the job to poll the master branch of the repo on a five-minute interval.

.. image:: images/ci-scm.png
   :alt: Source Code Management section
   :width: 600px

Next, we need to configure Jenkins to run the utility script that will automatically run the Test Benches and PETs in the OpenMETA project.

Add an **Execute Windows batch command** build step to the Jenkins job.

.. code-block:: bat
   :linenos:

   "C:\Program Files (x86)\META\bin\Python27\Scripts\python" "C:\Program Files (x86)\META\bin\RunTestBenches.py" --max_configs 2 CyPhy_Model\ExampleSat_3_1.xme -- -s --with-xunit
   exit /b 0

Let's break down this command:

#. ``"C:\Program Files (x86)\META\bin\Python27\Scripts\python"``: Use OpenMETA's Python environment
#. ``"C:\Program Files (x86)\META\bin\RunTestBenches.py"``: This is the automation script
#. ``--max_configs 2``: *(optional)* If a Test Bench or PET has a Design Space as its System Under Test, choose at most 2 configurations to test
#. ``CyPhy_Model\ExampleSat_3_1.xme``: This is the path to the OpenMETA project to test
#. ``--``: Parameters after this mark are passed to the Python *nose* testing framework
#. ``-s``: Don’t capture stdout (any stdout output will be printed immediately)
#. ``--with-xunit``: Produce a JUnit-compatible XML file as output
#. ``exit /b 0``: This causes a build with out-of-spec values to be marked as "Unstable". Otherwise, it will be marked as "Failed", which makes it hard to distinguish from cases where the tests could not run.

The Python *nose* testing framework operates by generating a test function for
each design configuration in each Test Bench or PET. The name of each function
is produced using the pattern ``test_<analysis_name>__<config_name>``, where
``<analysis_name>`` is the name of the Test Bench or PET and ``<config_name>``
is the name of the design configuration that is being tested. (The
``<config_name`` will simply be the name of the Component Assembly if the
System Under Test is a Component Assembly.)

These additional arguments can be used after the ``--`` mark to afford more
control over the testing:

- ``-m <pattern>``: Include only the tests with names that match the pattern
  ``<pattern>``.

  - You may supply any number of ``-m`` arguments.
  - Once you've supplied at least one ``-m`` argument, only the tests that
    match all of the provided patterns will be executed.
  - E.g. ``-m CI`` would include any test that has the string
    "CI" in either the name of the Test Bench or PET or the specific
    configuration name to which that test corresponds.
  - Similarly ``-m test_Inertial_and_Geometry`` would include any Test Bench
    or PET that begins with the string *Inertial_and_Geometry*.

- ``-e <pattern>``: Exclude all the tests with names that match the pattern
  ``pattern``.

  - You may supply any number of ``-e`` arguments.
  - E.g. ``-e test_Inertial_and_Geometry`` would exclude any Test Benches or
    PETs that begins with the string *Inertial_and_Geometry*.

- ``-v --collect-only``: List all the tests that conform to the given available tests.

  - This can be useful to use when you are trying to find the right
    combination of ``-m`` and ``-e`` arguments to select the desired
    tests.

Examples of the these different patterns can be found in the
`Continuous Integration <https://github.com/metamorph-inc/openmeta-examples-and-templates/tree/master/continuous-integration>`_
project in the
`Openmeta Examples And Templates <https://github.com/metamorph-inc/openmeta-examples-and-templates>`_
repository.


We must also add a **Publish JUnit test result report** Post-build Action to the Jenkins job, telling it to grab the `nosetests.xml` test report.

.. image:: images/ci-build-and-post-build.png
   :alt: build and post-build
   :width: 600px

Test reports in Jenkins include a list of tests, markings for those passing and failing, and duration measurements for the time it took to conduct the test. For failed tests, status messages indicate the nature and reason for failure.

.. image:: images/ci-test-report.png
   :alt: CI test report
   :width: 400px

Configuring the OpenMETA Project
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
By default, the automation script will run each Test Bench and inform Jenkins if any of them fail to run. However, the model creator can add more detail, setting target and threshold values for Test Bench parameters. If the Test Bench results fail to meet these targets, the test report will mark them as failing tests.

This can be useful for regression-testing the performance of a design, warning when performance has been compromised by a new model change.

Using our  `Spacecraft Study <https://github.com/metamorph-inc/openmeta-spacecraft-study>`_ example, we'll add a **Metric Constraint** object to our **PowerAnalysis** Test Bench. By connecting it to the *minBusVoltage* Metric, then setting it to have **TargetType "MustExceed"** and **TargetValue** of **14V**, we tell the testing script to mark the Test Bench as *failed* if the calculated *minBusVoltage* drops below 14V due to a model change.

.. image:: images/ci-metric-constraint.png
   :alt: metric constraint
   :width: 600px
