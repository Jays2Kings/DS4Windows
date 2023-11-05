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
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class LogViewModel
    {
        //private object _colLockobj = new object();
        private ReaderWriterLockSlim _logListLocker = new ReaderWriterLockSlim();
        private ObservableCollection<LogItem> logItems = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> LogItems => logItems;

        public ReaderWriterLockSlim LogListLocker => _logListLocker;

        public LogViewModel(DS4Windows.ControlService service)
        {
            string version = DS4Windows.Global.exeversion;
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"DS4Windows version {version}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"DS4Windows Assembly Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"OS Version: {Environment.OSVersion}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"OS Product Name: {DS4Windows.Util.GetOSProductName()}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"OS Release ID: {DS4Windows.Util.GetOSReleaseId()}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"System Architecture: {(Environment.Is64BitOperatingSystem ? "x64" : "x32")}" });

            //logItems.Add(new LogItem { Datetime = DateTime.Now, Message = "DS4Windows version 2.0" });
            //BindingOperations.EnableCollectionSynchronization(logItems, _colLockobj);
            BindingOperations.EnableCollectionSynchronization(logItems, _logListLocker, LogLockCallback);
            service.Debug += AddLogMessage;
            DS4Windows.AppLogger.GuiLog += AddLogMessage;
        }

        private void LogLockCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            if (writeAccess)
            {
                using (WriteLocker locker = new WriteLocker(_logListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
            else
            {
                using (ReadLocker locker = new ReadLocker(_logListLocker))
                {
                    accessMethod?.Invoke();
                }
            }
        }

        private void AddLogMessage(object sender, DS4Windows.DebugEventArgs e)
        {
            LogItem item = new LogItem { Datetime = e.Time, Message = e.Data, Warning = e.Warning };
            _logListLocker.EnterWriteLock();
            logItems.Add(item);
            _logListLocker.ExitWriteLock();
            //lock (_colLockobj)
            //{
            //    logItems.Add(item);
            //}
        }
    }
}
