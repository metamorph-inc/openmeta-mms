.. _eda:

Electronic Design Automation (EDA)
===================================

Electronic Design Automation (EDA) tools enable the design and manufacture of
electronic systems and integrated circuits. [1]_ In the integration of EDA tools
with OpenMETA we all the capture of `Schematic Models`_ for each of the
components used in a model and expose the pins of the component to be used
for the composition in a design.

Autodesk Eagle
~~~~~~~~~~~~~~

`Autodesk Eagle <https://www.autodesk.com/products/eagle/overview>`_ is a free
schematic and layout editor for electronic circuit design and the creation of
Printed Circuit Boards (PCBs). When used in conjunction with OpenMETA, the user
is able compose designs using a rich library of components in the tools and
automatically generate ``.brd`` files from the model.

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

Bill of Materials Analysis
~~~~~~~~~~~~~~~~~~~~~~~~~~

When a component is created and a Schematic Model is added using the
Component Authoring Tool, OpenMETA automatically generates and populates a
collection of propertiesbased on the provided part number when a Schematic
Model.

These part numbers are later used to quickly generate a Bill of Materials
(BOM) from the parts present in a particular design. This feature relies upon
open `Octopart <https://octopart.com/>`_ API.

------

**Footnotes**

.. [1] `<https://en.wikipedia.org/wiki/Electronic_design_automation>`_
