.. _metalink:

Metalink
========

Metalink allows a user to edit an OpenMETA component assembly and immediately
see those changes reflected in the CAD representation.
To open a connection between OpenMETA and the CAD tool, simply open the
desired Component Assembly in the editor and click the Metalink Button,
|METALINK_BUTTON|.

When you click different components in the OpenMETA project, you will
see the same components highlighted in the CAD representation.

.. |METALINK_BUTTON| image:: images/metalink_button.png
   :width: 24px

.. figure:: images/metalink_example.png
   :alt: Running Metalink with a Simple Assembly

   Running Metalink with a Simple Assembly

Metalink is also capable of regenerating the CAD representation as parameters
in the OpenMETA project are changed or even entirely new components are added
to the assembly.

Requirements
------------

In addition to Creo, as noted in the :ref:`installation` chapter, Metalink
requires Java JRE 7 or higher.

Since Java 7 is considered an archived version of Java by Oracle, we
recommend you download the Java 8 JRE from the Oracle `Java Download
<https://www.java.com/en/download/>`_
page if you do not already have Java installed on your target system.
