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

        static byte[,,] oldRealAxisPlus = new byte[5, 4, 12];
        //[device, axis, oldbyte]


        public static byte RemoveFlicN1(byte AxisValue, int device, int AxisId)
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


        public static byte RemoveFlicN2(byte AxisValue, int device, int AxisId)
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


        public static byte RemoveFlicN1Future(byte AxisValue, byte AxisFuture, int device, int AxisId)
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


        public static byte RemoveFlicN2Future(byte AxisValue, byte AxisFuture, int device, int AxisId)
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


        public  static byte RemoveFlicN1Dynamic12Values(byte AxisValue, int device, int AxisId) 
        {
            byte res = AxisValue;

            if (AxisValue != 0xFF && AxisValue != 0x0)
            {
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

            }

            oldAxis[device, AxisId] = res;
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


        public static byte RemoveFlicN2Dynamic12Values(byte AxisValue, int device, int AxisId)
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

                    // }
                }

            }

            oldAxis[device, AxisId] = res;

            //oldRealAxisPlus[device, AxisId, 15] = oldRealAxisPlus[device, AxisId, 14];
            //oldRealAxisPlus[device, AxisId, 14] = oldRealAxisPlus[device, AxisId, 13];
            //oldRealAxisPlus[device, AxisId, 13] = oldRealAxisPlus[device, AxisId, 12];
            //oldRealAxisPlus[device, AxisId, 12] = oldRealAxisPlus[device, AxisId, 11];
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


        public static byte RemoveFlicN1Dynamic7Values(byte AxisValue, int device, int AxisId)
        {
            byte res = AxisValue;

            if (AxisValue != 0xFF && AxisValue != 0x0)
            {
                if (!(
                    //AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
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

            }

            oldAxis[device, AxisId] = res;
            //oldRealAxisPlus[device, AxisId, 11] = oldRealAxisPlus[device, AxisId, 10];
            //oldRealAxisPlus[device, AxisId, 10] = oldRealAxisPlus[device, AxisId, 9];
            //oldRealAxisPlus[device, AxisId, 9] = oldRealAxisPlus[device, AxisId, 8];
            //oldRealAxisPlus[device, AxisId, 8] = oldRealAxisPlus[device, AxisId, 7];
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


        public static byte RemoveFlicN2Dynamic7Values(byte AxisValue, int device, int AxisId)
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
                    //AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
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

                    //}
                }
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
                    //AxisValue == oldRealAxisPlus[device, AxisId, 11] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 10] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 9] &&
                    //AxisValue == oldRealAxisPlus[device, AxisId, 8] &&
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

                    //}
                }

            }

            oldAxis[device, AxisId] = res;

            //oldRealAxisPlus[device, AxisId, 15] = oldRealAxisPlus[device, AxisId, 14];
            //oldRealAxisPlus[device, AxisId, 14] = oldRealAxisPlus[device, AxisId, 13];
            //oldRealAxisPlus[device, AxisId, 13] = oldRealAxisPlus[device, AxisId, 12];
            //oldRealAxisPlus[device, AxisId, 12] = oldRealAxisPlus[device, AxisId, 11];
            //oldRealAxisPlus[device, AxisId, 11] = oldRealAxisPlus[device, AxisId, 10];
            //oldRealAxisPlus[device, AxisId, 10] = oldRealAxisPlus[device, AxisId, 9];
            //oldRealAxisPlus[device, AxisId, 9] = oldRealAxisPlus[device, AxisId, 8];
            //oldRealAxisPlus[device, AxisId, 8] = oldRealAxisPlus[device, AxisId, 7];
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
