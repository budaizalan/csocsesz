using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public static class DataStore
    {

        public static List<Player> Players = new List<Player>();
        public static List<Match> Matches = new List<Match>();

        public static string defaultPlayerRedId = "hugo0001";
        public static string defaultPlayerBlueId = "zazzzzuska0001";


        public static Color red = Color.FromArgb("#FF0000");
        public static Color blue = Color.FromArgb("#2121E3");
        public static Color green = Color.FromArgb("#1FDB48");
        public static Color gray = Color.FromArgb("#7f7f7f");

        public static int pageIdx = 0;

        public static Match selectedMatch;
    }
}
