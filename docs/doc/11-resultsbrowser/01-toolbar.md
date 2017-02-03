<img src="images/pettab.png" alt="Toolbar and PET Tab" style="width: 800px;"/>

## Toolbar

**New Window**

This button will open a new instance of the Results Browser.

**Select Results Folder**

This button allows you to specify a new working directory for the Results Browser. This directory must include at a minimum a 'results' folder with some results already generated.

**Refresh**

This button will force a refresh of the PET Tab and Test Benches Tab results lists. A refresh happens automatically at the conclusion of each active job.

**Cleanup**

This button acts like the 'Move to Recycle Bin' button in Windows. When you delete a PetResult, Archive, or TestBenchResult, they are only removed from the index while the raw data is still left on disk; this is done as a safety precaution against accidental deletion.

The cleanup button collects all the deleted folders and moves them into the '_deleted' folder within the project directory. If you want to totally delete old data, you can remove this folder after using the Cleanup button.