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


using DS4Windows;

namespace DS4WindowsTests
{
    [TestClass]
    public class ProfileMigrationTests
    {
        private string ds4winJays2KingsOldProfile = string.Empty;

        public ProfileMigrationTests()
        {
            #region TempDS4WinProfileXML
            ds4winJays2KingsOldProfile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!-- DS4Windows Configuration Data. 12/6/2023 4:31:21 PM -->

<DS4Windows>
  <flushHIDQueue>True</flushHIDQueue>
  <idleDisconnectTimeout>0</idleDisconnectTimeout>
  <Color>0,0,255</Color>
  <RumbleBoost>100</RumbleBoost>
  <ledAsBatteryIndicator>False</ledAsBatteryIndicator>
  <FlashType>0</FlashType>
  <flashBatteryAt>0</flashBatteryAt>
  <touchSensitivity>100</touchSensitivity>
  <LowColor>0,0,0</LowColor>
  <ChargingColor>0,0,0</ChargingColor>
  <FlashColor>0,0,0</FlashColor>
  <touchpadJitterCompensation>True</touchpadJitterCompensation>
  <lowerRCOn>False</lowerRCOn>
  <tapSensitivity>0</tapSensitivity>
  <doubleTap>False</doubleTap>
  <scrollSensitivity>0</scrollSensitivity>
  <LeftTriggerMiddle>0</LeftTriggerMiddle>
  <RightTriggerMiddle>0</RightTriggerMiddle>
  <ButtonMouseSensitivity>25</ButtonMouseSensitivity>
  <Rainbow>0</Rainbow>
  <LSDeadZone>0</LSDeadZone>
  <RSDeadZone>0</RSDeadZone>
  <SXDeadZone>0.25</SXDeadZone>
  <SZDeadZone>0.25</SZDeadZone>
  <Sensitivity>1|1|1|1|1|1</Sensitivity>
  <ChargingType>0</ChargingType>
  <MouseAcceleration>True</MouseAcceleration>
  <LaunchProgram>
  </LaunchProgram>
  <DinputOnly>False</DinputOnly>
  <StartTouchpadOff>False</StartTouchpadOff>
  <UseTPforControls>False</UseTPforControls>
  <UseSAforMouse>False</UseSAforMouse>
  <SATriggers>
  </SATriggers>
  <GyroSensitivity>100</GyroSensitivity>
  <GyroInvert>0</GyroInvert>
  <LSCurve>0</LSCurve>
  <RSCurve>0</RSCurve>
  <ProfileActions>Disconnect Controller</ProfileActions>
  <Control />
  <ShiftControl />
</DS4Windows>";
            #endregion
        }

        private void Setup()
        {

        }

        [TestMethod]
        public void CheckMigration()
        {
            ProfileMigration migration = new ProfileMigration(ds4winJays2KingsOldProfile);
            bool requiredMigration = migration.RequiresMigration();

            Assert.AreEqual(true, requiredMigration);

            migration.Migrate();
            string profileXml = migration.CurrentMigrationText;

            Assert.AreEqual(true, !string.IsNullOrEmpty(profileXml));
        }
    }
}
