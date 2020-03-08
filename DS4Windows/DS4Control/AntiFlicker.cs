using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    public static class AntiFlicker
    {
        // old RX RY LX LY value
        static byte[,] oldAxis = new byte[5,4];
        //[device, axis]
        // 0 = LX
        // 1 = LY
        // 2 = RX
        // 3 = RY

        // old Real RX RY LX LY value
        //static byte[,] oldRealAxis = new byte[5, 4];
        static byte[,,] oldRealAxisPlus = new byte[5, 4, 13];
        //[device, axis, oldbyte]


        public static DS4State RemoveFlickAll(DS4State cState, DS4State pState, int device) 
        {
            //Console.WriteLine(cState.RY);
            
            if (device < 5)
            {
                int configL = Global.getLSAntiFlickerMode(device);
                int configR = Global.getRSAntiFlickerMode(device);

                // L
                if (configL != 0)
                {
                    switch (configL)
                    {
                        case 1:
                            cState.LX = RemoveFlicN1(cState.LX, device, 0);
                            cState.LY = RemoveFlicN1(cState.LY, device, 1);
                            break;
                        case 2:
                            cState.LX = RemoveFlicN2(cState.LX, device, 0);
                            cState.LY = RemoveFlicN2(cState.LY, device, 1);
                            break;
                        case 3:
                            cState.LX = RemoveFlicN1Dynamic(cState.LX, pState.LX, device, 0);
                            cState.LY = RemoveFlicN1Dynamic(cState.LY, pState.LY, device, 1);
                            break;
                        case 4:
                            cState.LX = RemoveFlicN2Dynamic(cState.LX, pState.LX, device, 0);
                            cState.LY = RemoveFlicN2Dynamic(cState.LY, pState.LY, device, 1);
                            break;
                        case 5:
                            cState.LX = RemoveFlicN1Dynamic2(cState.LX, pState.LX, device, 0);
                            cState.LY = RemoveFlicN1Dynamic2(cState.LY, pState.LY, device, 1);
                            break;
                        case 6:
                            cState.LX = RemoveFlicN2Dynamic2(cState.LX, pState.LX, device, 0);
                            cState.LY = RemoveFlicN2Dynamic2(cState.LY, pState.LY, device, 1);
                            break;
                        default:
                            break;
                    }
                }

                // R
                if (configR != 0)
                {
                    switch (configR)
                    {
                        case 1:
                            cState.RX = RemoveFlicN1(cState.RX, device, 2);
                            cState.RY = RemoveFlicN1(cState.RY, device, 3);
                            break;
                        case 2:
                            cState.RX = RemoveFlicN2(cState.RX, device, 2);
                            cState.RY = RemoveFlicN2(cState.RY, device, 3);
                            break;
                        case 3:
                            cState.RX = RemoveFlicN1Dynamic(cState.RX, pState.RX, device, 2);
                            cState.RY = RemoveFlicN1Dynamic(cState.RY, pState.RY, device, 3);
                            break;
                        case 4:
                            cState.RX = RemoveFlicN2Dynamic(cState.RX, pState.RX, device, 2);
                            cState.RY = RemoveFlicN2Dynamic(cState.RY, pState.RY, device, 3);
                            break;
                        case 5:
                            cState.RX = RemoveFlicN1Dynamic2(cState.RX, pState.RX, device, 2);
                            cState.RY = RemoveFlicN1Dynamic2(cState.RY, pState.RY, device, 3);
                            break;
                        case 6:
                            cState.RX = RemoveFlicN2Dynamic2(cState.RX, pState.RX, device, 2);
                            cState.RY = RemoveFlicN2Dynamic2(cState.RY, pState.RY, device, 3);
                            break;
                        default:
                            break;
                    }
                }
            }
            //Console.WriteLine(cState.RY);

            return cState;
        }

        static byte RemoveFlicN1(byte AxisValue, int device, int AxisId)
        {
            if (AxisValue != 0xFF && AxisValue != 0x0 && AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                {
                    AxisValue = oldAxis[device, AxisId];
                }
            }
            oldAxis[device, AxisId] = AxisValue;
            return AxisValue;
        }


        static byte RemoveFlicN2(byte AxisValue, int device, int AxisId)
        {
            if (AxisValue != 0xFF &&
                AxisValue != 0xFE &&
                AxisValue != 0x0 &&
                AxisValue != 0x1 &&
                AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue + 1 == oldAxis[device, AxisId] ||
                    AxisValue + 2 == oldAxis[device, AxisId] ||
                    AxisValue - 2 == oldAxis[device, AxisId] ||
                    AxisValue - 1 == oldAxis[device, AxisId])
                {
                    AxisValue = oldAxis[device, AxisId];
                }
            }

            if ((AxisValue == 0xFE || AxisValue == 0x1) && AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                {
                    AxisValue = oldAxis[device, AxisId];
                }
            }

            oldAxis[device, AxisId] = AxisValue;
            return AxisValue;
        }


        static byte RemoveFlicN1Dynamic(byte AxisValue, byte AxisFuture, int device, int AxisId)
        {
            if (AxisValue != 0xFF && AxisValue != 0x0 && AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue == AxisFuture || AxisValue +1 == AxisFuture || AxisValue - 1 == AxisFuture)
                {
                    if (AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                    {
                        AxisValue = oldAxis[device, AxisId];
                    }
                }
                         
            }
            oldAxis[device, AxisId] = AxisValue;
            return AxisValue;
        }


        static byte RemoveFlicN2Dynamic(byte AxisValue, byte AxisFuture, int device, int AxisId)
        {
            if (AxisValue != 0xFF &&
                AxisValue != 0xFE &&
                AxisValue != 0x0 &&
                AxisValue != 0x1 &&
                AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue == AxisFuture || 
                    AxisValue + 1 == AxisFuture || 
                    AxisValue - 1 == AxisFuture ||
                    AxisValue + 2 == AxisFuture ||
                    AxisValue - 2 == AxisFuture
                   )
                {
                    if (AxisValue + 1 == oldAxis[device, AxisId] ||
                        AxisValue + 2 == oldAxis[device, AxisId] ||
                        AxisValue - 2 == oldAxis[device, AxisId] ||
                        AxisValue - 1 == oldAxis[device, AxisId])
                    {
                        AxisValue = oldAxis[device, AxisId];
                    }

                }
            }

            if ((AxisValue == 0xFE || AxisValue == 0x1) && AxisValue != oldAxis[device, AxisId])
            {
                if (AxisValue == AxisFuture || AxisValue + 1 == AxisFuture || AxisValue - 1 == AxisFuture)
                {
                    if (AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                    {
                        AxisValue = oldAxis[device, AxisId];
                    }
                }

            }
  
            oldAxis[device, AxisId] = AxisValue;
            return AxisValue;
        }


        static byte RemoveFlicN1Dynamic2(byte AxisValue, byte AxisFuture, int device, int AxisId) 
        {
            byte res = AxisValue;

            if (AxisValue != 0xFF && AxisValue != 0x0) //&& AxisValue != oldAxis[device, AxisId])
            {
               // if (AxisValue == AxisFuture || AxisValue + 1 == AxisFuture || AxisValue - 1 == AxisFuture)
               // {
                    if (!(
                        AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 7] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 6] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 5] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 4] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 3] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 2] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 1] &&
                        AxisValue == oldRealAxisPlus[device, AxisId, 0]))
                    {
                        if (AxisValue == oldAxis[device, AxisId] || AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                        {
                            res = oldAxis[device, AxisId];
                        }

                    }
               // else
               // {
                //    res = AxisValue;
                //}
                // }

            }

            oldAxis[device, AxisId] = res;
            oldRealAxisPlus[device, AxisId, 12] = oldRealAxisPlus[device, AxisId, 11];
            oldRealAxisPlus[device, AxisId, 11] = oldRealAxisPlus[device, AxisId, 10];
            oldRealAxisPlus[device, AxisId, 10] = oldRealAxisPlus[device, AxisId, 9];
            oldRealAxisPlus[device, AxisId, 9] = oldRealAxisPlus[device, AxisId, 8];
            oldRealAxisPlus[device, AxisId, 8] = oldRealAxisPlus[device, AxisId, 7];
            oldRealAxisPlus[device, AxisId, 7] = oldRealAxisPlus[device, AxisId, 6];
            oldRealAxisPlus[device, AxisId, 6] = oldRealAxisPlus[device, AxisId, 5];
            oldRealAxisPlus[device, AxisId, 5] = oldRealAxisPlus[device, AxisId, 4];
            oldRealAxisPlus[device, AxisId, 4] = oldRealAxisPlus[device, AxisId, 3];
            oldRealAxisPlus[device, AxisId, 3] = oldRealAxisPlus[device, AxisId, 2];
            oldRealAxisPlus[device, AxisId, 2] = oldRealAxisPlus[device, AxisId, 1];
            oldRealAxisPlus[device, AxisId, 1] = oldRealAxisPlus[device, AxisId, 0];
            oldRealAxisPlus[device, AxisId, 0] = AxisValue;
            return res;
        }


        static byte RemoveFlicN2Dynamic2(byte AxisValue, byte AxisFuture, int device, int AxisId)
        {
            byte res = AxisValue;

            if (AxisValue != 0xFF &&
                AxisValue != 0xFE &&
                AxisValue != 0x0 &&
                AxisValue != 0x1) //&&
                                  //AxisValue != oldAxis[device, AxisId])
            {
                //if (AxisValue == AxisFuture ||
                //    AxisValue + 1 == AxisFuture ||
                //     AxisValue - 1 == AxisFuture ||
                //     AxisValue + 2 == AxisFuture ||
                //      AxisValue - 2 == AxisFuture
                //    )
                //  {
                if (!(
                    //AxisValue == oldRealAxisPlus[device, AxisId, 15] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 14] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 13] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 12] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 7] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 6] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 5] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 4] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 3] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 2] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 1] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 0]))
                {
                    if (AxisValue == oldAxis[device, AxisId] ||
                        AxisValue + 1 == oldAxis[device, AxisId] ||        
                        AxisValue - 1 == oldAxis[device, AxisId])
                    {
                        res = oldAxis[device, AxisId];
                    }

                    if (AxisValue + 2 == oldAxis[device, AxisId] ||
                        AxisValue - 2 == oldAxis[device, AxisId])
                    {
                        if (!(
                            AxisValue == oldRealAxisPlus[device, AxisId, 5] &&
                            AxisValue == oldRealAxisPlus[device, AxisId, 4] &&
                            AxisValue == oldRealAxisPlus[device, AxisId, 3] &&
                            AxisValue == oldRealAxisPlus[device, AxisId, 2] &&
                            AxisValue == oldRealAxisPlus[device, AxisId, 1] &&
                            AxisValue == oldRealAxisPlus[device, AxisId, 0]))
                        {
                            res = oldAxis[device, AxisId];
                        }
                    }

                }
                //else
               //{
                //    res = AxisValue;
               // }

                //}
            }

            if ((AxisValue == 0xFE || AxisValue == 0x1)) //&& AxisValue != oldAxis[device, AxisId])
            {
                //if (AxisValue == AxisFuture || AxisValue + 1 == AxisFuture || AxisValue - 1 == AxisFuture)
                //{
                if (!(
                    //AxisValue == oldRealAxisPlus[device, AxisId, 15] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 14] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 13] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 12] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 7] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 6] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 5] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 4] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 3] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 2] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 1] &&
                    AxisValue == oldRealAxisPlus[device, AxisId, 0]))
                {
                    if (AxisValue == oldAxis[device, AxisId] || AxisValue + 1 == oldAxis[device, AxisId] || AxisValue - 1 == oldAxis[device, AxisId])
                    {
                        res = oldAxis[device, AxisId];
                    }

                    ///    }
                }
               // else
               // {
               //     res = AxisValue;
               // }

            }

            oldAxis[device, AxisId] = res;

            //oldRealAxisPlus[device, AxisId, 15] = oldRealAxisPlus[device, AxisId, 14];
            //oldRealAxisPlus[device, AxisId, 14] = oldRealAxisPlus[device, AxisId, 13];
            //oldRealAxisPlus[device, AxisId, 13] = oldRealAxisPlus[device, AxisId, 12];
            oldRealAxisPlus[device, AxisId, 12] = oldRealAxisPlus[device, AxisId, 11];
            oldRealAxisPlus[device, AxisId, 11] = oldRealAxisPlus[device, AxisId, 10];
            oldRealAxisPlus[device, AxisId, 10] = oldRealAxisPlus[device, AxisId, 9];
            oldRealAxisPlus[device, AxisId, 9] = oldRealAxisPlus[device, AxisId, 8];
            oldRealAxisPlus[device, AxisId, 8] = oldRealAxisPlus[device, AxisId, 7];
            oldRealAxisPlus[device, AxisId, 7] = oldRealAxisPlus[device, AxisId, 6];
            oldRealAxisPlus[device, AxisId, 6] = oldRealAxisPlus[device, AxisId, 5];
            oldRealAxisPlus[device, AxisId, 5] = oldRealAxisPlus[device, AxisId, 4];
            oldRealAxisPlus[device, AxisId, 4] = oldRealAxisPlus[device, AxisId, 3];
            oldRealAxisPlus[device, AxisId, 3] = oldRealAxisPlus[device, AxisId, 2];
            oldRealAxisPlus[device, AxisId, 2] = oldRealAxisPlus[device, AxisId, 1];
            oldRealAxisPlus[device, AxisId, 1] = oldRealAxisPlus[device, AxisId, 0];
            oldRealAxisPlus[device, AxisId, 0] = AxisValue;
            return res;
        }




    }
}
