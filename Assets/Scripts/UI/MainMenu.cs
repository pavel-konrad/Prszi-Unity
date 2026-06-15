using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField nameInput;
    public AvatarPicker avatarPicker;
    public System.Action OnStart;

public void OnStartClicked()
{
    string n = string.IsNullOrWhiteSpace(nameInput ? nameInput.text : null)
        ? "Player" : nameInput.text.Trim();

    var sprite = avatarPicker ? avatarPicker.SelectedSprite : null;
    int idx    = avatarPicker ? avatarPicker.SelectedIndex : 0;

    Debug.Log($"[MainMenu] Start: name='{n}', sprite={(sprite? sprite.name : "NULL")}, idx={idx}");

    if (GameSession.I != null)
        GameSession.I.ApplyHumanFromMenu(n, sprite, idx);

    OnStart?.Invoke();
}

}
