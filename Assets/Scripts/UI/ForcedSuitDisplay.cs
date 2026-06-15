using UnityEngine;
using UnityEngine.UI;
using Prsi.Core.Cards;
using Prsi.Core.Game;

/// <summary>
/// Shows the forced suit on the bar of the player who played the Queen.
/// Passive view driven by the shared GameContext: lights up the matching suit
/// symbol on suit change, hides everything when the forced suit is cleared or a
/// new hand is dealt. All indicators are hidden by default.
/// </summary>
public class ForcedSuitDisplay : MonoBehaviour
{
    [Tooltip("Suit indicator per player, indexed by player Id (0 = human, 1..3 = enemies).")]
    [SerializeField] private RawImage[] indicators;

    [SerializeField] private Texture heartsTexture;
    [SerializeField] private Texture diamondsTexture;
    [SerializeField] private Texture clubsTexture;
    [SerializeField] private Texture spadesTexture;

    private GameContext _ctx;

    void Start()
    {
        HideAll();

        _ctx = GameSession.I != null ? GameSession.I.Rules : null;
        if (_ctx == null) return;

        _ctx.OnSuitChanged += HandleSuitChanged;
        _ctx.OnForcedSuitCleared += HideAll;
    }

    void OnDestroy()
    {
        if (_ctx == null) return;
        _ctx.OnSuitChanged -= HandleSuitChanged;
        _ctx.OnForcedSuitCleared -= HideAll;
    }

    void HandleSuitChanged(Suit suit)
    {
        HideAll();

        int idx = GameSession.I.ActiveIndex; // the player who just played the Queen
        if (indicators == null || idx < 0 || idx >= indicators.Length) return;

        RawImage img = indicators[idx];
        if (img == null) return;

        img.texture = TextureFor(suit);
        img.gameObject.SetActive(true);
    }

    Texture TextureFor(Suit suit) => suit switch
    {
        Suit.Hearts => heartsTexture,
        Suit.Diamonds => diamondsTexture,
        Suit.Clubs => clubsTexture,
        Suit.Spades => spadesTexture,
        _ => null
    };

    void HideAll()
    {
        if (indicators == null) return;
        foreach (RawImage img in indicators)
            if (img != null) img.gameObject.SetActive(false);
    }
}
