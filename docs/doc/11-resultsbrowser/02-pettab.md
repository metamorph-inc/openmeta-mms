## PET Tab

### PET Dataset List Pane

This pane on the left of the PET tab shows the available datasets.

#### Column Headers

**Dataset Types**

* PetResult: These are results that were aggregated from a single execution of the Master Interpreter and are linked together only by sharing the same created timestamp. They are spread out across a number of unique folders in the results directory that correspond directly to the number of configurations that were executed by the master interpreter.
* MergedPet: These are results that that were processed from one or more PetResult entries. The Reslts Browser creates these MergedPet entries by merging the data from the source PetResult(s) and placing that resulting .csv file along with a number of metadata files into a single unique folder in the ./merged folder in the project directory. When a PetResult is launched into the Visualizer, the user will be prompted to enter a name as a MergedPet must be created now before lanuching the Visualizer.
* Archive: These are archives of PET Results that were created with the 'Archive Selected' button [deprecated with OpenMETA 0.11] at the bottom of the PET Dataset Pane. They reside in .csv format in the ./archive folder in the root of the project

**Count**

This shows the number of discrete configurations that were executed for a given PetResult. (For an 'Archive' or 'MergedPet' this will always be 1.)

**Name**

This name is taken from the OpenMETA model at the time of execution by the Master Interpreter

**Time**

This is the time that the PET execution was initiated by the Master Interpreter.

#### Action Buttons

**Merge Selected**

This button will merge and archive the selected datasets into a unique folder in the ./merged folder in the project directory that includes a .csv of the aggregated data as well as metadata about the dataset.  

**Analyze Selected with Tool**

This button is under development; more documentation is forthcoming.

**Open Selected in Visualizer**

This button will launch the visualizer with selected (checked) dataset(s) after prompting the user to created a MergedPet. If more than one is selected it will attempt to merge them. If none are selected, it will launch the highlighted dataset.

### PET Details Pane

This pane shows details about the currently-highlighted dataset in the PET Dataset Pane. The displayed information in the header includes:

* Name of the PET
* Location of the PET in the OpenMETA model
* Time of execution
* Hyperlink to the project .mga
* Count of the individual points sampled in this PET
* Count of the discrete configurations evaluated

Below the header is a summary of the dataset. Each of the DesignVariables and Objectives are listed along with the minimum, average, and maximum values represented in the dataset.
