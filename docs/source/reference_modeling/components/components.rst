.. _components:

Components
==========

The **component** is the atomic model construct in OpenMETA, and it serves
as the basis for the multi-domain nature of the tools. Components touch
virtually every other concept in OpenMETA.

What's Inside a Component Model?
--------------------------------

Inside a component, you will typically see one or more domain models
and a number of interfaces exposing parts of the component to the containing
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

   *OpenMETA Component Model of a Capacitor*

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
interface from the **ModelicaModel**.

.. note:: We need to update this image to include the new *Connector* look.

.. image:: images/01-01-connectors-in-LED-model.png
   :alt: Connectors in LED model

The simplified diagram below abstractly shows the structure of a similar
component, with its individual domain-specific interfaces grouped into
connectors. For more information on how *Connectors* are used to compose
OpenMETA *Components*, visit the :ref:`component_composition` section of the
next chapter.

Managing Complexity
-------------------

As the size and complexity of a project grow, the number of components and
difficulty of maintaining them also increases. To ease the task of component
management, OpenMETA supports *component references*, *component
instantiation*, and *component class inheritance*.

Component References
~~~~~~~~~~~~~~~~~~~~

Components can be added to CAs and DCs as references.
This allows us to use the exact same component in multiple places.
Without references, it would be necessary to update the component everywhere
it is used when a change was made to its definition.
We recommend keeping all the components in the project confined to
Components Folders and using components references everywhere that component is
used.
This reduces the size of model?

Instantiation
~~~~~~~~~~~~~

You can create instances of components in CA's, DCs, <what else?>.
Offers the same benefits of references, but keeps the object in an
expanded form.

Class Inheritance
~~~~~~~~~~~~~~~~~

Components can be subclasses of other Components.
This is useful for managing different classes of components that share
many of the same attributes.
When model objects are added to the base class components, they are
automatically added to all derived component classes.
