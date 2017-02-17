## PET Tab

### PET Dataset List Pane

This pane on the left of the PET tab shows the available datasets.

#### Column Headers

**Dataset Types**

* Archive: These are archives of PET Results that were created with the 'Archive Selected' button at the bottom of the PET Dataset Pane.
* PetResult: These are results that were aggregated from a single execution of the Master Interpreter.

**Count**

This shows the number of discrete configurations that were executed for a given PetResult. (For an 'archive' this will always be 1.)

**Name**

This name is taken from the OpenMETA model at the time of execution by the Master Interpreter

**Time**

This is the time that the PET execution was initiated by the Master Interpreter.

#### Action Buttons

**Archive Selected**

This button will merge and archive the selected datasets into a single .csv file and save this file to the 'archive' folder in the project directory.  

**Analyze Selected with Tool**

This button is under development; more documentation is forthcoming.

**Open Selected in Visualizer**

This button will launch the visualizer with selected (checked) dataset(s). If more than one is selected it will attempt to merge them. If none are selected, it will launch the highlighted dataset.

### PET Details Pane

This pane shows details about the currently-highlighted dataset in the PET Dataset Pane. The displayed information in the header includes:

* Name of the PET
* Location of the PET in the OpenMETA model
* Time of execution
* Hyperlink to the project .mga
* Count of the individual points sampled in this PET
* Count of the discrete configurations evaluated

Below the header is a summary of the dataset. Each of the DesignVariables and Objectives are listed along with the minimum, average, and maximum values represented in the dataset.
