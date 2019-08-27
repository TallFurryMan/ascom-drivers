namespace ASCOM.EQ500X
{
    partial class SetupDialogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialogForm));
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.comboBoxComPort = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.GeographicalSiteGroupBox = new System.Windows.Forms.GroupBox();
            this.ElevationLabel = new System.Windows.Forms.Label();
            this.LatitudeLabel = new System.Windows.Forms.Label();
            this.LongitudeLabel = new System.Windows.Forms.Label();
            this.ElevationBox = new System.Windows.Forms.TextBox();
            this.LatitudeBox = new System.Windows.Forms.TextBox();
            this.LongitudeBox = new System.Windows.Forms.TextBox();
            this.ConnectionGroupBox = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.GeographicalSiteGroupBox.SuspendLayout();
            this.ConnectionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(3, 176);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(93, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(3, 206);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(93, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.EQ500X.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(42, 3);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 29);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Comm Port";
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(91, 53);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(69, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // comboBoxComPort
            // 
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(91, 26);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(90, 21);
            this.comboBoxComPort.TabIndex = 7;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(13, 13);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.picASCOM);
            this.splitContainer1.Panel2.Controls.Add(this.cmdOK);
            this.splitContainer1.Panel2.Controls.Add(this.cmdCancel);
            this.splitContainer1.Size = new System.Drawing.Size(373, 234);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.TabIndex = 8;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.GeographicalSiteGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.ConnectionGroupBox);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(237, 201);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // GeographicalSiteGroupBox
            // 
            this.GeographicalSiteGroupBox.Controls.Add(this.ElevationLabel);
            this.GeographicalSiteGroupBox.Controls.Add(this.LatitudeLabel);
            this.GeographicalSiteGroupBox.Controls.Add(this.LongitudeLabel);
            this.GeographicalSiteGroupBox.Controls.Add(this.ElevationBox);
            this.GeographicalSiteGroupBox.Controls.Add(this.LatitudeBox);
            this.GeographicalSiteGroupBox.Controls.Add(this.LongitudeBox);
            this.GeographicalSiteGroupBox.Location = new System.Drawing.Point(3, 3);
            this.GeographicalSiteGroupBox.Name = "GeographicalSiteGroupBox";
            this.GeographicalSiteGroupBox.Size = new System.Drawing.Size(234, 100);
            this.GeographicalSiteGroupBox.TabIndex = 0;
            this.GeographicalSiteGroupBox.TabStop = false;
            this.GeographicalSiteGroupBox.Text = "Geographical Site";
            // 
            // ElevationLabel
            // 
            this.ElevationLabel.AutoSize = true;
            this.ElevationLabel.Location = new System.Drawing.Point(6, 74);
            this.ElevationLabel.Name = "ElevationLabel";
            this.ElevationLabel.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.ElevationLabel.Size = new System.Drawing.Size(57, 13);
            this.ElevationLabel.TabIndex = 10;
            this.ElevationLabel.Text = "Elevation";
            // 
            // LatitudeLabel
            // 
            this.LatitudeLabel.AutoSize = true;
            this.LatitudeLabel.Location = new System.Drawing.Point(6, 48);
            this.LatitudeLabel.Name = "LatitudeLabel";
            this.LatitudeLabel.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.LatitudeLabel.Size = new System.Drawing.Size(51, 13);
            this.LatitudeLabel.TabIndex = 9;
            this.LatitudeLabel.Text = "Latitude";
            // 
            // LongitudeLabel
            // 
            this.LongitudeLabel.AutoSize = true;
            this.LongitudeLabel.Location = new System.Drawing.Point(6, 22);
            this.LongitudeLabel.Name = "LongitudeLabel";
            this.LongitudeLabel.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.LongitudeLabel.Size = new System.Drawing.Size(60, 13);
            this.LongitudeLabel.TabIndex = 8;
            this.LongitudeLabel.Text = "Longitude";
            // 
            // ElevationBox
            // 
            this.ElevationBox.Location = new System.Drawing.Point(91, 71);
            this.ElevationBox.Name = "ElevationBox";
            this.ElevationBox.Size = new System.Drawing.Size(90, 20);
            this.ElevationBox.TabIndex = 2;
            this.toolTip1.SetToolTip(this.ElevationBox, "Enter the elevation of the geographical site the mount is located at.\r\nYou may us" +
        "e a decimal value in meters.\r\nAllowed interval is -300 meters to +10000 meters.");
            this.ElevationBox.Validating += new System.ComponentModel.CancelEventHandler(this.ElevationBox_Validating);
            // 
            // LatitudeBox
            // 
            this.LatitudeBox.Location = new System.Drawing.Point(91, 45);
            this.LatitudeBox.Name = "LatitudeBox";
            this.LatitudeBox.Size = new System.Drawing.Size(90, 20);
            this.LatitudeBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this.LatitudeBox, resources.GetString("LatitudeBox.ToolTip"));
            this.LatitudeBox.Validating += new System.ComponentModel.CancelEventHandler(this.LatitudeBox_Validating);
            // 
            // LongitudeBox
            // 
            this.LongitudeBox.Location = new System.Drawing.Point(91, 19);
            this.LongitudeBox.Name = "LongitudeBox";
            this.LongitudeBox.Size = new System.Drawing.Size(90, 20);
            this.LongitudeBox.TabIndex = 0;
            this.toolTip1.SetToolTip(this.LongitudeBox, resources.GetString("LongitudeBox.ToolTip"));
            this.LongitudeBox.Validating += new System.ComponentModel.CancelEventHandler(this.LongitudeBox_Validating);
            // 
            // ConnectionGroupBox
            // 
            this.ConnectionGroupBox.Controls.Add(this.comboBoxComPort);
            this.ConnectionGroupBox.Controls.Add(this.chkTrace);
            this.ConnectionGroupBox.Controls.Add(this.label2);
            this.ConnectionGroupBox.Location = new System.Drawing.Point(3, 109);
            this.ConnectionGroupBox.Name = "ConnectionGroupBox";
            this.ConnectionGroupBox.Size = new System.Drawing.Size(234, 79);
            this.ConnectionGroupBox.TabIndex = 1;
            this.ConnectionGroupBox.TabStop = false;
            this.ConnectionGroupBox.Text = "Hardware Connection";
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 100;
            this.toolTip1.AutoPopDelay = 30000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 20;
            this.toolTip1.ShowAlways = true;
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 259);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EQ500X Setup";
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.GeographicalSiteGroupBox.ResumeLayout(false);
            this.GeographicalSiteGroupBox.PerformLayout();
            this.ConnectionGroupBox.ResumeLayout(false);
            this.ConnectionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.ComboBox comboBoxComPort;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox GeographicalSiteGroupBox;
        private System.Windows.Forms.Label ElevationLabel;
        private System.Windows.Forms.Label LatitudeLabel;
        private System.Windows.Forms.Label LongitudeLabel;
        private System.Windows.Forms.TextBox ElevationBox;
        private System.Windows.Forms.TextBox LatitudeBox;
        private System.Windows.Forms.TextBox LongitudeBox;
        private System.Windows.Forms.GroupBox ConnectionGroupBox;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}