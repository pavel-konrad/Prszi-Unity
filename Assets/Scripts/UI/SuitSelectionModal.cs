using System;
using UnityEngine;
using UnityEngine.UI;
using Prsi.Core.Cards;

/// <summary>
/// Simple modal that lets the human pick a suit after playing a Queen.
/// Passive view: holds the four suit buttons and an overlay panel; raises the
/// supplied callback with the chosen suit. Wired in code so no inspector
/// onClick hookup is needed — only the button/panel references are assigned.
/// </summary>
public class SuitSelectionModal : MonoBehaviour
{
    public static SuitSelectionModal Instance { get; private set; }

    [SerializeField] private GameObject panel;       // overlay root toggled on/off
    [SerializeField] private Button heartsButton;
    [SerializeField] private Button diamondsButton;
    [SerializeField] private Button clubsButton;
    [SerializeField] private Button spadesButton;

    private Action<Suit> _callback;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        Instance = this;

        if (heartsButton)   heartsButton.onClick.AddListener(() => Choose(Suit.Hearts));
        if (diamondsButton) diamondsButton.onClick.AddListener(() => Choose(Suit.Diamonds));
        if (clubsButton)    clubsButton.onClick.AddListener(() => Choose(Suit.Clubs));
        if (spadesButton)   spadesButton.onClick.AddListener(() => Choose(Suit.Spades));

        if (panel) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Opens the modal; <paramref name="onSelected"/> fires once a suit is picked.</summary>
    public void Show(Action<Suit> onSelected)
    {
        _callback = onSelected;
        if (panel) panel.SetActive(true);
    }

    void Choose(Suit suit)
    {
        if (panel) panel.SetActive(false);
        var cb = _callback;
        _callback = null;
        cb?.Invoke(suit);
    }
}
