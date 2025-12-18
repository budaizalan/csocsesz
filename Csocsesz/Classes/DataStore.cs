using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public static class DataStore
    {
        public const string apiPlayerUrl = "https://csocsesz-backend-g2bsedgra6b8aydz.swedencentral-01.azurewebsites.net/api/users";
        public const string apiMatchUrl = "https://csocsesz-backend-g2bsedgra6b8aydz.swedencentral-01.azurewebsites.net/api/matches";

        public static List<Player> Players = new List<Player>();
        public static List<MatchResults> Matches = new List<MatchResults>();

        public static Color red = Color.FromArgb("#FF0000");
        public static Color blue = Color.FromArgb("##2121E3");
        public static Color green = Color.FromArgb("#1FDB48");
        public static Color gray = Color.FromArgb("#7f7f7f");

        public static readonly ImageSource hugoImage = ImageSource.FromFile("hugo_icon.png");
        public static readonly ImageSource hugoSadImage = ImageSource.FromFile("hugosad_icon.png");
        public static readonly ImageSource zalanImage = ImageSource.FromFile("zalan_icon.png");
        public static readonly ImageSource zalanSadImage = ImageSource.FromFile("zalansad_icon.png");
    }
}
