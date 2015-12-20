using System;

namespace WrtSettings {
    internal static class Settings {

        public static bool ShowAsuswrt2Random {
            get { return Medo.Configuration.Settings.Read("ShowAsuswrt2Random", false); }
        }

    }
}
