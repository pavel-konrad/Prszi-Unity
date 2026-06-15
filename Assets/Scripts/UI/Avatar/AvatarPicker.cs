using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarPicker : MonoBehaviour
{
    public Image previewImage;
    public Animator previewAnimator;

    readonly List<AvatarItem> items = new();
    int selectedIndex = -1;
    public int SelectedIndex => selectedIndex;
    public Sprite SelectedSprite => (selectedIndex>=0 && selectedIndex<items.Count) ? items[selectedIndex].GetSprite() : null;

    void Awake(){
        items.Clear();
        items.AddRange(GetComponentsInChildren<AvatarItem>(true));

        for (int i=0;i<items.Count;i++){
            var btn = items[i].GetComponent<Button>();
            if (!btn){ Debug.LogError($"Missing Button on {items[i].name}"); continue; }
            int idx = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(()=> SelectIndex(idx));
        }
        int saved = PlayerPrefs.GetInt("p_avatar_idx", 0);
        SelectIndex(Mathf.Clamp(saved, 0, Mathf.Max(items.Count-1,0)));
    }

    public void SelectIndex(int idx){
	if (idx<0 || idx>=items.Count) return;
	selectedIndex = idx;

	var sprite = items[idx].GetSprite();
	if (previewImage && sprite){ previewImage.enabled=true; previewImage.sprite = sprite; }
	if (previewAnimator) 
	{
		previewAnimator.SetTrigger("Pop");
		// Zvuk se spustí přes Animation Event
	}

	var gs = GameSession.I;
	if (gs != null && gs.Players.Count > 0) {
		gs.Players[0].SetAvatar(sprite);  // uloží sprite do hráče
	}

	PlayerPrefs.SetInt("p_avatar_idx", selectedIndex);
	PlayerPrefs.Save();
}

}
