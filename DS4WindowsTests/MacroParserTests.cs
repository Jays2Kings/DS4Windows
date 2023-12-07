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
    public class MacroParserTests
    {
        private int[] testMacro = new int[]
        {
            87, // W key down
            330, // Wait period 30 ms
            1090220220, // Change lightbar (90, 220, 220)
            83, // S key down
            330, // Wait period 30 ms
            1000000000, // Reset lightbar
            83, // S key up
            330, // Wait period 30 ms
            87, // W key up
        };

        private MacroParser parser;

        public MacroParserTests()
        {
            Setup();
        }

        private void Setup()
        {
            parser = new MacroParser(testMacro);
            parser.LoadMacro();
        }

        [TestMethod]
        public void CheckNumberSteps()
        {
            List<MacroStep> steps = parser.MacroSteps;
            // Make sure parser interpreted all steps
            Assert.AreEqual(testMacro.Length, steps.Count);
        }

        [TestMethod]
        public void CheckStepTypes()
        {
            List<MacroStep> steps = parser.MacroSteps;

            int waitStep = 1;
            Assert.AreEqual(MacroStep.StepType.Wait, steps[waitStep].ActType);

            int changeLightbarStep = 2;
            Assert.AreEqual(MacroStep.StepOutput.Lightbar, steps[changeLightbarStep].OutputType);
            Assert.AreEqual(MacroStep.StepType.ActDown, steps[changeLightbarStep].ActType);

            int resetLightbarStep = 5;
            Assert.AreEqual(MacroStep.StepOutput.Lightbar, steps[resetLightbarStep].OutputType);
            Assert.AreEqual(MacroStep.StepType.ActUp, steps[resetLightbarStep].ActType);

            int lastStep = testMacro.Length - 1;
            Assert.AreEqual(MacroStep.StepType.ActUp, steps[lastStep].ActType);

            return;
        }
    }
}