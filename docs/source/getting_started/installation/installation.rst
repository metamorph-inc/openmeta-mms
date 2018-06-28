.. _installation:

Installation
============

System Requirements
~~~~~~~~~~~~~~~~~~~

-  **Platform:** Windows 7, 8.1, or 10.
-  **CPU:** 64-bit processor
-  **RAM:** 1.5 GB minimum (4 GB or more recommended)
-  **HDD:** 2 GB minimum (10 GB or more recommended)
-  Connection to the Internet (for downloading tools during installation)

License
~~~~~~~

OpenMETA is licensed under the MIT License. For more information please see
the
`license.txt <https://github.com/metamorph-inc/openmeta-mms/blob/master/license.txt>`_
file in the
`openmeta-mms <https://github.com/metamorph-inc/openmeta-mms>`_
GitHub repository.

Install Steps
~~~~~~~~~~~~~

The OpenMETA tools can be downloaded and installed using a single installer.
Follow the steps below to install OpenMETA:

1. Download the latest installer from `Releases Page
   <https://openmeta.metamorphsoftware.com/releases>`_ on the OpenMETA website.

   .. image:: images/downloadPage.png
      :width: 970 px

   .. note:: The "Offline" version of the installer includes all the necessary
      dependencies and should therefore be used only when you need to install
      OpenMETA in an environment that does not have access to the Internet.

2. After the download is complete, run the installer  ``META_<version>.exe``
   where *<version>* corresponds to the version you downloaded.
3. Check **I agree to the license terms and conditions** and then click
   **Install** when the license dialog appears.

   .. image:: images/licenseDialog.png
      :width: 481 px

4. Click **Yes** if prompted for permission to make changes to your system.

When you are upgrading, you should not need to uninstall the OpenMETA tools --
the installer will automatically remove any unneeded components.

Included Tools
~~~~~~~~~~~~~~

The OpenMETA Bundle installer combines of a number of tools that work together
to provide the functionality of OpenMETA as a whole. The individual tools and
patches, most of which are downloaded automatically during installation,
include:

-  **Generic Modeling Environment (GME)** [#]_ [#download]_ : This provides the
   backbone model-editing environment for OpenMETA Projects.
-  **OpenMETA Toolchain**: This contains the core of OpenMETA, the **CyPhyML**
   *modeling paradigm*, and accompanying *model interpreters*.
-  **OpenMETA Visualizer** [#]_ : The OpenMETA :ref:`visualizer`
   provides a simple and extensible framework for visualizing data generated
   with the OpenMETA toolchain that integrates seemlessly with the OpenMETA
   :ref:`results_browser`.

-  **Microsoft Visual Studio Redistributables** [#download]_

   -  `Visual C++ Redistributable for Visual Studio 2015
      <https://www.microsoft.com/en-us/download/details.aspx?id=48145>`_

-  **Required Windows Updates** [#download]_

   -  `KB2999226 - Update for Universal C Runtime in Windows <https://support.microsoft.com/en-us/help/2999226/update-for-universal-c-runtime-in-windows>`_

.. [#] `GME Tool Description <http://www.isis.vanderbilt.edu/Projects/gme/>`_
.. [#download] Downloaded automatically during installation.
.. [#] `OpenMETA Visualizer GitHub Repository
       <https://github.com/metamorph-inc/openmeta-visualizer/>`_

For more information on installed dependencies and packages please see the
`THIRD_PARTY.md <https://github.com/metamorph-inc/openmeta-mms/blob/master/THIRD_PARTY.md>`_
file in the
`openmeta-mms <https://github.com/metamorph-inc/openmeta-mms>`_
GitHub repository.
