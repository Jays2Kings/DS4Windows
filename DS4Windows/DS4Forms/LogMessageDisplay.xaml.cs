/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
            Regex urlReg = new Regex(@"http(s)?://([\w-]+.+[/]){1}([#&?][\w-]+)?");
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
