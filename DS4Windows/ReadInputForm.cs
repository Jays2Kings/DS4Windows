using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DS4Control;
namespace ScpServer
{
    public enum InputType : int { Keyboard, Controller }
    public partial class ReadInputForm : Form
    {
        private DS4Library.DS4Device device = null;
        private DateTime startTime;
        private bool repeatKey = false;
        private Keys keyCode;
        private int keyValue;
        private X360Controls X360Control = X360Controls.Unbound;
        private int timeOut = 10;
        private bool finished = false;
        private InputType inputType;
        private bool readingKey = false;

        public bool RepeatKey { get { return repeatKey; } }
        public X360Controls X360Input { get { return X360Control; } }
        public Keys KeyCode { get { return keyCode; } }
        public int KeyValue { get { return keyValue; } }
        public DS4Library.DS4Device DS4Device { set { device = value; } }
        public InputType InputType { get { return inputType; } set { inputType = value; } }

        public ReadInputForm()
        {
            InitializeComponent();
        }

        private void ReadInputForm_Shown(object sender, EventArgs e)
        {
            timeOutTimer.Enabled = true;
            if (device != null && inputType == InputType.Controller)
                new System.Threading.Thread(readX360Control).Start();
        }

        private void ReadInputForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (inputType == InputType.Keyboard && !readingKey)
            {
                startTime = DateTime.UtcNow;
                this.keyCode = e.KeyCode;
                this.keyValue = e.KeyValue;
                readingKey = true;
            }
        }

        private void ReadInputForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (inputType == InputType.Keyboard)
            {
                if (this.keyCode == e.KeyCode && (startTime + TimeSpan.FromSeconds(3)) < DateTime.UtcNow)
                    repeatKey = true;
                this.keyCode = e.KeyCode;
                this.keyValue = e.KeyValue;
                readingKey = false;
                finished = true;
                this.Close();
            }
        }

        private void timeOutTimer_Tick(object sender, EventArgs e)
        {
            int time = Int32.Parse(timeLabel.Text);
            time--;
            if (time < 0)
            {
                Close();
                return;
            }
            timeLabel.Text = String.Format("{0}", time);
        }

        private void readX360Control()
        { 
            DS4Library.DS4State cState = new DS4Library.DS4State();
            X360Controls control = X360Controls.Unbound;
            DateTime timeStamp = DateTime.UtcNow;
            while (!finished && timeStamp + TimeSpan.FromSeconds(timeOut) > DateTime.UtcNow)
            {
                device.getCurrentState(cState);

                if (cState.Square)
                    control = X360Controls.X;
                else if (cState.Triangle)
                    control = X360Controls.Y;
                else if (cState.Cross)
                    control = X360Controls.A;
                else if (cState.Circle)
                    control = X360Controls.B;

                else if (cState.DpadUp)
                    control = X360Controls.DpadUp;
                else if (cState.DpadRight)
                    control = X360Controls.DpadRight;
                else if (cState.DpadDown)
                    control = X360Controls.DpadDown;
                else if (cState.DpadLeft)
                    control = X360Controls.DpadLeft;

                else if (cState.Share)
                    control = X360Controls.Back;
                else if (cState.Options)
                    control = X360Controls.Start;
                else if (cState.L1)
                    control = X360Controls.LB;
                else if (cState.R1)
                    control = X360Controls.RB;
                else if (cState.L3)
                    control = X360Controls.LS;
                else if (cState.R3)
                    control = X360Controls.RS;
                else if (cState.PS)
                    control = X360Controls.Guide;
                else if (cState.L2 > 100)
                    control = X360Controls.LT;
                else if (cState.R2 > 100)
                    control = X360Controls.RT;
                else if (cState.LX < 35)
                    control = X360Controls.LXNeg;
                else if (cState.RX < 35)
                    control = X360Controls.RXNeg;
                else if (cState.LY < 35)
                    control = X360Controls.LYNeg;
                else if (cState.RY < 35)
                    control = X360Controls.RYNeg;
                else if (cState.LX > 220)
                    control = X360Controls.LXPos;
                else if (cState.RX > 220)
                    control = X360Controls.RXPos;
                else if (cState.LY > 220)
                    control = X360Controls.LYPos;
                else if (cState.RY > 220)
                    control = X360Controls.RYPos;

                if (control != X360Controls.Unbound)
                {
                    finished = true;
                    X360Control = control;
                    this.Invoke(new EventHandler(
                        delegate
                        {
                            Close(); 
                        }));
                }
            }
        }
    }
}
