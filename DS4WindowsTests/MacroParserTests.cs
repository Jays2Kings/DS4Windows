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