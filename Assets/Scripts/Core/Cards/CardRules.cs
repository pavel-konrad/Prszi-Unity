using Prsi.Core;
using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Stateless rule helper: evaluates playability of flat card data through the
    /// polymorphic card engine. Callers that only hold <see cref="ICardData"/>
    /// (e.g. the legacy UI <c>Card</c>) get the real Prší rules — special cards
    /// included — without owning <see cref="BaseCard"/> instances themselves.
    /// </summary>
    public static class CardRules
    {
        /// <summary>
        /// True if <paramref name="card"/> may be played on <paramref name="top"/>
        /// under the current rule state in <paramref name="context"/>
        /// (forced suit, draw penalty). A null top means an empty discard pile.
        /// </summary>
        public static bool CanPlay(ICardData card, ICardData top, GameContext context)
        {
            if (card == null) return false;

            BaseCard domainCard = CardFactory.Create(card.Suit, card.Rank);
            BaseCard domainTop = top == null ? null : CardFactory.Create(top.Suit, top.Rank);
            return domainCard.CanPlayOn(domainTop, context);
        }
    }
}
