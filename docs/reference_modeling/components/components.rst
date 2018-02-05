.. _components:

Components
==========

The **component** is the atomic model construct in OpenMETA, and it serves
as the basis for the multi-domain nature of the tools. Components touch
virtually every other concept in OpenMETA.

What's Inside a Component Model?
--------------------------------

Inside a component, you will typically see one or more domain models
and a number of interfaces exposing parts of the component the containing
environment. For example, you could have a schematic model, CAD model,
and Modelica model, along with many properties, connectors, and other
essential parts. The component model captures several qualities of the
physical component, including its geometry (3-dimensional CAD model),
its dynamic behavior (an acausal power flow and transfer function), and
its numerical properties (characteristics such as weight). The component
also has connectors, which allow connection to other components.

.. figure:: images/LED_Diagram_lores.png
   :align: center

   *Concept drawing of an OpenMETA Component Model of an LED*

The OpenMETA Component Model aggregates these various models, providing a
single set of properties and connectors. When two components are
composed via these connectors, they are joined in many analysis domains
at once.

The figure below shows a simple capacitor component as represented in OpenMETA.

.. _capacitor:

.. figure:: images/capacitor.png
   :align: center

   *OpenMETA Model of a Capacitor*

Domain-Specific Models
----------------------

For each of the domains represented in the component model, there is a
domain-specific model that exposes the necessary features of that domain.

For example, in the :ref:`capacitor` figure above we see that this capacitor has both an EDA and a
SPICE model. The EDA model exposes the two pins which represent the phyiscal
pads of the part footprint on a printed circuit board (PCB) to which other components need to
connect. The SPICE model also exposes the two pins of the component but additionally exposes four values
needed to construct an accurate representation of the capacitor in the SPICE
domain. The **properties** that specify the appropriate values for the SPICE
model as well as a number of other **properties** that describe this
component are described in the next subsection.

Properties & Parameters
-----------------------

Components will typically contain a number of different **properties**
and **parameters**. Properties and parameters are ways of capturing
values that describe components. **Properties** are values that are
fixed for a given component and cannot be changed directly by a designer
using that component. **Parameters** are values that can be varied by a
system designer. For example, in the case of a drive shaft where the
designer can have one manufactured to a custom length, the component
model for that drive shaft will have length as a parameter.

A property may also be calculated automatically based on the values of
other properties or parameters. In the example of a drive shaft, the
mass of the drive shaft is calculated from the length.

This extends to the domain models as well. Again in the case of the
drive shaft example, the user-selected length can be assigned to a
parameter of the CAD model, adjusting the 3D geometry based on the
designer's selection. The calculated mass can be assigned to a parameter
of the dynamics model, ensuring that the correct inertia is used when
simulating its behavior.

.. note:: Insert an image of the drive train component.

Connectors
----------

OpenMETA components also contains **connectors**, which define interfaces
across multiple domain models. For the case of an electrical pin
connecting to a printed circuit board (PCB), the joining of two
connectors can capture the geometry (the center axis and mount plane
where the pin and board meet) and the schematic diagram relation (which
pins/nets are being joined) at the same time.

In the screenshot below, the connector **Cathode** represents both an
electrical terminal from the **SchematicModel** and an electrical
interfaces from the **ModelicaModel**.

.. note:: We need to update this image to include the new *Connector* look.

.. image:: images/01-01-connectors-in-LED-model.png
   :alt: Connectors in LED model

The simplified diagram below abstractly shows the structure of a similar
component, with its individual domain-specific interfaces grouped into
connectors.

Component Composition
---------------------

Components are designed to be composed with other components via their
**Connectors**. When two component connectors are composed, then their
corresponding **Role** elements are also matched, and the
**DomainPorts** so mapped will be connected together in a generated
domain model.

In the example shown below, two components each have embedded Domain
Models of type **ModelicaModel**. They also each feature **Connector**
objects that share a common definition. The **role** objects within each
**connector** instance are mapped to the **Modelica connectors** of each
component's Modelica model. In the generated Modelica model, the
corresponding Modelica class representing each component is
instantiated, and their connectors are joined by following the :menuselection:`Modelica
Connector --> Role --> Connector --> Connector --> Role --> Modelica
Connector` chain from the source OpenMETA composition.

.. figure:: images/CompositionExample.png
   :align: center
   :alt: Composition Example

   *Example of Composition*