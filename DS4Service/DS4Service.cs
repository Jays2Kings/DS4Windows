using System;
using System.ComponentModel;
using System.ServiceProcess;
using DS4Control;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
namespace DS4Service
{
    public partial class DS4Service : ServiceBase
    {
        private Control rootHub;
        StreamWriter logWriter;
        string logFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\DS4Service.log";
        public DS4Service()
        {
            InitializeComponent();
            rootHub = new Control();
            rootHub.Debug += On_Debug;
            logWriter = File.AppendText(logFile);
        }

        protected override void OnStart(string[] args)
        {
            rootHub.Start();
        }

        protected override void OnStop()
        {
            rootHub.Stop();
        }

        protected void On_Debug(object sender, DebugEventArgs e)
        {
            logWriter.WriteLine(e.Time + ":\t" + e.Data);
            logWriter.Flush();
        }
    }
}
