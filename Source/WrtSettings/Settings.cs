using System;

namespace WrtSettings {
    internal static class Settings {

        /// <summary>
        /// Controls whether random number specific for AsusWRT version 2 file format is shown as .Random upon load.
        /// </summary>
        public static bool ShowAsuswrt2Random {
            get { return Medo.Configuration.Settings.Read("ShowAsuswrt2Random", false); }
        }

        /// <summary>
        /// Adds additional scale factor for toolbar images in additional to desktop scaling factor.
        /// </summary>
        public static double ScaleBoost {
            get { return Medo.Configuration.Settings.Read("ScaleBoost", 0.00); }
        }

    }
}
