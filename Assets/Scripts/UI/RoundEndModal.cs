using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Inter-round modal: shows the round winner and a score list (cash, "OUT"),
/// with a "Next round" button. Passive view, built/wired via MCP. Found by states
/// through the static Instance, like SuitSelectionModal.
/// </summary>
public class RoundEndModal : MonoBehaviour
{
    public static RoundEndModal Instance { get; private set; }

    public struct ScoreRow
    {
        public string Name;
        public int Cash;
        public bool Eliminated;
    }

    [SerializeField] private GameObject panel;       // overlay root toggled on/off
    [SerializeField] private TMP_Text titleText;     // "Vítěz kola: X"
    [SerializeField] private TMP_Text scoreText;     // multi-line score list
    [SerializeField] private Button nextButton;      // "Další kolo"

    private Action _onNext;

    void Awake()
    {
        Instance = this;
        if (nextButton) nextButton.onClick.AddListener(HandleNext);
        if (panel) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Opens the modal with the winner and score rows; onNext fires on the button.</summary>
    public void Show(string winnerName, IReadOnlyList<ScoreRow> rows, Action onNext)
    {
        _onNext = onNext;

        if (titleText) titleText.text = $"Vítěz kola: {winnerName}";
        if (scoreText)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var r in rows)
                sb.AppendLine(r.Eliminated ? $"{r.Name}: OUT" : $"{r.Name}: {r.Cash}");
            scoreText.text = sb.ToString();
        }

        if (panel) panel.SetActive(true);
    }

    void HandleNext()
    {
        if (panel) panel.SetActive(false);
        var cb = _onNext;
        _onNext = null;
        cb?.Invoke();
    }
}
