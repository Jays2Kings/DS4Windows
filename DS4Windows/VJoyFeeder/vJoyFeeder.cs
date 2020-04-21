// VJoy C# interface file taken from an excellent Shaul's virtual joystick driver project.
// Licensed to public domain as is (http://vjoystick.sourceforge.net/site/index.php/forum/5-Discussion/104-what-is-the-usage-license-for-this-software).
// http://vjoystick.sourceforge.net/site/
// https://github.com/shauleiz/vJoy/tree/master/apps/common/vJoyInterfaceCS
//
// This module is a feeder for VJoy virtual joystick driver. DS4Windows can optionally re-map and feed buttons and analog axis values from DS4 Controller to VJoy device.
// At first this may seem silly because DS4Windows can already do re-mapping by using a virtual X360 Controller driver, so why feed VJoy virtual driver also? 
// Sometimes X360 driver may run out of analog axis options, so for example "SA motion sensor steering wheel emulation" in DS4Windows would reserve a thumbstick X or Y 
// axis for SA steering wheel emulation usage. That thumbstick axis would be unavailable for "normal" thumbstick usage after this re-mapping. 
// The problem can be solved by configuring DS4Windows to re-map SA steering wheel emulation axis to VJoy axis, so all analog axies in DS4 controller are still available for normal usage.
//

using System;
using System.Runtime.InteropServices;
using System.Security;    // SuppressUnmanagedCodeSecurity support to optimize for performance instead of code security

namespace DS4Windows.VJoyFeeder
{
    [Flags]
    public enum HID_USAGES
    {
        HID_USAGE_X = 0x30,
        HID_USAGE_Y = 0x31,
        HID_USAGE_Z = 0x32,
        HID_USAGE_RX = 0x33,
        HID_USAGE_RY = 0x34,
        HID_USAGE_RZ = 0x35,
        HID_USAGE_SL0 = 0x36,
        HID_USAGE_SL1 = 0x37,
        HID_USAGE_WHL = 0x38,
        HID_USAGE_POV = 0x39,
    }

    public enum VjdStat  /* Declares an enumeration data type called BOOLEAN */
    {
        VJD_STAT_OWN,   // The  vJoy Device is owned by this application.
        VJD_STAT_FREE,  // The  vJoy Device is NOT owned by any application (including this one).
        VJD_STAT_BUSY,  // The  vJoy Device is owned by another application. It cannot be acquired by this application.
        VJD_STAT_MISS,  // The  vJoy Device is missing. It either does not exist or the driver is down.
        VJD_STAT_UNKN   // Unknown
    };


    // FFB Declarations

    // HID Descriptor definitions - FFB Report IDs

    public enum FFBPType // FFB Packet Type
    {
        // Write
        PT_EFFREP = 0x01,   // Usage Set Effect Report
        PT_ENVREP = 0x02,   // Usage Set Envelope Report
        PT_CONDREP = 0x03,  // Usage Set Condition Report
        PT_PRIDREP = 0x04,  // Usage Set Periodic Report
        PT_CONSTREP = 0x05, // Usage Set Constant Force Report
        PT_RAMPREP = 0x06,  // Usage Set Ramp Force Report
        PT_CSTMREP = 0x07,  // Usage Custom Force Data Report
        PT_SMPLREP = 0x08,  // Usage Download Force Sample
        PT_EFOPREP = 0x0A,  // Usage Effect Operation Report
        PT_BLKFRREP = 0x0B, // Usage PID Block Free Report
        PT_CTRLREP = 0x0C,  // Usage PID Device Control
        PT_GAINREP = 0x0D,  // Usage Device Gain Report
        PT_SETCREP = 0x0E,  // Usage Set Custom Force Report

        // Feature
        PT_NEWEFREP = 0x01 + 0x10,  // Usage Create New Effect Report
        PT_BLKLDREP = 0x02 + 0x10,  // Usage Block Load Report
        PT_POOLREP = 0x03 + 0x10,   // Usage PID Pool Report
    };

    public enum FFBEType // FFB Effect Type
    {

        // Effect Type
        ET_NONE = 0,      //    No Force
        ET_CONST = 1,    //    Constant Force
        ET_RAMP = 2,    //    Ramp
        ET_SQR = 3,    //    Square
        ET_SINE = 4,    //    Sine
        ET_TRNGL = 5,    //    Triangle
        ET_STUP = 6,    //    Sawtooth Up
        ET_STDN = 7,    //    Sawtooth Down
        ET_SPRNG = 8,    //    Spring
        ET_DMPR = 9,    //    Damper
        ET_INRT = 10,   //    Inertia
        ET_FRCTN = 11,   //    Friction
        ET_CSTM = 12,   //    Custom Force Data
    };

    public enum FFB_CTRL
    {
        CTRL_ENACT = 1, // Enable all device actuators.
        CTRL_DISACT = 2,    // Disable all the device actuators.
        CTRL_STOPALL = 3,   // Stop All Effects­ Issues a stop on every running effect.
        CTRL_DEVRST = 4,    // Device Reset– Clears any device paused condition, enables all actuators and clears all effects from memory.
        CTRL_DEVPAUSE = 5,  // Device Pause– The all effects on the device are paused at the current time step.
        CTRL_DEVCONT = 6,   // Device Continue– The all effects that running when the device was paused are restarted from their last time step.
    };

    public enum FFBOP
    {
        EFF_START = 1, // EFFECT START
        EFF_SOLO = 2, // EFFECT SOLO START
        EFF_STOP = 3, // EFFECT STOP
    };

    //namespace vJoyInterfaceWrap
    //{
    [SuppressUnmanagedCodeSecurity]
    public class VJoy
        {

            /***************************************************/
            /*********** Various declarations ******************/
            /***************************************************/
            private static RemovalCbFunc UserRemCB;
            private static WrapRemovalCbFunc wrf;
            private static GCHandle hRemUserData;


            private static FfbCbFunc UserFfbCB;
            private static WrapFfbCbFunc wf;
            private static GCHandle hFfbUserData;

            [StructLayout(LayoutKind.Sequential)]
            public struct JoystickState
            {
                public byte bDevice;
                public Int32 Throttle;
                public Int32 Rudder;
                public Int32 Aileron;
                public Int32 AxisX;
                public Int32 AxisY;
                public Int32 AxisZ;
                public Int32 AxisXRot;
                public Int32 AxisYRot;
                public Int32 AxisZRot;
                public Int32 Slider;
                public Int32 Dial;
                public Int32 Wheel;
                public Int32 AxisVX;
                public Int32 AxisVY;
                public Int32 AxisVZ;
                public Int32 AxisVBRX;
                public Int32 AxisVBRY;
                public Int32 AxisVBRZ;
                public UInt32 Buttons;
                public UInt32 bHats;    // Lower 4 bits: HAT switch or 16-bit of continuous HAT switch
                public UInt32 bHatsEx1; // Lower 4 bits: HAT switch or 16-bit of continuous HAT switch
                public UInt32 bHatsEx2; // Lower 4 bits: HAT switch or 16-bit of continuous HAT switch
                public UInt32 bHatsEx3; // Lower 4 bits: HAT switch or 16-bit of continuous HAT switch
                public UInt32 ButtonsEx1;
                public UInt32 ButtonsEx2;
                public UInt32 ButtonsEx3;
            };

            [StructLayout(LayoutKind.Sequential)]
            private struct FFB_DATA
            {
                private UInt32 size;
                private UInt32 cmd;
                private IntPtr data;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_CONSTANT
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public Int16 Magnitude;
            }

            [System.Obsolete("use FFB_EFF_REPORT")]
            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_CONST
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public FFBEType EffectType;
                [FieldOffset(8)]
                public UInt16 Duration;// Value in milliseconds. 0xFFFF means infinite
                [FieldOffset(10)]
                public UInt16 TrigerRpt;
                [FieldOffset(12)]
                public UInt16 SamplePrd;
                [FieldOffset(14)]
                public Byte Gain;
                [FieldOffset(15)]
                public Byte TrigerBtn;
                [FieldOffset(16)]
                public bool Polar; // How to interpret force direction Polar (0-360°) or Cartesian (X,Y)
                [FieldOffset(20)]
                public Byte Direction; // Polar direction: (0x00-0xFF correspond to 0-360°)
                [FieldOffset(20)]
                public Byte DirX; // X direction: Positive values are To the right of the center (X); Negative are Two's complement
                [FieldOffset(21)]
                public Byte DirY; // Y direction: Positive values are below the center (Y); Negative are Two's complement
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_REPORT
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public FFBEType EffectType;
                [FieldOffset(8)]
                public UInt16 Duration;// Value in milliseconds. 0xFFFF means infinite
                [FieldOffset(10)]
                public UInt16 TrigerRpt;
                [FieldOffset(12)]
                public UInt16 SamplePrd;
                [FieldOffset(14)]
                public Byte Gain;
                [FieldOffset(15)]
                public Byte TrigerBtn;
                [FieldOffset(16)]
                public bool Polar; // How to interpret force direction Polar (0-360°) or Cartesian (X,Y)
                [FieldOffset(20)]
                public Byte Direction; // Polar direction: (0x00-0xFF correspond to 0-360°)
                [FieldOffset(20)]
                public Byte DirX; // X direction: Positive values are To the right of the center (X); Negative are Two's complement
                [FieldOffset(21)]
                public Byte DirY; // Y direction: Positive values are below the center (Y); Negative are Two's complement
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_OP
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public FFBOP EffectOp;
                [FieldOffset(8)]
                public Byte LoopCount;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_COND
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public bool isY;
                [FieldOffset(8)]
                public Int16 CenterPointOffset; // CP Offset: Range 0x80­0x7F (­10000 ­ 10000)
                [FieldOffset(12)]
                public Int16 PosCoeff; // Positive Coefficient: Range 0x80­0x7F (­10000 ­ 10000)
                [FieldOffset(16)]
                public Int16 NegCoeff; // Negative Coefficient: Range 0x80­0x7F (­10000 ­ 10000)
                [FieldOffset(20)]
                public UInt32 PosSatur; // Positive Saturation: Range 0x00­0xFF (0 – 10000)
                [FieldOffset(24)]
                public UInt32 NegSatur; // Negative Saturation: Range 0x00­0xFF (0 – 10000)
                [FieldOffset(28)]
                public Int32 DeadBand; // Dead Band: : Range 0x00­0xFF (0 – 10000)
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_ENVLP
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public UInt16 AttackLevel;
                [FieldOffset(8)]
                public UInt16 FadeLevel;
                [FieldOffset(12)]
                public UInt32 AttackTime;
                [FieldOffset(16)]
                public UInt32 FadeTime;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_PERIOD
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public UInt32 Magnitude;
                [FieldOffset(8)]
                public Int16 Offset;
                [FieldOffset(12)]
                public UInt32 Phase;
                [FieldOffset(16)]
                public UInt32 Period;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FFB_EFF_RAMP
            {
                [FieldOffset(0)]
                public Byte EffectBlockIndex;
                [FieldOffset(4)]
                public Int16 Start;             // The Normalized magnitude at the start of the effect
                [FieldOffset(8)]
                public Int16 End;               // The Normalized magnitude at the end of the effect
            }


            /***************************************************/
            /***** Import from file vJoyInterface.dll (C) ******/
            /***************************************************/

            /////	General driver data
            [DllImport("vJoyInterface.dll", EntryPoint = "GetvJoyVersion")]
            private static extern short _GetvJoyVersion();

            [DllImport("vJoyInterface.dll", EntryPoint = "vJoyEnabled")]
            private static extern bool _vJoyEnabled();

            [DllImport("vJoyInterface.dll", EntryPoint = "GetvJoyProductString")]
            private static extern IntPtr _GetvJoyProductString();

            [DllImport("vJoyInterface.dll", EntryPoint = "GetvJoyManufacturerString")]
            private static extern IntPtr _GetvJoyManufacturerString();

            [DllImport("vJoyInterface.dll", EntryPoint = "GetvJoySerialNumberString")]
            private static extern IntPtr _GetvJoySerialNumberString();

            [DllImport("vJoyInterface.dll", EntryPoint = "DriverMatch")]
            private static extern bool _DriverMatch(ref UInt32 DllVer, ref UInt32 DrvVer);

            /////	vJoy Device properties
            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDButtonNumber")]
            private static extern int _GetVJDButtonNumber(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDDiscPovNumber")]
            private static extern int _GetVJDDiscPovNumber(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDContPovNumber")]
            private static extern int _GetVJDContPovNumber(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDAxisExist")]
            private static extern UInt32 _GetVJDAxisExist(UInt32 rID, UInt32 Axis);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDAxisMax")]
            private static extern bool _GetVJDAxisMax(UInt32 rID, UInt32 Axis, ref long Max);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDAxisMin")]
            private static extern bool _GetVJDAxisMin(UInt32 rID, UInt32 Axis, ref long Min);

            [DllImport("vJoyInterface.dll", EntryPoint = "isVJDExists")]
            private static extern bool _isVJDExists(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetOwnerPid")]
            private static extern int _GetOwnerPid(UInt32 rID);

            /////	Write access to vJoy Device - Basic
            [DllImport("vJoyInterface.dll", EntryPoint = "AcquireVJD")]
            private static extern bool _AcquireVJD(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "RelinquishVJD")]
            private static extern void _RelinquishVJD(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "UpdateVJD")]
            private static extern bool _UpdateVJD(UInt32 rID, ref JoystickState pData);

            [DllImport("vJoyInterface.dll", EntryPoint = "GetVJDStatus")]
            private static extern int _GetVJDStatus(UInt32 rID);


            //// Reset functions
            [DllImport("vJoyInterface.dll", EntryPoint = "ResetVJD")]
            private static extern bool _ResetVJD(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "ResetAll")]
            private static extern bool _ResetAll();

            [DllImport("vJoyInterface.dll", EntryPoint = "ResetButtons")]
            private static extern bool _ResetButtons(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "ResetPovs")]
            private static extern bool _ResetPovs(UInt32 rID);

            ////// Write data
            [DllImport("vJoyInterface.dll", EntryPoint = "SetAxis")]
            private static extern bool _SetAxis(Int32 Value, UInt32 rID, HID_USAGES Axis);

            [DllImport("vJoyInterface.dll", EntryPoint = "SetBtn")]
            private static extern bool _SetBtn(bool Value, UInt32 rID, Byte nBtn);

            [DllImport("vJoyInterface.dll", EntryPoint = "SetDiscPov")]
            private static extern bool _SetDiscPov(Int32 Value, UInt32 rID, uint nPov);

            [DllImport("vJoyInterface.dll", EntryPoint = "SetContPov")]
            private static extern bool _SetContPov(Int32 Value, UInt32 rID, uint nPov);

            [DllImport("vJoyInterface.dll", EntryPoint = "RegisterRemovalCB", CallingConvention = CallingConvention.Cdecl)]
            private extern static void _RegisterRemovalCB(WrapRemovalCbFunc cb, IntPtr data);

            public delegate void RemovalCbFunc(bool complete, bool First, object userData);
            public delegate void WrapRemovalCbFunc(bool complete, bool First, IntPtr userData);

            public static void WrapperRemCB(bool complete, bool First, IntPtr userData)
            {

                object obj = null;

                if (userData != IntPtr.Zero)
                {
                    // Convert userData from pointer to object
                    GCHandle handle2 = (GCHandle)userData;
                    obj = handle2.Target as object;
                }

                // Call user-defined CB function
                UserRemCB(complete, First, obj);
            }

            // Force Feedback (FFB)
            [DllImport("vJoyInterface.dll", EntryPoint = "FfbRegisterGenCB", CallingConvention = CallingConvention.Cdecl)]
            private extern static void _FfbRegisterGenCB(WrapFfbCbFunc cb, IntPtr data);

            public delegate void FfbCbFunc(IntPtr data, object userData);
            public delegate void WrapFfbCbFunc(IntPtr data, IntPtr userData);

            public static void WrapperFfbCB(IntPtr data, IntPtr userData)
            {

                object obj = null;

                if (userData != IntPtr.Zero)
                {
                    // Convert userData from pointer to object
                    GCHandle handle2 = (GCHandle)userData;
                    obj = handle2.Target as object;
                }

                // Call user-defined CB function
                UserFfbCB(data, obj);
            }

            [DllImport("vJoyInterface.dll", EntryPoint = "FfbStart")]
            private static extern bool _FfbStart(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "FfbStop")]
            private static extern bool _FfbStop(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "IsDeviceFfb")]
            private static extern bool _IsDeviceFfb(UInt32 rID);

            [DllImport("vJoyInterface.dll", EntryPoint = "IsDeviceFfbEffect")]
            private static extern bool _IsDeviceFfbEffect(UInt32 rID, UInt32 Effect);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_DeviceID")]
            private static extern UInt32 _Ffb_h_DeviceID(IntPtr Packet, ref int DeviceID);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Type")]
            private static extern UInt32 _Ffb_h_Type(IntPtr Packet, ref FFBPType Type);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Packet")]
            private static extern UInt32 _Ffb_h_Packet(IntPtr Packet, ref UInt32 Type, ref Int32 DataSize, ref IntPtr Data);


            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_EBI")]
            private static extern UInt32 _Ffb_h_EBI(IntPtr Packet, ref Int32 Index);

#pragma warning disable 618
            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Const")]
            private static extern UInt32 _Ffb_h_Eff_Const(IntPtr Packet, ref FFB_EFF_CONST Effect);
#pragma warning restore 618

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Report")]
            private static extern UInt32 _Ffb_h_Eff_Report(IntPtr Packet, ref FFB_EFF_REPORT Effect);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_DevCtrl")]
            private static extern UInt32 _Ffb_h_DevCtrl(IntPtr Packet, ref FFB_CTRL Control);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_EffOp")]
            private static extern UInt32 _Ffb_h_EffOp(IntPtr Packet, ref FFB_EFF_OP Operation);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_DevGain")]
            private static extern UInt32 _Ffb_h_DevGain(IntPtr Packet, ref Byte Gain);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Cond")]
            private static extern UInt32 _Ffb_h_Eff_Cond(IntPtr Packet, ref FFB_EFF_COND Condition);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Envlp")]
            private static extern UInt32 _Ffb_h_Eff_Envlp(IntPtr Packet, ref FFB_EFF_ENVLP Envelope);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Period")]
            private static extern UInt32 _Ffb_h_Eff_Period(IntPtr Packet, ref FFB_EFF_PERIOD Effect);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_EffNew")]
            private static extern UInt32 _Ffb_h_EffNew(IntPtr Packet, ref FFBEType Effect);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Ramp")]
            private static extern UInt32 _Ffb_h_Eff_Ramp(IntPtr Packet, ref FFB_EFF_RAMP RampEffect);

            [DllImport("vJoyInterface.dll", EntryPoint = "Ffb_h_Eff_Constant")]
            private static extern UInt32 _Ffb_h_Eff_Constant(IntPtr Packet, ref FFB_EFF_CONSTANT ConstantEffect);

            /***************************************************/
            /********** Export functions (C#) ******************/
            /***************************************************/

            /////	General driver data
            public short GetvJoyVersion() { return _GetvJoyVersion(); }
            public bool vJoyEnabled() { return _vJoyEnabled(); }
            public string GetvJoyProductString() { return Marshal.PtrToStringAuto(_GetvJoyProductString()); }
            public string GetvJoyManufacturerString() { return Marshal.PtrToStringAuto(_GetvJoyManufacturerString()); }
            public string GetvJoySerialNumberString() { return Marshal.PtrToStringAuto(_GetvJoySerialNumberString()); }
            public bool DriverMatch(ref UInt32 DllVer, ref UInt32 DrvVer) { return _DriverMatch(ref DllVer, ref DrvVer); }

            /////	vJoy Device properties
            public int GetVJDButtonNumber(uint rID) { return _GetVJDButtonNumber(rID); }
            public int GetVJDDiscPovNumber(uint rID) { return _GetVJDDiscPovNumber(rID); }
            public int GetVJDContPovNumber(uint rID) { return _GetVJDContPovNumber(rID); }
            public bool GetVJDAxisExist(UInt32 rID, HID_USAGES Axis)
            {
                UInt32 res = _GetVJDAxisExist(rID, (uint)Axis);
                if (res == 1)
                    return true;
                else
                    return false;
            }
            public bool GetVJDAxisMax(UInt32 rID, HID_USAGES Axis, ref long Max) { return _GetVJDAxisMax(rID, (uint)Axis, ref Max); }
            public bool GetVJDAxisMin(UInt32 rID, HID_USAGES Axis, ref long Min) { return _GetVJDAxisMin(rID, (uint)Axis, ref Min); }
            public bool isVJDExists(UInt32 rID) { return _isVJDExists(rID); }
            public int GetOwnerPid(UInt32 rID) { return _GetOwnerPid(rID); }

            /////	Write access to vJoy Device - Basic
            public bool AcquireVJD(UInt32 rID) { return _AcquireVJD(rID); }
            public void RelinquishVJD(uint rID) { _RelinquishVJD(rID); }
            public bool UpdateVJD(UInt32 rID, ref JoystickState pData) { return _UpdateVJD(rID, ref pData); }
            public VjdStat GetVJDStatus(UInt32 rID) { return (VjdStat)_GetVJDStatus(rID); }

            //// Reset functions
            public bool ResetVJD(UInt32 rID) { return _ResetVJD(rID); }
            public bool ResetAll() { return _ResetAll(); }
            public bool ResetButtons(UInt32 rID) { return _ResetButtons(rID); }
            public bool ResetPovs(UInt32 rID) { return _ResetPovs(rID); }

            ////// Write data
            public bool SetAxis(Int32 Value, UInt32 rID, HID_USAGES Axis) { return _SetAxis(Value, rID, Axis); }
            public bool SetBtn(bool Value, UInt32 rID, uint nBtn) { return _SetBtn(Value, rID, (Byte)nBtn); }
            public bool SetDiscPov(Int32 Value, UInt32 rID, uint nPov) { return _SetDiscPov(Value, rID, nPov); }
            public bool SetContPov(Int32 Value, UInt32 rID, uint nPov) { return _SetContPov(Value, rID, nPov); }

            // Register CB function that takes a C# object as userdata
            public void RegisterRemovalCB(RemovalCbFunc cb, object data)
            {
                // Free existing GCHandle (if exists)
                if (hRemUserData.IsAllocated && hRemUserData.Target != null)
                    hRemUserData.Free();

                // Convert object to pointer
                hRemUserData = GCHandle.Alloc(data);

                // Apply the user-defined CB function          
                UserRemCB = new RemovalCbFunc(cb);
                wrf = new WrapRemovalCbFunc(WrapperRemCB);

                _RegisterRemovalCB(wrf, (IntPtr)hRemUserData);
            }

            // Register CB function that takes a pointer as userdata
            public void RegisterRemovalCB(WrapRemovalCbFunc cb, IntPtr data)
            {
                wrf = new WrapRemovalCbFunc(cb);
                _RegisterRemovalCB(wrf, data);
            }


            /////////////////////////////////////////////////////////////////////////////////////////////
            //// Force Feedback (FFB)

            // Register CB function that takes a C# object as userdata
            public void FfbRegisterGenCB(FfbCbFunc cb, object data)
            {
                // Free existing GCHandle (if exists)
                if (hFfbUserData.IsAllocated && hFfbUserData.Target != null)
                    hFfbUserData.Free();

                // Convert object to pointer
                hFfbUserData = GCHandle.Alloc(data);

                // Apply the user-defined CB function          
                UserFfbCB = new FfbCbFunc(cb);
                wf = new WrapFfbCbFunc(WrapperFfbCB);

                _FfbRegisterGenCB(wf, (IntPtr)hFfbUserData);
            }

            // Register CB function that takes a pointer as userdata
            public void FfbRegisterGenCB(WrapFfbCbFunc cb, IntPtr data)
            {
                wf = new WrapFfbCbFunc(cb);
                _FfbRegisterGenCB(wf, data);
            }

            [Obsolete("you can remove the function from your code")]
            public bool FfbStart(UInt32 rID) { return _FfbStart(rID); }
            [Obsolete("you can remove the function from your code")]
            public bool FfbStop(UInt32 rID) { return _FfbStop(rID); }
            public bool IsDeviceFfb(UInt32 rID) { return _IsDeviceFfb(rID); }
            public bool IsDeviceFfbEffect(UInt32 rID, UInt32 Effect) { return _IsDeviceFfbEffect(rID, Effect); }
            public UInt32 Ffb_h_DeviceID(IntPtr Packet, ref int DeviceID) { return _Ffb_h_DeviceID(Packet, ref DeviceID); }
            public UInt32 Ffb_h_Type(IntPtr Packet, ref FFBPType Type) { return _Ffb_h_Type(Packet, ref Type); }
            public UInt32 Ffb_h_Packet(IntPtr Packet, ref UInt32 Type, ref Int32 DataSize, ref Byte[] Data)
            {
                IntPtr buf = IntPtr.Zero;
                UInt32 res = _Ffb_h_Packet(Packet, ref Type, ref DataSize, ref buf);
                if (res != 0)
                    return res;

                DataSize -= 8;
                Data = new byte[DataSize];
                Marshal.Copy(buf, Data, 0, DataSize);
                return res;
            }
            public UInt32 Ffb_h_EBI(IntPtr Packet, ref Int32 Index) { return _Ffb_h_EBI(Packet, ref Index); }
            [Obsolete("use Ffb_h_Eff_Report instead")]
            public UInt32 Ffb_h_Eff_Const(IntPtr Packet, ref FFB_EFF_CONST Effect) { return _Ffb_h_Eff_Const(Packet, ref Effect); }
            public UInt32 Ffb_h_Eff_Report(IntPtr Packet, ref FFB_EFF_REPORT Effect) { return _Ffb_h_Eff_Report(Packet, ref Effect); }
            public UInt32 Ffb_h_DevCtrl(IntPtr Packet, ref FFB_CTRL Control) { return _Ffb_h_DevCtrl(Packet, ref Control); }
            public UInt32 Ffb_h_EffOp(IntPtr Packet, ref FFB_EFF_OP Operation) { return _Ffb_h_EffOp(Packet, ref Operation); }
            public UInt32 Ffb_h_DevGain(IntPtr Packet, ref Byte Gain) { return _Ffb_h_DevGain(Packet, ref Gain); }
            public UInt32 Ffb_h_Eff_Cond(IntPtr Packet, ref FFB_EFF_COND Condition) { return _Ffb_h_Eff_Cond(Packet, ref Condition); }
            public UInt32 Ffb_h_Eff_Envlp(IntPtr Packet, ref FFB_EFF_ENVLP Envelope) { return _Ffb_h_Eff_Envlp(Packet, ref Envelope); }
            public UInt32 Ffb_h_Eff_Period(IntPtr Packet, ref FFB_EFF_PERIOD Effect) { return _Ffb_h_Eff_Period(Packet, ref Effect); }
            public UInt32 Ffb_h_EffNew(IntPtr Packet, ref FFBEType Effect) { return _Ffb_h_EffNew(Packet, ref Effect); }
            public UInt32 Ffb_h_Eff_Ramp(IntPtr Packet, ref FFB_EFF_RAMP RampEffect) { return _Ffb_h_Eff_Ramp(Packet, ref RampEffect); }
            public UInt32 Ffb_h_Eff_Constant(IntPtr Packet, ref FFB_EFF_CONSTANT ConstantEffect) { return _Ffb_h_Eff_Constant(Packet, ref ConstantEffect); }
        }
    //}

    public class vJoyFeeder
    {
        private static readonly object vJoyLocker = new object();

        static bool[] vJoyInitialized = new bool[2] { false, false };
        static bool[] vJoyAvailable = new bool[2] { false, false };
        static VJoy[] vJoyObj = new VJoy[2] { null, null }; 

        vJoyFeeder()
        {
            // Do nothing
        }

        ~vJoyFeeder()
        {
            // Do nothing
        }

        public static void InitializeVJoyDevice(uint vJoyID, HID_USAGES axis)
        {
            lock (vJoyLocker)
            {
                if (vJoyInitialized[vJoyID - 1]) return;

                vJoyInitialized[vJoyID - 1] = true;
                AppLogger.LogToGui("Initializing VJoy virtual joystick driver via vJoyInterface.dll interface", false);

                try
                {
                    VJoy currVJoy;

                    if (vJoyObj[vJoyID - 1] == null) vJoyObj[vJoyID - 1] = new VJoy();
                    currVJoy = vJoyObj[vJoyID - 1];

                    if (currVJoy.vJoyEnabled() && currVJoy.GetVJDAxisExist(vJoyID, axis))
                    {
                        AppLogger.LogToGui("Connection to VJoy virtual joystick established", false);
                        AppLogger.LogToGui($"VJoy driver. Vendor={currVJoy.GetvJoyManufacturerString()}  Product={currVJoy.GetvJoyProductString()}  Version={currVJoy.GetvJoySerialNumberString()}  Device#={vJoyID}  Axis={axis}", false);

                        // Test if DLL matches the driver
                        UInt32 DllVer = 0, DrvVer = 0;
                        if (!currVJoy.DriverMatch(ref DllVer, ref DrvVer))
                            AppLogger.LogToGui("WARNING. VJoy version of Driver {DrvVer}) does not match interface DLL Version {DllVer}. This may lead to unexpected problems or crashes. Update VJoy driver and vJoyInterface.dll", false);

                        VjdStat status = currVJoy.GetVJDStatus(vJoyID);
                        if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!currVJoy.AcquireVJD(vJoyID))))
                        {
                            vJoyAvailable[vJoyID - 1] = false;
                            AppLogger.LogToGui("ERROR. Failed to acquire vJoy device# {vJoyID}. Use another VJoy device or make sure there are no other VJoy feeder apps using the same device", false);
                        }
                        else
                        {
                            //vJoyObj.GetVJDAxisMax(vJoyID, axis, ref vJoyAxisMaxValue);
                            //AppLogger.LogToGui($"VJoy axis {axis} max value={vJoyAxisMaxValue}", false);
                            currVJoy.ResetVJD(vJoyID);
                            vJoyAvailable[vJoyID - 1] = true;
                        }
                    }
                    else
                    {
                        vJoyAvailable[vJoyID - 1] = false;
                        AppLogger.LogToGui($"ERROR. VJoy device# {vJoyID} or {axis} axis not available. Check vJoy driver installation and configuration", false);
                    }
                }
                catch
                {
                    vJoyAvailable[vJoyID - 1] = false;
                    AppLogger.LogToGui("ERROR. vJoy initialization failed. Make sure that DS4Windows application can find vJoyInterface.dll library file", false);
                }
            }
        }

        // Feed axis value to VJoy virtual joystic driver (DS4Windows sixaxis (SA) motion sensor steering wheel emulation feature can optionally feed VJoy analog axis instead of ScpVBus x360 axis
        public static void FeedAxisValue(int value, uint vJoyID, HID_USAGES axis)
        {
            if (vJoyAvailable[vJoyID - 1])
            {
                vJoyObj[vJoyID - 1].SetAxis(value, vJoyID, axis);
            }
            else if (!vJoyInitialized[vJoyID - 1])
            {
                // If this was the first call to this FeedAxisValue function and VJoy driver connection is not yet initialized
                // then try to do it now. Subsequent calls will see the the vJoy as available (if connection succeeded) and 
                // there is no need to re-initialize the connection everytime the feeder is used.
                InitializeVJoyDevice(vJoyID, axis);
            }
        }
    }
}
