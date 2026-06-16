using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Final modal: announces the overall tournament winner with a "Do menu" button.
/// Passive view, built/wired via MCP, found by states through the static Instance.
/// </summary>
public class GameOverModal : MonoBehaviour
{
    public static GameOverModal Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;   // "Vítěz turnaje: X"
    [SerializeField] private Button menuButton;    // "Do menu"

    private Action _onMenu;

    void Awake()
    {
        Instance = this;
        if (menuButton) menuButton.onClick.AddListener(HandleMenu);
        if (panel) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string message, Action onMenu)
    {
        _onMenu = onMenu;
        if (titleText) titleText.text = message;
        if (panel) panel.SetActive(true);
    }

    void HandleMenu()
    {
        if (panel) panel.SetActive(false);
        var cb = _onMenu;
        _onMenu = null;
        cb?.Invoke();
    }
}
