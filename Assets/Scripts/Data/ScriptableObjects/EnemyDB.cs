using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Prsi.Data
{
    /// <summary>
    /// Enemy database (AI players)
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDB", menuName = "Prsi/Enemy Database", order = 2)]
    public class EnemyDB : ScriptableObject
    {
        [Header("Enemies")]
        [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();
        
        /// <summary>
        /// Returns all enemies
        /// </summary>
        public IReadOnlyList<EnemyData> GetAllEnemies() => enemies;
        
        /// <summary>
        /// Returns enemy by ID
        /// </summary>
        public EnemyData GetEnemyById(string id)
        {
            return enemies.FirstOrDefault(e => e.id == id);
        }
        
        /// <summary>
        /// Returns enemy by index
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
        /// Returns a random enemy
        /// </summary>
        public EnemyData GetRandomEnemy()
        {
            if (enemies.Count == 0) return null;
            return enemies[Random.Range(0, enemies.Count)];
        }
        
        /// <summary>
        /// Returns random enemies without duplicates
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
                // No duplicates
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
        /// Returns enemy count
        /// </summary>
        public int Count => enemies.Count;
        
        /// <summary>
        /// Validate data in the editor
        /// </summary>
        private void OnValidate()
        {
            // Remove duplicate IDs
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

