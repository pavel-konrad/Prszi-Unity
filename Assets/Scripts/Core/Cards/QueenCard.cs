using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Dáma (svršek) - speciální karta se změnou barvy
    /// 
    /// Pravidla Prší:
    /// - Lze zahrát na COKOLIV (nezáleží na barvě ani hodnotě)
    /// - Hráč si vybírá novou barvu, kterou musí další hráč hrát
    /// </summary>
    public class QueenCard : BaseCard
    {
        public override bool IsSpecial => true;
        
        /// <summary>
        /// Vybraná barva po zahrání dámy
        /// Nastavuje se před voláním OnPlay()
        /// </summary>
        public Card.Suit? SelectedSuit { get; set; }
        
        public QueenCard(Card.Suit suit, Card.Rank rank) : base(suit, rank)
        {
        }
        
        /// <summary>
        /// Dámu lze zahrát VŽDY (na cokoliv)
        /// Výjimka: nelze zahrát při aktivní penalizaci (sedmy)
        /// </summary>
        public override bool CanPlayOn(ICardData topCard, GameContext context)
        {
            // Pokud je aktivní penalizace líznutí, nelze zahrát dámu
            if (context?.PendingDrawCount > 0)
            {
                return false;
            }
            
            // Dámu lze zahrát na cokoliv!
            return true;
        }
        
        /// <summary>
        /// Efekt dámy: nastaví vynucenou barvu
        /// 
        /// POZNÁMKA: SelectedSuit musí být nastavena před voláním této metody
        /// (typicky přes UI dialog pro výběr barvy)
        /// 
        /// Pokud SelectedSuit není nastavena, použije se barva dámy
        /// </summary>
        public override void OnPlay(GameContext context, IPlayerData player)
        {
            // Určit novou barvu - buď vybranou, nebo barvu dámy
            Card.Suit newSuit = SelectedSuit ?? Suit;
            
            // Nastavit vynucenou barvu
            context?.NotifySuitChanged(newSuit);
            
            // Notifikovat o zahrání karty
            context?.NotifyCardPlayed(this, player);
            
            // Reset vybrané barvy pro příští použití
            SelectedSuit = null;
        }
        
        /// <summary>
        /// Nastaví vybranou barvu před zahráním
        /// Volá se z UI před OnPlay()
        /// </summary>
        public void SelectSuit(Card.Suit suit)
        {
            SelectedSuit = suit;
        }
    }
}
