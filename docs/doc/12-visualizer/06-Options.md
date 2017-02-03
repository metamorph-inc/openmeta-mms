## Options

![Options](images/options.png)

**1. Reset Settings**
Clicking this button returns all items on this tab to return to their default state.

**2. Data Processing**
* _Remove Missing:_ This option filters out the data points in the set that have some missing attributes.
* _Remove Outliers:_ This option filters out the data points in the set that are a certain number of standard deviations away from the mean.  The number of standard deviations is set by the user (from 1 to 11), this selection shows up once this box is ticked.
* _Round Data Tables:_ This option sets the max number of decimal places in the data table tab.
* _Sticky Filters:_ [Soon to be always on and hidden from user] This option preserves the settings on filters as a user changes items 1 & 2, if possible.  Instead of returning all filters to default state anytime options 1 and 2 are modified, the filters on the data (at the bottom of the app) attempt to stay at the same position if they are within the new range.

**3. Render Options**
* _Automatically Rerender Plot:_ This option causes the pairs plot to automatically update anytime a setting is changed.  If this box is unselected, a ‘Render Plot’ button appears on the pairs plot tab where the plot will wait to update until a user clicks this button.
* _Overlay Trendlines:_ Activating this option displays trendlines that fit the data in the pairs plot tab.
* _Display upper panel:_ Activating this option displays the upper panel of the pairs plot.  

**4. Data Point Style**
* _Normal/Filled:_ This option selects between an open circle or colored dot as a data point.
* _Small/Medium/Large:_ This option sets the size of the data points.

**5. Auto-refresh**
* _Info Pane:_ Sets the info pane on the pairs plot to constantly reflect current coloring/filtering scheme.
* _Data Table Tab:_ Sets the data table tab to constantly reflect current filtering scheme.
* _Ranges Tab:_ Sets the ranges tab to constantly reflect current filtering scheme.

**6. Color options**
* _Normal:_ Sets the color for the default ‘uncolored’ data point.
* _Worst:_ Sets the color for data points below the ‘max/min’ threshold.
In Between: Sets the color for data points within the ‘max/min’ threshold.
* _Best:_ Sets the color for data points above the ‘max/min’ threshold.
* _Highlight:_ Sets the color for data points that are highlighted in the single plot tab.
* _Ranked:_ This is the color of selected points from the data table.
* _Histogram:_ This is the color of the histogram in the bayesian tab.
* _Original:_ This is the color of the main graph in the bayesian tab.
*  _Resampled:_ This is the color of the resampled plot in the bayesian tab.

**7. Session Options**
* _Save Session:_ A user can preserve the state of the app in a csv by entering a name and clicking the download button.  (If a user enter no name, a default name with the time and date will be generated).
* _Load session:_
A user can upload a session.csv file and recall the state of previously saved session.

**8. About**
Information about the current version of the app, last updated date, and support contact information.  

 