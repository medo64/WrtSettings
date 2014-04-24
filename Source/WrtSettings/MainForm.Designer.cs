namespace WrtSettings {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mnu = new System.Windows.Forms.ToolStrip();
            this.mnuOpenRoot = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenRecentSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSaveRoot = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuApp = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuAppFeedback = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAppUpgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAppDonate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAppAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.grid = new System.Windows.Forms.DataGridView();
            this.grid_colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grid_colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mnu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // mnu
            // 
            this.mnu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mnu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpenRoot,
            this.mnuSaveRoot,
            this.mnuApp});
            this.mnu.Location = new System.Drawing.Point(0, 0);
            this.mnu.Name = "mnu";
            this.mnu.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.mnu.Size = new System.Drawing.Size(442, 25);
            this.mnu.TabIndex = 0;
            // 
            // mnuOpenRoot
            // 
            this.mnuOpenRoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuOpenRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuOpenRecentSeparator});
            this.mnuOpenRoot.Image = ((System.Drawing.Image)(resources.GetObject("mnuOpenRoot.Image")));
            this.mnuOpenRoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuOpenRoot.Name = "mnuOpenRoot";
            this.mnuOpenRoot.Size = new System.Drawing.Size(32, 22);
            this.mnuOpenRoot.Text = "Open";
            this.mnuOpenRoot.ToolTipText = "Open (Ctrl+O)";
            this.mnuOpenRoot.ButtonClick += new System.EventHandler(this.mnuOpen_Click);
            this.mnuOpenRoot.DropDownOpening += new System.EventHandler(this.mnuOpenRoot_DropDownOpening);
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.ShortcutKeyDisplayString = "Ctrl+O";
            this.mnuOpen.Size = new System.Drawing.Size(167, 24);
            this.mnuOpen.Text = "&Open";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // mnuOpenRecentSeparator
            // 
            this.mnuOpenRecentSeparator.Name = "mnuOpenRecentSeparator";
            this.mnuOpenRecentSeparator.Size = new System.Drawing.Size(164, 6);
            // 
            // mnuSaveRoot
            // 
            this.mnuSaveRoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuSaveRoot.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSave,
            this.mnuSaveAs});
            this.mnuSaveRoot.Enabled = false;
            this.mnuSaveRoot.Image = ((System.Drawing.Image)(resources.GetObject("mnuSaveRoot.Image")));
            this.mnuSaveRoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuSaveRoot.Name = "mnuSaveRoot";
            this.mnuSaveRoot.Size = new System.Drawing.Size(32, 22);
            this.mnuSaveRoot.Text = "Save";
            this.mnuSaveRoot.ToolTipText = "Save (Ctrl+S)";
            this.mnuSaveRoot.ButtonClick += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuSave
            // 
            this.mnuSave.Name = "mnuSave";
            this.mnuSave.ShortcutKeyDisplayString = "Ctrl+S";
            this.mnuSave.Size = new System.Drawing.Size(159, 24);
            this.mnuSave.Text = "&Save";
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuSaveAs
            // 
            this.mnuSaveAs.Name = "mnuSaveAs";
            this.mnuSaveAs.Size = new System.Drawing.Size(159, 24);
            this.mnuSaveAs.Text = "Save &as";
            this.mnuSaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
            // 
            // mnuApp
            // 
            this.mnuApp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.mnuApp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuApp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAppFeedback,
            this.mnuAppUpgrade,
            this.mnuAppDonate,
            this.toolStripMenuItem2,
            this.mnuAppAbout});
            this.mnuApp.Image = ((System.Drawing.Image)(resources.GetObject("mnuApp.Image")));
            this.mnuApp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuApp.Name = "mnuApp";
            this.mnuApp.Size = new System.Drawing.Size(29, 22);
            this.mnuApp.Text = "Application";
            // 
            // mnuAppFeedback
            // 
            this.mnuAppFeedback.Name = "mnuAppFeedback";
            this.mnuAppFeedback.Size = new System.Drawing.Size(200, 24);
            this.mnuAppFeedback.Text = "Send &feedback";
            this.mnuAppFeedback.Click += new System.EventHandler(this.mnuAppFeedback_Click);
            // 
            // mnuAppUpgrade
            // 
            this.mnuAppUpgrade.Name = "mnuAppUpgrade";
            this.mnuAppUpgrade.Size = new System.Drawing.Size(200, 24);
            this.mnuAppUpgrade.Text = "Check for &upgrade";
            this.mnuAppUpgrade.Click += new System.EventHandler(this.mnuAppUpgrade_Click);
            // 
            // mnuAppDonate
            // 
            this.mnuAppDonate.Name = "mnuAppDonate";
            this.mnuAppDonate.Size = new System.Drawing.Size(200, 24);
            this.mnuAppDonate.Text = "&Donate";
            this.mnuAppDonate.Click += new System.EventHandler(this.mnuAppDonate_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(197, 6);
            // 
            // mnuAppAbout
            // 
            this.mnuAppAbout.Name = "mnuAppAbout";
            this.mnuAppAbout.Size = new System.Drawing.Size(200, 24);
            this.mnuAppAbout.Text = "&About";
            this.mnuAppAbout.Click += new System.EventHandler(this.mnuAppAbout_Click);
            // 
            // grid
            // 
            this.grid.AllowUserToResizeRows = false;
            this.grid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.grid_colKey,
            this.grid_colValue});
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 25);
            this.grid.Name = "grid";
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grid.Size = new System.Drawing.Size(442, 368);
            this.grid.TabIndex = 1;
            this.grid.Visible = false;
            this.grid.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.grid_CellBeginEdit);
            this.grid.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.grid_CellParsing);
            this.grid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.grid_CellValidating);
            this.grid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grid_KeyDown);
            // 
            // grid_colKey
            // 
            this.grid_colKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.grid_colKey.Frozen = true;
            this.grid_colKey.HeaderText = "Key";
            this.grid_colKey.MinimumWidth = 120;
            this.grid_colKey.Name = "grid_colKey";
            this.grid_colKey.Width = 180;
            // 
            // grid_colValue
            // 
            this.grid_colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.grid_colValue.DividerWidth = 1;
            this.grid_colValue.HeaderText = "Value";
            this.grid_colValue.MinimumWidth = 60;
            this.grid_colValue.Name = "grid_colValue";
            this.grid_colValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 393);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.mnu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(400, 320);
            this.Name = "MainForm";
            this.Text = "WRT Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.Form_DragOver);
            this.mnu.ResumeLayout(false);
            this.mnu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mnu;
        private System.Windows.Forms.ToolStripDropDownButton mnuApp;
        private System.Windows.Forms.ToolStripSplitButton mnuOpenRoot;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripSeparator mnuOpenRecentSeparator;
        private System.Windows.Forms.ToolStripSplitButton mnuSaveRoot;
        private System.Windows.Forms.ToolStripMenuItem mnuSave;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuAppFeedback;
        private System.Windows.Forms.ToolStripMenuItem mnuAppUpgrade;
        private System.Windows.Forms.ToolStripMenuItem mnuAppDonate;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuAppAbout;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn grid_colKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn grid_colValue;
    }
}

