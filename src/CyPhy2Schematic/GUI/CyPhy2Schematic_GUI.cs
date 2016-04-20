using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CyPhy2Schematic.GUI
{
    public partial class CyPhy2Schematic_GUI : Form
    {
        public CyPhy2Schematic_GUI()
        {
            InitializeComponent();
        }

        private void edaModeButton_CheckedChanged(object sender, EventArgs e)
        {
            cb_PlaceAndRoute.Enabled = edaModeButton.Checked;
            cb_TestForChipFit.Enabled = edaModeButton.Checked;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
