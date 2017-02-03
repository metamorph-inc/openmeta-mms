___
##  Continuous Integration

OpenMETA models can be integrated with Continuous Integration (CI) methods to provide automatic testing for regressions. OpenMETA includes a utility script that automatically runs Test Benches and checks the resulting Metric values against target values provided by the model author.

### Configuring the Jenkins job
For this example, we'll use the Jenkins CI platform. We'll create a new Freestyle project that polls the repository for changes. Our test model will be the open-source [<b>Spacecraft Study</b>](https://github.com/metamorph-inc/openmeta-spacecraft-study) model project available on GitHub.

![](images/12-04-JobName.png)

In the Source Code Management section, we'll configure the job to poll the master branch of the repo on a five-minute interval.

![](images/12-04-SCM.png)

Next, we need to configure Jenkins to run the utility script that will automatically run the Test Benches in the OpenMETA project.

Add an **Execute Windows batch command** build step to the Jenkins job.

~~~~~~~~~{.cmd}
"C:\Program Files (x86)\META\bin\Python27\Scripts\python" "C:\Program Files (x86)\META\bin\RunTestBenches.py" --max_configs 2 CyPhy_Model\ExampleSat_3_1.xme -- -s --with-xunit
exit /b 0
~~~~~~~~~

Let's break down this command:
Part | Description
---- | -----------
`"C:\Program Files (x86)\META\bin\Python27\Scripts\python"` | Use OpenMETA's Python environment
`"C:\Program Files (x86)\META\bin\RunTestBenches.py"` | This is the automation script
`--max_configs 2` | If a Test Bench has a Design Space as its System Under Test, choose at most 2 configurations to test
`CyPhy_Model\ExampleSat_3_1.xme` | This is the path to the OpenMETA model to test
`--` | Parameters after this mark are passed to the Python *nose* testing framework
`-s` | Don’t capture stdout (any stdout output will be printed immediately)
`--with-xunit` | Produce a JUnit-compatible XML file as output
`exit /b 0` | This causes a build with out-of-spec values to be marked as "Unstable". Otherwise, it will be marked as "Failed", which makes it hard to distinguish from cases where the tests could not run.

We must also add a **Publish JUnit test result report** Post-build Action to the Jenkins job, telling it to grab the `nosetests.xml` test report.

![](images/12-04-BuildAndPostBuild.png)

Test reports in Jenkins include a list of tests, markings for those passing and failing, and duration measurements for the time it took to conduct the test. For failed tests, status messages indicate the nature and reason for failure.

![](images/12-04-TestReport.png)

### Configuring the OpenMETA Model
By default, the automation script will run each Test Bench and inform Jenkins if any of them fail to run. However, the model creator can add more detail, setting target and threshold values for Test Bench parameters. If the Test Bench results fail to meet these targets, the test report will mark them as failing tests.

This can be useful for regression-testing the _performance_ of a design, warning when performance has been compromised by a new model change.

Using our [<b>Spacecraft Study</b>](https://github.com/metamorph-inc/openmeta-spacecraft-study) example, we'll add a **Metric Constraint** object to our **PowerAnalysis** Test Bench. By connecting it to the *minBusVoltage* Metric, then setting it to have **TargetType "MustExceed"** and **TargetValue** of **14V**, we tell the testing script to mark the Test Bench as *failed* if the calculated *minBusVoltage* drops below 14V due to a model change.

![](images/12-04-MetricConstraint.png)

