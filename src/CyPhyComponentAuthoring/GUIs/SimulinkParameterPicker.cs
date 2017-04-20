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
    public partial class SimulinkParameterPicker: Form
    {
        private IList<string> _paramNames;

        public IList<string> ParamNames
        {
            get { return _paramNames; }

            set
            {
                _paramNames = value;

                ParamListBox.BeginUpdate();
                ParamListBox.Items.Clear();

                foreach (var blockName in _paramNames)
                {
                    ParamListBox.Items.Add(blockName);
                }

                ParamListBox.EndUpdate();
            }
        }

        public IList<string> SelectedParams
        {
            get { return ParamListBox.CheckedItems.Cast<string>().ToList(); }
        }

        public SimulinkParameterPicker()
        {
            InitializeComponent();
        }
        

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
