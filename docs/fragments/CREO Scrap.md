### 6.6: Inspecting a META2CAD Assembly in Creo

	

After using the META2CAD interpreter, the next step is often to view the assembly produced by the interpreter in a CAD program, i.e. PTC Creo. Some issues with the model only become apparent when the model is opened this way. However, it is easy to make a mistake when opening the model, which may cause it to reference the wrong set of parameterized parts. This can give the impression that the model was not assembled correctly in META, when the model is actually fine.

#### 6.6.1: Parametric parts appear incorrectly

Depending on how a model is opened in Creo, some parameterized components in the assembly may be shuffled around to create a geometrically different result. In Figure 7.6.1, a drivetrain assembly with driveshafts of parameterized length is assembled as an example.

->![](reference_68.png)<-

->*Figure 6.6.1a: An assembly with the six driveshafts in correct positions*<-

->![](reference_69.png)<-

->*Figure 6.6.1b: The same assembly, with the driveshafts shuffled*<-

Either of these assemblies may be created from the same set of results, depending on the way the model is opened in Creo. To ensure it is opened correctly, follow these steps:

* Open Creo Parametric 2.0

* Click Select Working Directory

->![](reference_70.png)<-

->*Figure 6.6.2: Working Directory*<-

* Select the results folder and click **OK**

->![](reference_71.png)<-

->*Figure 6.6.3: Results Folder*<-

* Click **Open**

->![](reference_72.png)<-

->*Figure 6.6.4: Opened Results Folder*<-

* Open the *.asm* file for the main assembly

->![](reference_73.png)<-

->*Figure 6.6.5: .asm File*<-

* Non-parametric parts will be missing; right-click one of them in the Model Tree, and select **Retrieve Missing Component**

->![](reference_74.png)<-

->*Figure 6.6.6: Retrieve Missing Components*<-

* Navigate to the project’s CAD reference directory and open the correct part file

->![](reference_75.png)<-

->*Figure 6.6.7: Opening Reference File*<-

Other broken part references will correct themselves as they are found in the CAD reference directory. Note that when viewing the Model Tree, every part and sub-assembly will be highlighted in red, even though some of those parts are parametric and have already been found in the results folder.

If you wish to view a different results set in the same Creo session, click **Close**, then go to **File -> Manage Session -> Erase Not Displayed**, and click **OK**. Repeat the bulleted procedure, using the folder for the next results set as the Working Directory.

->![](reference_76.png)<-

->*Figure 6.6.8: The Close button*<-

->![](reference_77.png)<-

->*Figure 6.6.9: The Manage Session section, with “Erase Not Displayed” and “Select Working Directory”*<-
