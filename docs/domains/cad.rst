.. _cad:

Computer-Aided Design (CAD)
===========================

Computer-Aided Design (CAD) allows for a 3-D representation of the components
in a design.

OpenMETA currently has a single CAD integration: PTC Creo Parametric 3.0.
Creo is a CAD package that gives the user the ability to construct parametric
parts. Within OpenMETA these parts can be composed and their parameters driven
from an OpenMETA model.

PTC Creo Parametric 3.0
-----------------------

Congfiguration Instructions
~~~~~~~~~~~~~~~~~~~~~~~~~~~

1. Install Creo.
2. ...

.. ADD: Instructions on how to configure Creo for used
   with OpenMETA.

CAD Models
~~~~~~~~~~

The precise three-dimensional geometry of a META component is expressed
with a **CAD model**. Key connection points on the component are marked
with *datums*, which are joined with the datums of other connected
components to generate a three-dimensional model of a system. By relying
on these connection points, instead of relative-position offsets, a
complex component can be automatically composed out of different types of 
other simpler components.

CAD model elements within META components contain references to any
datums *(planes, axis, coordinate systems, and points)* that are
required to define interfaces between components. The block can also
contain parameters, which can be used to change the geometry of the
model based on values given in a design.

.. figure:: images/01-01-led-cad-model.png
   :alt: LED CAD model

   *CAD model of a Light-Emitting Diode (LED)*

Examples
~~~~~~~~

With Creo installed, check out the :ref:`spacecraft_model` walkthrough.
