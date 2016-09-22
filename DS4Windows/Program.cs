using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;

namespace DS4Windows
{
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        // Add "global\" in front of the EventName, then only one instance is allowed on the
        // whole system, including other users. But the application can not be brought
        // into view, of course. 
        private static String SingleAppComEventName = "{a52b5b20-d9ee-4f32-8518-307fa14aa0c6}";
        static Mutex mutex = new Mutex(true, "{FI329DM2-DS4W-J2K2-HYES-92H21B3WJARG}");
        private static BackgroundWorker singleAppComThread = null;
        private static EventWaitHandle threadComEvent = null;
        public static ControlService rootHub;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("he");
            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                if (s == "driverinstall" || s == "-driverinstall")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new WelcomeDialog());
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
            }
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
            try
            {
                Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
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
            threadComEvent = new EventWaitHandle(false, EventResetMode.AutoReset, SingleAppComEventName);
            CreateInterAppComThread();

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                rootHub = new ControlService();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DS4Form(args));
                mutex.ReleaseMutex();
            }

            // End the communication thread.
            singleAppComThread.CancelAsync();
            while (singleAppComThread.IsBusy)
                Thread.Sleep(50);
            threadComEvent.Close();
        }

        static private void CreateInterAppComThread()
        {
            singleAppComThread = new BackgroundWorker();
            singleAppComThread.WorkerReportsProgress = false;
            singleAppComThread.WorkerSupportsCancellation = true;
            singleAppComThread.DoWork += new DoWorkEventHandler(singleAppComThread_DoWork);
            singleAppComThread.RunWorkerAsync();
        }

        static private void singleAppComThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            WaitHandle[] waitHandles = new WaitHandle[] { threadComEvent };

            while (!worker.CancellationPending)
            {
                // check every second for a signal.
                if (WaitHandle.WaitAny(waitHandles, 1000) == 0)
                {
                    // The user tried to start another instance. We can't allow that, 
                    // so bring the other instance back into view and enable that one. 
                    // That form is created in another thread, so we need some thread sync magic.
                    if (Application.OpenForms.Count > 0)
                    {
                        Form mainForm = Application.OpenForms[0];
                        mainForm.Invoke(new SetFormVisableDelegate(ThreadFormVisable), mainForm);
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