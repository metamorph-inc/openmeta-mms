namespace CyPhyComponentAuthoring.GUIs
{
    partial class SimulinkLibraryPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SimulinkLibraryGridView = new System.Windows.Forms.DataGridView();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.simulinkNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.SimulinkLibraryGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // SimulinkLibraryGridView
            // 
            this.SimulinkLibraryGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SimulinkLibraryGridView.AutoGenerateColumns = false;
            this.SimulinkLibraryGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SimulinkLibraryGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.descriptionDataGridViewTextBoxColumn,
            this.simulinkNameDataGridViewTextBoxColumn});
            this.SimulinkLibraryGridView.DataSource = this.bindingSource1;
            this.SimulinkLibraryGridView.Location = new System.Drawing.Point(13, 13);
            this.SimulinkLibraryGridView.MultiSelect = false;
            this.SimulinkLibraryGridView.Name = "SimulinkLibraryGridView";
            this.SimulinkLibraryGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.SimulinkLibraryGridView.Size = new System.Drawing.Size(572, 195);
            this.SimulinkLibraryGridView.TabIndex = 0;
            this.SimulinkLibraryGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SimulinkLibraryGridView_CellContentClick);
            this.SimulinkLibraryGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.SimulinkLibraryGridView_SelectionChanged);
            this.SimulinkLibraryGridView.SelectionChanged += new System.EventHandler(this.SimulinkLibraryGridView_SelectionChanged);
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkButton.Enabled = false;
            this.OkButton.Location = new System.Drawing.Point(429, 214);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 1;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(510, 214);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            // 
            // simulinkNameDataGridViewTextBoxColumn
            // 
            this.simulinkNameDataGridViewTextBoxColumn.DataPropertyName = "SimulinkName";
            this.simulinkNameDataGridViewTextBoxColumn.HeaderText = "Library Name";
            this.simulinkNameDataGridViewTextBoxColumn.Name = "simulinkNameDataGridViewTextBoxColumn";
            this.simulinkNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // bindingSource1
            // 
            this.bindingSource1.DataSource = typeof(CyPhyComponentAuthoring.GUIs.SimulinkLibrary);
            // 
            // SimulinkLibraryPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 249);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.SimulinkLibraryGridView);
            this.Name = "SimulinkLibraryPicker";
            this.Text = "SimulinkLibraryPicker";
            ((System.ComponentModel.ISupportInitialize)(this.SimulinkLibraryGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView SimulinkLibraryGridView;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn simulinkNameDataGridViewTextBoxColumn;
    }
}