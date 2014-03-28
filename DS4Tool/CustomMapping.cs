using DS4Control;
using DS4Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScpServer
{
    public partial class CustomMapping : Form
    {
        private int device;
        private bool handleNextKeyPress = false;
        private bool MouseMoveAdded = false;
        private List<ComboBox> comboBoxes = new List<ComboBox>();
        private List<Button> buttons = new List<Button>();
        private Dictionary<string, string> defaults = new Dictionary<string, string>();
        private ComboBox lastSelected;
        private Button lastSelectedBn;
        private Dictionary<DS4Controls, GraphicsPath> pictureBoxZones = new Dictionary<DS4Controls, GraphicsPath>();
        private DS4Control.Control rootHub = null;
        private System.Windows.Forms.Control mappingControl = null;
        List<object> availableButtons = new List<object>();

        public CustomMapping(DS4Control.Control rootHub, int deviceNum)
        {
            InitializeComponent();
            this.rootHub = rootHub;
            device = deviceNum;
            DS4Color color = Global.loadColor(device);
            pictureBox.BackColor = Color.FromArgb(color.red, color.green, color.blue);
            foreach (System.Windows.Forms.Control control in this.Controls)
            {
                if (control is Button)
                    if (((Button)control).Text != "Save" && ((Button)control).Text != "Load")
                {
                    buttons.Add((Button)control);
                    availableButtons.Add(control.Text);
                    // Add defaults
                    defaults.Add(((Button)control).Name, ((Button)control).Text);
                    // Add events here (easier for modification/addition)
                    ((Button)control).Enter += new System.EventHandler(this.EnterCommand);
                    ((Button)control).KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownCommand);
                    ((Button)control).Enter += new System.EventHandler(this.TopofListChanged);
                    ((Button)control).KeyDown += new System.Windows.Forms.KeyEventHandler(this.TopofListChanged);
                    ((Button)control).KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressCommand);
                    ((Button)control).PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PreviewKeyDownCommand);
                    //lbControls.Items.Add(((Button)control).Text);
                    lbControls.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChangedCommand);
                }
            }
            availableButtons.Sort();
            foreach (string s in availableButtons)
                lbControls.Items.Add(s);
            Global.loadCustomMapping(Global.getCustomMap(device), buttons.ToArray());
        }

        private void EnterCommand(object sender, EventArgs e)
        {
            if (sender is Button)
            {
            lbControls.SetSelected(0, true);
            for (int i = 1; i < lbControls.Items.Count; i++)
                if (lbControls.Items[i].ToString().Contains(" (default)"))
                {
                    lbControls.Items[i] = lbControls.Items[i].ToString().Remove(lbControls.Items[i].ToString().Length - 10);
                }
            //Change image to represent button            
                lastSelectedBn = (Button)sender;
                switch (lastSelectedBn.Name)
                {
                    #region Set pictureBox.Image to relevant Properties.Resources image
                    case "bnL2": pictureBox.Image = Properties.Resources._2;
                        break;
                    case "bnL1": pictureBox.Image = Properties.Resources._3;
                        break;
                    case "bnR2": pictureBox.Image = Properties.Resources._4;
                        break;
                    case "bnR1": pictureBox.Image = Properties.Resources._5;
                        break;
                    case "bnUp": pictureBox.Image = Properties.Resources._6;
                        break;
                    case "bnLeft": pictureBox.Image = Properties.Resources._7;
                        break;
                    case "bnDown": pictureBox.Image = Properties.Resources._8;
                        break;
                    case "bnRight": pictureBox.Image = Properties.Resources._9;
                        break;
                    case "bnL3": pictureBox.Image = Properties.Resources._10;
                        break;
                    case "bnLY": pictureBox.Image = Properties.Resources._29;
                        break;
                    case "bnLX": pictureBox.Image = Properties.Resources._27;
                        break;
                    case "bnLY2": pictureBox.Image = Properties.Resources._28;
                        break;
                    case "bnLX2": pictureBox.Image = Properties.Resources._26;
                        break;
                    case "bnR3": pictureBox.Image = Properties.Resources._11;
                        break;
                    case "bnRY": pictureBox.Image = Properties.Resources._22;
                        break;
                    case "bnRX": pictureBox.Image = Properties.Resources._23;
                        break;
                    case "bnRY2": pictureBox.Image = Properties.Resources._24;
                        break;
                    case "bnRX2": pictureBox.Image = Properties.Resources._25;
                        break;
                    case "bnSquare": pictureBox.Image = Properties.Resources._12;
                        break;
                    case "bnCross": pictureBox.Image = Properties.Resources._13;
                        break;
                    case "bnCircle": pictureBox.Image = Properties.Resources._14;
                        break;
                    case "bnTriangle": pictureBox.Image = Properties.Resources._15;
                        break;
                    case "bnOptions": pictureBox.Image = Properties.Resources._16;
                        break;
                    case "bnShare": pictureBox.Image = Properties.Resources._17;
                        break;
                    case "bnTouchpad": pictureBox.Image = Properties.Resources._18;
                        break;
                    case "bnTouchUpper": pictureBox.Image = Properties.Resources._20;
                        break;
                    case "bnTouchMulti": pictureBox.Image = Properties.Resources._21;
                        break;
                    case "bnPS": pictureBox.Image = Properties.Resources._19;
                        break;
                    default: pictureBox.Image = Properties.Resources._1;
                        break;
                    #endregion
                }
                if (lastSelectedBn.ForeColor == Color.Red)
                    cbRepeat.Checked = true;
                else cbRepeat.Checked = false;
                if (lastSelectedBn.Font.Bold)
                    cbScanCode.Checked = true;
                else cbScanCode.Checked = false;
            }

            //Show certain list item as default button
            for (int i = 1; i < lbControls.Items.Count; i++)
                if (defaults[((Button)sender).Name] == lbControls.Items[i].ToString())
                {
                    for (int j = lbControls.Items.Count-1; j >= 1; j--)
                        lbControls.Items.RemoveAt(j);
                    foreach (string s in availableButtons)                        
                        lbControls.Items.Add(s);
                    for (int t = 1; t < lbControls.Items.Count; t++)
                        if (defaults[((Button)sender).Name] == lbControls.Items[t].ToString())
                        {
                            lbControls.Items[t] = lbControls.Items[t] + " (default)";
                            string temp = lbControls.Items[t].ToString();
                            lbControls.Items.RemoveAt(t);
                            lbControls.Items.Insert(1, temp);
                            break;
                        }
                    if (((Button)sender).Name.Contains("bnLX") || ((Button)sender).Name.Contains("bnLY") || ((Button)sender).Name.Contains("bnRX") || ((Button)sender).Name.Contains("bnRY"))
                    {
                        lbControls.Items.Insert(2, "Mouse Right");
                        lbControls.Items.Insert(2, "Mouse Left");
                        lbControls.Items.Insert(2, "Mouse Down");
                        lbControls.Items.Insert(2, "Mouse Up");
                    }
                    break;
                }
        }

        private void PreviewKeyDownCommand(object sender, PreviewKeyDownEventArgs e)
        {
            if (sender is Button)
            {
                if (e.KeyCode == Keys.Tab)
                    if (((Button)sender).Text.Length == 0)
                    {
                        ((Button)sender).Tag = e.KeyValue;
                        ((Button)sender).Text = e.KeyCode.ToString();
                        handleNextKeyPress = true;
                    }
            }
        }

        private void KeyDownCommand(object sender, KeyEventArgs e)
        {
            if (sender is Button)
                if (((Button)sender).Text != "Save" && ((Button)sender).Text != "Load")
                {
                lbControls.Items[0] = ((Button)sender).Text;
                if (((Button)sender).Tag is int)
                {
                    if (e.KeyValue == (int)(((Button)sender).Tag)
                        && !((Button)sender).Name.Contains("Touch"))
                    {
                        if (((Button)sender).ForeColor == SystemColors.WindowText)
                        {
                            ((Button)sender).ForeColor = Color.Red;
                            cbRepeat.Checked = true;
                        }
                        else
                        {
                            ((Button)sender).ForeColor = SystemColors.WindowText;
                            cbRepeat.Checked = false;
                        }
                        return;
                    }
                }
                if (((Button)sender).Text.Length != 0)
                    ((Button)sender).Text = string.Empty;
                else if (e.KeyCode == Keys.Delete)
                {
                    ((Button)sender).Tag = e.KeyValue;
                    ((Button)sender).Text = e.KeyCode.ToString();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                if (e.KeyCode != Keys.Delete)
                {
                    ((Button)sender).Tag = e.KeyValue;
                    ((Button)sender).Text = e.KeyCode.ToString();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void KeyPressCommand(object sender, KeyPressEventArgs e)
        {
            if (handleNextKeyPress)
            {
                e.Handled = true;
                handleNextKeyPress = false;
            }
        }
        private void SelectedIndexChangedCommand(object sender, EventArgs e)
        {
            if (lbControls.SelectedIndex > 0)
            {
                if (lastSelectedBn.Text != lbControls.Items[lbControls.SelectedIndex].ToString())
                {
                    if (lbControls.Items[lbControls.SelectedIndex].ToString().Contains(" (default)"))
                    {
                        lastSelectedBn.Text = lbControls.Items[lbControls.SelectedIndex].ToString().Remove(lbControls.Items[lbControls.SelectedIndex].ToString().Length - 10);
                        lastSelectedBn.Tag = lbControls.Items[lbControls.SelectedIndex].ToString().Remove(lbControls.Items[lbControls.SelectedIndex].ToString().Length - 10);
                    }
                    else
                    lastSelectedBn.Text = lbControls.Items[lbControls.SelectedIndex].ToString();
                    lastSelectedBn.Tag = lbControls.Items[lbControls.SelectedIndex].ToString();
                }
            }
        }
        private void cbRepeat_CheckedChanged(object sender, EventArgs e)
        {
            if (!lastSelectedBn.Name.Contains("Touch") && (lastSelectedBn.Tag is int || lastSelectedBn.Tag is UInt16))
                if (cbRepeat.Checked)
                    lastSelectedBn.ForeColor = Color.Red;
                else lastSelectedBn.ForeColor = SystemColors.WindowText;
            else
            {
                cbRepeat.Checked = false;
                lastSelectedBn.ForeColor = SystemColors.WindowText;
            }
        }
        private void cbScanCode_CheckedChanged(object sender, EventArgs e)
        {
            if (lastSelectedBn.Tag is int || lastSelectedBn.Tag is UInt16)
            //if (lastSelected.Tag is int || lastSelected.Tag is UInt16)
                if (cbScanCode.Checked)
                    lastSelectedBn.Font = new Font(lastSelectedBn.Font, FontStyle.Bold);
                else lastSelectedBn.Font = new Font(lastSelectedBn.Font, FontStyle.Regular);
            else
            {
                cbScanCode.Checked = false;
                lastSelectedBn.Font = new Font(lastSelectedBn.Font, FontStyle.Regular);
            }
        }

        private void TopofListChanged(object sender, EventArgs e)
        {
            if (((Button)sender).Text != "Save" && ((Button)sender).Text != "Load")
                lbControls.Items[0] = ((Button)sender).Text;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDg = new SaveFileDialog();
            saveFileDg.DefaultExt = "xml";
            saveFileDg.Filter = "SCP Custom Map Files (*.xml)|*.xml";
            saveFileDg.FileName = "SCP Custom Mapping.xml";
            if (saveFileDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (Global.saveCustomMapping(saveFileDg.FileName, buttons.ToArray()))
                {
                    if (MessageBox.Show("Custom mapping saved. Enable now?",
                        "Save Successfull", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        == System.Windows.Forms.DialogResult.Yes)
                    {
                        Global.setCustomMap(device, saveFileDg.FileName);
                        Global.Save();
                        Global.loadCustomMapping(device);
                    }
                }
                else MessageBox.Show("Custom mapping did not save successfully.", 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDg = new OpenFileDialog();
            openFileDg.CheckFileExists = true;
            openFileDg.CheckPathExists = true;
            openFileDg.DefaultExt = "xml";
            openFileDg.Filter = "SCP Custom Map Files (*.xml)|*.xml";
            openFileDg.Multiselect = false;
            if (openFileDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Global.loadCustomMapping(openFileDg.FileName, buttons.ToArray());
                Global.setCustomMap(device, openFileDg.FileName);
                Global.Save();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}
