.. _modelica:

Modelica
================

OpenModelica is an open-source Modelica-based modeling and simulation environment intended for industrial and academic usage. META uses OpenModelica for physics simulations, including mechanical, thermal, and power analysis.

Installation
^^^^^^^^^^^^

1. Download `OpenModelica <https://build.openmodelica.org/omc/builds/windows/releases/1.9.1/beta2/OpenModelica-1.9.1-Beta2-revision-19512.exe>`_.
2. Run the executable.

Modelica Models
^^^^^^^^^^^^^^^

The dynamics of a system are expressed in the Modelica language, which
uses a mix of *causal* relationships (directional input or output is
assigned to each port) and *acausal* relationships (power flows in
either direction based on the context, as in most physical systems).
Modelica models are used to simultaneously model components of multiple
engineering domains such as electrical, hydraulic, mechanical, and
thermal. For the purposes of Ara modules, we will be focusing on the
power and thermal abilities provided by the Modelica solver.

Within META components are Modelica models that contain a set of
Modelica ports and parameters. These ports represent the dynamics
interfaces for the represented component, while the parameters capture
the elements of the model that may be altered.

.. figure:: images/01-diode-in-modelica.png
   :alt: Diode Model in Modelica

   *Modelica Model of a Diode*
