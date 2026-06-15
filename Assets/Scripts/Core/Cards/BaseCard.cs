using Prsi.Core.Game;

namespace Prsi.Core.Cards
{
    /// <summary>
    /// Abstraktní základní třída pro všechny karty
    /// Ekvivalent JS base_card.js z prszi projektu
    /// 
    /// Implementuje polymorfismus - každý typ karty má vlastní chování:
    /// - CanPlayOn() - zda lze kartu zahrát na vrchní kartu
    /// - OnPlay() - co se stane po zahrání karty
    /// </summary>
    public abstract class BaseCard : ICardData
    {
        public Card.Suit Suit { get; }
        public Card.Rank Rank { get; }
        
        /// <summary>
        /// Zda je karta speciální (má zvláštní efekt)
        /// Override v potomcích pro SevenCard, AceCard, QueenCard
        /// </summary>
        public virtual bool IsSpecial => false;
        
        protected BaseCard(Card.Suit suit, Card.Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        
        /// <summary>
        /// Zkontroluje, zda lze tuto kartu zahrát na vrchní kartu
        /// 
        /// Základní pravidla Prší:
        /// - Stejná barva
        /// - Stejná hodnota
        /// - Speciální karty mohou mít vlastní pravidla (např. dáma na cokoliv)
        /// </summary>
        /// <param name="topCard">Vrchní karta na odhazovacím balíčku</param>
        /// <param name="context">Herní kontext (pro kontrolu vynucené barvy atd.)</param>
        /// <returns>True pokud lze kartu zahrát</returns>
        public abstract bool CanPlayOn(ICardData topCard, GameContext context);
        
        /// <summary>
        /// Provede efekt karty po zahrání
        /// 
        /// Polymorfní metoda - každý typ karty má vlastní implementaci:
        /// - RegularCard: žádný speciální efekt
        /// - SevenCard: další hráč musí líznout 2 karty
        /// - AceCard: přeskočí dalšího hráče
        /// - QueenCard: hráč vybírá novou barvu
        /// </summary>
        /// <param name="context">Herní kontext pro modifikaci stavu hry</param>
        /// <param name="player">Hráč, který kartu zahrál</param>
        public abstract void OnPlay(GameContext context, IPlayerData player);
        
        /// <summary>
        /// Základní kontrola hratelnosti - stejná barva nebo hodnota
        /// Pomocná metoda pro potomky
        /// </summary>
        protected bool MatchesSuitOrRank(ICardData topCard)
        {
            if (topCard == null) return true; // První karta
            return Suit == topCard.Suit || Rank == topCard.Rank;
        }
        
        /// <summary>
        /// Kontrola vynucené barvy (po zahrání dámy)
        /// </summary>
        protected bool MatchesForcedSuit(GameContext context)
        {
            if (context?.ForcedSuit == null) return true;
            return Suit == context.ForcedSuit.Value;
        }
        
        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
        
        public override bool Equals(object obj)
        {
            if (obj is BaseCard other)
            {
                return Suit == other.Suit && Rank == other.Rank;
            }
            if (obj is ICardData cardData)
            {
                return Suit == cardData.Suit && Rank == cardData.Rank;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return (int)Suit * 100 + (int)Rank;
        }
    }
}
