___
## Running Tests on Multiple Designs
Now that we've created a single _design space_ that yields 9 potential _design points_, we can use test benches to evaluate them more quickly. By configuring a test bench against a _design space_, we can easily run that test bench against any or all of the potential _design points_.

We will now run three test benches on our ***NewDC\_\_SimpleLEDCircuit*** and view all the results in the Project Analyzer. You are already familiar with [configuring any test bench](@ref configuring-a-test-bench) and the only difference is here you will be creating a reference of ***NewDC\_\_SimpleLEDCircuit*** instead of ***SimpleLEDCircuit***.

_Note: You still want to choose TopLevelSystemUnderTest as a reference role type._

### Generating Schematics
1. Configure the **PlaceAndRoute\_1x2** Test Bench with our new design space, ***NewDC\_\_SimpleLEDCircuit***.
2. Click the **Master Interpreter** icon.
3. Select all configurations and Check _Post to META Job Manager_.
4. Click OK.

![Place and Route test](images/03-06-design-space-eagle.png)

Right-click on any job and choose Show in explorer to browse the generated artifacts.  You can open up the `Schema.sch` file as before to view the resulting **Eagle** file. 

### Performing a SPICE Analysis
Now that we've shown how to generate schematics from all _design points_ in a _design space_, let's perform some analysis. We'll perform circuit analysis via **SPICE** as before. 

1. Configure the **SPICETest** test bench with our new design space, ***NewDC\_\_SimpleLEDCircuit***.
2. Make the same connections as before with **VCC** and **GND**.
3. Click the **Master Interpreter** icon.
4. Select all configurations and Check _Post to META Job Manager_.
5. Click OK.

<!-- ![Design Space Spice Test](images/03-06-ds-spice-test.gif) -->

### Performing Parts Cost Estimation
The META tools include a _Test Bench_ for estimating the parts cost of a design. For a given design, it will check current part prices based on the part quantity and specified number of designs.

1. Configure the **CostEstimation** test bench with our new design space, ***NewDC\_\_SimpleLEDCircuit***.
2. Click the **Master Interpreter** icon.
3. Select all configurations and check _Post to META Job Manager_.
4. Click OK.
