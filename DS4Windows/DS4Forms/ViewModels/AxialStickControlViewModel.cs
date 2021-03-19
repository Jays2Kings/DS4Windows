using System;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class AxialStickControlViewModel
    {
        private StickDeadZoneInfo stickInfo;

        public double DeadZoneX
        {
            get => Math.Round(stickInfo.xAxisDeadInfo.deadZone / 127d, 2);
            set
            {
                double temp = Math.Round(stickInfo.xAxisDeadInfo.deadZone / 127d, 2);
                if (temp == value) return;
                stickInfo.xAxisDeadInfo.deadZone = (int)Math.Round(value * 127d);
                DeadZoneXChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneXChanged;

        public double DeadZoneY
        {
            get => Math.Round(stickInfo.yAxisDeadInfo.deadZone / 127d, 2);
            set
            {
                double temp = Math.Round(stickInfo.yAxisDeadInfo.deadZone / 127d, 2);
                if (temp == value) return;
                stickInfo.yAxisDeadInfo.deadZone = (int)Math.Round(value * 127d);
                DeadZoneYChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneYChanged;

        public double MaxZoneX
        {
            get => stickInfo.xAxisDeadInfo.maxZone / 100.0;
            set => stickInfo.xAxisDeadInfo.maxZone = (int)(value * 100.0);
        }

        public double MaxZoneY
        {
            get => stickInfo.yAxisDeadInfo.maxZone / 100.0;
            set => stickInfo.yAxisDeadInfo.maxZone = (int)(value * 100.0);
        }

        public double AntiDeadZoneX
        {
            get => stickInfo.xAxisDeadInfo.antiDeadZone / 100.0;
            set => stickInfo.xAxisDeadInfo.antiDeadZone = (int)(value * 100.0);
        }

        public double AntiDeadZoneY
        {
            get => stickInfo.yAxisDeadInfo.antiDeadZone / 100.0;
            set => stickInfo.yAxisDeadInfo.antiDeadZone = (int)(value * 100.0);
        }

        public double MaxOutputX
        {
            get => stickInfo.xAxisDeadInfo.maxOutput / 100.0;
            set => stickInfo.xAxisDeadInfo.maxOutput = value * 100.0;
        }

        public double MaxOutputY
        {
            get => stickInfo.yAxisDeadInfo.maxOutput / 100.0;
            set => stickInfo.yAxisDeadInfo.maxOutput = value * 100.0;
        }

        public AxialStickControlViewModel(StickDeadZoneInfo deadInfo)
        {
            this.stickInfo = deadInfo;
        }
    }
}
