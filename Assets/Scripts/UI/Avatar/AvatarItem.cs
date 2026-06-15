using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarItem : MonoBehaviour
{
    [SerializeField] Image thumbnail;
    public Sprite GetSprite() => thumbnail ? thumbnail.sprite : null;
#if UNITY_EDITOR
    void OnValidate(){ if (!thumbnail) thumbnail = GetComponentInChildren<Image>(true); }
#endif
}
