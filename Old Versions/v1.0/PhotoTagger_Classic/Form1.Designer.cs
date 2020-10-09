namespace PhotoTagger_Classic
{
    partial class PhotoTaggerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhotoTaggerForm));
            this.pickFolderButton = new System.Windows.Forms.Button();
            this.foldersLabel = new System.Windows.Forms.Label();
            this.clearButton = new System.Windows.Forms.Button();
            this.tagButton = new System.Windows.Forms.Button();
            this.cancelTagButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.folderListText = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.openFolderCheckBox = new System.Windows.Forms.CheckBox();
            this.LogoPreview = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // pickFolderButton
            // 
            this.pickFolderButton.Location = new System.Drawing.Point(12, 12);
            this.pickFolderButton.Name = "pickFolderButton";
            this.pickFolderButton.Size = new System.Drawing.Size(75, 23);
            this.pickFolderButton.TabIndex = 0;
            this.pickFolderButton.Text = "Pick Folder";
            this.pickFolderButton.UseVisualStyleBackColor = true;
            this.pickFolderButton.Click += new System.EventHandler(this.PickFolderButton_Click);
            // 
            // foldersLabel
            // 
            this.foldersLabel.AutoSize = true;
            this.foldersLabel.Location = new System.Drawing.Point(12, 74);
            this.foldersLabel.Name = "foldersLabel";
            this.foldersLabel.Size = new System.Drawing.Size(89, 13);
            this.foldersLabel.TabIndex = 1;
            this.foldersLabel.Text = "Folders Selected:";
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(93, 12);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(77, 23);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear Folders";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // tagButton
            // 
            this.tagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tagButton.Location = new System.Drawing.Point(397, 232);
            this.tagButton.Name = "tagButton";
            this.tagButton.Size = new System.Drawing.Size(75, 23);
            this.tagButton.TabIndex = 4;
            this.tagButton.Text = "Tag Images";
            this.tagButton.UseVisualStyleBackColor = true;
            this.tagButton.Click += new System.EventHandler(this.tagButton_Click);
            // 
            // cancelTagButton
            // 
            this.cancelTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelTagButton.Enabled = false;
            this.cancelTagButton.Location = new System.Drawing.Point(12, 232);
            this.cancelTagButton.Name = "cancelTagButton";
            this.cancelTagButton.Size = new System.Drawing.Size(75, 23);
            this.cancelTagButton.TabIndex = 6;
            this.cancelTagButton.Text = "Cancel";
            this.cancelTagButton.UseVisualStyleBackColor = true;
            this.cancelTagButton.Click += new System.EventHandler(this.cancelTagButton_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.folderListText);
            this.panel1.Location = new System.Drawing.Point(12, 90);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(460, 136);
            this.panel1.TabIndex = 7;
            // 
            // folderListText
            // 
            this.folderListText.AutoSize = true;
            this.folderListText.Location = new System.Drawing.Point(4, 4);
            this.folderListText.Name = "folderListText";
            this.folderListText.Size = new System.Drawing.Size(0, 13);
            this.folderListText.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(93, 232);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(298, 23);
            this.progressBar1.TabIndex = 8;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // openFolderCheckBox
            // 
            this.openFolderCheckBox.AutoSize = true;
            this.openFolderCheckBox.Checked = true;
            this.openFolderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.openFolderCheckBox.Location = new System.Drawing.Point(12, 41);
            this.openFolderCheckBox.Name = "openFolderCheckBox";
            this.openFolderCheckBox.Size = new System.Drawing.Size(142, 17);
            this.openFolderCheckBox.TabIndex = 9;
            this.openFolderCheckBox.Text = "Open folders when done";
            this.openFolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // LogoPreview
            // 
            this.LogoPreview.ImageLocation = "logo.png";
            this.LogoPreview.Location = new System.Drawing.Point(177, 28);
            this.LogoPreview.Name = "LogoPreview";
            this.LogoPreview.Size = new System.Drawing.Size(295, 46);
            this.LogoPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.LogoPreview.TabIndex = 10;
            this.LogoPreview.TabStop = false;
            this.LogoPreview.Click += new System.EventHandler(this.LogoPreview_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(176, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Logo Preview (Click to change):";
            // 
            // PhotoTaggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(484, 265);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LogoPreview);
            this.Controls.Add(this.openFolderCheckBox);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cancelTagButton);
            this.Controls.Add(this.tagButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.foldersLabel);
            this.Controls.Add(this.pickFolderButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "PhotoTaggerForm";
            this.Text = "Photo Tagger";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button pickFolderButton;
        private System.Windows.Forms.Label foldersLabel;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button tagButton;
        private System.Windows.Forms.Button cancelTagButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label folderListText;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox openFolderCheckBox;
        private System.Windows.Forms.PictureBox LogoPreview;
        private System.Windows.Forms.Label label1;
    }
}

