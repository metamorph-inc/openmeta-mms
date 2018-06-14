using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CyPhyComponentAuthoring.GUIs
{
    public partial class SimulinkLibraryPicker : Form
    {
        public BindingList<SimulinkLibrary> SimulinkLibraries;

        public SimulinkLibraryPicker()
        {
            var simulinkLibraries = new List<SimulinkLibrary>();

            simulinkLibraries.Add(new SimulinkLibrary()
            {
                Description = "Simulink",
                SimulinkName = "simulink"
            });

            simulinkLibraries.Add(new SimulinkLibrary()
            {
                Description = "Simscape Foundation Library",
                SimulinkName = "fl_lib"
            });

            simulinkLibraries.Add(new SimulinkLibrary()
            {
                Description = "Simscape Utilities",
                SimulinkName = "nesl_utility"
            });

            SimulinkLibraries = new BindingList<SimulinkLibrary>(simulinkLibraries);

            InitializeComponent();

            SimulinkLibraryGridView.DataSource = SimulinkLibraries;
        }

        public string SelectedSimulinkLibraryName
        {
            get
            {
                if (SimulinkLibraryGridView.SelectedRows.Count > 0)
                {
                    var item = SimulinkLibraryGridView.SelectedRows[0].DataBoundItem as SimulinkLibrary;
                    if (item == null || String.IsNullOrEmpty(item.SimulinkName))
                    {
                        return "";
                    }
                    else
                    {
                        return item.SimulinkName;
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        private void SimulinkLibraryGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void SimulinkLibraryGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (SimulinkLibraryGridView.SelectedRows.Count > 0)
            {
                var item = SimulinkLibraryGridView.SelectedRows[0].DataBoundItem as SimulinkLibrary;
                if (item == null || String.IsNullOrEmpty(item.SimulinkName))
                {
                    OkButton.Enabled = false;
                }
                else
                {
                    OkButton.Enabled = true;
                }
            }
            else
            {
                OkButton.Enabled = false;
            }
        }

        private void SimulinkLibraryGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
        }
    }

    public class SimulinkLibrary
    {
        private sealed class DescriptionSimulinkNameEqualityComparer : IEqualityComparer<SimulinkLibrary>
        {
            public bool Equals(SimulinkLibrary x, SimulinkLibrary y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Description, y.Description, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.SimulinkName, y.SimulinkName, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(SimulinkLibrary obj)
            {
                unchecked
                {
                    return ((obj.Description != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Description) : 0) * 397) ^ (obj.SimulinkName != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.SimulinkName) : 0);
                }
            }
        }

        public static IEqualityComparer<SimulinkLibrary> DescriptionSimulinkNameComparer { get; } = new DescriptionSimulinkNameEqualityComparer();

        public string Description { get; set; }

        public string SimulinkName { get; set; }
    }
}
