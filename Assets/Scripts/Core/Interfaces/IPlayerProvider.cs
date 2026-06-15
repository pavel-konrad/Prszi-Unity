using System.Collections.Generic;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro poskytování hráčů
    /// </summary>
    public interface IPlayerProvider
    {
        IPlayerData GetPlayer(int id);
        IPlayerData GetHumanPlayer();
        IReadOnlyList<IPlayerData> GetAllPlayers();
    }
}

