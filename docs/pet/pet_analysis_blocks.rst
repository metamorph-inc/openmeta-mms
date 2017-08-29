.. _pet_analysis_blocks:

PET Analysis Blocks
===================

PET Analysis Blocks are Test Benches and External Tool Wrappers that can be
placed within a PET to model the system to be analyzed. Therefore, PET Analysis
Blocks serve as modular building blocks that can be combined within a single PET
to perform a full-system analysis using subanalyses from multiple domains.

.. TODO: Comment on how users can easily connect different Analysis Blocks in order
.. to use the output of one External Tool as the input to a second External Tool.

.. ADD: picture of PET containing all different types of analysis blocks connected
.. together

Test Benches
------------

.. note:: This section is under construction. Please check back later for updates!

.. TODO: "I'm not well acquainted with how Test Benches work in a PET. Might need
.. to redo the LED Tutorial" - Joseph

Excel Wrappers
--------------

Preparing an Excel Spreadsheet
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Before adding an Excel Spreadsheet, you must name all the cells that you desire
to be exposed in the Excel Wrapper analysis block. You can do this in Excel in
one of two ways:

#. selecting a cell and then typing a name in the **Name Box** in the upper left
   portion of the Excel window, or

#. using the **Name Manager** tool in the **Formula** tab on the ribbon.

Any cells that are formulas will be interpreted as outputs; all others will be
interpreted as inputs.

.. figure:: images/ExcelWrapperConfig.png
   :alt: text

   An Excel Spreadsheet Being Prepared For Use As an Analysis Block


Adding Excel Wrappers to a PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

To add an Excel Wrapper to a PET, simply drag the Excel Wrapper icon from the
Part Browser and onto the PET canvas. Double-click on the Excel Wrapper and use 
the **Open** dialogue to select the Excel file to be added to the PET.

.. figure:: images/ExcelWrapperAddition.png
   :alt: text

You should then see your component with the exposed inputs and outputs in the
PET Canvas.

.. figure:: images/ExcelWrapperAdditionComplete.png
   :alt: text

Matlab Wrappers
---------------

.. note:: This section is under construction. Please check back later for updates!

These allow the user to add Matlab scripts to a PET.

.. ADD: current limitation on how many outputs a Matlab script can have

Adding Matlab Wrappers to a PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

To add a Matlab Wrapper to a PET, simply drag the Matlab Wrapper icon from the
Part Browser and onto the PET canvas.

.. ADD: picture of Matlab Wrapper being dragged into PET

Loading Matlab Wrappers
~~~~~~~~~~~~~~~~~~~~~~~

To load a Matlab Wrapper, double-click on the Matlab Wrapper and use the
file explorer to select the Matlab script to be added to the PET.

.. ADD: picture of Matlab Wrapper being loaded with Matlab script

.. TODO: "Never used" - Joseph

.. _pet_analysis_blocks_python_wrappers:

PythonWrappers
--------------

These serve as the most generic integration point. Practically any Python model or
tool can be added to a PET using Python Wrappers.

Adding PythonWrappers to a PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

To add a PythonWrapper to a PET, simply drag the PythonWrapper icon from the
Part Browser and onto the PET canvas.

.. figure:: images/PythonWrapper.png
   :alt: text

   A PythonWrapper in a PET

A PythonWrapper can be loaded with specially-formatted python scripts.

Below is a template PythonWrapper script:

.. highlight:: python
.. :linenothreshold: 5

::

	from __future__ import print_function
	from openmdao.api import Component
	from pprint import pprint

	''' First, let's create the component defining our system. We'll call it 'Paraboloid'. '''
	class Paraboloid(Component):
		''' Evaluates the equation f(x,y) = (x-3)^2 +xy +(y+4)^2 - 3 '''

		def __init__(self):
			super(Paraboloid, self).__init__()

			''' Inputs to the PythonWrapper Component are added here as params '''
			self.add_param('x', val=0.0)
			self.add_param('y', val=0.0)

			''' Outputs from the PythonWrapper Component are added here as unknowns '''
			self.add_output('f_xy', shape=1)

		def solve_nonlinear(self, params, unknowns, resids):
			''' This is where we describe the system that we want to add to OpenMETA '''
			''' f(x,y) = (x-3)^2 + xy + (y+4)^2 - 3 '''

			x = params['x']
			y = params['y']

			f_xy = (x-3.0)**2 + x*y + (y+4.0)**2 - 3.0

			unknowns['f_xy'] = f_xy

			''' This is an equivalent expression to the one above
			unknowns['f_xy'] = (params['x']-3.0)**2 + params['x']*y + (params['y']+4.0)**2 - 3.0
			'''

Loading PythonWrappers
~~~~~~~~~~~~~~~~~~~~~~~

To load a PythonWrapper, double-click on the PythonWrapper and use the
file explorer to select the Python script to be added to the PET.

.. figure:: images/LoadingPythonWrapper.png
   :alt: text

   Loading a PythonWrapper with a Python script

.. figure:: images/PythonWrapperComponent.png
   :alt: text

   A PythonWrapper loaded with a Python script

Reloading PythonWrappers
~~~~~~~~~~~~~~~~~~~~~~~~

To reload a PythonWrapper, left-click on the |RELOAD| icon.

.. |RELOAD| image:: images/icons/reload.png
      :alt: Load icon
      :width: 25px

.. figure:: images/LoadingPythonWrapper.png
   :alt: text

   Reloading a PythonWrapper with a Python script

.. figure:: images/PythonWrapperComponent.png
   :alt: text

   A PythonWrapper loaded with a Python script

.. note:: You have to manually reload PythonWrappers whenever you
   change the exposed Params and Unknowns within the Python script.

Editing PythonWrappers
~~~~~~~~~~~~~~~~~~~~~~

To edit a PythonWrapper script from within OpenMETA, left-click on the |EDIT| icon

.. |EDIT| image:: images/icons/edit.png
      :alt: Edit icon
      :width: 25px

.. figure:: images/EditingPythonWrapper.png
   :alt: text

   Editing a PythonWrapper script

Constants Blocks
----------------

These allow the user to set constant scalar values that can then be used to drive
other Analysis blocks within a PET.

Adding Constants Blocks to a PET
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

To add a Constants Block to a PET, simply drag the Constants block icon from the
Part Browser and onto the PET canvas.

.. figure:: images/Constants.png
   :alt: text

   A Constants block in a PET

Populating Constants Blocks with Metrics
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Constants Blocks within a PET can contain Metrics that hold scalar values.

To add a Metric to a Constants Block, double-click on the Constants Block
to open it, then drag the Metric icon from the Parts Browser into the Constants
Block canvas. The Metric can be renamed and its value can be set via
Object Inspector > Attributes > Value.

.. figure:: images/Metric.png
   :alt: text

   A Metric in a Constants block

.. figure:: images/MetricValue.png
   :alt: text

   A Metric's value being set

.. figure:: images/MetricConnected.png
   :alt: text

   A Constants's Metric connected to a PET Analysis Block

Multiple Metrics can be added to the same Constants Block. Each one will display as
a separate port on the Constants Block model.

.. figure:: images/MetricMultiple.png
   :alt: text

   Two Metrics with set values in a Constants block

.. figure:: images/MetricConnectedMultiple.png
   :alt: text

   Two Constants's Metric connected to a PET Analysis Block
