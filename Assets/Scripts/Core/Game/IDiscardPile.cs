using System.Collections.Generic;

namespace Prsi.Core.Game
{
    /// <summary>
    /// Interface pro odhazovací balíček
    /// </summary>
    public interface IDiscardPile
    {
        /// <summary>
        /// Vrchní karta na odhazovacím balíčku
        /// </summary>
        ICardData TopCard { get; }
        
        /// <summary>
        /// Počet karet v odhazovacím balíčku
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Všechny karty v odhazovacím balíčku
        /// </summary>
        IReadOnlyList<ICardData> Cards { get; }
        
        /// <summary>
        /// Přidá kartu na odhazovací balíček
        /// </summary>
        void Discard(ICardData card);
        
        /// <summary>
        /// Vyčistí odhazovací balíček
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Odebere všechny karty kromě vrchní (pro přehození do balíčku)
        /// </summary>
        IReadOnlyList<ICardData> TakeAllExceptTop();
    }
}
