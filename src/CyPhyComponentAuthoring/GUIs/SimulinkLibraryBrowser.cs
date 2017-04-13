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
    public partial class SimulinkLibraryBrowser : Form
    {
        private IList<string> _blockNames;

        public IList<string> BlockNames
        {
            get { return _blockNames; }

            set
            {
                _blockNames = value;

                BlockListBox.BeginUpdate();
                BlockListBox.Items.Clear();

                foreach (var blockName in _blockNames)
                {
                    BlockListBox.Items.Add(blockName);
                }

                BlockListBox.EndUpdate();
            }
        }

        public string SelectedBlockName
        {
            get
            {
                if (BlockListBox.SelectedItem != null)
                {
                    return (string) BlockListBox.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public SimulinkLibraryBrowser()
        {
            InitializeComponent();
        }

        private void BlockListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BlockListBox.SelectedItem != null)
            {
                OkButton.Enabled = true;
            }
            else
            {
                OkButton.Enabled = false;
            }
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
