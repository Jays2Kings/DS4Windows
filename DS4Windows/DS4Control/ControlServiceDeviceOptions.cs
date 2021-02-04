using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DS4Windows.InputDevices;

namespace DS4Windows
{
    public class ControlServiceDeviceOptions
    {
        private DS4DeviceOptions dS4DeviceOpts = new DS4DeviceOptions();
        public DS4DeviceOptions DS4DeviceOpts { get => dS4DeviceOpts; }

        private DualSenseDeviceOptions dualSenseOpts =
            new DualSenseDeviceOptions();
        public DualSenseDeviceOptions DualSenseOpts { get => dualSenseOpts; }

        private SwitchProDeviceOptions switchProDeviceOpts = new SwitchProDeviceOptions();
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => switchProDeviceOpts; }

        private JoyConDeviceOptions joyConDeviceOpts = new JoyConDeviceOptions();
        public JoyConDeviceOptions JoyConDeviceOpts { get => joyConDeviceOpts; }
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

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            
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
        public enum LEDBarMode : ushort
        {
            Off,
            MultipleControllers,
            On,
        }

        private bool enableRumble = true;
        public bool EnableRumble
        {
            get => enableRumble;
            set
            {
                if (enableRumble == value) return;
                enableRumble = value;
                EnableRumbleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableRumbleChanged;

        private DualSenseDevice.HapticIntensity hapticIntensity = DualSenseDevice.HapticIntensity.Medium;
        public DualSenseDevice.HapticIntensity HapticIntensity
        {
            get => hapticIntensity;
            set
            {
                if (hapticIntensity == value) return;
                hapticIntensity = value;
                HapticIntensityChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler HapticIntensityChanged;

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

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode tempOptsNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("DualSenseSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempRumbleNode = xmlDoc.CreateElement("EnableRumble");
            tempRumbleNode.InnerText = enableRumble.ToString();
            tempOptsNode.AppendChild(tempRumbleNode);

            XmlNode tempRumbleStrengthNode = xmlDoc.CreateElement("RumbleStrength");
            tempRumbleStrengthNode.InnerText = hapticIntensity.ToString();
            tempOptsNode.AppendChild(tempRumbleStrengthNode);

            XmlNode tempLedMode = xmlDoc.CreateElement("LEDBarMode");
            tempLedMode.InnerText = ledMode.ToString();
            tempOptsNode.AppendChild(tempLedMode);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableRumble");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableRumble = temp;
                }

                XmlNode itemStrength = baseNode.SelectSingleNode("RumbleStrength");
                if (Enum.TryParse(itemStrength?.InnerText ?? "",
                    out DualSenseDevice.HapticIntensity tempHap))
                {
                    hapticIntensity = tempHap;
                }

                XmlNode itemLedMode = baseNode.SelectSingleNode("LEDBarMode");
                if (Enum.TryParse(itemLedMode?.InnerText ?? "",
                    out LEDBarMode tempLED))
                {
                    ledMode = tempLED;
                }
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
            XmlNode tempOptsNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("SwitchProSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableHomeLED = temp;
                }
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
    }

    public class JoyConControllerOptions : ControllerOptionsStore
    {
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
            XmlNode tempOptsNode = node.SelectSingleNode("JoyConSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("JoyConSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("JoyConSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableHomeLED = temp;
                }
            }
        }
    }
}
