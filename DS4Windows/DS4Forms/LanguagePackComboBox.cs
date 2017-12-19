using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS4Windows.DS4Forms
{
    public partial class LanguagePackComboBox : UserControl
    {
        private string _invariantCultureText = "No (English UI)";
        private string _probingPath = "";
        private string _languageAssemblyName = "DS4Windows.resources.dll";
        private TaskCompletionSource<bool> _languageListInitialized = new TaskCompletionSource<bool>();

        [Category("Action")]
        [Description("Fires when the combo box selected index is changed.")]
        public event EventHandler SelectedIndexChanged;

        [Category("Action")]
        [Description("Fires when the combo box selected value is changed.")]
        public event EventHandler SelectedValueChanged;

        [Category("Data")]
        [Description("Text used for invariant culture name in the combo box.")]
        [Localizable(true)]
        public string InvariantCultureText
        {
            get { return _invariantCultureText; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("InvariantCultureText_Changed call will complete when ready, no need for a warning", "CS4014:Await.Warning")]
            set {
                _invariantCultureText = value;
                InvariantCultureText_Changed(value);
            }
        }

        [Category("Data")]
        [Description("Text for label before the combo box.")]
        [Localizable(true)]
        public string LabelText {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        [Category("Data")]
        [Description("If probing path has been changed in App.config, add the same string here.")]
        public string ProbingPath
        {
            get { return this._probingPath; }
            set { this._probingPath = value; }
        }

        [Category("Data")]
        [Description("Filter language assembly file names in order to ont include irrelevant assemblies to the combo box.")]
        public string LanguageAssemblyName
        {
            get { return this._languageAssemblyName; }
            set { this._languageAssemblyName = value; }
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
            cbCulture.Enabled = false;

            Task.Run(() => {
                // Find available language assemblies and bind the list to the combo box.
                cbCulture.DataSource = this.CreateLanguageAssembliesBindingSource();
                cbCulture.SelectedValue = Thread.CurrentThread.CurrentUICulture.Name;

                // This must be set here instead of Designer or event would fire at initial selected value setting above.
                cbCulture.SelectedIndexChanged += new EventHandler(CbCulture_SelectedIndexChanged);
                cbCulture.SelectedValueChanged += new EventHandler(CbCulture_SelectedValueChanged);

                cbCulture.Enabled = true;
                _languageListInitialized.SetResult(true);
            });
        }

        private BindingSource CreateLanguageAssembliesBindingSource()
        {
            // Find the location where application installed.
            string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            List<string> lookupPaths = this.ProbingPath.Split(';')
                .Select(path => Path.Combine(exeLocation, path))
                .Where(path => path != exeLocation)
                .ToList();
            lookupPaths.Insert(0, exeLocation);

            // Get all culture for which satellite folder found with culture code, then insert invariant culture at the beginning.
            List<KeyValuePair<string, string>> cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => this.IsLanguageAssemblyAvailable(lookupPaths, c))
                .Select(c => new KeyValuePair<string, string>(c.Name, c.NativeName))
                .ToList();
            cultures.Insert(0, new KeyValuePair<string, string>("", this.InvariantCultureText));

            return new BindingSource(cultures, null);
        }

        private bool IsLanguageAssemblyAvailable(List<string> lookupPaths, CultureInfo culture)
        {
            return lookupPaths.Select(path => Path.Combine(path, culture.Name, this.LanguageAssemblyName))
                .Where(path => File.Exists(path))
                .Count() > 0;
        }

        private async Task InvariantCultureText_Changed(string value)
        {
            // Normally the completion flag will be long set by the time this method is called.
            await _languageListInitialized.Task;
            BindingSource dataSource = ((BindingSource)cbCulture.DataSource);
            dataSource[0] = new KeyValuePair<string, string>("", value);
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
