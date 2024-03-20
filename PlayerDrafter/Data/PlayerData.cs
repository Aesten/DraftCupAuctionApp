using System.Collections.Generic;

namespace PlayerDrafter.Data
{
    public class PlayerData
    {
        public PlayerData(string player, List<string> classes, string comment)
        {
            Player = player;
            Classes = classes;
            Comment = comment;
        }

        public string Player { get; set; }
        public List<string> Classes { get; set; }
        public string Comment { get; set; }
    }
}