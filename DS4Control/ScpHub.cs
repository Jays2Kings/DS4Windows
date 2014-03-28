using System;
using System.ComponentModel;

namespace DS4Control 
{
    public partial class ScpHub : Component 
    {
        protected IntPtr m_Reference = IntPtr.Zero;
        protected volatile Boolean m_Started = false;

        public event EventHandler<DebugEventArgs>   Debug   = null;
       
        public event EventHandler<ReportEventArgs>  Report  = null;

        protected virtual Boolean LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            On_Debug(this, args);

            return true;
        }

        public Boolean Active 
        {
            get { return m_Started; }
        }


        public ScpHub() 
        {
            InitializeComponent();
        }

        public ScpHub(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public virtual Boolean Open()  
        {
            return true;
        }

        public virtual Boolean Start() 
        {
            return m_Started;
        }

        public virtual Boolean Stop()  
        {
            return !m_Started;
        }

        public virtual Boolean Close() 
        {
            if (m_Reference != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Reference);

            return !m_Started;
        }


        public virtual Boolean Suspend() 
        {
            return true;
        }

        public virtual Boolean Resume()  
        {
            return true;
        }

        protected virtual void On_Debug(object sender, DebugEventArgs e)     
        {
            if (Debug != null) Debug(sender, e);
        }


        protected virtual void On_Report(object sender, ReportEventArgs e)   
        {
            if (Report != null) Report(sender, e);
        }
    }
}
