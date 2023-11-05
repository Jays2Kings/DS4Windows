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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HttpProgress;
using System.Text.Json;
using MarkdownEngine = MdXaml.Markdown;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ChangelogViewModel
    {
        private FlowDocument changelogDocument;
        public FlowDocument ChangelogDocument
        {
            get => changelogDocument;
            private set
            {
                if (changelogDocument == value) return;
                changelogDocument = value;
                ChangelogDocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ChangelogDocumentChanged;

        public ChangelogViewModel()
        {
            BuildTempDocument("Retrieving changelog info.Please wait...");
        }

        private void BuildTempDocument(string message)
        {
            FlowDocument flow = new FlowDocument();
            flow.Blocks.Add(new Paragraph(new Run(message)));
            ChangelogDocument = flow;
        }

        public async void RetrieveChangelogInfo()
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri(UpdaterWindowViewModel.CHANGELOG_URI);
            string filename = Path.Combine(Path.GetTempPath(), "Changelog.min.json");
            bool readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    await temp.ConfigureAwait(true);
                    if (temp.Result.IsSuccessStatusCode) readFile = true;
                }
                catch (System.Net.Http.HttpRequestException) { }
            }

            bool fileExists = File.Exists(filename);
            if (fileExists && readFile)
            {
                string temp = File.ReadAllText(filename).Trim();
                try
                {
                    JsonSerializerOptions options = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    options.Converters.Add(new DateTimeJsonConverter.DateTimeConverterUsingDateTimeParse());
                    ChangelogInfo tempInfo = JsonSerializer.Deserialize<ChangelogInfo>(temp, options);
                    BuildChangelogDocument(tempInfo);
                }
                catch (JsonException) {}
            }
            else if (!readFile)
            {
                BuildTempDocument("Failed to retrieve information");
            }

            if (fileExists)
            {
                File.Delete(filename);
            }
        }

        private void BuildChangelogDocument(ChangelogInfo tempInfo)
        {
            MarkdownEngine engine = new MarkdownEngine();
            FlowDocument flow = new FlowDocument();
            foreach (ChangeVersionInfo versionInfo in tempInfo.Changelog.Versions)
            {
                VersionLogLocale tmpLog = versionInfo.ApplicableInfo(DS4Windows.Global.UseLang);
                if (tmpLog != null)
                {
                    Paragraph tmpPar = new Paragraph();
                    string tmp = tmpLog.Header;
                    tmpPar.Inlines.Add(new Run(tmp) { Tag = "Header" });
                    flow.Blocks.Add(tmpPar);

                    tmpPar.Inlines.Add(new LineBreak());
                    tmpPar.Inlines.Add(new Run(versionInfo.ReleaseDate.ToUniversalTime().ToString("r")) { Tag = "ReleaseDate" });

                    tmpLog.BuildDisplayText();

                    FlowDocument tmpDoc = engine.Transform(tmpLog.DisplayLogText);
                    flow.Blocks.AddRange(new List<Block>(tmpDoc.Blocks));

                    tmpPar = new Paragraph();
                    flow.Blocks.Add(tmpPar);
                }
            }

            ChangelogDocument = flow;
        }
    }
}
