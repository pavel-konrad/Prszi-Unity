using System.Collections.Generic;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro správu stavů karet
    /// </summary>
    public interface ICardStateProvider
    {
        ICardState GetCardState(ICardData card);
        void SetCardLocation(ICardData card, CardLocation location, int? ownerId = null);
        IEnumerable<ICardState> GetCardsInLocation(CardLocation location);
        IEnumerable<ICardState> GetPlayerCards(int playerId);
    }
}

