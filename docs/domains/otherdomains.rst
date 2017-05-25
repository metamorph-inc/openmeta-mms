.. _otherdomains:

Other Domains
=============

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
