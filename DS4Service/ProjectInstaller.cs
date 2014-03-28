using System;
using System.ComponentModel;
using System.Configuration.Install;

namespace DS4Service 
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer 
    {
        public ProjectInstaller() 
        {
            InitializeComponent();
        }
    }
}
