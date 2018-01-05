.. _otherdomains:

Other Domains
=============

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

   RF model of a 2.4 GHz Inverted-F antenna
