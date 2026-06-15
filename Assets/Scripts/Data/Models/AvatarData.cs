using UnityEngine;

namespace Prsi.Data
{
    /// <summary>
    /// Data pro jeden avatar
    /// </summary>
    [System.Serializable]
    public class AvatarData
    {
        [Header("Avatar Info")]
        public string id;
        public string displayName;
        public Sprite sprite;
        
        [Header("Visual")]
        public Color tintColor = Color.white;
        
        public AvatarData(string id, string displayName, Sprite sprite)
        {
            this.id = id;
            this.displayName = displayName;
            this.sprite = sprite;
        }
    }
}

