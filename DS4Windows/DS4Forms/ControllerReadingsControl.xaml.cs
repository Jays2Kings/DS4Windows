using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NonFormTimer = System.Timers.Timer;
using DS4Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ControllerReadingsControl.xaml
    /// </summary>
    public partial class ControllerReadingsControl : UserControl
    {
        private enum LatencyWarnMode : uint
        {
            None,
            Caution,
            Warn,
        }

        private int deviceNum;
        private NonFormTimer readingTimer;
        private bool useTimer;
        private double lsDead;
        private double rsDead;
        private double sixAxisXDead;
        private double sixAxisZDead;
        private double l2Dead;
        private double r2Dead;

        public double LsDead
        {
            get => lsDead;
            set
            {
                lsDead = value;
                LsDeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LsDeadChanged;

        public double RsDead
        {
            get => rsDead;
            set
            {
                rsDead = value;
                RsDeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RsDeadChanged;

        public double SixAxisXDead
        {
            get => sixAxisXDead;
            set
            {
                sixAxisXDead = value;
                SixAxisDeadXChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SixAxisDeadXChanged;

        public double SixAxisZDead
        {
            get => sixAxisZDead;
            set
            {
                sixAxisZDead = value;
                SixAxisDeadZChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SixAxisDeadZChanged;

        public double L2Dead
        {
            get => l2Dead;
            set
            {
                l2Dead = value;
                L2DeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler L2DeadChanged;

        public double R2Dead
        {
            get => r2Dead;
            set
            {
                r2Dead = value;
                R2DeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler R2DeadChanged;

        private LatencyWarnMode warnMode;
        private LatencyWarnMode prevWarnMode;
        private const int canvasWidth = 130;
        private const int canvasMidpoint = canvasWidth / 2;

        public ControllerReadingsControl()
        {
            InitializeComponent();

            readingTimer = new NonFormTimer();
            readingTimer.Interval = 1000 / 60.0;

            LsDeadChanged += ChangeLsDeadControls;
            RsDeadChanged += ChangeRsDeadControls;
            SixAxisDeadXChanged += ChangeSixAxisDeadControls;
            SixAxisDeadZChanged += ChangeSixAxisDeadControls;
        }

        private void ChangeSixAxisDeadControls(object sender, EventArgs e)
        {
            sixAxisDeadEllipse.Width = sixAxisXDead * canvasWidth;
            sixAxisDeadEllipse.Height = sixAxisZDead * canvasWidth;
            Canvas.SetLeft(sixAxisDeadEllipse, canvasMidpoint - (sixAxisXDead * canvasWidth / 2.0));
            Canvas.SetTop(sixAxisDeadEllipse, canvasMidpoint - (sixAxisZDead * canvasWidth / 2.0));
        }

        private void ChangeRsDeadControls(object sender, EventArgs e)
        {
            rsDeadEllipse.Width = rsDead * canvasWidth;
            rsDeadEllipse.Height = rsDead * canvasWidth;
            Canvas.SetLeft(rsDeadEllipse, canvasMidpoint - (rsDead * canvasWidth / 2.0));
            Canvas.SetTop(rsDeadEllipse, canvasMidpoint - (rsDead * canvasWidth / 2.0));
        }

        private void ChangeLsDeadControls(object sender, EventArgs e)
        {
            lsDeadEllipse.Width = lsDead * canvasWidth;
            lsDeadEllipse.Height = lsDead * canvasWidth;
            Canvas.SetLeft(lsDeadEllipse, canvasMidpoint - (lsDead * canvasWidth / 2.0));
            Canvas.SetTop(lsDeadEllipse, canvasMidpoint - (lsDead * canvasWidth / 2.0));
        }

        public void UseDevice(int index)
        {
            deviceNum = index;
        }

        public void EnableControl(bool state)
        {
            if (state)
            {
                IsEnabled = true;
                useTimer = true;
                readingTimer.Elapsed += ControllerReadingTimer_Elapsed;
                readingTimer.Start();
            }
            else
            {
                IsEnabled = false;
                useTimer = false;
                readingTimer.Elapsed -= ControllerReadingTimer_Elapsed;
                readingTimer.Stop();
            }
        }

        private void ControllerReadingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            readingTimer.Stop();

            DS4Device ds = Program.rootHub.DS4Controllers[deviceNum];
            if (ds != null)
            {
                DS4StateExposed exposeState = Program.rootHub.ExposedState[deviceNum];
                DS4State baseState = Program.rootHub.getDS4State(deviceNum);
                DS4State interState = Program.rootHub.getDS4StateTemp(deviceNum);

                Dispatcher.Invoke(() =>
                {
                    int x = baseState.LX;
                    int y = baseState.LY;

                    Canvas.SetLeft(lsValRec, x / 255.0 * canvasWidth - 3);
                    Canvas.SetTop(lsValRec, y / 255.0 * canvasWidth - 3);
                    //bool mappedLS = interState.LX != x || interState.LY != y;
                    //if (mappedLS)
                    //{
                        Canvas.SetLeft(lsMapValRec, interState.LX / 255.0 * canvasWidth - 3);
                        Canvas.SetTop(lsMapValRec, interState.LY / 255.0 * canvasWidth - 3);
                    //}

                    x = baseState.RX;
                    y = baseState.RY;
                    Canvas.SetLeft(rsValRec, x / 255.0 * canvasWidth - 3);
                    Canvas.SetTop(rsValRec, y / 255.0 * canvasWidth - 3);
                    Canvas.SetLeft(rsMapValRec, interState.RX / 255.0 * canvasWidth - 3);
                    Canvas.SetTop(rsMapValRec, interState.RY / 255.0 * canvasWidth - 3);

                    x = exposeState.getAccelX() + 127;
                    y = exposeState.getAccelZ() + 127;
                    Canvas.SetLeft(sixAxisValRec, x / 255.0 * canvasWidth - 3);
                    Canvas.SetTop(sixAxisValRec, y / 255.0 * canvasWidth - 3);
                    Canvas.SetLeft(sixAxisMapValRec, Math.Min(Math.Max(interState.Motion.outputAccelX + 127.0, 0), 255.0) / 255.0 * canvasWidth - 3);
                    Canvas.SetTop(sixAxisMapValRec, Math.Min(Math.Max(interState.Motion.outputAccelZ + 127.0, 0), 255.0) / 255.0 * canvasWidth - 3);

                    l2Slider.Value = baseState.L2;
                    l2ValLbTrans.Y = Math.Min(interState.L2, Math.Max(0, 255)) / 255.0 * -70.0 + 77.0;
                    if (interState.L2 >= 255)
                    {
                        l2ValLbBrush.Color = Colors.Green;
                    }
                    else if (interState.L2 == 0)
                    {
                        l2ValLbBrush.Color = Colors.Red;
                    }
                    else
                    {
                        l2ValLbBrush.Color = Colors.Black;
                    }

                    r2Slider.Value = baseState.R2;
                    r2ValLbTrans.Y = Math.Min(interState.R2, Math.Max(0, 255)) / 255.0 * -70.0 + 77.0;
                    if (interState.R2 >= 255)
                    {
                        r2ValLbBrush.Color = Colors.Green;
                    }
                    else if (interState.R2 == 0)
                    {
                        r2ValLbBrush.Color = Colors.Red;
                    }
                    else
                    {
                        r2ValLbBrush.Color = Colors.Black;
                    }

                    gyroYawSlider.Value = baseState.Motion.gyroYawFull;
                    gyroPitchSlider.Value = baseState.Motion.gyroPitchFull;
                    gyroRollSlider.Value = baseState.Motion.gyroRollFull;

                    accelXSlider.Value = exposeState.getAccelX();
                    accelYSlider.Value = exposeState.getAccelY();
                    accelZSlider.Value = exposeState.getAccelZ();

                    double latency = ds.Latency;
                    int warnInterval = ds.getWarnInterval();
                    inputDelayLb.Content = string.Format(Properties.Resources.InputDelay,
                        latency.ToString());

                    if (latency > warnInterval)
                    {
                        warnMode = LatencyWarnMode.Warn;
                        inpuDelayBackBrush.Color = Colors.Red;
                        inpuDelayForeBrush.Color = Colors.White;
                    }
                    else if (latency > (warnInterval * 0.5))
                    {
                        warnMode = LatencyWarnMode.Caution;
                        inpuDelayBackBrush.Color = Colors.Yellow;
                        inpuDelayForeBrush.Color = Colors.Black;
                    }
                    else
                    {
                        warnMode = LatencyWarnMode.None;
                        inpuDelayBackBrush.Color = Colors.Transparent;
                        inpuDelayForeBrush.Color = SystemColors.WindowTextColor;
                    }

                    prevWarnMode = warnMode;
                });
            }

            if (useTimer)
            {
                readingTimer.Start();
            }
        }
    }
}
