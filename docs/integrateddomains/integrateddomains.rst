.. _integrateddomains:

Integrated Domains
==================

.. note:: Take content from old 'Chapter 7: Domains'.

General
-------

Component Model Assets
~~~~~~~~~~~~~~~~~~~~~~

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

SystemC Models
^^^^^^^^^^^^^^

`SystemC <http://www.accellera.org/downloads/standards/systemc>`__ is a
versatile discrete event simulation framework for developing event-based
behavioral models of hardware, software and testbench components. These
models are captured in C++ using the SystemC class library, utility
functions and macros. The library also contains a discrete event
scheduler for executing the models. The models can be captured at
arbitrary levels of abstraction, but *cycle-accurate* and
*transaction-level* (TLM) models are the most typical. Due to the
discrete event model of computation, the simulation is executed in
*logical* time and is not tied to the wall clock (*real time*). Each and
every event in SystemC has a well-defined timestamp in the simulated
clock domain. Concurrency is a simulated concept, the actual execution
of the simulator engine is single threaded by design. In the Ara
development ecosystem, SystemC is well suited for capturing and
experimenting with new peripheral modules, bus protocols and embedded
software (*firmware*) and to validate interaction patterns among these
and with the applications running on the core platform.

RF Models
^^^^^^^^^

An RF model of a META component comprises three-dimensional geometric
shapes associated with materials of different electromagnetic
properties. The META tools currently support models that are in the
CSXCAD format supported by the **OpenEMS** simulator.
`OpenEMS <http://openems.de>`__ uses a finite-difference time-domain
(FDTD) approach, where the problem space is first discretized along a
rectilinear grid, then the electric (E) and magnetic (H) fields are
cyclically updated in each timestep, for each grid point, using a
finite-difference approach. As the direct simulation output is the
*time-domain* evolution of the fields, frequency-domain characteristics
of the model are deduced from the Fourier-transformed response to an
adequately constructed excitation signal. In the context of the Ara
module development, OpenEMS allows us to evaluate antenna performance
(Zin, S11, directivity, etc.) and estimate the maximum SAR prior to
production and FCC regulatory testing.

.. figure:: images/01-inverted-f.png
   :alt: Stripline antenna model in OpenEMS

   *RF model of a 2.4 GHz Inverted-F antenna*


Electronics Design Automation (EDA)
-----------------------------------

Electronics Design Automation (EDA)

Integrated Tools
~~~~~~~~~~~~~~~~

Autodesk Eagle [#eagle]_ is a free schematic and layout editor for electronic circuit design and the creation of Printed Circuit Boards (PCBs). When used in conjunction with OpenMETA, the user is able compose designs using a rich library of components in the tools and automatically generate ``.brd`` files from the model.

We recommend using the latest version of the EAGLE tools that can
be found on the `Autodesk Eagle Download Page
<https://www.autodesk.com/products/eagle/free-download>`_.

Schematic Models
~~~~~~~~~~~~~~~~

**Schematics** represent the elements of an electrical system using
abstracted symbols of components. Schematics excel at providing a clean,
efficient view of an electronic system. In electronic design the
location of the symbols in a schematic do not necessary correlate with
the physical location of the components. META currently uses **Eagle**
models to represent a component's schematic model.

.. figure:: images/01-eagle-model-of-diode.png
   :alt: Diode Model in EAGLE

   *EAGLE Model of a Light-Emitting Diode (LED)*

Computer-Aided Design (CAD)
---------------------------

Computer-Aided Design (CAD) allows for a 3-D representation of the components
in a design.

Integrated Tools
~~~~~~~~~~~~~~~~

OpenMETA currently has a single CAD integration: PTC Creo Parametric 3.0.
Creo is a CAD package that gives the user the ability to construct parametric
parts. Within OpenMETA these parts can be composed and their parameters driven
from an OpenMETA model.

CAD Models
~~~~~~~~~~

The precise three-dimensional geometry of a META component is expressed
with a **CAD model**. Key connection points on the component are marked
with *datums*, which are joined with the datums of other connected
components to generate a three-dimensional model of a system. By relying
on these connection points, instead of on relative-position offsets, a
component can be composed with many different types of components
automatically.

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

SPICE Circuit Simulation
------------------------



Ngspice [#ngspice]_ is an open-source SPICE simulator. This engine which ships
installed with the OpenMETA tools allows for the execution of a
time-step-based circuit simulation.

NGSPICE Models
~~~~~~~~~~~~~~

**SPICE** is a time-tested simulation tool for electronic circuits.
`NGSPICE <http://ngspice.sourceforge.net>`__ is an open-source version
that is used by the META tools. META components can use **NGSPICE**
models to represent their electrical behavior. They can do this by
either parameterizing common SPICE primitives or by providing their own
implementations in standalone files.



Footnotes
---------

.. [#eagle] https://www.autodesk.com/products/eagle/overview
.. [#ngspice] http://ngspice.sourceforge.net/
