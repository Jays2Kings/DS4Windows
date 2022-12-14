using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.DTOXml;

namespace DS4Windows
{
    public class ControlServiceDeviceOptions
    {
        private DS4DeviceOptions dS4DeviceOpts = new DS4DeviceOptions();
        public DS4DeviceOptions DS4DeviceOpts { get => dS4DeviceOpts; }

        private DualSenseDeviceOptions dualSenseOpts = new DualSenseDeviceOptions();
        public DualSenseDeviceOptions DualSenseOpts { get => dualSenseOpts; }

        private SwitchProDeviceOptions switchProDeviceOpts = new SwitchProDeviceOptions();
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => switchProDeviceOpts; }

        private JoyConDeviceOptions joyConDeviceOpts = new JoyConDeviceOptions();
        public JoyConDeviceOptions JoyConDeviceOpts { get => joyConDeviceOpts; }

        private bool verboseLogMessages;
        public bool VerboseLogMessages { get => verboseLogMessages; set => verboseLogMessages = value; }

        public ControlServiceDeviceOptions()
        {
            // If enabled then DS4Windows shows additional log messages when a gamepad is connected (may be useful to diagnose connection problems).
            // This option is not persistent (ie. not saved into config files), so if enabled then it is reset back to FALSE when DS4Windows is restarted.
            verboseLogMessages = false;
        }
    }

    public abstract class ControllerOptionsStore
    {
        protected InputDeviceType deviceType;
        public InputDeviceType DeviceType { get => deviceType; }

        public ControllerOptionsStore(InputDeviceType deviceType)
        {
            this.deviceType = deviceType;
        }

        public virtual void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }

        public virtual void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }
    }

    public class DS4DeviceOptions
    {
        public const bool DEFAULT_ENABLE = true;
        private bool enabled = DEFAULT_ENABLE;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "DS4SupportSettings";
        private bool copyCatController;
        public bool IsCopyCat
        {
            get => copyCatController;
            set
            {
                if (copyCatController == value) return;
                copyCatController = value;
                IsCopyCatChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsCopyCatChanged;

        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(DS4ControllerOptsDTO));

            using (Utf8StringWriter strWriter = new Utf8StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = false,
                        OmitXmlDeclaration = true, // only partial XML with no declaration
                    });

                // Write root element and children
                DS4ControllerOptsDTO dto = new DS4ControllerOptsDTO();
                dto.MapFrom(this);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            XmlNode tempDS4Node = xmlDoc.CreateDocumentFragment();
            tempDS4Node.InnerXml = testStr;

            XmlNode tempOptsNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (tempOptsNode != null)
            {
                node.RemoveChild(tempOptsNode);
            }

            tempOptsNode = tempDS4Node;
            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DS4ControllerOptsDTO));
            XmlNode baseNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (baseNode == null)
                return;

            try
            {
                using var stringReader = new StringReader(baseNode.OuterXml);
                using var xmlReader = XmlReader.Create(stringReader);
                DS4ControllerOptsDTO dto = serializer.Deserialize(xmlReader) as DS4ControllerOptsDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public class DualSenseDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class DualSenseControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "DualSenseSupportSettings";

        public enum LEDBarMode : ushort
        {
            Off,
            MultipleControllers,
            BatteryPercentage,
            On,
        }

        public enum MuteLEDMode : ushort
        {
            Off,
            On,
            Pulse
        }

        private LEDBarMode ledMode = LEDBarMode.MultipleControllers;
        public LEDBarMode LedMode
        {
            get => ledMode;
            set
            {
                if (ledMode == value) return;
                ledMode = value;
                LedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LedModeChanged;

        private MuteLEDMode muteLedMode = MuteLEDMode.Off;
        public MuteLEDMode MuteLedMode
        {
            get => muteLedMode;
            set
            {
                if (muteLedMode == value) return;
                muteLedMode = value;
                MuteLedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MuteLedModeChanged;

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(DualSenseControllerOptsDTO));

            using (Utf8StringWriter strWriter = new Utf8StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = false,
                        OmitXmlDeclaration = true, // only partial XML with no declaration
                    });

                // Write root element and children
                DualSenseControllerOptsDTO dto = new DualSenseControllerOptsDTO();
                dto.MapFrom(this);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            XmlNode tempDSNode = xmlDoc.CreateDocumentFragment();
            tempDSNode.InnerXml = testStr;

            XmlNode tempOptsNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (tempOptsNode != null)
            {
                node.RemoveChild(tempOptsNode);
            }

            tempOptsNode = tempDSNode;
            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DualSenseControllerOptsDTO));
            XmlNode baseNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (baseNode == null)
                return;

            try
            {
                using var stringReader = new StringReader(baseNode.OuterXml);
                using var xmlReader = XmlReader.Create(stringReader);
                DualSenseControllerOptsDTO dto = serializer.Deserialize(xmlReader) as DualSenseControllerOptsDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public class SwitchProDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "SwitchProSupportSettings";

        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public SwitchProControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(SwitchProControllerOptsDTO));

            using (Utf8StringWriter strWriter = new Utf8StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = false,
                        OmitXmlDeclaration = true, // only partial XML with no declaration
                    });

                // Write root element and children
                SwitchProControllerOptsDTO dto = new SwitchProControllerOptsDTO();
                dto.MapFrom(this);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            XmlNode tempSwitchProNode = xmlDoc.CreateDocumentFragment();
            tempSwitchProNode.InnerXml = testStr;

            XmlNode tempOptsNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (tempOptsNode != null)
            {
                node.RemoveChild(tempOptsNode);
            }

            tempOptsNode = tempSwitchProNode;
            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SwitchProControllerOptsDTO));
            XmlNode baseNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (baseNode == null)
                return;

            try
            {
                using var stringReader = new StringReader(baseNode.OuterXml);
                using var xmlReader = XmlReader.Create(stringReader);
                SwitchProControllerOptsDTO dto = serializer.Deserialize(xmlReader) as SwitchProControllerOptsDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public class JoyConDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;

        public enum LinkMode : ushort
        {
            Split,
            Joined,
        }

        private LinkMode linkedMode = LinkMode.Joined;
        public LinkMode LinkedMode
        {
            get => linkedMode;
            set
            {
                if (linkedMode == value) return;
                linkedMode = value;
            }
        }

        public enum JoinedGyroProvider : ushort
        {
            JoyConL,
            JoyConR,
        }

        private JoinedGyroProvider joinGyroProv = JoinedGyroProvider.JoyConR;
        public JoinedGyroProvider JoinGyroProv
        {
            get => joinGyroProv;
            set
            {
                if (joinGyroProv == value) return;
                joinGyroProv = value;
            }
        }
    }

    public class JoyConControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "JoyConSupportSettings";

        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public JoyConControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(JoyConControllerOptsDTO));

            using (Utf8StringWriter strWriter = new Utf8StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = false,
                        OmitXmlDeclaration = true, // only partial XML with no declaration
                    });

                // Write root element and children
                JoyConControllerOptsDTO dto = new JoyConControllerOptsDTO();
                dto.MapFrom(this);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            XmlNode tempJoyConNode = xmlDoc.CreateDocumentFragment();
            tempJoyConNode.InnerXml = testStr;

            XmlNode tempOptsNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (tempOptsNode != null)
            {
                node.RemoveChild(tempOptsNode);
            }

            tempOptsNode = tempJoyConNode;
            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(JoyConControllerOptsDTO));
            XmlNode baseNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (baseNode == null)
                return;

            try
            {
                using var stringReader = new StringReader(baseNode.OuterXml);
                using var xmlReader = XmlReader.Create(stringReader);
                JoyConControllerOptsDTO dto = serializer.Deserialize(xmlReader) as JoyConControllerOptsDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
