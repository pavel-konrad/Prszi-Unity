using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Prsi.Data
{
    /// <summary>
    /// Card theme database
    /// </summary>
    [CreateAssetMenu(fileName = "CardThemeDB", menuName = "Prsi/Card Theme Database", order = 3)]
    public class CardThemeDB : ScriptableObject
    {
        [Header("Themes")]
        [SerializeField] private List<CardThemeData> themes = new List<CardThemeData>();
        
        [Header("Default Theme")]
        [SerializeField] private string defaultThemeId;
        
        /// <summary>
        /// Returns all themes
        /// </summary>
        public IReadOnlyList<CardThemeData> GetAllThemes() => themes;
        
        /// <summary>
        /// Returns theme by ID
        /// </summary>
        public CardThemeData GetThemeById(string id)
        {
            return themes.FirstOrDefault(t => t.id == id);
        }
        
        /// <summary>
        /// Returns theme by index
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
        /// Returns the default theme
        /// </summary>
        public CardThemeData GetDefaultTheme()
        {
            if (!string.IsNullOrEmpty(defaultThemeId))
            {
                var theme = GetThemeById(defaultThemeId);
                if (theme != null) return theme;
            }
            
            // If no default theme is set, return the first one
            if (themes.Count > 0)
            {
                return themes[0];
            }
            
            return null;
        }
        
        /// <summary>
        /// Sets the default theme
        /// </summary>
        public void SetDefaultTheme(string themeId)
        {
            if (GetThemeById(themeId) != null)
            {
                defaultThemeId = themeId;
            }
        }
        
        /// <summary>
        /// Returns theme count
        /// </summary>
        public int Count => themes.Count;
        
        /// <summary>
        /// Validate data in the editor
        /// </summary>
        private void OnValidate()
        {
            // Remove duplicate IDs
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
            
            // Validate default theme
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

