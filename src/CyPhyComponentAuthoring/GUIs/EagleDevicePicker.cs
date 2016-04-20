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
    public partial class EagleDevicePicker : Form
    {
        public EagleDevicePicker(List<String> devices)
        {
            InitializeComponent();
            
            lbDevices.MultiColumn = false;
            lbDevices.SelectionMode = SelectionMode.One;

            lbDevices.BeginUpdate();
            foreach (var device in devices)
            {
                lbDevices.Items.Add(device);
            }
            lbDevices.EndUpdate();
        }

        private void select_Click(object sender, EventArgs e)
        {
            selectedDevice = lbDevices.SelectedItem as String;
            this.Close();
        }

        public String selectedDevice { get; private set;}

    }
}
