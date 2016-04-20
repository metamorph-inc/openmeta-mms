using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CyPhy2Schematic.GUI
{
    public partial class CyPhy2Schematic_GUI : Form
    {
        RadioButton checkedRadioButton;

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control control in groupBox1.Controls)
            {
                RadioButton radioButton = control as RadioButton;
                if (radioButton != null)
                {
                    radioButton.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
                }
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                if (radioButton.Checked)
                {
                    checkedRadioButton = radioButton;
                }
                else if (checkedRadioButton == radioButton)
                {
                    checkedRadioButton = null;
                }
            }
        }

        public CyPhy2Schematic_Settings settings
        {
            get
            {
                return new CyPhy2Schematic_Settings()
                {
                    doSpice = spiceModeButton.Checked ? "true" : null,
                    doChipFit = (!spiceModeButton.Checked && cb_TestForChipFit.Checked) ? "true" : null,
                    doPlaceRoute = (!spiceModeButton.Checked && cb_PlaceAndRoute.Checked) ? "true" : null
                };
            }
            set
            {
                if (value != null)
                {
                    spiceModeButton.Checked = value.doSpice != null;
                    edaModeButton.Checked = !spiceModeButton.Checked;
                    cb_TestForChipFit.Checked = value.doChipFit == "true";
                    cb_PlaceAndRoute.Checked = value.doPlaceRoute == "true";

                    edaModeButton_CheckedChanged(this, new EventArgs());
                }
            }
        }
    }
}
