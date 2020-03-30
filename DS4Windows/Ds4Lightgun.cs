using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows {

    /**
     * Loaded profile must be called 'Light Gun.xml' for this to work. 
     * Gyro Must be set to Mouse Mode for this to work.
     * Mouse Deadzone should be set to zero.
     * 
     */
    public static class DS4Lightgun {

        public static bool SET_R2_AS_LIGHTGUN = false;
        public static readonly bool RECENTER_LIGHTGUN_WITH_CROSS_BTN = true;

        public static int Clamp(int val) {
            if (val < 0) return 0;
            else if (val > 255) return 255;
            else return val;
        }

        public static void SetLightgunProfileLoaded(bool v) {
            SET_R2_AS_LIGHTGUN = v;
        }
    }
}
