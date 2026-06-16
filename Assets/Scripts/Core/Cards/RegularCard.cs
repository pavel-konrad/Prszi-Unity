using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Běžná karta bez speciálního efektu
    /// Hodnoty: 8, 9, 10, J (kluk), K (král)
    /// 
    /// Pravidla:
    /// - Lze zahrát na kartu stejné barvy nebo hodnoty
    /// - Respektuje vynucenou barvu po dámě
    /// - Nemá žádný speciální efekt
    /// </summary>
    public class RegularCard : BaseCard
    {
        public override bool IsSpecial => false;
        
        public RegularCard(Suit suit, Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Běžná karta lze zahrát pokud:
        /// 1. Souhlasí barva nebo hodnota s vrchní kartou
        /// 2. Není aktivní vynucená barva, nebo souhlasí s vynucenou barvou
        /// 3. Pokud je aktivní penalizace (sedmy), nelze zahrát běžnou kartu
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Na čekající eso lze reagovat jen esem.
            if (context?.AcePending == true)
            {
                return false;
            }

            // Pokud je aktivní penalizace líznutí, nelze zahrát běžnou kartu
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
        /// Běžná karta nemá žádný speciální efekt
        /// Pouze zruší vynucenou barvu (pokud byla aktivní)
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Zrušit vynucenou barvu po dámě
            context?.ClearForcedSuit();
            
            // Notifikovat o zahrání karty
            context?.NotifyCardPlayed(this, player);
        }
    }
}
