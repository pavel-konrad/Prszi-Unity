using UnityEngine;
using Prsi.Core.Cards;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro data karty (bez Unity závislostí)
    /// </summary>
    public interface ICardData
    {
        Suit Suit { get; }
        Rank Rank { get; }
    }
}

