using UnityEngine;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro data karty (bez Unity závislostí)
    /// </summary>
    public interface ICardData
    {
        Card.Suit Suit { get; }
        Card.Rank Rank { get; }
    }
}

