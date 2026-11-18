using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public static class AppSettings
    {
        public static Player playerRed;
        public static Player playerBlue;
        // Second team members, used in 2v2 only.
        public static Player? playerRed2;
        public static Player? playerBlue2;
        // Settings survive restarts through Preferences.
        public static Mode mode
        {
            get => (Mode)Preferences.Get("mode", 0);
            set => Preferences.Set("mode", (int)value);
        }
        public static bool changingSide
        {
            get => Preferences.Get("changingSide", true);
            set => Preferences.Set("changingSide", value);
        }
        // At 9-10 and beyond a game is won only with a 2 goal lead.
        public static bool overtime
        {
            get => Preferences.Get("overtime", false);
            set => Preferences.Set("overtime", value);
        }
        public static int punishmentMultiplier
        {
            get => Preferences.Get("punishmentMultiplier", 3);
            set => Preferences.Set("punishmentMultiplier", value);
        }
        public static bool sendTestMatches
        {
            get => Preferences.Get("sendTestMatches", true);
            set => Preferences.Set("sendTestMatches", value);
        }
    }
}
