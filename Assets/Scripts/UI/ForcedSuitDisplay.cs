using UnityEngine;
using UnityEngine.UI;
using Prsi.Core.Cards;
using Prsi.Core.Game;

/// <summary>
/// Shows the forced suit on the bar of the player who played the
/// Queen. The whole indicator (frame + symbol) is hidden by default and shown only
/// while a forced suit is active. Passive view driven by the shared GameContext.
///
/// The bar is matched to the player via PlayerUI.Bound (not array order), so it
/// lands on the right enemy regardless of how the bars are ordered in the scene.
/// </summary>
public class ForcedSuitDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct Indicator
    {
        public PlayerUI bar;          // identifies which player this bar shows
        public GameObject root;       // EnemyGameStat (frame + symbol) toggled on/off
        public RawImage suitSymbol;   // CardSuit image set to the forced suit
    }

    [SerializeField] private Indicator[] indicators;

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

        Player active = ActivePlayer();
        if (active == null) return;

        // Show the indicator on the bar bound to the player who played the Queen.
        foreach (var ind in indicators)
        {
            if (ind.bar == null || ind.bar.Bound != active) continue;
            if (ind.suitSymbol != null) ind.suitSymbol.texture = TextureFor(suit);
            if (ind.root != null) ind.root.SetActive(true);
            break;
        }
    }

    Player ActivePlayer()
    {
        var gs = GameSession.I;
        if (gs == null) return null;
        int i = gs.ActiveIndex;
        return i >= 0 && i < gs.Players.Count ? gs.Players[i] : null;
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
        foreach (var ind in indicators)
            if (ind.root != null) ind.root.SetActive(false);
    }
}
