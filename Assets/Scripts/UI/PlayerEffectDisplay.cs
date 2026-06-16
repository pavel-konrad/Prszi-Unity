using UnityEngine;
using TMPro;

/// <summary>
/// Short per-player effect callouts in a player's bar: "STŮJ!" when an Ace skips
/// them, "+N" when a Seven makes them draw. Passive view; states/CardManager call
/// the static Instance. Bars are matched to players via PlayerUI.Bound, like
/// ForcedSuitDisplay. Labels are hidden by default.
/// </summary>
public class PlayerEffectDisplay : MonoBehaviour
{
    public static PlayerEffectDisplay Instance { get; private set; }

    [System.Serializable]
    public struct Slot
    {
        public PlayerUI bar;     // identifies which player this label belongs to
        public TMP_Text label;   // the callout text in that bar
    }

    [SerializeField] private Slot[] slots;

    void Awake()
    {
        Instance = this;
        HideAll();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Red "!" — the player is skipped by an Ace (stands).</summary>
    public void ShowSkip(Player player) => Show(player, "!", new Color(1f, 0.25f, 0.25f));

    /// <summary>"+N" — the player must draw N cards from a Seven (auto-hides).</summary>
    public void ShowDraw(Player player, int count)
    {
        Show(player, $"+{count}", new Color(1f, 0.75f, 0.2f));
        StartCoroutine(HideAfter(player, 1.5f));
    }

    System.Collections.IEnumerator HideAfter(Player player, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide(player);
    }

    void Show(Player player, string text, Color color)
    {
        var label = LabelFor(player);
        if (label == null) return;
        label.text = text;
        label.color = color;
        label.gameObject.SetActive(true);
    }

    public void Hide(Player player)
    {
        var label = LabelFor(player);
        if (label != null) label.gameObject.SetActive(false);
    }

    TMP_Text LabelFor(Player player)
    {
        if (slots == null) return null;
        foreach (var s in slots)
            if (s.bar != null && s.bar.Bound == player) return s.label;
        return null;
    }

    void HideAll()
    {
        if (slots == null) return;
        foreach (var s in slots)
            if (s.label != null) s.label.gameObject.SetActive(false);
    }
}
