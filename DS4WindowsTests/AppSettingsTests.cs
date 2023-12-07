/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Xml.Serialization;
using DS4Windows;
using DS4WinWPF.DS4Control.DTOXml;

namespace DS4WindowsTests
{
    [TestClass]
    public class AppSettingsTests
    {
        private string appSettingsXml = string.Empty;

        public AppSettingsTests()
        {
            #region SettingsXml
            appSettingsXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!-- Profile Configuration Data. 12/05/2023 00:24:21 -->
<!-- Made with DS4Windows version 3.2.21 -->

<Profile app_version=""3.2.21"" config_version=""2"">
  <useExclusiveMode>False</useExclusiveMode>
  <startMinimized>False</startMinimized>
  <minimizeToTaskbar>False</minimizeToTaskbar>
  <formWidth>782</formWidth>
  <formHeight>550</formHeight>
  <formLocationX>0</formLocationX>
  <formLocationY>0</formLocationY>
  <Controller1>Default</Controller1>
  <Controller2>Default</Controller2>
  <Controller3>Default</Controller3>
  <Controller4>Default</Controller4>
  <Controller5>Default</Controller5>
  <Controller6>Default</Controller6>
  <Controller7>Default</Controller7>
  <Controller8>Default</Controller8>
  <LastChecked>12/05/2023 00:24:15</LastChecked>
  <CheckWhen>24</CheckWhen>
  <Notifications>2</Notifications>
  <DisconnectBTAtStop>False</DisconnectBTAtStop>
  <SwipeProfiles>True</SwipeProfiles>
  <QuickCharge>False</QuickCharge>
  <CloseMinimizes>False</CloseMinimizes>
  <UseLang />
  <DownloadLang>False</DownloadLang>
  <FlashWhenLate>True</FlashWhenLate>
  <FlashWhenLateAt>500</FlashWhenLateAt>
  <AppIcon>Default</AppIcon>
  <AppTheme>Default</AppTheme>
  <UseOSCServer>False</UseOSCServer>
  <OSCServerPort>9000</OSCServerPort>
  <InterpretingOscMonitoring>False</InterpretingOscMonitoring>
  <UseOSCSender>False</UseOSCSender>
  <OSCSenderPort>9001</OSCSenderPort>
  <OSCSenderAddress>127.0.0.1</OSCSenderAddress>
  <UseUDPServer>False</UseUDPServer>
  <UDPServerPort>26760</UDPServerPort>
  <UDPServerListenAddress>127.0.0.1</UDPServerListenAddress>
  <UDPServerSmoothingOptions>
    <UseSmoothing>False</UseSmoothing>
    <UdpSmoothMinCutoff>0.4</UdpSmoothMinCutoff>
    <UdpSmoothBeta>0.2</UdpSmoothBeta>
  </UDPServerSmoothingOptions>
  <UseCustomSteamFolder>False</UseCustomSteamFolder>
  <CustomSteamFolder />
  <AutoProfileRevertDefaultProfile>True</AutoProfileRevertDefaultProfile>
  <AbsRegionDisplay />
  <DeviceOptions>
    <DS4SupportSettings>
      <Enabled>True</Enabled>
    </DS4SupportSettings>
    <DualSenseSupportSettings>
      <Enabled>True</Enabled>
    </DualSenseSupportSettings>
    <SwitchProSupportSettings>
      <Enabled>False</Enabled>
    </SwitchProSupportSettings>
    <JoyConSupportSettings>
      <Enabled>False</Enabled>
      <LinkMode>Joined</LinkMode>
      <JoinedGyroProvider>JoyConL</JoinedGyroProvider>
    </JoyConSupportSettings>
    <DS3SupportSettings>
      <Enabled>False</Enabled>
    </DS3SupportSettings>
  </DeviceOptions>
  <Net8Check>True</Net8Check>
  <CustomLed1>False:0,0,255</CustomLed1>
  <CustomLed2>False:0,0,255</CustomLed2>
  <CustomLed3>False:0,0,255</CustomLed3>
  <CustomLed4>False:0,0,255</CustomLed4>
  <CustomLed5>False:0,0,255</CustomLed5>
  <CustomLed6>False:0,0,255</CustomLed6>
  <CustomLed7>False:0,0,255</CustomLed7>
  <CustomLed8>False:0,0,255</CustomLed8>
</Profile>";
            #endregion
        }

        [TestMethod]
        public void CheckSettingsRead()
        {
            // Test settings reading. Will fail if an XML exception is thrown
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsDTO));
            using StringReader sr = new StringReader(appSettingsXml);
            BackingStore tempStore = new BackingStore();
            AppSettingsDTO dto = serializer.Deserialize(sr) as AppSettingsDTO;
            dto.MapTo(tempStore);

            // Check settings
            DateTime.TryParse(dto.LastCheckString, out DateTime tempLastChecked);
            Assert.AreEqual(tempLastChecked, tempStore.lastChecked);
        }
    }
}
