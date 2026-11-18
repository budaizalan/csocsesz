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
        public static Mode mode = Mode.OneVsOne;

        public static bool changingSide = true;

        // At 9-10 and beyond a game is won only with a 2 goal lead.
        public static bool overtime = false;

        public static int punishmentMultiplier = 3;

        public static bool sendTestMatches = true;
    }
}
