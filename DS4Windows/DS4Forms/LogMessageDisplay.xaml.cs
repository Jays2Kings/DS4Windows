using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using MarkdownEngine = Markdown.Xaml.Markdown;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for LogMessageDisplay.xaml
    /// </summary>
    public partial class LogMessageDisplay : Window
    {
        public LogMessageDisplay()
        {
            InitializeComponent();
        }

        public LogMessageDisplay(string message) : this()
        {
            Regex urlReg = new Regex(@"http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?");
            message = urlReg.Replace(message, "[$0]($0)");

            MarkdownEngine engine = new MarkdownEngine();
            FlowDocument tmpDoc = engine.Transform(message);
            tmpDoc.TextAlignment = TextAlignment.Center;

            richMessageBox.CommandBindings.Add(new CommandBinding(
                NavigationCommands.GoToPage,
                (sender, e) =>
                {
                    Process proc = new Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = (string)e.Parameter;

                    proc.Start();
                }));

            richMessageBox.Document = tmpDoc;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
