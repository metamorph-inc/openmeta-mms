.. _cad:

Computer-Aided Design (CAD)
===========================

Computer-Aided Design (CAD) allows for a 3-D representation of the components
in a design.

.. figure:: images/01-01-led-cad-model.png
   :alt: LED CAD model

   *CAD model of a Light-Emitting Diode (LED)*

.. TODO: replac with better figure:: images/cad_assembly.png

   <Insert image of CAD assembly>

OpenMETA has generic support for integrating CAD tools. This support allows
for OpenMETA components to reference CAD part and assembly files and to expose
parameters, CAD datums, and features of those parts and assemblies. Exposed CAD
datums can then be used to compose CAD assemblies.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   cad_concepts
   cad_creo
   
