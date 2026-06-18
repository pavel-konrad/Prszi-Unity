using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Prsi.Data
{
    /// <summary>
    /// Avatar database for players
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarDB", menuName = "Prsi/Avatar Database", order = 1)]
    public class AvatarDB : ScriptableObject
    {
        [Header("Avatars")]
        [SerializeField] private List<AvatarData> avatars = new List<AvatarData>();
        
        /// <summary>
        /// Returns all avatars
        /// </summary>
        public IReadOnlyList<AvatarData> GetAllAvatars() => avatars;
        
        /// <summary>
        /// Returns avatar by ID
        /// </summary>
        public AvatarData GetAvatarById(string id)
        {
            return avatars.FirstOrDefault(a => a.id == id);
        }
        
        /// <summary>
        /// Returns avatar by index
        /// </summary>
        public AvatarData GetAvatarByIndex(int index)
        {
            if (index >= 0 && index < avatars.Count)
            {
                return avatars[index];
            }
            return null;
        }
        
        /// <summary>
        /// Returns avatar count
        /// </summary>
        public int Count => avatars.Count;
        
        /// <summary>
        /// Validate data in the editor
        /// </summary>
        private void OnValidate()
        {
            // Remove duplicate IDs
            var seenIds = new HashSet<string>();
            for (int i = avatars.Count - 1; i >= 0; i--)
            {
                if (avatars[i] == null)
                {
                    avatars.RemoveAt(i);
                    continue;
                }
                
                if (string.IsNullOrEmpty(avatars[i].id))
                {
                    avatars[i].id = $"avatar_{i}";
                }
                
                if (seenIds.Contains(avatars[i].id))
                {
                    avatars[i].id = $"{avatars[i].id}_{i}";
                }
                
                seenIds.Add(avatars[i].id);
            }
        }
    }
}

