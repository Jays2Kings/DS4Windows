using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS4Windows
{
    public partial class Alreadyrunning : Form
    {
        Stopwatch sw;

        public Alreadyrunning()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            Hide();
            Timer t = new Timer();
            t.Start();
            t.Tick += t_Tick;
            sw = new Stopwatch();
            sw.Start();        
        }

        void t_Tick(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds >= 10)
                this.Close();
        }
    }
}
