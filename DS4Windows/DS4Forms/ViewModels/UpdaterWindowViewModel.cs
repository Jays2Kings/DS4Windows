using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HttpProgress;
using Newtonsoft.Json;
using MarkdownEngine = Markdown.Xaml.Markdown;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    class UpdaterWindowViewModel
    {
        public const string CHANGELOG_URI = "https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/Changelog.min.json";

        private string newversion;
        public string Newversion { get => newversion; }

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


        public UpdaterWindowViewModel(string newversion)
        {
            BuildTempDocument("Retrieving changelog info.Please wait...");
            this.newversion = newversion;
            //RetrieveChangelogInfo();
        }

        public async void RetrieveChangelogInfo()
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri(CHANGELOG_URI);
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
                    ChangelogInfo tempInfo = JsonConvert.DeserializeObject<ChangelogInfo>(temp);
                    BuildChangelogDocument(tempInfo);
                }
                catch (JsonSerializationException) { }
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
                ulong versionNumber = versionInfo.VersionNumberInfo.GetVersionNumber();
                if (versionNumber > DS4Windows.Global.exeversionLong)
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
            }

            ChangelogDocument = flow;
        }

        private void BuildTempDocument(string message)
        {
            FlowDocument flow = new FlowDocument();
            flow.Blocks.Add(new Paragraph(new Run(message)));
            ChangelogDocument = flow;
        }

        public void SetSkippedVersion()
        {
            if (!string.IsNullOrEmpty(newversion))
            {
                DS4Windows.Global.LastVersionChecked = newversion;
            }
        }

        public void BlankSkippedVersion()
        {
            DS4Windows.Global.LastVersionChecked = string.Empty;
        }
    }

    public class ChangelogInfo
    {
        private string latestVersion;
        private ChangeVersionNumberInfo latestVersionInfo;
        private DateTime updatedAt;
        private ChangelogVersions changelog;

        [JsonProperty("latest_version")]
        public string LatestVersion { get => latestVersion; set => latestVersion = value; }


        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get => updatedAt; set => updatedAt = value; }

        [JsonProperty("changelog")]
        public ChangelogVersions Changelog { get => changelog; set => changelog = value; }

        [JsonProperty("latest_version_number_info", Required = Required.Always)]
        public ChangeVersionNumberInfo LatestVersionInfo
        {
            get => latestVersionInfo;
            set => latestVersionInfo = value;
        }
    }

    public class ChangeVersionNumberInfo
    {
        private ushort majorPart;
        private ushort minorPart;
        private ushort buildPart;
        private ushort privatePart;

        [JsonProperty("majorPart")]
        public ushort MajorPart { get => majorPart; set => majorPart = value; }

        [JsonProperty("minorPart")]
        public ushort MinorPart { get => minorPart; set => minorPart = value; }

        [JsonProperty("buildPart")]
        public ushort BuildPart { get => buildPart; set => buildPart = value; }

        [JsonProperty("privatePart")]
        public ushort PrivatePart { get => privatePart; set => privatePart = value; }

        public ulong GetVersionNumber()
        {
            ulong temp = (ulong)majorPart << 48 | (ulong)minorPart << 32 |
                (ulong)buildPart << 16 | privatePart;
            return temp;
        }
    }

    public class ChangelogVersions
    {
        private List<ChangeVersionInfo> versions;

        [JsonProperty("versions")]
        public List<ChangeVersionInfo> Versions { get => versions; set => versions = value; }
    }

    public class ChangeVersionInfo
    {
        private string version;
        private ChangeVersionNumberInfo versionNumberInfo;
        private string baseHeader;
        private DateTime releaseDate;
        private List<VersionLogLocale> versionLocales;

        [JsonProperty("version_str")]
        public string Version { get => version; set => version = value; }

        [JsonProperty("base_header")]
        public string BaseHeader { get => baseHeader; set => baseHeader = value; }

        [JsonProperty("release_date")]
        public DateTime ReleaseDate { get => releaseDate; set => releaseDate = value; }

        [JsonProperty("locales")]
        public List<VersionLogLocale> VersionLocales { get => versionLocales; set => versionLocales = value; }

        [JsonProperty("version_number_info", Required = Required.Always)]
        public ChangeVersionNumberInfo VersionNumberInfo
        {
            get => versionNumberInfo; set => versionNumberInfo = value;
        }

        public VersionLogLocale ApplicableInfo(string culture)
        {
            Dictionary<string, VersionLogLocale> tempDict =
                new Dictionary<string, VersionLogLocale>();

            foreach (VersionLogLocale logLoc in versionLocales)
            {
                tempDict.Add(logLoc.Code, logLoc);
            }

            VersionLogLocale result = null;
            CultureInfo hairyLegs = null;
            try
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    hairyLegs = CultureInfo.GetCultureInfo(culture);
                }
            }
            catch (CultureNotFoundException) { }

            if (hairyLegs != null)
            {
                if (tempDict.ContainsKey(hairyLegs.Name))
                {
                    result = tempDict[hairyLegs.Name];
                }
                else if (tempDict.ContainsKey(hairyLegs.TwoLetterISOLanguageName))
                {
                    result =
                        tempDict[hairyLegs.TwoLetterISOLanguageName];
                }
            }

            if (result == null && versionLocales.Count > 0)
            {
                // Default to first entry if specific culture info not found
                result = versionLocales[0];
            }

            return result;
        }
    }

    public class VersionLogLocale
    {
        private string code;
        private string header;
        private List<string> logText;
        private string editor;
        private List<string> editorsNote;
        private DateTime editedAt;

        private string displayLogText;
        public string DisplayLogText { get => displayLogText; }

        public string Code { get => code; set => code = value; }
        public string Header { get => header; set => header = value; }

        [JsonProperty("log_text")]
        public List<string> LogText
        {
            get => logText;
            set
            {
                logText = value;
            }
        }

        [JsonProperty("editor")]
        public string Editor { get => editor; set => editor = value; }

        [JsonProperty("editors_note")]
        public List<string> EditorsNote { get => editorsNote; set => editorsNote = value; }

        [JsonProperty("updated_at")]
        public DateTime EditedAt { get => editedAt; set => editedAt = value; }

        public void BuildDisplayText()
        {
            displayLogText = string.Join("\n", logText);
        }
    }
}
