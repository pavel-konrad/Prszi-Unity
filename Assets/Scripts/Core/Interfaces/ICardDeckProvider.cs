using Prsi.Core.Cards;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro správu balíčku karet
    /// </summary>
    public interface ICardDeckProvider
    {
        /// <summary>
        /// Lízne kartu z balíčku (jako ICardData pro kompatibilitu)
        /// </summary>
        ICardData DrawCard();
        
        /// <summary>
        /// Lízne kartu jako BaseCard (pro polymorfismus)
        /// </summary>
        BaseCard DrawBaseCard();
        
        /// <summary>
        /// Zamíchá balíček
        /// </summary>
        void Shuffle();
        
        /// <summary>
        /// Počet zbývajících karet
        /// </summary>
        int RemainingCards { get; }
    }
}

