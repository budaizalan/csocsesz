using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public static class AppSettings
    {
        public static Player defaultPlayer1 = new Player("694077cfe93c946a4ce8fdaf", "Hugo", 0, 0, 0, 0, 0, 0, Side.red);
        public static Player defaultPlayer2 = new Player("694077dbe93c946a4ce8fdb1", "Zalan", 0, 0, 0, 0, 0, 0, Side.blue);
        public static Player player1;
        public static Player player2;

        public static bool changingSide = true;

        public static int pushUpsMultiplier = 3;
    }
}
