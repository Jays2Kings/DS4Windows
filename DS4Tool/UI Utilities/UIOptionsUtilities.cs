using DS4Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS4Windows.UI_Utilities
{
    public struct XY
    {
        public double x;
        public double y;
    }

    public struct MinMax
    {
        public double min;
        public double max;
    }


    class UIOptionsUtilities
    {
        public static void AddButtonsToButtonCollection(System.Windows.Forms.Control.ControlCollection controls, List<Button> buttonCollection, string contains)
        {
            foreach (System.Windows.Forms.Control control in controls)
            {
                if (control is Button && !((Button)control).Name.Contains(contains))
                {
                    buttonCollection.Add((Button)control);
                }
            }
        }

        public static void ApplyMouseHoverEventToControls(System.Windows.Forms.Control.ControlCollection controls, EventHandler handler)
        {
            foreach (System.Windows.Forms.Control control in controls)
            {
                if (control.HasChildren)
                {
                    ApplyMouseHoverEventToControls(control.Controls, handler);
                }

                control.MouseHover += handler;
            }
        }

        public static void ApplyMouseHoverEventToButtonList(List<Button> buttons, EventHandler handler)
        {
            foreach (var button in buttons)
            {
                button.MouseHover += handler;
            }
        }

        public static void InitializeColorTrackBar(TrackBar bar, DS4Color color)
        {
            bar.Value = color.red;
            bar.Value = color.green;
            bar.Value = color.blue;
        }

        public static double TValue(double value1, double value2, double percent)
        {
            percent /= 100f;
            return value1 * percent + value2 * (1 - percent);
        }

        public static MinMax FindCurveMixMan(NumericUpDown upDown, double maxValue)
        {
            MinMax minMax;
            minMax.max = TValue(382.5d, maxValue, (double)upDown.Value);
            minMax.min = TValue(127.5d, maxValue, (double)upDown.Value);

            return minMax;
        }

        public static Point ConvertAxisToUIPoint(XY axis, Label track, double dpix, double dpiy)
        {
            return new Point((int)(dpix * axis.x / 2.09 + track.Location.X), (int)(dpiy * axis.y / 2.09 + track.Location.Y));
        }

        public static XY CalculateCurve(XY axisXY, MinMax minMax, double max)
        {
            XY curve;

            curve.x = (axisXY.x > 127.5f ? Math.Min(axisXY.x, (axisXY.x / max) * minMax.max) : Math.Max(axisXY.x, (axisXY.x / max) * minMax.min));
            curve.y = (axisXY.y > 127.5f ? Math.Min(axisXY.y, (axisXY.y / max) * minMax.max) : Math.Max(axisXY.y, (axisXY.y / max) * minMax.min));
            return curve;
        }

        public static void ProcessCurve(XY axisXY, Button stickTrack, Label labelTrack, NumericUpDown stickCurve, double dpix, double dpiy)
        {
            if (stickCurve.Value > 0)
            {
                double max = axisXY.x + axisXY.y;

                XY curve;

                MinMax minMax = FindCurveMixMan(stickCurve, max);
                if ((axisXY.x > 127.5d && axisXY.y > 127.5d) || (axisXY.x < 127.5d && axisXY.y < 127.5d))
                {
                    curve = CalculateCurve(axisXY, minMax, max);
                    stickTrack.Location = ConvertAxisToUIPoint(curve, labelTrack, dpix, dpiy);
                }
                else
                {
                    if (axisXY.x < 127.5d)
                    {
                        curve.x = Math.Min(axisXY.x, (axisXY.x / max) * minMax.max);
                        curve.y = Math.Min(axisXY.y, (-(axisXY.y / max) * minMax.max + 510));
                    }
                    else
                    {
                        curve.x = Math.Min(axisXY.x, (-(axisXY.x / max) * minMax.max + 510));
                        curve.y = Math.Min(axisXY.y, (axisXY.y / max) * minMax.max);
                    }
                    stickTrack.Location = ConvertAxisToUIPoint(curve, labelTrack, dpix, dpiy);
                }
            }
            else
            {
                stickTrack.Location = ConvertAxisToUIPoint(axisXY, labelTrack, dpix, dpiy);
            }
        }

        public static Point CalculateTrackBarPoint(TrackBar trackBar, double dpix, double dpiy)
        {
            return new Point(trackBar.Location.X - (int)(dpix * 15), (int)((dpix * (24 - trackBar.Value / 10.625) + 10)));
        }

        public static void ProcessBumperTrackPositionAndColor(TrackBar bar, Label barLabel, NumericUpDown upDown)
        {
            if (bar.Value == 255)
            {
                barLabel.ForeColor = Color.Green;
            }
            else if (bar.Value < (double)upDown.Value * 255.0d)
            {
                barLabel.ForeColor = Color.Red;
            }
            else
            {
                barLabel.ForeColor = Color.Black;
            }
        }
    }
}
