using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using MarkdownEngine = MdXaml.Markdown;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for Net8NoticeWindow.xaml
    /// </summary>
    public partial class Net8NoticeWindow : Window
    {
        private MessageBoxResult result = MessageBoxResult.No;
        public MessageBoxResult Result { get => result; }

        public Net8NoticeWindow()
        {
            InitializeComponent();
        }

        public Net8NoticeWindow(string message) : this()
        {
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

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            DialogResult = true;
            Close();
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            DialogResult = false;
            Close();
        }
    }
}
