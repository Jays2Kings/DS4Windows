using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DS4Control;
using System.IO;
using System.Xml;
namespace DS4Windows
{
    public partial class SpecActions : Form
    {
        Options opt;
        RecordBox rb;
        public List<string> macros = new List<string>();
        public List<string> controls = new List<string>();
        public List<string> ucontrols = new List<string>();
        public List<int> macrostag = new List<int>();
        public bool macrorepeat, newaction;
        public string program;
        int editIndex;
        protected String m_Actions = Global.appdatapath + "\\Actions.xml";
        string oldprofilename;
        public SpecActions(Options opt, string edit = "", int editindex = -1)
        {
            InitializeComponent();
            this.opt = opt;
            cBProfiles.Items.Add("[none]");
            cBProfiles.SelectedIndex = 0;
            cBActions.SelectedIndex = 0;
            foreach (object s in opt.root.lBProfiles.Items)
                cBProfiles.Items.Add(s.ToString());
            editIndex = editindex;
            if (edit != string.Empty)
            {
                oldprofilename = edit;
                tBName.Text = edit;
                LoadAction();
            }
        }

        void LoadAction()
        {
            SpecialAction act = Global.GetAction(oldprofilename);
            foreach (string s in act.controls.Split('/'))
                foreach (ListViewItem lvi in lVTrigger.Items)
                    if (lvi.Text == s)
                    {
                        lvi.Checked = true;
                        break;
                    }
            switch (act.type)
            {
                case "Macro":
                    cBActions.SelectedIndex = 1; 
                    macrostag = act.macro; 
                    lbMacroRecorded.Text = "Macro Recored";
                    cBMacroScanCode.Checked = act.keyType.HasFlag(DS4KeyType.ScanCode);
                    break;
                case "Program": cBActions.SelectedIndex = 2; LoadProgram(act.details); break;
                case "Profile": 
                    cBActions.SelectedIndex = 3;
                    cBProfiles.Text = act.details;
                    foreach (string s in act.ucontrols.Split('/'))
                        foreach (ListViewItem lvi in lVUnloadTrigger.Items)
                            if (lvi.Text == s)
                            {
                                lvi.Checked = true;
                                break;
                            }
                    break;
            }
        }

        private void btnRecordMacro_Click(object sender, EventArgs e)
        {
            rb = new RecordBox(this);
            rb.TopLevel = false;
            rb.Dock = DockStyle.Fill;
            rb.Visible = true;
            Controls.Add(rb);
            rb.BringToFront();
        }

        private void lVUnloadTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBProfiles.SelectedIndex > 0)
            {
                btnSetUTrigger.Enabled = true;
            }
            else
            {
                btnSetUTrigger.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (SpecialAction sA in Global.GetActions())
            {
                if ((sA.name == tBName.Text && editIndex > -1 && tBName.Text != oldprofilename) ||
                   (sA.name == tBName.Text && editIndex == -1))
                {
                    MessageBox.Show(Properties.Resources.ActionExists);
                    return;
                }
            }
            controls.Clear();
            ucontrols.Clear();
            foreach (ListViewItem lvi in lVTrigger.Items)
                    if (lvi.Checked)
                        controls.Add(lvi.Text);
            if (cBActions.SelectedIndex == 3)
            {
                foreach (ListViewItem lvi in lVUnloadTrigger.Items)
                    if (lvi.Checked)
                        ucontrols.Add(lvi.Text);
            }
            if (!string.IsNullOrEmpty(tBName.Text) && controls.Count > 0 && cBActions.SelectedIndex > 0)
            {      
                bool actRe = false;
                string action = "null";
                bool edit = (!string.IsNullOrEmpty(oldprofilename) && tBName.Text == oldprofilename);
                switch (cBActions.SelectedIndex)
                {
                    case 1:
                        if (macrostag.Count > 0)
                        {
                            action = "Macro" + (cBMacroScanCode.Checked ? " (Scan Code)" : "");                            
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, String.Join("/", macrostag), edit, (cBMacroScanCode.Checked ? "Scan Code" : ""));
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(program))
                        {
                            action = Properties.Resources.LaunchProgram.Replace("*program*", lbProgram.Text);
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, program, edit);
                        }
                        break;
                    case 3:
                        if (cBProfiles.SelectedIndex > 0 && ucontrols.Count > 0)
                        {
                            action = Properties.Resources.LoadProfile.Replace("*profile*", cBProfiles.Text);
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, cBProfiles.Text, edit, String.Join("/", ucontrols));
                        }
                        else
                        {
                            btnSetUTrigger.ForeColor = Color.Red;
                        }
                        break;
                }
                if (actRe)
                {                    
                    ListViewItem lvi = new ListViewItem(tBName.Text);
                    lvi.SubItems.Add(String.Join(", ", controls));
                    lvi.SubItems.Add(action);
                    lvi.Checked = true;
                    if (editIndex > -1)
                        opt.lVActions.Items.RemoveAt(editIndex);
                    opt.lVActions.Items.Add(lvi);
                    Close();
                }
            }
            else if (string.IsNullOrEmpty(tBName.Text))
            {
                lbName.ForeColor = Color.Red;
            }
            else
            {
                btnBorder.BackColor = Color.Red;
            }
        }

        private void tBName_TextChanged(object sender, EventArgs e)
        {
            lbName.ForeColor = Color.Black;
            if (tBName.Text.Contains('/')) { tBName.ForeColor = Color.Red; btnSave.Enabled = false; }
            else { tBName.ForeColor = Color.Black; btnSave.Enabled = true; }
        }

        private void cBActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cBActions.SelectedIndex)
            {
                default:
                    pnlMacro.Visible = false;
                    pnlProgram.Visible = false;
                    pnlProfile.Visible = false;
                    btnSave.Enabled = false;
                    break;
                case 1:
                    pnlMacro.Visible = true;
                    pnlProgram.Visible = false;
                    pnlProfile.Visible = false;
                    btnSave.Enabled = true; 
                    break;
                case 2:
                    pnlMacro.Visible = false;
                    pnlProgram.Visible = true;
                    pnlProfile.Visible = false;
                    btnSave.Enabled = true;
                    break;
                case 3:
                    pnlMacro.Visible = false;
                    pnlProgram.Visible = false;
                    pnlProfile.Visible = true;
                    btnSave.Enabled = true;
                    break;
            }
        }

        private void btnBroswe_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadProgram(openFileDialog1.FileName);
        }

        void LoadProgram(string path)
        {
            pBProgram.Image = Icon.ExtractAssociatedIcon(path).ToBitmap();
                lbProgram.Text = Path.GetFileNameWithoutExtension(path);
                program = path;
        }

        private void lVTrigger_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnBorder.BackColor = SystemColors.ControlDark;
        }

        private void btnSetUTrigger_Click(object sender, EventArgs e)
        {
            lVTrigger.Visible = !lVTrigger.Visible;
            if (lVTrigger.Visible)
                btnSetUTrigger.Text = "Set Unload Trigger";
            else
                btnSetUTrigger.Text = "Set Regular Trigger";
        }

        private void lVUnloadTrigger_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnSetUTrigger.ForeColor = Color.Black;
        }
    }
}
