using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace DS4Windows.DS4Forms
{
    public partial class LanguagePackComboBox : UserControl
    {
        [Category("Action")]
        [Description("Fires when the combo box selected index is changed.")]
        public event EventHandler SelectedIndexChanged;

        [Category("Action")]
        [Description("Fires when the combo box selected value is changed.")]
        public event EventHandler SelectedValueChanged;

        [Category("Data")]
        [Description("Text used for invariant culture name in the combo box.")]
        [Localizable(true)]
        public string InvariantCultureText { get; set; }

        [Category("Data")]
        [Description("Text for label before the combo box.")]
        [Localizable(true)]
        public string LabelText {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return cbCulture.SelectedIndex; }
            set { cbCulture.SelectedIndex = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return cbCulture.SelectedText; }
            set { cbCulture.SelectedText = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get { return cbCulture.SelectedValue; }
            set { cbCulture.SelectedValue = value; }
        }

        public LanguagePackComboBox()
        {
            InitializeComponent();

            // Find the location where application installed.
            string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

            // Get all culture for which satellite folder found with culture code.
            Dictionary<string, string> cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => Directory.Exists(Path.Combine(exeLocation, c.Name)))
                .ToDictionary(c => c.Name, c => c.Equals(CultureInfo.InvariantCulture) ? this.InvariantCultureText : c.NativeName);

            cbCulture.DataSource = new BindingSource(cultures, null);
            cbCulture.SelectedValue = Thread.CurrentThread.CurrentUICulture.Name;

            // This must be set here instead of Designer or event would fire at initial selected value setting above.
            cbCulture.SelectedIndexChanged += new EventHandler(CbCulture_SelectedIndexChanged);
            cbCulture.SelectedValueChanged += new EventHandler(CbCulture_SelectedValueChanged);
        }

        private void LanguagePackComboBox_SizeChanged(object sender, EventArgs e)
        {
            cbCulture.Left = label1.Margin.Left + label1.Width + label1.Margin.Right;
            cbCulture.Width = this.Width - cbCulture.Left - cbCulture.Margin.Right - cbCulture.Margin.Left;
        }

        private void CbCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            this.SelectedIndexChanged?.Invoke(this, e);
            
        }

        private void CbCulture_SelectedValueChanged(object sender, EventArgs e)
        {
            this.SelectedValueChanged?.Invoke(this, e);
        }
    }
}
