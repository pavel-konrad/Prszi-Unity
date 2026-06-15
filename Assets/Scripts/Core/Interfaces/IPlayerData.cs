using System.Collections.Generic;
using UnityEngine;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro data hráče
    /// </summary>
    public interface IPlayerData
    {
        int Id { get; }
        string Name { get; }
        bool IsHuman { get; }
        Sprite Avatar { get; }
        int Cash { get; }
        int Bet { get; }
        IReadOnlyList<ICardData> Hand { get; }
        bool HasWon { get; }
        bool CanPlay { get; }
        bool IsInGame { get; }
    }
}

