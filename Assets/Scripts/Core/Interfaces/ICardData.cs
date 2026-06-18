using UnityEngine;
using Prsi.Core.Cards;

namespace Prsi.Core
{
    /// <summary>
    /// Interface for card data (no Unity dependencies)
    /// </summary>
    public interface ICardData
    {
        Suit Suit { get; }
        Rank Rank { get; }
    }
}

