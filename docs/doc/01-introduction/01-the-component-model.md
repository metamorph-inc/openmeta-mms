## The Component Model

In META, a **Component** is a self-contained, reusable entity that can be used a design. A component can be anything from an electrical resistor or light-emitting diode (LED) to a diesel engine or rubber tire. A **Component Model** captures everything that we know about a component. Component models are self-contained and can include schematic, geometric, physical, and behavioral models of the component. Additionally, component models can provide data sheet and interface information. A META component model is intended to be reusable across all META designs.

In a traditional design process, designs are constructed independently within many design and analysis tools by experts in those domains. META designs centralize the composition of these models, allowing engineers to assess design choices with all of the data available.


### What's Inside a Component Model?

Inside a component, you will typically see a schematic model, CAD model, and Modelica model, along with many properties, connectors, and other essential parts. The component model captures several qualities of the physical component, including its geometry (3-dimensional CAD model), its dynamic behavior (an acausal power flow and transfer function), and its numerical properties (characteristics such as weight). The component also has connectors, which allow connection to other components.

![Image](images/LED_Diagram_lores.png)

The META Component Model aggregates these various models, providing a single set of properties and connectors. When two components are composed via these connectors, they are joined in many analysis domains at once.


### Component Model Assets

#### Schematic Models

**Schematics** represent the elements of an electrical system using abstracted symbols of components. Schematics excel at providing a clean, efficient view of an electronic system. In electronic design the location of the symbols in a schematic do not necessary correlate with the physical location of the components. META currently uses **Eagle** models to represent a component's schematic model.

![Diode Model in EAGLE](images/01-eagle-model-of-diode.png)

*EAGLE Model of a Light-Emitting Diode (LED)*


#### NGSPICE Models
**SPICE** is a time-tested simulation tool for electronic circuits. [<b>NGSPICE</b>](http://ngspice.sourceforge.net) is an open-source version that is used by the META tools. META components can use **NGSPICE** models to represent their electrical behavior. They can do this by either parameterizing common SPICE primitives or by providing their own implementations in standalone files.


#### Modelica Models
The dynamics of a system are expressed in the Modelica language, which uses a mix of *causal* relationships (directional input or output is assigned to each port) and *acausal* relationships (power flows in either direction based on the context, as in most physical systems). Modelica models are used to simultaneously model components of multiple engineering domains such as electrical, hydraulic, mechanical, and thermal. For the purposes of Ara modules, we will be focusing on the power and thermal abilities provided by the Modelica solver.

Within META components are Modelica models that contain a set of Modelica ports and parameters. These ports represent the dynamics interfaces for the represented component, while the parameters capture the elements of the model that may be altered.

![Diode Model in Modelica](images/01-diode-in-modelica.png)

*Modelica Model of a Diode*


#### SystemC Models
[<b>SystemC</b>][systemc] is a versatile discrete event simulation framework for developing event-based behavioral models of hardware, software and testbench components. These models are captured in C++ using the SystemC class library, utility functions and macros. The library also contains a discrete event scheduler for executing the models. The models can be captured at arbitrary levels of abstraction, but *cycle-accurate* and *transaction-level* (TLM) models are the most typical. Due to the discrete event model of computation, the simulation is executed in *logical* time and is not tied to the wall clock (*real time*). Each and every event in SystemC has a well-defined timestamp in the simulated clock domain. Concurrency is a simulated concept, the actual execution of the simulator engine is single threaded by design. In the Ara development ecosystem, SystemC is well suited for capturing and experimenting with new peripheral modules, bus protocols and embedded software (*firmware*) and to validate interaction patterns among these and with the applications running on the core platform.

[systemc]: http://www.accellera.org/downloads/standards/systemc "SystemC - Accellera Systems Initiative"

#### RF Models

An RF model of a META component comprises three-dimensional geometric shapes associated with materials of different electromagnetic properties. The META tools currently support models that are in the CSXCAD format supported by the **OpenEMS** simulator. [OpenEMS][openems] uses a finite-difference time-domain (FDTD) approach, where the problem space is first discretized along a rectilinear grid, then the electric (E) and magnetic (H) fields are cyclically updated in each timestep, for each grid point, using a finite-difference approach. As the direct simulation output is the *time-domain* evolution of the fields, frequency-domain characteristics of the model are deduced from the Fourier-transformed response to an adequately constructed excitation signal. In the context of the Ara module development, OpenEMS allows us to evaluate antenna performance <i>(Z<SUB>in</SUB>, S<SUB>11</SUB>, directivity, etc.)</i> and estimate the maximum SAR prior to production and FCC regulatory testing.

[openems]: http://openems.de "OpenEMS - An open-source FDTD electromagnetic field solver"

![Stripline antenna model in OpenEMS](images/01-inverted-f.png)

<center><i>RF model of a 2.4 GHz Inverted-F antenna</i></center>

#### CAD Models
The precise three-dimensional geometry of a META component is expressed with a **CAD model**. Key connection points on the component are marked with *datums*, which are joined with the datums of other connected components to generate a three-dimensional model of a system. By relying on these connection points, instead of on relative-position offsets, a component can be composed with many different types of components automatically.

CAD model elements within META components contain references to any datums *(planes, axis, coordinate systems, and points)* that are required to define interfaces between components. The block can also contain parameters, which can be used to change the geometry of the model based on values given in a design.

![LED CAD model](images/01-01-led-cad-model.png)

*CAD model of a Light-Emitting Diode (LED)*


#### Properties & Parameters
Components will typically contain a number of different **properties** and **parameters**. Properties and parameters are ways of capturing values that describe components. **Properties** are values that are fixed for a given component and cannot be changed directly by a designer using that component. **Parameters** are values that can be varied by a system designer. For example, in the case of a drive shaft where the designer can have one manufactured to a custom length, the component model for that drive shaft will have length as a parameter.

A property may also be calculated automatically based on the values of other properties or parameters. In the example of a drive shaft, the mass of the drive shaft is calculated from the length.

This extends to the domain models as well. Again in the case of the drive shaft example, the user-selected length can be assigned to a parameter of the CAD model, adjusting the 3D geometry based on the designer's selection. The calculated mass can be assigned to a parameter of the dynamics model, ensuring that the correct inertia is used when simulating its behavior.

![Resistor properties](images/01-01-properties-of-a-resistor.png)

*Properties of a resistor*


#### Connectors
META components also contains **connectors**, which define interfaces across multiple domain models. For the case of an electrical pin connecting to a printed circuit board (PCB), the joining of two connectors can capture the geometry (the center axis and mount plane where the pin and board meet) and the schematic diagram relation (which pins/nets are being joined) at the same time.

In the screenshot below, the connector ***Cathode*** represents both an electrical terminal from the ***SchematicModel*** and an electrical interfaces from the ***ModelicaModel***.

![Connectors in LED model](images/01-01-connectors-in-LED-model.png)

The simplified diagram below abstractly shows the structure of a similar component, with its individual domain-specific interfaces grouped into connectors.

![Image](images/LED_Diagram_lores.png)

### Component Composition
Components are designed to be composed with other components via their **Connectors**. When two component connectors are composed, then their corresponding **Role** elements are also matched, and the **DomainPorts** so mapped will be connected together in a generated domain model.

In the example shown below, two components each have embedded Domain Models of type **ModelicaModel**. They also each feature **Connector** objects that share a common definition. The **role** objects within each **connector** instance are mapped to the **Modelica connectors** of each component's Modelica model. In the generated Modelica model, the corresponding Modelica class representing each component is instantiated, and their connectors are joined by following the _Modelica Connector -> Role -> Connector -> Connector -> Role -> Modelica Connector_ chain from the source META composition.

![Composition Example](images/CompositionExample.png)