using UnityEngine;
using Prsi.Data;

namespace Prsi.Core
{
    /// <summary>
    /// Interface pro poskytování spriteů karet podle tématu
    /// </summary>
    public interface ICardThemeProvider
    {
        /// <summary>
        /// Aktuální téma karet (CardThemeData, ne CardThemeDB)
        /// </summary>
        CardThemeData CurrentTheme { get; }
        
        /// <summary>
        /// Získá sprite pro kartu podle barvy a hodnoty
        /// </summary>
        Sprite GetCardSprite(Card.Suit suit, Card.Rank rank);
        
        /// <summary>
        /// Získá sprite pro rubovou stranu karty
        /// </summary>
        Sprite GetCardBackSprite();
        
        /// <summary>
        /// Nastaví téma z CardThemeDB (použije výchozí téma)
        /// </summary>
        void SetTheme(CardThemeDB themeDB);
        
        /// <summary>
        /// Nastaví konkrétní téma
        /// </summary>
        void SetTheme(CardThemeData theme);
    }
}
