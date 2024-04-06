using System.Collections.Generic;

namespace PlayerDrafter.Data
{
    public class PlayerData
    {
        public PlayerData(string player, List<string> classes)
        {
            Player = player;
            Classes = classes;
        }

        public string Player { get; set; }
        public List<string> Classes { get; set; }
    }
}