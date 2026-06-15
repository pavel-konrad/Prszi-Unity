using UnityEngine;
using Prsi.Data;
using Prsi.Core;

namespace Prsi.Core.Services
{
    /// <summary>
    /// Service pro poskytování spriteů karet podle tématu
    /// </summary>
    public class CardThemeService : MonoBehaviour, ICardThemeProvider
    {
        [Header("Theme Database")]
        [SerializeField] private CardThemeDB themeDB;
        
        private CardThemeData currentTheme;
        
        public CardThemeData CurrentTheme => currentTheme;
        
        private void Awake()
        {
            if (themeDB != null)
            {
                SetTheme(themeDB.GetDefaultTheme());
            }
        }
        
        public Sprite GetCardSprite(Card.Suit suit, Card.Rank rank)
        {
            if (currentTheme == null)
            {
                Debug.LogWarning("[CardThemeService] CurrentTheme je null, nelze získat sprite");
                return null;
            }
            
            return currentTheme.GetCardSprite(suit, rank);
        }
        
        public Sprite GetCardBackSprite()
        {
            if (currentTheme == null)
            {
                Debug.LogWarning("[CardThemeService] CurrentTheme je null, nelze získat card back sprite");
                return null;
            }
            
            return currentTheme.cardBackSprite;
        }
        
        public void SetTheme(CardThemeData theme)
        {
            if (theme == null)
            {
                Debug.LogWarning("[CardThemeService] Pokus o nastavení null tématu");
                return;
            }
            
            currentTheme = theme;
            Debug.Log($"[CardThemeService] Téma nastaveno: {theme.displayName}");
        }
        
        public void SetTheme(CardThemeDB themeDB)
        {
            if (themeDB == null)
            {
                Debug.LogWarning("[CardThemeService] Pokus o nastavení null themeDB");
                return;
            }
            
            this.themeDB = themeDB;
            SetTheme(themeDB.GetDefaultTheme());
        }
        
        /// <summary>
        /// Nastaví téma podle ID
        /// </summary>
        public void SetThemeById(string themeId)
        {
            if (themeDB == null)
            {
                Debug.LogWarning("[CardThemeService] ThemeDB je null");
                return;
            }
            
            var theme = themeDB.GetThemeById(themeId);
            if (theme != null)
            {
                SetTheme(theme);
            }
            else
            {
                Debug.LogWarning($"[CardThemeService] Téma s ID '{themeId}' nebylo nalezeno");
            }
        }
    }
}

