using System.Collections.Generic;

namespace PlayerDrafter.Data
{
    public class Team
    {
        public Team(string captain, decimal money, List<Player> players)
        {
            Captain = captain;
            Money = money;
            Players = players;
        }

        public string Captain { get; set; }
        public decimal Money { get; set; }
        public List<Player> Players { get; set; }
    }
}