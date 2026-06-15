using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Eso - speciální karta s efektem přeskočení
    /// 
    /// Pravidla Prší:
    /// - Další hráč je přeskočen (stojí)
    /// - Lze zahrát na kartu stejné barvy nebo hodnoty
    /// </summary>
    public class AceCard : BaseCard
    {
        public override bool IsSpecial => true;
        
        public AceCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Eso lze zahrát pokud:
        /// 1. Není aktivní penalizace (sedmy) - eso se nemůže bránit
        /// 2. Standardní pravidlo: stejná barva/hodnota
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Pokud je aktivní penalizace líznutí, nelze zahrát eso
            if (context?.PendingDrawCount > 0)
            {
                return false;
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
        /// Efekt esa: přeskočí dalšího hráče
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Zrušit vynucenou barvu
            context?.ClearForcedSuit();
            
            // Nastavit přeskočení dalšího hráče
            if (context != null)
            {
                context.SkipNextPlayer = true;
            }
            
            // Notifikovat o zahrání karty
            context?.NotifyCardPlayed(this, player);
        }
    }
}
