using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Xml;
namespace DS4Windows
{
    public partial class SpecActions : Form
    {
        Options opt;
        RecordBox rb;
        int device;
        public List<string> macros = new List<string>();
        public List<string> controls = new List<string>();
        public List<string> ucontrols = new List<string>();
        public List<int> macrostag = new List<int>();
        public List<int>[] multiMacrostag = { new List<int>(), new List<int>(), new List<int>() };
        public bool macrorepeat, newaction;
        public string program;
        int editIndex;
        protected String m_Actions = Global.appdatapath + "\\Actions.xml";
        string oldprofilename;
        bool loadingAction = true;
        public SpecActions(Options opt, string edit = "", int editindex = -1)
        {
            InitializeComponent();
            this.opt = opt;
            lbHoldForBatt.Text = lbHoldForProg.Text = lbHoldFor.Text;
            lbSecsBatt.Text = lbSecsBatt.Text = lbSecsBatt.Text;
            device = opt.device;
            cBProfiles.Items.Add(Properties.Resources.noneProfile);
            cBProfiles.SelectedIndex = 0;
            cBActions.SelectedIndex = 0;
            cBPressRelease.SelectedIndex = 0;
            /*if (Environment.OSVersion.Version.Major >= 10)
            {
                cBActions.Items.Add("Xbox Game DVR");
                cBTapDVR.SelectedIndex = 0;
                cBHoldDVR.SelectedIndex = 2;
                cBDTapDVR.SelectedIndex = 1;
            }*/
            cBActions.Items[7] = Properties.Resources.MultiAction;
            foreach (object s in opt.root.lBProfiles.Items)
                cBProfiles.Items.Add(s.ToString());
            editIndex = editindex;
            if (edit != string.Empty)
            {
                oldprofilename = edit;
                tBName.Text = edit;
                LoadAction();
                loadingAction = false;
            }
        }

        void LoadAction()
        {
            SpecialAction act = Global.GetAction(oldprofilename);
            string[] dets;
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
                case "Program": 
                    cBActions.SelectedIndex = 2; 
                    LoadProgram(act.details);
                    nUDProg.Value = (decimal)act.delayTime;
                    tBArg.Text = act.extra;
                    break;

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
                case "Key":
                    cBActions.SelectedIndex = 4;
                    int key = int.Parse(act.details);
                    btnSelectKey.Text = ((Keys)key).ToString() +
                        (act.keyType.HasFlag(DS4KeyType.ScanCode) ? " (SC)" : "") + 
                        (!string.IsNullOrEmpty(act.ucontrols) ? " (Toggle)" : "");
                    btnSelectKey.Tag = key;
                    if (act.pressRelease)
                        cBPressRelease.SelectedIndex = 1;
                    if (!string.IsNullOrEmpty(act.ucontrols))
                    {
                        //cBPressToggleKeys.SelectedIndex = 1;
                        foreach (string s in act.ucontrols.Split('/'))
                            foreach (ListViewItem lvi in lVUnloadTrigger.Items)
                                if (lvi.Text == s)
                                {
                                    lvi.Checked = true;
                                    break;
                                }
                    }
                    break;
                case "DisconnectBT":
                    cBActions.SelectedIndex = 5;
                    nUDDCBT.Value = (decimal)act.delayTime;
                    break;
                case "BatteryCheck":
                    cBActions.SelectedIndex = 6;
                    dets = act.details.Split(',');
                    nUDDCBatt.Value = (decimal)act.delayTime;
                    cBNotificationBatt.Checked = bool.Parse(dets[1]);
                    cbLightbarBatt.Checked = bool.Parse(dets[2]);
                    bnEmptyColor.BackColor = Color.FromArgb(byte.Parse(dets[3]), byte.Parse(dets[4]), byte.Parse(dets[5]));
                    bnFullColor.BackColor = Color.FromArgb(byte.Parse(dets[6]), byte.Parse(dets[7]), byte.Parse(dets[8]));
                    break;
                case "XboxGameDVR":
                   /* if (cBActions.Items.Count < 8)
                    {
                        cBActions.Items.Add("Xbox Game DVR");
                        cBTapDVR.SelectedIndex = 0;
                        cBHoldDVR.SelectedIndex = 2;
                        cBDTapDVR.SelectedIndex = 1;
                    }
                    cBActions.SelectedIndex = 7;
                    dets = act.details.Split(',');
                    if (int.Parse(dets[3]) == 0)
                        btnCustomDVRKey.Text = "Custom Key";
                    else
                        btnCustomDVRKey.Text = ((Keys)(int.Parse(dets[3]))).ToString();
                    btnCustomDVRKey.Tag = int.Parse(dets[3]);
                    cBTapDVR.SelectedIndex = int.Parse(dets[0]);
                    cBHoldDVR.SelectedIndex = int.Parse(dets[1]);
                    cBDTapDVR.SelectedIndex = int.Parse(dets[2]);*/
                    break;
                case "MultiAction":
                    cBActions.SelectedIndex = 7;
                    dets = act.details.Split(',');
                    for (int i = 0; i < 3; i++)
                    {
                        string[] macs = dets[i].Split('/');
                        foreach (string s in macs)
                        {
                            int v;
                            if (int.TryParse(s, out v))
                                multiMacrostag[i].Add(v);
                        }
                        switch (i)
                        {
                            case 0: btnSTapT.Text = macs.Length > 1 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                            case 1: btnHoldT.Text = macs.Length > 1 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                            case 2: btnDTapT.Text = macs.Length > 1 ? Properties.Resources.MacroRecorded : Properties.Resources.SelectMacro; break;
                        }
                    }
                    /*if (int.Parse(dets[3]) == 0)
                        btnCustomDVRKey.Text = "Custom Key";
                    else
                        btnCustomDVRKey.Text = ((Keys)(int.Parse(dets[3]))).ToString();
                    btnCustomDVRKey.Tag = int.Parse(dets[3]);
                    cBTapDVR.SelectedIndex = int.Parse(dets[0]);
                    cBHoldDVR.SelectedIndex = int.Parse(dets[1]);
                    cBDTapDVR.SelectedIndex = int.Parse(dets[2]);*/
                    break;
            }
        }

        private void btnRecordMacro_Click(object sender, EventArgs e)
        {
            rb = new RecordBox(this);
            rb.TopLevel = true;
            rb.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            rb.ShowDialog();
        }

        private void lVUnloadTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBProfiles.SelectedIndex > 0)
            {
                btnSetUTriggerProfile.Enabled = true;
            }
            else
            {
                btnSetUTriggerProfile.Enabled = false;
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
                foreach (ListViewItem lvi in lVUnloadTrigger.Items)
                    if (lvi.Checked)
                        ucontrols.Add(lvi.Text);
            if (!string.IsNullOrEmpty(tBName.Text) && controls.Count > 0 && cBActions.SelectedIndex > 0)
            {      
                bool actRe = false;
                string action = "null";
                string dets;
                bool edit = (!string.IsNullOrEmpty(oldprofilename) && tBName.Text == oldprofilename);
                switch (cBActions.SelectedIndex)
                {
                    case 1:
                        if (macrostag.Count > 0)
                        {
                            action = Properties.Resources.Macro + (cBMacroScanCode.Checked ? " (" + Properties.Resources.ScanCode + ")" : "");
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
                            Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, program + "?" + nUDProg.Value, edit, tBArg.Text);
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
                            btnSetUTriggerProfile.ForeColor = Color.Red;
                        break;
                    case 4:
                        if (btnSelectKey.Tag != null &&
                            (!btnSelectKey.Text.Contains("(Toggle)") || (btnSelectKey.Text.Contains("(Toggle)") && ucontrols.Count > 0)))
                        {
                            action = ((Keys)int.Parse(btnSelectKey.Tag.ToString())).ToString() + ((btnSelectKey.Text.Contains("(Toggle)") ? " (Toggle)" : ""));
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            if (btnSelectKey.Text.Contains("(Toggle)") && ucontrols.Count > 0)
                            {
                                string uaction;
                                if (cBPressRelease.SelectedIndex == 1)
                                    uaction = "Release";
                                else
                                    uaction = "Press";
                                Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, btnSelectKey.Tag.ToString() + (btnSelectKey.Text.Contains("(SC)") ? " Scan Code" : ""),
                                    edit, uaction + '\n' + String.Join("/", ucontrols));
                            }
                            else
                                Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, btnSelectKey.Tag.ToString() + (btnSelectKey.Text.Contains("(SC)") ? " Scan Code" : ""), edit);
                        }
                        else if (btnSelectKey.Tag == null)
                            btnSelectKey.ForeColor = Color.Red;
                        else if (ucontrols.Count == 0)
                            btnSetUTriggerKeys.ForeColor = Color.Red;
                        break;
                    case 5:
                        action = Properties.Resources.DisconnectBT;
                        actRe = true;
                        if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                            Global.RemoveAction(oldprofilename);
                        Global.SaveAction(tBName.Text, String.Join("/", controls), cBActions.SelectedIndex, Math.Round(nUDDCBT.Value, 1).ToString(), edit);
                        break;
                    case 6:
                        if (cbLightbarBatt.Checked || cBNotificationBatt.Checked)
                        {
                            action = Properties.Resources.CheckBattery;
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            dets = Math.Round(nUDDCBatt.Value, 1).ToString() + "|" + cBNotificationBatt.Checked + "|" + cbLightbarBatt.Checked + "|" +
                                bnEmptyColor.BackColor.R + "|" + bnEmptyColor.BackColor.G + "|" + bnEmptyColor.BackColor.B + "|" +
                                bnFullColor.BackColor.R + "|" + bnFullColor.BackColor.G + "|" + bnFullColor.BackColor.B;
                            Global.SaveAction(tBName.Text, string.Join("/", controls), cBActions.SelectedIndex, dets, edit);
                        }
                        else
                        {
                            cbLightbarBatt.ForeColor = Color.Red;
                            cBNotificationBatt.ForeColor = Color.Red;
                        }
                        break;
                    case 7:
                        if (multiMacrostag[0].Count + multiMacrostag[1].Count + multiMacrostag[2].Count > 0)
                        {
                            action = Properties.Resources.MultiAction;
                            actRe = true;
                            if (!string.IsNullOrEmpty(oldprofilename) && oldprofilename != tBName.Text)
                                Global.RemoveAction(oldprofilename);
                            //dets = cBTapDVR.SelectedIndex + "," + cBHoldDVR.SelectedIndex + "," + cBDTapDVR.SelectedIndex + "," + int.Parse(btnCustomDVRKey.Tag.ToString());
                            dets = string.Join("/", multiMacrostag[0]) + "," + string.Join("/", multiMacrostag[1]) + "," + string.Join("/", multiMacrostag[2]);
                            Global.SaveAction(tBName.Text, controls[0], cBActions.SelectedIndex, dets, edit);
                        }
                        else
                        {

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
            int i = cBActions.SelectedIndex;
            pnlMacro.Visible = i == 1;
            pnlProgram.Visible = i == 2;
            pnlProfile.Visible = i == 3;
            pnlKeys.Visible = i == 4;
            pnlDisconnectBT.Visible = i == 5;
            pnlBatteryCheck.Visible = i == 6;
            pnlGameDVR.Visible = i == 7;
            btnSave.Enabled = i > 0;
        }

        private void btnBroswe_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadProgram(openFileDialog1.FileName);
        }

        void LoadProgram(string path)
        {
            try
            {
                pBProgram.Image = Icon.ExtractAssociatedIcon(path).ToBitmap();
                lbProgram.Text = Path.GetFileNameWithoutExtension(path);
                program = path;
            }
            catch { }
        }

        private void lVTrigger_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnBorder.BackColor = SystemColors.ControlDark;
            if (cBActions.SelectedIndex == 7 && e.Item.Checked)
            {
                foreach (ListViewItem lvi in lVTrigger.Items)
                    if (lvi != null && lvi.Checked && lvi != e.Item)
                        lvi.Checked = false;
            }
        }

        private void btnSetUTrigger_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            lVTrigger.Visible = !lVTrigger.Visible;
            if (lVTrigger.Visible)
                button.Text = Properties.Resources.SetUnloadTrigger;
            else
                button.Text = Properties.Resources.SetRegularTrigger;
        }

        private void lVUnloadTrigger_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnSetUTriggerProfile.ForeColor = Color.Black;
            btnSetUTriggerKeys.ForeColor = Color.Black;
        }

        private void btnSelectKey_Click(object sender, EventArgs e)
        {
            new KBM360(this, btnSelectKey, true).ShowDialog();
        }

        private void btnSelectKey_TextChanged(object sender, EventArgs e)
        {
            btnSetUTriggerKeys.Visible = btnSelectKey.Text.Contains("(Toggle)");
            lbUnloadTipKey.Visible = btnSelectKey.Text.Contains("(Toggle)");
            cBPressRelease.Visible = btnSelectKey.Text.Contains("(Toggle)");
            if (!btnSelectKey.Text.Contains("(Toggle)"))
            {
                lVTrigger.Visible = true;
                btnSetUTriggerKeys.Text = "Set Unload Trigger";
            }
        }

        private void bnEmptyColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = bnEmptyColor.BackColor;
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                cbLightbarBatt.Checked = true;
                bnEmptyColor.BackColor = advColorDialog.Color;
                pBGraident.Refresh();
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void bnFullColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = bnFullColor.BackColor;
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                cbLightbarBatt.Checked = true;
                bnFullColor.BackColor = advColorDialog.Color;
                pBGraident.Refresh();
            }
            if (device < 4)
                DS4LightBar.forcelight[device] = false;
        }

        private void pBGraident_Paint(object sender, PaintEventArgs e)
        {

            System.Drawing.Drawing2D.LinearGradientBrush linGrBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
   new Point(0, pBGraident.Height),
   new Point(pBGraident.Width, pBGraident.Height),
   bnEmptyColor.BackColor,   // Opaque red
   bnFullColor.BackColor);  // Opaque blue

            Pen pen = new Pen(linGrBrush);
            // e.Graphics.DrawLine(pen, 0, 10, 200, 10);
            //e.Graphics.FillEllipse(linGrBrush, 0, 30, 200, 100);
            e.Graphics.FillRectangle(linGrBrush, 0, 0, pBGraident.Width, pBGraident.Height);
        }

        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (sender is Color && device < 4)
            {
                Color color = (Color)sender;
                DS4Color dcolor = new DS4Color { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[device] = dcolor;
                DS4LightBar.forcedFlash[device] = 0;
                DS4LightBar.forcelight[device] = true;
            }
        }
        private void cBDVR_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (((ComboBox)sender).SelectedIndex == 3)
            {
                if (!loadingAction)
                    new KBM360(this, btnCustomDVRKey, false).ShowDialog();
                cBTapDVR.Items.RemoveAt(3);
                cBTapDVR.Items.Insert(3, "Take Screenshot (" + btnCustomDVRKey.Text + ")");
                cBHoldDVR.Items.RemoveAt(3);
                cBHoldDVR.Items.Insert(3, "Take Screenshot (" + btnCustomDVRKey.Text + ")");
                cBDTapDVR.Items.RemoveAt(3);
                cBDTapDVR.Items.Insert(3, "Take Screenshot (" + btnCustomDVRKey.Text + ")");
                ((ComboBox)sender).SelectedIndexChanged -= cBDVR_SelectedIndexChanged;
                ((ComboBox)sender).SelectedIndex = 3;
                ((ComboBox)sender).SelectedIndexChanged += cBDVR_SelectedIndexChanged;
            }*/
        }

        private void btnSTapT_Click(object sender, EventArgs e)
        {
            OpenRecordBox(0);
        }

        private void btnHoldT_Click(object sender, EventArgs e)
        {
            OpenRecordBox(1);
        }

        private void btnDTapT_Click(object sender, EventArgs e)
        {
            OpenRecordBox(2);
        }

        void OpenRecordBox(int i)
        {
            rb = new RecordBox(this, i);
            rb.TopLevel = true;
            rb.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            rb.ShowDialog();
        }
        private void cBBatt_CheckedChanged(object sender, EventArgs e)
        {
            cbLightbarBatt.ForeColor = Color.Black;
            cBNotificationBatt.ForeColor = Color.Black;
        }
    }
}