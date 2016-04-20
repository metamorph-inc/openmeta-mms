namespace CyPhy2Schematic.GUI
{
    partial class CyPhy2Schematic_GUI
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
            this.edaModeButton = new System.Windows.Forms.RadioButton();
            this.spiceModeButton = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_PlaceAndRoute = new System.Windows.Forms.CheckBox();
            this.cb_TestForChipFit = new System.Windows.Forms.CheckBox();
            this.ok = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // edaModeButton
            // 
            this.edaModeButton.AutoSize = true;
            this.edaModeButton.Location = new System.Drawing.Point(13, 13);
            this.edaModeButton.Name = "edaModeButton";
            this.edaModeButton.Size = new System.Drawing.Size(77, 17);
            this.edaModeButton.TabIndex = 0;
            this.edaModeButton.TabStop = true;
            this.edaModeButton.Text = "EDA Mode";
            this.edaModeButton.UseVisualStyleBackColor = true;
            this.edaModeButton.CheckedChanged += new System.EventHandler(this.edaModeButton_CheckedChanged);
            // 
            // spiceModeButton
            // 
            this.spiceModeButton.AutoSize = true;
            this.spiceModeButton.Location = new System.Drawing.Point(13, 70);
            this.spiceModeButton.Name = "spiceModeButton";
            this.spiceModeButton.Size = new System.Drawing.Size(86, 17);
            this.spiceModeButton.TabIndex = 1;
            this.spiceModeButton.TabStop = true;
            this.spiceModeButton.Text = "SPICE Mode";
            this.spiceModeButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cb_PlaceAndRoute);
            this.groupBox2.Controls.Add(this.cb_TestForChipFit);
            this.groupBox2.Location = new System.Drawing.Point(6, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(231, 51);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // cb_PlaceAndRoute
            // 
            this.cb_PlaceAndRoute.AutoSize = true;
            this.cb_PlaceAndRoute.Location = new System.Drawing.Point(117, 23);
            this.cb_PlaceAndRoute.Name = "cb_PlaceAndRoute";
            this.cb_PlaceAndRoute.Size = new System.Drawing.Size(106, 17);
            this.cb_PlaceAndRoute.TabIndex = 1;
            this.cb_PlaceAndRoute.Text = "Place and Route";
            this.cb_PlaceAndRoute.UseVisualStyleBackColor = true;
            // 
            // cb_TestForChipFit
            // 
            this.cb_TestForChipFit.AutoSize = true;
            this.cb_TestForChipFit.Location = new System.Drawing.Point(7, 23);
            this.cb_TestForChipFit.Name = "cb_TestForChipFit";
            this.cb_TestForChipFit.Size = new System.Drawing.Size(100, 17);
            this.cb_TestForChipFit.TabIndex = 0;
            this.cb_TestForChipFit.Text = "Test for Chip Fit";
            this.cb_TestForChipFit.UseVisualStyleBackColor = true;
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(162, 107);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 4;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(6, 70);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(231, 31);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // CyPhy2Schematic_GUI
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 140);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.spiceModeButton);
            this.Controls.Add(this.edaModeButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CyPhy2Schematic_GUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CyPhy2Schematic";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton edaModeButton;
        private System.Windows.Forms.RadioButton spiceModeButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.CheckBox cb_PlaceAndRoute;
        private System.Windows.Forms.CheckBox cb_TestForChipFit;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}