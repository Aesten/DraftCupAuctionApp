using System.Collections.Generic;
using System.Linq;

namespace PlayerDrafter.Data
{
    public static class DataUtil
    {
        public static bool IsValid<T>(IEnumerable<T> list) {
            if (typeof(T) == typeof(PlayerData))
            {
                return list.Select(item => (PlayerData)(object)item).All(playerData => playerData.Player != null && playerData.Classes != null);
            }

            if (typeof(T) != typeof(Team)) return false;
            
            foreach (var team in list.Select(item => (Team)(object)item))
            {
                if (team.Captain == null) return false;
                if (team.Players == null) return false;
                if (team.Players.Any(player => player.Name == null)) return false;
            }
            return true;
        }
    }
}