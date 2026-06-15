using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Prsi.Data
{
    /// <summary>
    /// Databáze nepřátel (AI hráčů)
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDB", menuName = "Prsi/Enemy Database", order = 2)]
    public class EnemyDB : ScriptableObject
    {
        [Header("Enemies")]
        [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();
        
        /// <summary>
        /// Vrátí všechny nepřátele
        /// </summary>
        public IReadOnlyList<EnemyData> GetAllEnemies() => enemies;
        
        /// <summary>
        /// Vrátí nepřítele podle ID
        /// </summary>
        public EnemyData GetEnemyById(string id)
        {
            return enemies.FirstOrDefault(e => e.id == id);
        }
        
        /// <summary>
        /// Vrátí nepřítele podle indexu
        /// </summary>
        public EnemyData GetEnemyByIndex(int index)
        {
            if (index >= 0 && index < enemies.Count)
            {
                return enemies[index];
            }
            return null;
        }
        
        /// <summary>
        /// Vrátí náhodného nepřítele
        /// </summary>
        public EnemyData GetRandomEnemy()
        {
            if (enemies.Count == 0) return null;
            return enemies[Random.Range(0, enemies.Count)];
        }
        
        /// <summary>
        /// Vrátí náhodné nepřátele bez duplicit
        /// </summary>
        public List<EnemyData> GetRandomEnemies(int count, bool allowDuplicates = false)
        {
            if (enemies.Count == 0) return new List<EnemyData>();
            
            if (allowDuplicates)
            {
                var result = new List<EnemyData>();
                for (int i = 0; i < count; i++)
                {
                    result.Add(GetRandomEnemy());
                }
                return result;
            }
            else
            {
                // Bez duplicit
                var available = new List<EnemyData>(enemies);
                var result = new List<EnemyData>();
                
                int takeCount = Mathf.Min(count, available.Count);
                for (int i = 0; i < takeCount; i++)
                {
                    int randomIndex = Random.Range(0, available.Count);
                    result.Add(available[randomIndex]);
                    available.RemoveAt(randomIndex);
                }
                
                return result;
            }
        }
        
        /// <summary>
        /// Vrátí počet nepřátel
        /// </summary>
        public int Count => enemies.Count;
        
        /// <summary>
        /// Validace dat v editoru
        /// </summary>
        private void OnValidate()
        {
            // Odstranit duplicitní ID
            var seenIds = new HashSet<string>();
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] == null)
                {
                    enemies.RemoveAt(i);
                    continue;
                }
                
                if (string.IsNullOrEmpty(enemies[i].id))
                {
                    enemies[i].id = $"enemy_{i}";
                }
                
                if (seenIds.Contains(enemies[i].id))
                {
                    enemies[i].id = $"{enemies[i].id}_{i}";
                }
                
                seenIds.Add(enemies[i].id);
            }
        }
    }
}

