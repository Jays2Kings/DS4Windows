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
            List<string> lookupPaths = Global.PROBING_PATH.Split(';')
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
            return lookupPaths.Select(path => Path.Combine(path, culture.Name,
                Global.LANGUAGE_ASSEMBLY_NAME))
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
