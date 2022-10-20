using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.StickModifiers
{
    public static class StickOutCurve
    {
        public enum Curve : uint
        {
            Linear,
            EnhancedPrecision,
            Quadratic,
            Cubic,
            EaseoutQuad,
            EaseoutCubic,
            //Bezier,
        }

        public static void CalcOutValue(Curve type, double axisXValue, double axisYValue,
            out double axisOutXValue, out double axisOutYValue)
        {
            if (type == Curve.Linear)
            {
                axisOutXValue = axisXValue;
                axisOutYValue = axisYValue;
                return;
            }

            double r = Math.Atan2(axisYValue, axisXValue);
            //Console.WriteLine(r);
            double maxOutXRatio = Math.Abs(Math.Cos(r));
            double maxOutYRatio = Math.Abs(Math.Sin(r));
            double capX = axisXValue >= 0.0 ? maxOutXRatio * 1.0 : maxOutXRatio * 1.0;
            double capY = axisYValue >= 0.0 ? maxOutYRatio * 1.0 : maxOutYRatio * 1.0;
            double absSideX = Math.Abs(axisXValue); double absSideY = Math.Abs(axisYValue);
            if (absSideX > capX) capX = absSideX;
            if (absSideY > capY) capY = absSideY;
            double tempRatioX = capX > 0 ? axisXValue / capX : 0;
            double tempRatioY = capY > 0 ? axisYValue / capY : 0;
            //Console.WriteLine("{0} {1} {2}", axisYValue, tempRatioY, capY);
            double signX = tempRatioX >= 0.0 ? 1.0 : -1.0;
            double signY = tempRatioY >= 0.0 ? 1.0 : -1.0;

            double outputXValue = 0.0;
            double outputYValue = 0.0;
            switch (type)
            {
                case Curve.Linear:
                    outputXValue = axisXValue;
                    outputYValue = axisYValue;
                    break;

                case Curve.EnhancedPrecision:
                    {
                        double absX = Math.Abs(tempRatioX);
                        double absY = Math.Abs(tempRatioY);
                        double temp = outputXValue;

                        if (absX <= 0.4)
                        {
                            temp = 0.8 * absX;
                        }
                        else if (absX <= 0.75)
                        {
                            temp = absX - 0.08;
                        }
                        else if (absX > 0.75)
                        {
                            temp = (absX * 1.32) - 0.32;
                        }

                        outputXValue = signX * temp * capX;

                        if (absY <= 0.4)
                        {
                            temp = 0.8 * absY;
                        }
                        else if (absY <= 0.75)
                        {
                            temp = absY - 0.08;
                        }
                        else if (absY > 0.75)
                        {
                            temp = (absY * 1.32) - 0.32;
                        }

                        outputYValue = signY * temp * capY;
                    }

                    break;

                case Curve.Quadratic:
                    outputXValue = signX * tempRatioX * tempRatioX * capX;
                    outputYValue = signY * tempRatioY * tempRatioY * capY;
                    break;

                case Curve.Cubic:
                    outputXValue = tempRatioX * tempRatioX * tempRatioX * capX;
                    outputYValue = tempRatioY * tempRatioY * tempRatioY * capY;
                    break;

                case Curve.EaseoutQuad:
                    {
                        double absX = Math.Abs(tempRatioX);
                        double absY = Math.Abs(tempRatioY);
                        double outputX = absX * (absX - 2.0);
                        double outputY = absY * (absY - 2.0);

                        outputXValue = -1.0 * outputX * signX * capX;
                        outputYValue = -1.0 * outputY * signY * capY;
                    }

                    break;

                case Curve.EaseoutCubic:
                    {
                        double innerX = Math.Abs(tempRatioX) - 1.0;
                        double innerY = Math.Abs(tempRatioY) - 1.0;
                        double outputX = innerX * innerX * innerX + 1.0;
                        double outputY = innerY * innerY * innerY + 1.0;

                        outputXValue = 1.0 * outputX * signX * capX;
                        outputYValue = 1.0 * outputY * signY * capY;
                    }

                    break;

                //case Curve.Bezier:
                //    outputXValue = axisXValue * capX;
                //    outputYValue = axisYValue * capY;
                //    break;

                default:
                    outputXValue = axisXValue;
                    outputYValue = axisYValue;
                    break;
            }

            axisOutXValue = outputXValue;
            axisOutYValue = outputYValue;
        }
    }
}
