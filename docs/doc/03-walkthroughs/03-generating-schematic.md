___
## Generating a Schematic

<!--
###SECOND VIDEO
1. Do everything from to the end of "performing circuit analysis" section.
2. "...at this point you can jump ahead to the section on Adding Design Space Concepts."
-->

The Metamorphosys tools support generating schematics from the **Eagle** CAD design software. For this type of analysis we will use the ***PlaceAndRoute\_1x2*** test bench, which generates manufacture-ready files of our component assembly for a 20mm x 40mm printed circuit board.

### Configuring a Test Bench

1. In your **GME Browser**, expand the ***TestBenches -> ARA TestBenches*** folders.
2. Locate the ***PlaceAndRoute\_1x2*** test bench.  
3. Double-click it to open it. 
4. Create a reference of ***SimpleLEDCircuit*** in the **Test Bench** by copying and pasting as a *reference* using the [same method as before](@ref populate-the-component-assembly).
6. Select the Reference role type: _TopLevelSystemUnderTest_.

_This is the same procedure for instantiating a design within any **GME Test Bench**._

<!-- ![Empty GenerateSchematic test bench](images/03-02-generate-schematic-tb-first-open.gif) -->

### Running a Test Bench
This is done by simply running the **Master Interpreter**. You can find it on the **GME Toolbar**:

![Master Interpreter on toolbar](images/03-02-master-interpreter-on-toolbar.png)

1. Click the **Master Interpreter** icon.
2. Make sure **Post job to META manager** is checked.
3. Once your job successfully completes and turns green, right-click it and select Show in explorer.

In the folder, you will find a number of different files. One of them is the generated Eagle schematic: **schema.sch**. Double-click it and open it with **Eagle**.

_Note: If this is your first time using Eagle, a dialouge box might ask how you want to run Eagle without a license. Click run as Freeware._

![Generated Eagle Schematic](images/03-03-schema-sch.png)

You'll see the green LED and 1k resistor that we selected. You may also notice that the two parts are not connected with lines, but instead with _nets_. Generated schematics will not include lines, but will instead use nets like this that identify any number of pins that are connected together.

_Note: If your job does not execute properly, send us a note at **beta\@metamorphsoftware.com**._
