## Modeling Systems

The first step in designing a system in META is creating a model of the system. A model is defined as an abstract representation of the design. A model is *abstract* if it does not contain all details about the system, but contains sufficient detail to express design choices with a minimal amount of effort. This level of detail is controlled by the designer, allowing a rapid definition of conceptual designs, with addition of detail as the design is refined. The META Language has been designed to strike a compromise between the conceptual and detailed models. These compromises will be clarified as we review the language and tools.

META emphasizes a component-based design methodology. Therefore, following the previous section's discussion of the modeling of a component, we will describe component connectivity, testing models, and design spaces.


### The Component Assembly
Components can be combined into a system or subsystem description by creating a Component Assembly Model. Assemblies are combinations of components that implement a desired function or behavior. For example, the subsystem could produce torque to create acceleration of a vehicle, or it could produce air flow to cool a heat exchanger.

In META models, component assemblies are built by creating references to one or more components and then creating relationships between their interfaces. 

Assemblies may be *nested*, that is, assemblies may contain other assemblies. Assemblies may have externally visible ports to allow connections to flow across subsystem boundaries.
