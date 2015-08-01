using System;
using EAll4Windows.EAll4Library;

namespace EAll4Windows
{
    public interface IEAll4Device
    {
        bool IsAlive();
        bool Charging { get; }
        string MacAddress { get; }
        int Battery { get; }
        double Latency { get; }
        ConnectionType ConnectionType { get; }
        byte LeftHeavySlowRumble { get; }
        byte LightBarOnDuration { get; }
        DateTime lastActive { get; }
        byte RightLightFastRumble { get; }
        EAll4Color LightBarColor { get; set; }
        HidDevice HidDevice { get; }
        bool IsExclusive { get; }
        EAll4Touchpad Touchpad { get; }
        bool IsDisconnecting { get; }
        string error { get; }
        DateTime firstActive { get; }
        int IdleTimeout { get; set; }
        int[] PIDs { get; }
        int[] VIDs { get; }
        event EventHandler<EventArgs> Removal;

        bool DisconnectBT();
        void setRumble(byte lightBoosted, byte heavyBoosted);
        void pushHapticState(EAll4HapticState haptics);
        ControllerState getCurrentState();
        void getCurrentState(ControllerState state);
        void Load(HidDevice hidDevice);
        void StartUpdate();
        void StopUpdate();
        event EventHandler<EventArgs> Report;
        void FlushHID();
        void getExposedState(EAll4StateExposed eAll4StateExposed, ControllerState controllerState);
        void getPreviousState(ControllerState controllerState);
        short InputReportByteLengthUSB { get; }
    }
}