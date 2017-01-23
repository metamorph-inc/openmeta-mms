- - -
## Chip Fit
**Location:** `TestBenches / AraTestBenches / ChipFit / (several available)`

This test bench is designed to estimate whether the components in your design will fit onto a given circuit board area. Taking the footprints of the individual components, it will try to find an arrangement that accomodates all parts. It will then report whether or not the elements can fit within the given area.

<i><b>Note:</b></i> The Chip Fit analysis should be used only to confirm that the selected components ***will not*** fit within the given area. A completed design will require more area for signal traces and other considerations. Consider a "no" answer definitive, but consider a "yes" answer to require further investigation.

Test benches are provided for these Ara module sizes:
- Front H
- Front I
- Rear 1x1
- Rear 1x2
- Rear 2x2

Consult the Ara MDK for details on each size.

### Configure
First, you'll need to create a copy of one of the ***chipfit*** test benches. For instructions, refer to the section [Using Test Benches](@ref using-test-benches).

No additional configuration is needed to test a design against one of the standard Ara sizes provided. However, if you would like to test for a different size, change the `boardHeight` and `boardWidth` _Parameters_ accordingly.

### Metrics
<i><b>Note:</b> In the Metric names that follow, <b>(x)</b> and <b>(y)</b> are replaced by the **boardWidth** and **boardHeight**, respectively, as defined above.</i>

| Name | Value Type | Description |
| :--- | ---------- | ----------- |
| fits_(x)_by_(y) | true/false | Whether the components in the design will fit within the given space
| pct_occupied_(x)_by_(y) | real number | The percentage of the available space occupied by the given components.

### Outputs
| Filename | Description |
| :-- | :---------- |
| `showChipFitResults.bat` | Launches a visualizer for the Chip Fit results.

### Chip Fit Visualizer
Running `showChipFitResults.bat` in the results folder will launch the **Chip Fit Visualizer**. This utility will show the best-case packing of the elements of the design.

If any elements failed to pack into the given area, their names will be listed under the **Failed To Place** heading at the right side of the diagram.

![Chip Fit Visualizer](images/11-02-chipfitvisualizer.png)

The **Chip Fit Visualizer** has the following controls:
- Numpad <b>+</b> key: zoom in 
- Numpad <b>-</b> key: zoom out
- **Esc**: quit

### Assumptions
This analysis assumes that each component that will be placed on the printed circuit board (PCB) has an _EDA Model_ (EAGLE Schematic file) associated with it. Information about the component's size is taken from these model files.