using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WrtSettings {
    internal partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            Medo.Windows.Forms.State.SetupOnLoadAndClose(this);
        }


        #region InitializeComponent

        private bool SuppressMenuKey = false;

        protected override bool ProcessDialogKey(Keys keyData) {
            if (((keyData & Keys.Alt) == Keys.Alt) && (keyData != (Keys.Alt | Keys.Menu))) { this.SuppressMenuKey = true; }

            switch (keyData) {

                case Keys.F10:
                    ToggleMenu();
                    return true;

                case Keys.Control | Keys.O:
                    mnuOpenRoot.PerformButtonClick();
                    return true;

                case Keys.Alt | Keys.O:
                    mnuOpenRoot.ShowDropDown();
                    return true;

                case Keys.Control | Keys.S:
                    mnuSaveRoot.PerformButtonClick();
                    return true;

                case Keys.Alt | Keys.S:
                    mnuSaveRoot.ShowDropDown();
                    return true;

                case Keys.F1:
                    mnuApp.ShowDropDown();
                    mnuAppAbout.Select();
                    return true;

                case Keys.Control | Keys.C:
                    //TODO
                    return true;

                case Keys.Control | Keys.A:
                    grid.SelectAll();
                    return true;

            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyData == Keys.Menu) {
                if (this.SuppressMenuKey) { this.SuppressMenuKey = false; return; }
                ToggleMenu();
                e.Handled = true;
                e.SuppressKeyPress = true;
            } else {
                base.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (e.KeyData == Keys.Menu) {
                if (this.SuppressMenuKey) { this.SuppressMenuKey = false; return; }
                ToggleMenu();
                e.Handled = true;
                e.SuppressKeyPress = true;
            } else {
                base.OnKeyUp(e);
            }
        }


        private void ToggleMenu() {
            if (mnu.ContainsFocus) {
                grid.Select();
            } else {
                mnu.Select();
                mnu.Items[0].Select();
            }
        }

        #endregion


        #region Form

        private void Form_FormClosing(object sender, FormClosingEventArgs e) {
            if (!HasSavedModifications()) { e.Cancel = true; }

        }

        #endregion


        #region Drag&Drop

        private void Form_DragOver(object sender, DragEventArgs e) {
            if (GetFileName(e.Data) != null) {
                e.Effect = DragDropEffects.Link;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form_DragDrop(object sender, DragEventArgs e) {
            var fileName = GetFileName(e.Data);
            if (fileName != null) {
                if (!HasSavedModifications()) { return; }

                try {
                    this.Document = new Nvram(fileName, NvramFormat.All);
                    this.Recent.Push(fileName);
                } catch (FormatException ex) {
                    Medo.MessageBox.ShowError(this, "Cannot open file!\n\n" + ex.Message);
                }
            }
        }


        private static string GetFileName(IDataObject data) {
            if (data.GetDataPresent("FileDrop")) {
                var fileNames = data.GetData("FileDrop") as string[];
                if ((fileNames != null) && (fileNames.Length == 1)) {
                    return fileNames[0];
                }
            }
            return null;
        }

        #endregion


        #region Grid

        private void grid_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyData) {
                case Keys.Control | Keys.C:
                    var sb = new StringBuilder();
                    foreach (DataGridViewRow row in grid.Rows) {
                        if (row.Selected && !row.IsNewRow) {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}={1}", row.Cells[0].Value, row.Cells[1].Value));
                        }
                    }
                    Clipboard.Clear();
                    if (sb.Length > 0) {
                        Clipboard.SetText(sb.ToString());
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "CellBeginEdit: C{0} R{1}", e.ColumnIndex, e.RowIndex));

            var row = grid.Rows[e.RowIndex];
            if (row.IsNewRow) {
                if ((e.ColumnIndex != 0) && (string.IsNullOrEmpty(row.Cells[0].Value as string))) { //cannot edit value if there is no key
                    e.Cancel = true;
                }
            }
        }

        private void grid_CellParsing(object sender, DataGridViewCellParsingEventArgs e) {
            var oldText = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
            var newText = e.Value.ToString();

            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "CellParsing: C{0} R{1}: '{2}' -> '{3}'", e.ColumnIndex, e.RowIndex, oldText, newText));

            e.ParsingApplied = true;

            try {
                oldText = Nvram.DecodeText((oldText ?? "").Trim());
                newText = Nvram.DecodeText(newText.Trim());

                if (e.ColumnIndex == 0) { //key

                    if (newText.Length == 0) {
                        throw new FormatException("Key must be at least one character!");
                    }

                    if (newText.Contains("=")) {
                        throw new FormatException("Key cannot contain equals (=) character!");
                    }

                    foreach (var ch in newText) {
                        var value = Encoding.ASCII.GetBytes(new char[] { ch })[0];
                        if ((value < 32) || (value > 127)) {
                            throw new FormatException("Key must be in ASCII 32-127 range!");
                        }
                    }

                    if (!oldText.Equals(newText, StringComparison.Ordinal)) {
                        if (this.Document.Variables.ContainsKey(newText)) {
                            throw new FormatException("Cannot have duplicate key!");
                        }
                        if (oldText.Length > 0) { //not a new key
                            this.Document.Variables.Remove(oldText);
                        }

                        var oldValue = (grid.Rows[e.RowIndex].Cells[1].Value as string) ?? "";
                        this.Document.Variables.Add(newText, oldValue);
                        this.HasChanged = true;
                    }

                } else { //value

                    var oldKey = (grid.Rows[e.RowIndex].Cells[0].Value as string) ?? "";
                    this.Document.Variables[oldKey] = newText;
                    this.HasChanged = true;

                }

                e.Value = Nvram.EncodeText(newText);
            } catch (FormatException ex) {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "CellParsing: C{0} R{1}: {2}", e.ColumnIndex, e.RowIndex, ex.Message));
                e.Value = oldText;
            }
        }

        private void grid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            var oldText = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
            var newText = e.FormattedValue.ToString();

            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "CellValidating: C{0} R{1}: '{2}' -> '{3}'", e.ColumnIndex, e.RowIndex, oldText, newText));

            if (e.ColumnIndex == 0) {
                if (string.IsNullOrEmpty(oldText) && (this.Document.Variables.ContainsKey(newText))) { //don't add new key as a duplicate
                    e.Cancel = true;
                }
            }
        }

        private void grid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e) {
            if (((DataGridView)sender).ReadOnly) { //because DataGridView allows row deletion even when readonly?!
                e.Cancel = true;
                return;
            }

            var key = (e.Row.Cells[0].Value as string) ?? "";
            if (this.Document.Variables.ContainsKey(key)) {
                this.Document.Variables.Remove(key);
                this.HasChanged = true;
            }
        }

        #endregion


        #region Document

        private Nvram _document;
        private Nvram Document {
            get { return this._document; }
            set {
                this._document = value;
                this._hasChanged = false;
                if (this.Document != null) {
                    mnuSaveRoot.Enabled = true;
                    mnuReadOnly.Enabled = true;
                    grid.Visible = false;
                    grid.CancelEdit();
                    grid.Rows.Clear();
                    foreach (var pair in this.Document.Variables) {
                        var row = new string[] { Nvram.EncodeText(pair.Key), Nvram.EncodeText(pair.Value) };
                        grid.Rows.Add(row);
                    }
                    grid.Sort(grid_colKey, ListSortDirection.Ascending);
                    grid.Visible = true;
                    grid.Refresh();
                    mnuReadOnly.Checked = true;
                }
                UpdateTitle();
            }
        }

        private bool _hasChanged;
        private bool HasChanged {
            get { return this._hasChanged; }
            set {
                if (this._hasChanged != value) {
                    this._hasChanged = value;
                    UpdateTitle();
                }
            }
        }

        #endregion


        #region Menu

        private void mnuOpenRoot_DropDownOpening(object sender, EventArgs e) {
            var startAt = mnuOpenRoot.DropDownItems.IndexOf(mnuOpenRecentSeparator);
            for (int i = mnuOpenRoot.DropDownItems.Count - 1; i > startAt; i--) {
                mnuOpenRoot.DropDownItems.RemoveAt(i);
            }

            mnuOpenRecentSeparator.Visible = false;

            foreach (var item in this.Recent.Items) {
                if (File.Exists(item.FileName)) {
                    var menuItem = new ToolStripMenuItem(item.Title, null, mnuOpenRecent_Click) { Tag = item.FileName };
                    mnuOpenRoot.DropDownItems.Add(menuItem);
                    mnuOpenRecentSeparator.Visible = true;
                }
            }
        }

        private void mnuOpen_Click(object sender, EventArgs e) {
            if (!HasSavedModifications()) { return; }

            using (var frm = new OpenFileDialog() { ShowReadOnly = true, ReadOnlyChecked = true, Filter = "Auto-detect configuration|*.cfg;*.bin;*.txt|AsusWRT configuration|*.cfg|Tomato configuration|*.cfg|DD-WRT configuration|*.bin|Text file|*.txt" }) {
                if (frm.ShowDialog(this) == DialogResult.OK) {
                    try {
                        switch (frm.FilterIndex) {
                            case 2: this.Document = new Nvram(frm.FileName, NvramFormat.AsuswrtVersion1 | NvramFormat.AsuswrtVersion2); break;
                            case 3: this.Document = new Nvram(frm.FileName, NvramFormat.Tomato); break;
                            case 4: this.Document = new Nvram(frm.FileName, NvramFormat.Text); break;
                            default: this.Document = new Nvram(frm.FileName, NvramFormat.All); break;
                        }
                        this.Recent.Push(this.Document.FileName);
                        mnuReadOnly.Checked = frm.ReadOnlyChecked;
                        grid.Select();
                    } catch (FormatException ex) {
                        Medo.MessageBox.ShowError(this, "Cannot open file!\n\n" + ex.Message);
                    }
                }
            }
        }

        private void mnuOpenRecent_Click(object sender, EventArgs e) {
            if (!HasSavedModifications()) { return; }

            var fileName = (string)(((ToolStripMenuItem)sender).Tag);
            try {
                this.Document = new Nvram(fileName, NvramFormat.All);
                this.Recent.Push(fileName);
                grid.Select();
            } catch (FormatException ex) {
                if (Medo.MessageBox.ShowError(this, "Cannot open file!\n\n" + ex.Message + "\n\nDo you want to remove it from recent files list?", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    this.Recent.Remove(fileName);
                }
            }
        }

        private bool HasSavedModifications() {
            if (this.HasChanged) {
                switch (Medo.MessageBox.ShowQuestion(this, "File has been changed. Do you want to save it?", MessageBoxButtons.YesNoCancel)) {
                    case DialogResult.Yes:
                        mnuSave_Click(null, null);
                        return !this.HasChanged;

                    case DialogResult.No: return true;
                    default: return false;
                }
            } else {
                return true;
            }
        }

        private void mnuSave_Click(object sender, EventArgs e) {
            try {
                this.Document.Save(this.Document.FileName);
                this.HasChanged = false;
                this.Recent.Push(this.Document.FileName);
            } catch (InvalidOperationException ex) {
                Medo.MessageBox.ShowError(this, "Cannot save file!\n\n" + ex.Message);
            }
            grid.Select();
            UpdateTitle();
        }

        private void mnuSaveAs_Click(object sender, EventArgs e) {
            using (var frm = new SaveFileDialog() { FileName = this.Document.FileName, Filter = "AsusWRT configuration (v1)|*.cfg|AsusWRT configuration (v2)|*.cfg|Tomato configuration|*.cfg|DD-WRT configuration|*.bin|Text file|*.txt" }) {
                switch (this.Document.Format) {
                    case NvramFormat.AsuswrtVersion1: frm.FilterIndex = 1; break;
                    case NvramFormat.AsuswrtVersion2: frm.FilterIndex = 2; break;
                    case NvramFormat.Tomato: frm.FilterIndex = 3; break;
                    case NvramFormat.DDWrt: frm.FilterIndex = 4; break;
                    case NvramFormat.Text: frm.FilterIndex = 5; break;
                }
                if (frm.ShowDialog(this) == DialogResult.OK) {
                    switch (frm.FilterIndex) {
                        case 1: this.Document.Format = NvramFormat.AsuswrtVersion1; break;
                        case 2: this.Document.Format = NvramFormat.AsuswrtVersion2; break;
                        case 3: this.Document.Format = NvramFormat.Tomato; break;
                        case 4: this.Document.Format = NvramFormat.DDWrt; break;
                        case 5: this.Document.Format = NvramFormat.Text; break;
                    }
                    try {
                        this.Document.Save(frm.FileName);
                        this.HasChanged = false;
                        this.Recent.Push(this.Document.FileName);
                    } catch (InvalidOperationException ex) {
                        Medo.MessageBox.ShowError(this, "Cannot save file!\n\n" + ex.Message);
                    }
                    grid.Select();
                    UpdateTitle();
                }
            }
        }


        private void mnuReadOnly_CheckedChanged(object sender, EventArgs e) {
            grid.ReadOnly = mnuReadOnly.Checked;
        }


        private void mnuAppFeedback_Click(object sender, EventArgs e) {
            Medo.Diagnostics.ErrorReport.ShowDialog(this, null, new Uri("http://jmedved.com/feedback/"));
        }

        private void mnuAppUpgrade_Click(object sender, EventArgs e) {
            Medo.Services.Upgrade.ShowDialog(this, new Uri("http://jmedved.com/upgrade/"));
        }

        private void mnuAppDonate_Click(object sender, EventArgs e) {
            Process.Start("http://www.jmedved.com/donate/");
        }

        private void mnuAppAbout_Click(object sender, EventArgs e) {
            Medo.Windows.Forms.AboutBox.ShowDialog(this, new Uri("http://www.jmedved.com/asuswrtsettings/"));
        }

        #endregion


        #region File

        #endregion


        #region Helpers

        private readonly Medo.Configuration.RecentFiles Recent = new Medo.Configuration.RecentFiles();

        private void UpdateTitle() {
            string newText;
            if (this.Document == null) {
                newText = (this.HasChanged ? "* " : "") + Medo.Reflection.EntryAssembly.Product;
            } else {
                newText = Path.GetFileNameWithoutExtension(this.Document.FileName) + (this.HasChanged ? "*" : "") + " - " + Medo.Reflection.EntryAssembly.Product;
            }
            if (this.Text != newText) { this.Text = newText; }
        }


        #endregion

    }
}
