using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32.TaskScheduler;

namespace DS4Windows
{
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // Add "global\" in front of the EventName, then only one instance is allowed on the
        // whole system, including other users. But the application can not be brought
        // into view, of course. 
        private static string SingleAppComEventName = "{a52b5b20-d9ee-4f32-8518-307fa14aa0c6}";
        private static EventWaitHandle threadComEvent = null;
        private static bool exitComThread = false;
        public static ControlService rootHub;
        private static Thread testThread;
        private static Thread controlThread;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ja");
            for (int i = 0, argsLen = args.Length; i < argsLen; i++)
            {
                string s = args[i];
                if (s == "driverinstall" || s == "-driverinstall")
                {
                    Application.EnableVisualStyles();
                    Application.Run(new WelcomeDialog(true));
                    return;
                }
                else if (s == "re-enabledevice" || s == "-re-enabledevice")
                {
                    try
                    {
                        i++;
                        string deviceInstanceId = args[i];
                        DS4Devices.reEnableDevice(deviceInstanceId);
                        Environment.ExitCode = 0;
                        return;
                    }
                    catch (Exception)
                    {
                        Environment.ExitCode = Marshal.GetLastWin32Error();
                        return;
                    }
                }
                else if (s == "runtask" || s == "-runtask")
                {
                    TaskService ts = new TaskService();
                    Task tasker = ts.FindTask("RunDS4Windows");
                    if (tasker != null)
                    {
                        tasker.Run("");
                    }

                    Environment.ExitCode = 0;
                    return;
                }
            }

            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;

            try
            {
                Process.GetCurrentProcess().PriorityClass = 
                    ProcessPriorityClass.High;
            }
            catch
            {
                // Ignore problems raising the priority.
            }

            try
            {
                // another instance is already running if OpenExsting succeeds.
                threadComEvent = EventWaitHandle.OpenExisting(SingleAppComEventName);
                threadComEvent.Set();  // signal the other instance.
                threadComEvent.Close();
                return;    // return immediatly.
            }
            catch { /* don't care about errors */     }

            // Create the Event handle
            threadComEvent = new EventWaitHandle(false, EventResetMode.ManualReset, SingleAppComEventName);
            //System.Threading.Tasks.Task.Run(() => CreateTempWorkerThread());
            //CreateInterAppComThread();
            CreateTempWorkerThread();
            //System.Threading.Tasks.Task.Run(() => { Thread.CurrentThread.Priority = ThreadPriority.Lowest; CreateInterAppComThread(); Thread.CurrentThread.Priority = ThreadPriority.Lowest; }).Wait();

            //if (mutex.WaitOne(TimeSpan.Zero, true))
            //{
                createControlService();
                rootHub.createHidGuardKey();
                //rootHub = new ControlService();
                Application.EnableVisualStyles();
                Application.Run(new DS4Form(args));
                rootHub.removeHidGuardKey();
            //mutex.ReleaseMutex();
            //}

            exitComThread = true;
            threadComEvent.Set();  // signal the other instance.
            while (testThread.IsAlive)
                Thread.SpinWait(500);
            threadComEvent.Close();
        }

        private static void createControlService()
        {
            controlThread = new Thread(() => { rootHub = new ControlService(); });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private static void CreateTempWorkerThread()
        {
            testThread = new Thread(singleAppComThread_DoWork);
            testThread.Priority = ThreadPriority.Lowest;
            testThread.IsBackground = true;
            testThread.Start();
        }

        private static void singleAppComThread_DoWork()
        {
            WaitHandle[] waitHandles = new WaitHandle[] { threadComEvent };

            while (!exitComThread)
            {
                // check every second for a signal.
                if (WaitHandle.WaitAny(waitHandles) == 0)
                {
                    threadComEvent.Reset();
                    // The user tried to start another instance. We can't allow that, 
                    // so bring the other instance back into view and enable that one. 
                    // That form is created in another thread, so we need some thread sync magic.
                    if (!exitComThread && Application.OpenForms.Count > 0)
                    {
                        Form mainForm = Application.OpenForms[0];
                        mainForm?.Invoke(new SetFormVisableDelegate(ThreadFormVisable), mainForm);
                    }
                }
            }
        }

        /// <summary>
        /// When this method is called using a Invoke then this runs in the thread
        /// that created the form, which is nice. 
        /// </summary>
        /// <param name="frm"></param>
        private delegate void SetFormVisableDelegate(Form frm);
        static private void ThreadFormVisable(Form frm)
        {
            if (frm != null)
            {
                if (frm is DS4Form)
                {
                    // display the form and bring to foreground.
                    frm.WindowState = FormWindowState.Normal;
                    frm.Focus();
                }
                else
                {
                    WinProgs wp = (WinProgs)frm;
                    wp.form.mAllowVisible = true;
                    wp.ShowMainWindow();
                    SetForegroundWindow(wp.form.Handle);
                }
            }

            SetForegroundWindow(frm.Handle);
        }
    }
}