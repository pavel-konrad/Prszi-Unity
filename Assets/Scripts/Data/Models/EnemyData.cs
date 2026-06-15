using UnityEngine;

namespace Prsi.Data
{
    /// <summary>
    /// Data pro jednoho nepřítele (AI hráče)
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        [Header("Enemy Info")]
        public string id;
        public string displayName;
        public Sprite avatarSprite;
        
        [Header("Economy")]
        public int startingCash = 1000;
        public int defaultBet = 25;
        
        [Header("Visual")]
        public Color tintColor = Color.white;
        
        public EnemyData(string id, string displayName, Sprite avatarSprite)
        {
            this.id = id;
            this.displayName = displayName;
            this.avatarSprite = avatarSprite;
        }
    }
}

