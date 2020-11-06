using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace DS4WinWPF
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    public static class StartupMethods
    {
        public static string lnkpath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk";
        private static string taskBatPath = Path.Combine(DS4Windows.Global.exedirpath, "task.bat");

        public static bool HasStartProgEntry()
        {
            // Exception handling should not be needed here. Method handles most cases
            bool exists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            return exists;
        }

        public static bool HasTaskEntry()
        {
            TaskService ts = new TaskService();
            Task tasker = ts.FindTask("RunDS4Windows");
            return tasker != null;
        }

        public static void WriteStartProgEntry()
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(lnkpath);
                try
                {
                    string app = DS4Windows.Global.exelocation;
                    lnk.TargetPath = DS4Windows.Global.exelocation;
                    lnk.Arguments = "-m";

                    //lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    //lnk.Arguments = "-m";
                    lnk.IconLocation = app.Replace('\\', '/');
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public static void DeleteStartProgEntry()
        {
            if (File.Exists(lnkpath) && !new FileInfo(lnkpath).IsReadOnly)
            {
                File.Delete(lnkpath);
            }
        }

        public static void DeleteOldTaskEntry()
        {
            TaskService ts = new TaskService();
            Task tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null)
            {
                foreach(Microsoft.Win32.TaskScheduler.Action act in tasker.Definition.Actions)
                {
                    if (act.ActionType == TaskActionType.Execute)
                    {
                        ExecAction temp = act as ExecAction;
                        if (temp.Path != taskBatPath)
                        {
                            ts.RootFolder.DeleteTask("RunDS4Windows");
                            break;
                        }
                    }
                }
            }
        }

        public static bool CanWriteStartEntry()
        {
            bool result = false;
            if (!new FileInfo(lnkpath).IsReadOnly)
            {
                result = true;
            }

            return result;
        }

        public static void WriteTaskEntry()
        {
            DeleteTaskEntry();

            // Create new version of task.bat file using current exe
            // filename. Allow dynamic file
            RefreshTaskBat();

            TaskService ts = new TaskService();
            TaskDefinition td = ts.NewTask();
            td.Triggers.Add(new LogonTrigger());
            string dir = DS4Windows.Global.exedirpath;
            td.Actions.Add(new ExecAction($@"{dir}\task.bat",
                "",
                dir));

            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.DisallowStartIfOnBatteries = false;
            ts.RootFolder.RegisterTaskDefinition("RunDS4Windows", td);
        }

        public static void DeleteTaskEntry()
        {
            TaskService ts = new TaskService();
            Task tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null)
            {
                ts.RootFolder.DeleteTask("RunDS4Windows");
            }
        }

        public static bool CheckStartupExeLocation()
        {
            string lnkprogpath = ResolveShortcut(lnkpath);
            return lnkprogpath != DS4Windows.Global.exelocation;
        }

        public static void LaunchOldTask()
        {
            TaskService ts = new TaskService();
            Task tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null)
            {
                tasker.Run("");
            }
        }

        private static string ResolveShortcut(string filePath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }

        private static void RefreshTaskBat()
        {
            string dir = DS4Windows.Global.exedirpath;
            string path = $@"{dir}\task.bat";
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using (StreamWriter w = new StreamWriter(fileStream))
            {
                string temp = string.Empty;
                w.WriteLine("@echo off"); // Turn off echo
                w.WriteLine("SET mypath=\"%~dp0\"");
                temp = $"cmd.exe /c start \"RunDS4Windows\" %mypath%\\{DS4Windows.Global.exeFileName} -m";
                w.WriteLine(temp);
                w.WriteLine("exit");
            }
        }
    }
}
