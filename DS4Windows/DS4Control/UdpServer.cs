using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;

namespace DS4Windows
{
    public enum DsState : byte
    {
        [Description("Disconnected")]
        Disconnected = 0x00,
        [Description("Reserved")]
        Reserved = 0x01,
        [Description("Connected")]
        Connected = 0x02
    };

    public enum DsConnection : byte
    {
        [Description("None")]
        None = 0x00,
        [Description("Usb")]
        Usb = 0x01,
        [Description("Bluetooth")]
        Bluetooth = 0x02
    };

    public enum DsModel : byte
    {
        [Description("None")]
        None = 0,
        [Description("DualShock 3")]
        DS3 = 1,
        [Description("DualShock 4")]
        DS4 = 2,
        [Description("Generic Gamepad")]
        Generic = 3
    }

    public enum DsBattery : byte
    {
        None = 0x00,
        Dying = 0x01,
        Low = 0x02,
        Medium = 0x03,
        High = 0x04,
        Full = 0x05,
        Charging = 0xEE,
        Charged = 0xEF
    };

    public struct DualShockPadMeta
    {
        public byte PadId;
        public DsState PadState;
        public DsConnection ConnectionType;
        public DsModel Model;
        public PhysicalAddress PadMacAddress;
        public DsBattery BatteryStatus;
        public bool IsActive;
    }

    [StructLayout(LayoutKind.Explicit, Size = 100)]
    unsafe struct PadDataRspPacket
    {
        // Header section
        [FieldOffset(0)]
        public fixed byte initCode[4];
        [FieldOffset(4)]
        public ushort protocolVersion;
        [FieldOffset(6)]
        public ushort messageLen;
        [FieldOffset(8)]
        public int crc;
        [FieldOffset(12)]
        public uint serverId;
        [FieldOffset(16)]
        public uint messageType;

        // Pad meta section
        [FieldOffset(20)]
        public byte padId;
        [FieldOffset(21)]
        public byte padState;
        [FieldOffset(22)]
        public byte model;
        [FieldOffset(23)]
        public byte connectionType;
        [FieldOffset(24)]
        public fixed byte address[6];
        [FieldOffset(30)]
        public byte batteryStatus;
        [FieldOffset(31)]
        public byte isActive;
        [FieldOffset(32)]
        public uint packetCounter;

        // Primary controls
        [FieldOffset(36)]
        public byte buttons1;
        [FieldOffset(37)]
        public byte buttons2;
        [FieldOffset(38)]
        public byte psButton;
        [FieldOffset(39)]
        public byte touchButton;
        [FieldOffset(40)]
        public byte lx;
        [FieldOffset(41)]
        public byte ly;
        [FieldOffset(42)]
        public byte rx;
        [FieldOffset(43)]
        public byte ry;
        [FieldOffset(44)]
        public byte dpadLeft;
        [FieldOffset(45)]
        public byte dpadDown;
        [FieldOffset(46)]
        public byte dpadRight;
        [FieldOffset(47)]
        public byte dpadUp;
        [FieldOffset(48)]
        public byte square;
        [FieldOffset(49)]
        public byte cross;
        [FieldOffset(50)]
        public byte circle;
        [FieldOffset(51)]
        public byte triangle;
        [FieldOffset(52)]
        public byte r1;
        [FieldOffset(53)]
        public byte l1;
        [FieldOffset(54)]
        public byte r2;
        [FieldOffset(55)]
        public byte l2;

        // Touch 1
        [FieldOffset(56)]
        public byte touch1Active;
        [FieldOffset(57)]
        public byte touch1PacketId;
        [FieldOffset(58)]
        public ushort touch1X;
        [FieldOffset(60)]
        public ushort touch1Y;

        // Touch 2
        [FieldOffset(62)]
        public byte touch2Active;
        [FieldOffset(63)]
        public byte touch2PacketId;
        [FieldOffset(64)]
        public ushort touch2X;
        [FieldOffset(66)]
        public ushort touch2Y;

        // Accel
        [FieldOffset(68)]
        public ulong totalMicroSec;
        [FieldOffset(76)]
        public float accelXG;
        [FieldOffset(80)]
        public float accelYG;
        [FieldOffset(84)]
        public float accelZG;

        // Gyro
        [FieldOffset(88)]
        public float angVelPitch;
        [FieldOffset(92)]
        public float angVelYaw;
        [FieldOffset(96)]
        public float angVelRoll;
    }

    class UdpServer
    {
        public const int NUMBER_SLOTS = 4;
        private Socket udpSock;
        private uint serverId;
        private bool running;
        private byte[] recvBuffer = new byte[1024];
        private SocketAsyncEventArgs[] argsList;
        private int listInd = 0;
        private ReaderWriterLockSlim poolLock = new ReaderWriterLockSlim();
        private SemaphoreSlim _pool;
        private const int ARG_BUFFER_LEN = 80;

        public delegate void GetPadDetail(int padIdx, ref DualShockPadMeta meta);

        private GetPadDetail portInfoGet;

        public UdpServer(GetPadDetail getPadDetailDel)
        {
            portInfoGet = getPadDetailDel;
            _pool = new SemaphoreSlim(ARG_BUFFER_LEN);
            argsList = new SocketAsyncEventArgs[ARG_BUFFER_LEN];
            for (int num = 0; num < ARG_BUFFER_LEN; num++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(new byte[100], 0, 100);
                args.Completed += SocketEvent_Completed;
                argsList[num] = args;
            }
        }

        private void SocketEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            _pool.Release();
        }

        private void CompletedSynchronousSocketEvent()
        {
            _pool.Release();
        }

        enum MessageType
        {
            DSUC_VersionReq = 0x100000,
            DSUS_VersionRsp = 0x100000,
            DSUC_ListPorts = 0x100001,
            DSUS_PortInfo = 0x100001,
            DSUC_PadDataReq = 0x100002,
            DSUS_PadDataRsp = 0x100002,
        };

        private const ushort MaxProtocolVersion = 1001;
        public const int DATA_RSP_PACKET_LEN = 100;

        class ClientRequestTimes
        {
            DateTime allPads;
            DateTime[] padIds;
            Dictionary<PhysicalAddress, DateTime> padMacs;

            public DateTime AllPadsTime { get { return allPads; } }
            public DateTime[] PadIdsTime { get { return padIds; } }
            public Dictionary<PhysicalAddress, DateTime> PadMacsTime { get { return padMacs; } }

            public ClientRequestTimes()
            {
                allPads = DateTime.MinValue;
                padIds = new DateTime[4];

                for (int i = 0; i < padIds.Length; i++)
                    padIds[i] = DateTime.MinValue;

                padMacs = new Dictionary<PhysicalAddress, DateTime>();
            }

            public void RequestPadInfo(byte regFlags, byte idToReg, PhysicalAddress macToReg)
            {
                if (regFlags == 0)
                    allPads = DateTime.UtcNow;
                else
                {
                    if ((regFlags & 0x01) != 0) //id valid
                    {
                        if (idToReg < padIds.Length)
                            padIds[idToReg] = DateTime.UtcNow;
                    }
                    if ((regFlags & 0x02) != 0) //mac valid
                    {
                        padMacs[macToReg] = DateTime.UtcNow;
                    }
                }
            }
        }

        private Dictionary<IPEndPoint, ClientRequestTimes> clients = new Dictionary<IPEndPoint, ClientRequestTimes>();

        private int BeginPacket(byte[] packetBuf, ushort reqProtocolVersion = MaxProtocolVersion)
        {
            int currIdx = 0;
            packetBuf[currIdx++] = (byte)'D';
            packetBuf[currIdx++] = (byte)'S';
            packetBuf[currIdx++] = (byte)'U';
            packetBuf[currIdx++] = (byte)'S';

            Array.Copy(BitConverter.GetBytes((ushort)reqProtocolVersion), 0, packetBuf, currIdx, 2);
            currIdx += 2;

            Array.Copy(BitConverter.GetBytes((ushort)packetBuf.Length - 16), 0, packetBuf, currIdx, 2);
            currIdx += 2;

            Array.Clear(packetBuf, currIdx, 4); //place for crc
            currIdx += 4;

            Array.Copy(BitConverter.GetBytes((uint)serverId), 0, packetBuf, currIdx, 4);
            currIdx += 4;

            return currIdx;
        }

        private unsafe void BeginDataRspPacket(ref PadDataRspPacket currentRsp, ushort reqProtocolVersion = MaxProtocolVersion)
        {
            const int outputPacketLen = 100;

            currentRsp.initCode[0] = (byte)'D';
            currentRsp.initCode[1] = (byte)'S';
            currentRsp.initCode[2] = (byte)'U';
            currentRsp.initCode[3] = (byte)'S';

            currentRsp.protocolVersion = reqProtocolVersion;
            currentRsp.messageLen = (ushort)outputPacketLen - 16;

            currentRsp.crc = 0;
            currentRsp.serverId = serverId;
        }

        private void FinishPacket(byte[] packetBuf)
        {
            Array.Clear(packetBuf, 8, 4);

            //uint crcCalc = Crc32Algorithm.Compute(packetBuf);
            uint seed = Crc32Algorithm.DefaultSeed;
            uint crcCalc = ~Crc32Algorithm.CalculateBasicHash(ref seed, ref packetBuf, 0, packetBuf.Length);
            Array.Copy(BitConverter.GetBytes((uint)crcCalc), 0, packetBuf, 8, 4);
        }

        private unsafe void FinishDataRspPacket(ref PadDataRspPacket currentRsp, byte[] packetBuf)
        {
            currentRsp.crc = 0;
            CopyBytes(ref currentRsp, packetBuf, DATA_RSP_PACKET_LEN);

            //uint crcCalc = Crc32Algorithm.Compute(packetBuf);
            uint seed = Crc32Algorithm.DefaultSeed;
            uint crcCalc = ~Crc32Algorithm.CalculateBasicHash(ref seed, ref packetBuf, 0, packetBuf.Length);
            Array.Copy(BitConverter.GetBytes((uint)crcCalc), 0, packetBuf, 8, 4);
        }

        private void SendPacket(IPEndPoint clientEP, byte[] usefulData, ushort reqProtocolVersion = MaxProtocolVersion)
        {
            byte[] packetData = new byte[usefulData.Length + 16];
            int currIdx = BeginPacket(packetData, reqProtocolVersion);
            Array.Copy(usefulData, 0, packetData, currIdx, usefulData.Length);
            FinishPacket(packetData);

            //try { udpSock.SendTo(packetData, clientEP); }
            int temp = 0;
            poolLock.EnterWriteLock();
            temp = listInd;
            listInd = ++listInd % ARG_BUFFER_LEN;
            SocketAsyncEventArgs args = argsList[temp];
            poolLock.ExitWriteLock();

            _pool.Wait();
            args.RemoteEndPoint = clientEP;
            Array.Copy(packetData, args.Buffer, packetData.Length);
            //args.SetBuffer(packetData, 0, packetData.Length);
            bool sentAsync = false;
            try {
                sentAsync = udpSock.SendToAsync(args);
                if (!sentAsync) CompletedSynchronousSocketEvent();
            }
            catch (Exception /*e*/) { }
            finally
            {
                if (!sentAsync) CompletedSynchronousSocketEvent();
            }
        }

        private void ProcessIncoming(byte[] localMsg, IPEndPoint clientEP)
        {
            try
            {
                int currIdx = 0;
                if (localMsg[0] != 'D' || localMsg[1] != 'S' || localMsg[2] != 'U' || localMsg[3] != 'C')
                    return;
                else
                    currIdx += 4;

                uint protocolVer = BitConverter.ToUInt16(localMsg, currIdx);
                currIdx += 2;

                if (protocolVer > MaxProtocolVersion)
                    return;

                uint packetSize = BitConverter.ToUInt16(localMsg, currIdx);
                currIdx += 2;

                if (packetSize < 0)
                    return;

                packetSize += 16; //size of header
                if (packetSize > localMsg.Length)
                    return;
                else if (packetSize < localMsg.Length)
                {
                    byte[] newMsg = new byte[packetSize];
                    Array.Copy(localMsg, newMsg, packetSize);
                    localMsg = newMsg;
                }

                uint crcValue = BitConverter.ToUInt32(localMsg, currIdx);
                //zero out the crc32 in the packet once we got it since that's whats needed for calculation
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;

                uint crcCalc = Crc32Algorithm.Compute(localMsg);
                if (crcValue != crcCalc)
                    return;

                uint clientId = BitConverter.ToUInt32(localMsg, currIdx);
                currIdx += 4;

                uint messageType = BitConverter.ToUInt32(localMsg, currIdx);
                currIdx += 4;

                if (messageType == (uint)MessageType.DSUC_VersionReq)
                {
                    byte[] outputData = new byte[8];
                    int outIdx = 0;
                    Array.Copy(BitConverter.GetBytes((uint)MessageType.DSUS_VersionRsp), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((ushort)MaxProtocolVersion), 0, outputData, outIdx, 2);
                    outIdx += 2;
                    outputData[outIdx++] = 0;
                    outputData[outIdx++] = 0;

                    SendPacket(clientEP, outputData, 1001);
                }
                else if (messageType == (uint)MessageType.DSUC_ListPorts)
                {
                    int numPadRequests = BitConverter.ToInt32(localMsg, currIdx);
                    currIdx += 4;
                    if (numPadRequests < 0 || numPadRequests > NUMBER_SLOTS)
                        return;

                    int requestsIdx = currIdx;
                    for (int i = 0; i < numPadRequests; i++)
                    {
                        byte currRequest = localMsg[requestsIdx + i];
                        if (currRequest >= NUMBER_SLOTS)
                            return;
                    }

                    byte[] outputData = new byte[16];
                    for (byte i = 0; i < numPadRequests; i++)
                    {
                        byte currRequest = localMsg[requestsIdx + i];
                        DualShockPadMeta padData = new DualShockPadMeta();
                        portInfoGet(currRequest, ref padData);

                        int outIdx = 0;
                        Array.Copy(BitConverter.GetBytes((uint)MessageType.DSUS_PortInfo), 0, outputData, outIdx, 4);
                        outIdx += 4;

                        outputData[outIdx++] = (byte)padData.PadId;
                        outputData[outIdx++] = (byte)padData.PadState;
                        outputData[outIdx++] = (byte)padData.Model;
                        outputData[outIdx++] = (byte)padData.ConnectionType;

                        byte[] addressBytes = null;
                        if (padData.PadMacAddress != null)
                            addressBytes = padData.PadMacAddress.GetAddressBytes();

                        if (addressBytes != null && addressBytes.Length == 6)
                        {
                            outputData[outIdx++] = addressBytes[0];
                            outputData[outIdx++] = addressBytes[1];
                            outputData[outIdx++] = addressBytes[2];
                            outputData[outIdx++] = addressBytes[3];
                            outputData[outIdx++] = addressBytes[4];
                            outputData[outIdx++] = addressBytes[5];
                        }
                        else
                        {
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                        }

                        outputData[outIdx++] = (byte)padData.BatteryStatus;
                        outputData[outIdx++] = 0;

                        SendPacket(clientEP, outputData, 1001);
                    }
                }
                else if (messageType == (uint)MessageType.DSUC_PadDataReq)
                {
                    byte regFlags = localMsg[currIdx++];
                    byte idToReg = localMsg[currIdx++];
                    PhysicalAddress macToReg = null;
                    {
                        byte[] macBytes = new byte[6];
                        Array.Copy(localMsg, currIdx, macBytes, 0, macBytes.Length);
                        currIdx += macBytes.Length;
                        macToReg = new PhysicalAddress(macBytes);
                    }

                    lock (clients)
                    {
                        if (clients.ContainsKey(clientEP))
                            clients[clientEP].RequestPadInfo(regFlags, idToReg, macToReg);
                        else
                        {
                            var clientTimes = new ClientRequestTimes();
                            clientTimes.RequestPadInfo(regFlags, idToReg, macToReg);
                            clients[clientEP] = clientTimes;
                        }
                    }
                }
            }
            catch (Exception /*e*/) { }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            byte[] localMsg = null;
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                //Get the received message.
                Socket recvSock = (Socket)iar.AsyncState;
                int msgLen = recvSock.EndReceiveFrom(iar, ref clientEP);

                localMsg = new byte[msgLen];
                Array.Copy(recvBuffer, localMsg, msgLen);
            }
            catch (SocketException)
            {
                ResetUDPConn();
            }
            catch (Exception /*e*/) { }

            //Start another receive as soon as we copied the data
            StartReceive();

            //Process the data if its valid
            if (localMsg != null)
                ProcessIncoming(localMsg, (IPEndPoint)clientEP);
        }
        private void StartReceive()
        {
            try
            {
                if (running)
                {
                    //Start listening for a new message.
                    EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                    udpSock.BeginReceiveFrom(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, ref newClientEP, ReceiveCallback, udpSock);
                }
            }
            catch (SocketException /*ex*/)
            {
                ResetUDPConn();
                StartReceive();
            }
        }

        /// <summary>
        /// Used to send CONNRESET ioControlCode to Socket used for UDP Server.
        /// Frees Socket from potentially firing SocketException instances after a client
        /// connection is terminated. Avoids memory leak
        /// </summary>
        private void ResetUDPConn()
        {
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            udpSock.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
        }

        public void Start(int port, string listenAddress = "")
        {
            if (running)
            {
                if (udpSock != null)
                {
                    udpSock.Close();
                    udpSock = null;
                }
                running = false;
            }

            udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                IPAddress udpListenIPAddress;
                if (listenAddress == "127.0.0.1" || listenAddress == "")
                {
                    // Listen on local looback interface (default option). Does not allow remote client connections
                    udpListenIPAddress = IPAddress.Loopback;
                }
                else if (listenAddress == "0.0.0.0")
                {
                    // Listen on all IPV4 interfaces. 
                    // Remote client connections allowed. If the local network is not "safe" then may not be a good idea, because at the moment incoming connections are not authenticated in any way
                    udpListenIPAddress = IPAddress.Any;                    
                }
                else
                {
                    // Listen on a specific hostname or IPV4 interface address. If the hostname has multiple interfaces then use the first IPV4 address because it is usually the primary IP addr.
                    // Remote client connections allowed.
                    IPAddress[] ipAddresses = Dns.GetHostAddresses(listenAddress);
                    udpListenIPAddress = null; 
                    foreach (IPAddress ip4 in ipAddresses.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                    {
                        udpListenIPAddress = ip4;
                        break;
                    }
                    if (udpListenIPAddress == null) throw new SocketException(10049 /*WSAEADDRNOTAVAIL*/);
                }
                udpSock.Bind(new IPEndPoint(udpListenIPAddress, port));
            }
            catch (SocketException ex)
            {
                udpSock.Close();
                udpSock = null;

                throw ex;
            }

            byte[] randomBuf = new byte[4];
            new Random().NextBytes(randomBuf);
            serverId = BitConverter.ToUInt32(randomBuf, 0);

            running = true;
            StartReceive();
        }

        public void Stop()
        {
            running = false;
            if (udpSock != null)
            {
                udpSock.Close();
                udpSock = null;
            }
        }

        private bool ReportToBuffer(DS4State hidReport, byte[] outputData, ref int outIdx)
        {
            unchecked
            {
                outputData[outIdx] = 0;

                if (hidReport.DpadLeft) outputData[outIdx] |= 0x80;
                if (hidReport.DpadDown) outputData[outIdx] |= 0x40;
                if (hidReport.DpadRight) outputData[outIdx] |= 0x20;
                if (hidReport.DpadUp) outputData[outIdx] |= 0x10;

                if (hidReport.Options) outputData[outIdx] |= 0x08;
                if (hidReport.R3) outputData[outIdx] |= 0x04;
                if (hidReport.L3) outputData[outIdx] |= 0x02;
                if (hidReport.Share) outputData[outIdx] |= 0x01;

                outputData[++outIdx] = 0;

                if (hidReport.Square) outputData[outIdx] |= 0x80;
                if (hidReport.Cross) outputData[outIdx] |= 0x40;
                if (hidReport.Circle) outputData[outIdx] |= 0x20;
                if (hidReport.Triangle) outputData[outIdx] |= 0x10;

                if (hidReport.R1) outputData[outIdx] |= 0x08;
                if (hidReport.L1) outputData[outIdx] |= 0x04;
                if (hidReport.R2Btn) outputData[outIdx] |= 0x02;
                if (hidReport.L2Btn) outputData[outIdx] |= 0x01;

                outputData[++outIdx] = (hidReport.PS) ? (byte)1 : (byte)0;
                outputData[++outIdx] = (hidReport.TouchButton) ? (byte)1 : (byte)0;

                //Left stick
                outputData[++outIdx] = hidReport.LX;
                outputData[++outIdx] = hidReport.LY;
                outputData[outIdx] = (byte)(255 - outputData[outIdx]); //invert Y by convention

                //Right stick
                outputData[++outIdx] = hidReport.RX;
                outputData[++outIdx] = hidReport.RY;
                outputData[outIdx] = (byte)(255 - outputData[outIdx]); //invert Y by convention

                //we don't have analog buttons on DS4 :(
                outputData[++outIdx] = hidReport.DpadLeft ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadDown ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadRight ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadUp ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.Square ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Cross ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Circle ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Triangle ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.R1 ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.L1 ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.R2;
                outputData[++outIdx] = hidReport.L2;

                outIdx++;

                //DS4 only: touchpad points
                for (int i = 0; i < 2; i++)
                {
                    var tpad = (i == 0) ? hidReport.TrackPadTouch0 : hidReport.TrackPadTouch1;

                    outputData[outIdx++] = tpad.IsActive ? (byte)1 : (byte)0;
                    outputData[outIdx++] = (byte)tpad.Id;
                    Array.Copy(BitConverter.GetBytes((ushort)tpad.X), 0, outputData, outIdx, 2);
                    outIdx += 2;
                    Array.Copy(BitConverter.GetBytes((ushort)tpad.Y), 0, outputData, outIdx, 2);
                    outIdx += 2;
                }

                //motion timestamp
                if (hidReport.Motion != null)
                    Array.Copy(BitConverter.GetBytes((ulong)hidReport.totalMicroSec), 0, outputData, outIdx, 8);
                else
                    Array.Clear(outputData, outIdx, 8);

                outIdx += 8;

                //accelerometer
                if (hidReport.Motion != null)
                {
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.accelXG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.accelYG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)-hidReport.Motion.accelZG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                }
                else
                {
                    Array.Clear(outputData, outIdx, 12);
                    outIdx += 12;
                }

                //gyroscope
                if (hidReport.Motion != null)
                {
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelPitch), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelYaw), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelRoll), 0, outputData, outIdx, 4);
                    outIdx += 4;
                }
                else
                {
                    Array.Clear(outputData, outIdx, 12);
                    outIdx += 12;
                }
            }

            return true;
        }

        private bool ReportToBufferDataRsp(DS4State hidReport, ref PadDataRspPacket currentRsp)
        {
            unchecked
            {
                currentRsp.buttons1 = 0;
                if (hidReport.DpadLeft) currentRsp.buttons1 |= 0x80;
                if (hidReport.DpadDown) currentRsp.buttons1 |= 0x40;
                if (hidReport.DpadRight) currentRsp.buttons1 |= 0x20;
                if (hidReport.DpadUp) currentRsp.buttons1 |= 0x10;

                if (hidReport.Options) currentRsp.buttons1 |= 0x08;
                if (hidReport.R3) currentRsp.buttons1 |= 0x04;
                if (hidReport.L3) currentRsp.buttons1 |= 0x02;
                if (hidReport.Share) currentRsp.buttons1 |= 0x01;

                currentRsp.buttons2 = 0;

                if (hidReport.Square) currentRsp.buttons2 |= 0x80;
                if (hidReport.Cross) currentRsp.buttons2 |= 0x40;
                if (hidReport.Circle) currentRsp.buttons2  |= 0x20;
                if (hidReport.Triangle) currentRsp.buttons2 |= 0x10;

                if (hidReport.R1) currentRsp.buttons2 |= 0x08;
                if (hidReport.L1) currentRsp.buttons2 |= 0x04;
                if (hidReport.R2Btn) currentRsp.buttons2 |= 0x02;
                if (hidReport.L2Btn) currentRsp.buttons2 |= 0x01;

                currentRsp.psButton = (hidReport.PS) ? (byte)1 : (byte)0;
                currentRsp.touchButton = (hidReport.TouchButton) ? (byte)1 : (byte)0;

                //Left stick
                currentRsp.lx = hidReport.LX;
                currentRsp.ly = hidReport.LY;
                currentRsp.ly = (byte)(255 - currentRsp.ly); //invert Y by convention

                //Right stick
                currentRsp.rx = hidReport.RX;
                currentRsp.ry = hidReport.RY;
                currentRsp.ry = (byte)(255 - currentRsp.ry); //invert Y by convention

                //we don't have analog buttons on DS4 :(
                currentRsp.dpadLeft = hidReport.DpadLeft ? (byte)0xFF : (byte)0x00;
                currentRsp.dpadDown = hidReport.DpadDown ? (byte)0xFF : (byte)0x00;
                currentRsp.dpadRight = hidReport.DpadRight ? (byte)0xFF : (byte)0x00;
                currentRsp.dpadUp = hidReport.DpadUp ? (byte)0xFF : (byte)0x00;

                currentRsp.square = hidReport.Square ? (byte)0xFF : (byte)0x00;
                currentRsp.cross = hidReport.Cross ? (byte)0xFF : (byte)0x00;
                currentRsp.circle = hidReport.Circle ? (byte)0xFF : (byte)0x00;
                currentRsp.triangle = hidReport.Triangle ? (byte)0xFF : (byte)0x00;

                currentRsp.r1 = hidReport.R1 ? (byte)0xFF : (byte)0x00;
                currentRsp.l1 = hidReport.L1 ? (byte)0xFF : (byte)0x00;

                currentRsp.r2 = hidReport.R2;
                currentRsp.l2 = hidReport.L2;

                //DS4 only: touchpad points
                for (int i = 0; i < 2; i++)
                {
                    var tpad = (i == 0) ? hidReport.TrackPadTouch0 : hidReport.TrackPadTouch1;
                    if (i == 0)
                    {
                        currentRsp.touch1Active = tpad.IsActive ? (byte)1 : (byte)0;
                        currentRsp.touch1PacketId = (byte)tpad.Id;
                        currentRsp.touch1X = (ushort)tpad.X;
                        currentRsp.touch1Y = (ushort)tpad.Y;
                    }
                    else if (i == 1)
                    {
                        currentRsp.touch2Active = tpad.IsActive ? (byte)1 : (byte)0;
                        currentRsp.touch2PacketId = (byte)tpad.Id;
                        currentRsp.touch2X = (ushort)tpad.X;
                        currentRsp.touch2Y = (ushort)tpad.Y;
                    }
                }

                //motion timestamp
                if (hidReport.Motion != null)
                    currentRsp.totalMicroSec = hidReport.totalMicroSec;
                else
                    currentRsp.totalMicroSec = 0;

                //accelerometer
                if (hidReport.Motion != null)
                {
                    currentRsp.accelXG = (float)hidReport.Motion.accelXG;
                    currentRsp.accelYG = (float)hidReport.Motion.accelYG;
                    currentRsp.accelZG = (float)-hidReport.Motion.accelZG;
                }
                else
                {
                    currentRsp.accelXG = 0;
                    currentRsp.accelYG = 0;
                    currentRsp.accelZG = 0;
                }

                //gyroscope
                if (hidReport.Motion != null)
                {
                    currentRsp.angVelPitch = (float)hidReport.Motion.angVelPitch;
                    currentRsp.angVelYaw = (float)hidReport.Motion.angVelYaw;
                    currentRsp.angVelRoll = (float)hidReport.Motion.angVelRoll;
                }
                else
                {
                    currentRsp.angVelPitch = 0;
                    currentRsp.angVelYaw = 0;
                    currentRsp.angVelRoll = 0;
                }
            }

            return true;
        }

        public unsafe void NewReportIncoming(ref DualShockPadMeta padMeta, DS4State hidReport, byte[] outputData)
        {
            if (!running)
                return;

            var clientsList = new List<IPEndPoint>();
            var now = DateTime.UtcNow;
            lock (clients)
            {
                var clientsToDelete = new List<IPEndPoint>();

                foreach (var cl in clients)
                {
                    const double TimeoutLimit = 5;

                    if ((now - cl.Value.AllPadsTime).TotalSeconds < TimeoutLimit)
                        clientsList.Add(cl.Key);
                    else if ((padMeta.PadId < cl.Value.PadIdsTime.Length) &&
                             (now - cl.Value.PadIdsTime[(byte)padMeta.PadId]).TotalSeconds < TimeoutLimit)
                        clientsList.Add(cl.Key);
                    else if (cl.Value.PadMacsTime.ContainsKey(padMeta.PadMacAddress) &&
                             (now - cl.Value.PadMacsTime[padMeta.PadMacAddress]).TotalSeconds < TimeoutLimit)
                        clientsList.Add(cl.Key);
                    else //check if this client is totally dead, and remove it if so
                    {
                        bool clientOk = false;
                        for (int i = 0; i < cl.Value.PadIdsTime.Length; i++)
                        {
                            var dur = (now - cl.Value.PadIdsTime[i]).TotalSeconds;
                            if (dur < TimeoutLimit)
                            {
                                clientOk = true;
                                break;
                            }
                        }
                        if (!clientOk)
                        {
                            foreach (var dict in cl.Value.PadMacsTime)
                            {
                                var dur = (now - dict.Value).TotalSeconds;
                                if (dur < TimeoutLimit)
                                {
                                    clientOk = true;
                                    break;
                                }
                            }

                            if (!clientOk)
                                clientsToDelete.Add(cl.Key);
                        }
                    }
                }

                foreach (var delCl in clientsToDelete)
                {
                    clients.Remove(delCl);
                }
                clientsToDelete.Clear();
                clientsToDelete = null;
            }

            if (clientsList.Count <= 0)
                return;

            unchecked
            {
                //byte[] outputData = new byte[100];
                //int outIdx = BeginPacket(outputData, 1001);
                PadDataRspPacket currentRsp = new PadDataRspPacket();
                BeginDataRspPacket(ref currentRsp, 1001);
                currentRsp.messageType = (uint)MessageType.DSUS_PadDataRsp;

                currentRsp.padId = (byte)padMeta.PadId;
                currentRsp.padState = (byte)padMeta.PadState;
                currentRsp.model = (byte)padMeta.Model;
                currentRsp.connectionType = (byte)padMeta.ConnectionType;
                {
                    byte[] padMac = padMeta.PadMacAddress.GetAddressBytes();
                    currentRsp.address[0] = padMac[0];
                    currentRsp.address[1] = padMac[1];
                    currentRsp.address[2] = padMac[2];
                    currentRsp.address[3] = padMac[3];
                    currentRsp.address[4] = padMac[4];
                    currentRsp.address[5] = padMac[5];
                }
                currentRsp.batteryStatus = (byte)padMeta.BatteryStatus;
                currentRsp.isActive = padMeta.IsActive ? (byte)1 : (byte)0;

                currentRsp.packetCounter = hidReport.PacketCounter;

                if (!ReportToBufferDataRsp(hidReport, ref currentRsp))
                    return;
                else
                    FinishDataRspPacket(ref currentRsp, outputData);

                foreach (var cl in clientsList)
                {
                    //try { udpSock.SendTo(outputData, cl); }
                    int temp = 0;
                    poolLock.EnterWriteLock();
                    temp = listInd;
                    listInd = ++listInd % ARG_BUFFER_LEN;
                    SocketAsyncEventArgs args = argsList[temp];
                    poolLock.ExitWriteLock();

                    _pool.Wait();
                    args.RemoteEndPoint = cl;
                    Array.Copy(outputData, args.Buffer, outputData.Length);
                    bool sentAsync = false;
                    try {
                        sentAsync = udpSock.SendToAsync(args);
                    }
                    catch (SocketException /*ex*/) { }
                    finally
                    {
                        if (!sentAsync) CompletedSynchronousSocketEvent();
                    }
                }
            }

            clientsList.Clear();
            clientsList = null;
        }
    
        private void CopyBytes(ref PadDataRspPacket outReport, byte[] outBuffer, int bufferLen)
        {
            GCHandle h = GCHandle.Alloc(outReport, GCHandleType.Pinned);
            Marshal.Copy(h.AddrOfPinnedObject(), outBuffer, 0, bufferLen);
            h.Free();
        }
    }
}
