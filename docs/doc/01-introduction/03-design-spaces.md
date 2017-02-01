## Design Spaces

In a conventional design process, the designer can only capture a single design architecture, with one choice of components. This method has several drawbacks:

- Requirements often change during the design process, sometimes necessitating a redesign.

- Component and subsystem behavior is discovered during the design process, and the optimal choice of architecture and components may not be apparent until late in the design process.

- The design is applicable to a single target use, and can require substantial rework for other applications.

Instead, META introduces the concept of a _design space_. The design space allows the models to contain multiple alternatives for components and assemblies. Any component or assembly can be substituted for another component or assembly with the same interface.

The META model editor offers a simple syntax for expressing design options. An *alternative* container is used to contain each valid option. The container presents a consistent interface with the outside system, while inside it contains mappings from its interface to the interfaces of each option.

The design space is the set of all options, considering all the alternatives. Consequently, the design space can get very large. While this is a powerful mechanism to expand the range of designs under consideration, a mechanism is also needed to limit the design space to a manageable size. For this purpose, design space *constraints* can be specified and evaluated by the Design Space Exploration Tool (DESERT).

![Design alternatives in GME](images/01-03-design-alternatives-in-gme.png)

*Design Alternatives captured in the META tool*


Design space constraints are simple, static operations & equations that can be specified for the properties and identities of components, as well as assemblies in the design alternative space. Operations on the properties can include total weight and cost, thresholds on a component property, or identity. An example of an identity constraint is that a designer would want all four tires on a truck to be of the same type. 

![Design alternatives in GME](images/01-03-property-constraint.png)

*A property constraint in the META tool*

The DESERT Tool uses scalable techniques to apply these constraints to very large design spaces to rapidly prune the choices to a manageable size. For example, a basic automotive drivetrain model may contain 288 configurations, capturing engine, transmission, and tire options. After applying constraints related to matching mechanical interfaces, the number of configurations drops to 48.

Typical design spaces can easily reach 10 billion configurations. After constraint application, the number of configurations can be reduced to thousands within seconds. Constraints can cover logical concerns, such as power compatibility, or user specified preferences such as "_only consider designs with X brand processors_". The remaining valid designs can be subjected to deeper (and more computationally expensive) analysis.

Design space creation and exploration is a process of expansion and contraction of the design space. It can be a powerful tool to build adaptable and flexible designs. 