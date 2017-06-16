.. _framework:

Visualizer Framework
====================

The Visualizer Framework provides an abstraction layer between the
Results Browser and the custom tabs that it hosts. As such it provides
the following services to the tabs:

1. Reads in data and wraps it as a Shiny reactive data frame.
2. Provides a global concept of *Filtered* and *Colored* data that tabs
   can access.
3. Provides a place to save user *Comments* and *Sets*.
4. Gives tabs the ability to add columns to the raw data frame as
   *Classifications*.
5. Saves the session state (including UI inputs and certain data the
   tabs specify) on close and restores the session when the Visualizer
   is later launched from the same config file.

To facilitate interaction with the Visualizer framework itself, a footer
is provided at the bottom of all the tabs that request it. This request
is made by placing a ``footer <- TRUE`` statement in their definition.
For example, the "Histogram.R" example tab below requests this footer.

Filters
-------

The Filters panel is the first panel in the Visualizer footer.

.. image:: images/filters.png
   :alt: Filters Panel of Visualizer Footer
   :width: 1459px

-  **View All Filters:** This selects between displaying either filters
   for every variable and classification or just filters for those
   variables currently "selected" in the open tab.

-  **Reset Visible Filters:** This button will return the visible
   filters to their original state, i.e. the full range is selected.

-  **Filters:** These filters exclude from the "Filtered" dataset data
   points that fall outside of the ranges specified for any of the
   variable. Be careful not to exclude all the points in your dataset.

.. image:: images/filter_exact_entry.png
   :alt: Exact Entry
   :width: 408px

-  **Exact Entry Window:** When a slider is 'double-clicked', a new
   window opens up allowing the user to enter an exact range for the
   filter. The window shows the name of the variable along with text
   fields for minimum and maximum range. The 'apply' button applies the
   new values set for the filter; if either or both of the fields are
   left blank or containing non-numeric numbers, they are ignored when
   this button is clicked.

Coloring
--------

.. image:: images/coloring.png
   :alt: Coloring Panel of Visualizer Footer
   :width: 1459px

The *Coloring* panel allows us to apply live and saved colorings to the
data. This information is passed to the tab as an addition column in the
data in the ``data$Colored`` data frame. The "Source" can take one of
three options:

1. **None:** This will assign "black" to the added column in *Colored*.
2. **Live:** This will use the "Live" options that are present here in
   the Coloring Panel to assign the added column in *Colored*.
3. <Saved Colorings>: Different desirable coloring schemes can be saved
   using the "Add Current 'Live' Coloring" button. These colorings will
   be persisted across the live of the session and can be applied by
   selecting them here in the "Source" select input.

Classifications
---------------

.. image:: images/classifications.png
   :alt: Classifications Panel of Visualizer Footer
   :width: 775px

The Visualizer allows for tabs to add additional columns to the dataset.
These added columns are referred to as "classifications." If one of the
tabs selected for the session offers the ability to save
classifications, they will appear here in the *Classifications* panel.

Configuration
-------------

Data Processing
~~~~~~~~~~~~~~~

-  **Remove Missing:** This removes rows from the dataset that are
   incomplete, i.e. one or more entries is missing data.
-  **Remove Outliers:** This option filters out any rows that include
   data more than a certain number of standard deviations away from the
   mean for that variable. The number of standard deviations used for
   filtering can be selected using the slider input.

About
~~~~~

Information about the current version of the app, date of last release,
and support contact information.
