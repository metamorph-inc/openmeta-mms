.. _integrateddomains:

Integrated Domains
==================

.. note:: Take content from old 'Chapter 7: Domains'
   and possibly 'Chapter 1, Section 2: What's inside
   a component model?'.

Autodesk EAGLE
--------------

Eagle [#eagle]_ is a free schematic and layout editor for electronic circuit design and the creation of Printed Circuit Boards (PCBs). When used in conjunction with OpenMETA, the user is able compose designs using a rich library of components in the tools and automatically generate ``.brd`` files from the model.

We recommend using the latest version of the EAGLE tools that can
be found on the `Autodesk Eagle Download Page
<https://www.autodesk.com/products/eagle/free-download>`_.

PTC Creo
--------

Creo Parametric 3.0 is a CAD package that gives the user the ability
to construct parametric parts. These parts can be composed and their
parameters driven from an OpenMETA model.

With Creo installed, check out the :ref:`spacecraft_model` walkthrough.

Ngspice
-------

Ngspice [#ngspice]_ is an open-source SPICE simulator. This engine which ships
installed with the OpenMETA tools allows for the execution of a 
time-step-based circuit simulation.

Footnotes
---------

.. [#eagle] https://www.autodesk.com/products/eagle/overview
.. [#ngspice] http://ngspice.sourceforge.net/