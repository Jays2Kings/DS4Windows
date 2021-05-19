using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class LanguagePackViewModel
    {
        private List<LangPackItem> langPackList;
        private const string invariantCultureTextValue = "No (English UI)";

        // If probing path has been changed in App.config, add the same string here.
        private const string probingPath = "Lang";

        // Filter language assembly file names in order to ont include irrelevant assemblies to the combo box.
        private const string languageAssemblyName = "DS4Windows.resources.dll";
        //private const string languageAssemblyName = "DS4WinWPF.resources.dll";

        private int selectedIndex;

        public List<LangPackItem> LangPackList { get => langPackList; }
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;
        public event EventHandler ScanFinished;

        public LanguagePackViewModel()
        {
        }

        public void ScanForLangPacks()
        {
            string tempculture = Thread.CurrentThread.CurrentUICulture.Name;
            //string tempculture = new CultureInfo(Global.UseLang).Name;
            Task.Run(() =>
            {
                CreateLanguageAssembliesBindingSource();

                int index = langPackList.Select((item, idx) => new { item, idx })
                                        .Where(x => x.item.Name == tempculture)
                                        .Select(x => x.idx)
                                        .DefaultIfEmpty(-1)
                                        .First();
                if (index > -1)
                {
                    selectedIndex = index;
                }

                ScanFinished?.Invoke(this, EventArgs.Empty);
            });
        }

        public bool ChangeLanguagePack()
        {
            bool result = false;
            if (selectedIndex > -1)
            {
                LangPackItem item = langPackList[selectedIndex];
                string newValue = item.Name;
                if (newValue != Global.UseLang)
                {
                    Global.UseLang = newValue;
                    //Global.Save();
                    result = true;
                }
            }

            return result;
        }

        private void CreateLanguageAssembliesBindingSource()
        {
            // Find the location where application installed.
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> lookupPaths = probingPath.Split(';')
                .Select(path => Path.Combine(exeLocation, path))
                .Where(path => path != exeLocation)
                .ToList();
            lookupPaths.Insert(0, exeLocation);

            // Get all culture for which satellite folder found with culture code, then insert invariant culture at the beginning.
            langPackList = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => IsLanguageAssemblyAvailable(lookupPaths, c))
                .Select(c => new LangPackItem(c.Name, c.NativeName))
                .ToList();
            langPackList.Insert(0, new LangPackItem("", invariantCultureTextValue));
        }

        private bool IsLanguageAssemblyAvailable(List<string> lookupPaths, CultureInfo culture)
        {
            return lookupPaths.Select(path => Path.Combine(path, culture.Name, languageAssemblyName))
                .Where(path => File.Exists(path))
                .Count() > 0;
        }
    }

    public class LangPackItem
    {
        private string name;
        private string nativeName;

        public string Name { get => name; }
        public string NativeName { get => nativeName; }

        public LangPackItem(string name, string nativeName)
        {
            this.name = name;
            this.nativeName = nativeName;
        }
    }
}
