using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Prsi.Data
{
    /// <summary>
    /// Databáze témat karet
    /// </summary>
    [CreateAssetMenu(fileName = "CardThemeDB", menuName = "Prsi/Card Theme Database", order = 3)]
    public class CardThemeDB : ScriptableObject
    {
        [Header("Themes")]
        [SerializeField] private List<CardThemeData> themes = new List<CardThemeData>();
        
        [Header("Default Theme")]
        [SerializeField] private string defaultThemeId;
        
        /// <summary>
        /// Vrátí všechna témata
        /// </summary>
        public IReadOnlyList<CardThemeData> GetAllThemes() => themes;
        
        /// <summary>
        /// Vrátí téma podle ID
        /// </summary>
        public CardThemeData GetThemeById(string id)
        {
            return themes.FirstOrDefault(t => t.id == id);
        }
        
        /// <summary>
        /// Vrátí téma podle indexu
        /// </summary>
        public CardThemeData GetThemeByIndex(int index)
        {
            if (index >= 0 && index < themes.Count)
            {
                return themes[index];
            }
            return null;
        }
        
        /// <summary>
        /// Vrátí výchozí téma
        /// </summary>
        public CardThemeData GetDefaultTheme()
        {
            if (!string.IsNullOrEmpty(defaultThemeId))
            {
                var theme = GetThemeById(defaultThemeId);
                if (theme != null) return theme;
            }
            
            // Pokud není nastaveno výchozí téma, vrátit první
            if (themes.Count > 0)
            {
                return themes[0];
            }
            
            return null;
        }
        
        /// <summary>
        /// Nastaví výchozí téma
        /// </summary>
        public void SetDefaultTheme(string themeId)
        {
            if (GetThemeById(themeId) != null)
            {
                defaultThemeId = themeId;
            }
        }
        
        /// <summary>
        /// Vrátí počet témat
        /// </summary>
        public int Count => themes.Count;
        
        /// <summary>
        /// Validace dat v editoru
        /// </summary>
        private void OnValidate()
        {
            // Odstranit duplicitní ID
            var seenIds = new HashSet<string>();
            for (int i = themes.Count - 1; i >= 0; i--)
            {
                if (themes[i] == null)
                {
                    themes.RemoveAt(i);
                    continue;
                }
                
                if (string.IsNullOrEmpty(themes[i].id))
                {
                    themes[i].id = $"theme_{i}";
                }
                
                if (seenIds.Contains(themes[i].id))
                {
                    themes[i].id = $"{themes[i].id}_{i}";
                }
                
                seenIds.Add(themes[i].id);
            }
            
            // Validace výchozího tématu
            if (!string.IsNullOrEmpty(defaultThemeId))
            {
                if (GetThemeById(defaultThemeId) == null)
                {
                    defaultThemeId = string.Empty;
                }
            }
        }
    }
}

