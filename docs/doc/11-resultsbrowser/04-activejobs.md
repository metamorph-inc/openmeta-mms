## Active Jobs Tab

<img src="images/activejobs.png" alt="Active Jobs" style="width: 800px;"/>

### Active Jobs List Pane

This pane lists all the active jobs.

#### Column Headers

**Title**

The title of the job, as generated at the time of execution by the Master Interpreter.

**Test Bench Name**

This name is taken from the OpenMETA model at the time of execution by the Master Interpreter.

**Working Directory**

This the directory that the job is executing or was executed in. You can open this directory by right-clicking a job and selecting 'Open in Explorer' or by highlighting the job and clicking 'Open Selected Job in Explorer.'

**Status**

This is the current status of the job, according to the Job Manager. The possible values are as follows:

* Succeeded: The job was executed and finished without errors.
* Failed: The job was executed, but the job was aborted or an error was encountered.
* Running: The job is currently being executed.
* In Queue: The job is waiting to be executed because the maximum number of simultaneous jobs has already been met.

**Run Command**

This is the command that is called by the job executor to invoke the job.

#### Action Buttons

**Abort Selected Job**

This button will kill the selected job (or PET?).

**Open Selected Job in Explorer**

This button will open Windows Explorer at the location of the execution directory for the highlighted job.
