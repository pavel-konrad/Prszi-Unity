using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Sedmička - speciální karta s efektem líznutí
    /// 
    /// Pravidla Prší:
    /// - Další hráč musí líznout 2 karty
    /// - Lze se bránit další sedmičkou (efekt se kumuluje)
    /// - Pokud je aktivní penalizace, lze zahrát pouze další sedmu
    /// </summary>
    public class SevenCard : BaseCard
    {
        /// <summary>
        /// Kolik karet musí další hráč líznout
        /// </summary>
        public const int DRAW_PENALTY = 2;
        
        public override bool IsSpecial => true;
        
        public SevenCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Sedmu lze zahrát pokud:
        /// 1. Je aktivní penalizace (obrana sedmou) - pouze sedma se může bránit
        /// 2. Nebo standardní pravidlo: stejná barva/hodnota bez vynucené barvy
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Pokud je aktivní penalizace, lze zahrát pouze sedmu (obrana)
            if (context?.PendingDrawCount > 0)
            {
                // Obrana sedmou - lze zahrát na sedmu
                return topCard?.Rank == Rank.Seven;
            }
            
            // Pokud je vynucená barva (po dámě), musí souhlasit barva
            if (context?.ForcedSuit != null)
            {
                return Suit == context.ForcedSuit.Value;
            }
            
            // Standardní pravidlo: stejná barva nebo hodnota
            return MatchesSuitOrRank(topCard);
        }
        
        /// <summary>
        /// Efekt sedmy: přidá penalizaci líznutí pro dalšího hráče
        /// Penalizace se kumuluje (2 + 2 + 2...)
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Zrušit vynucenou barvu
            context?.ClearForcedSuit();
            
            // Přidat penalizaci líznutí
            context?.NotifyDrawPenalty(DRAW_PENALTY);
            
            // Notifikovat o zahrání karty
            context?.NotifyCardPlayed(this, player);
        }
    }
}
