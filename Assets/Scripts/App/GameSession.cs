using System;
using System.Collections.Generic;
using UnityEngine;
using Prsi.Core;
using Prsi.Core.Game;

public class GameSession : MonoBehaviour
{
    [Header("AI Names (optional)")]
    public string[] aiNames = { "Bot Bob", "Luna", "Karel", "Zeta", "Maverick", "Echo", "Tina", "Dex" };

    [Header("AI Avatars (optional)")]
    public Sprite[] aiAvatars;

    [Header("Economy")]
    public int startingCash = 1000;
    [Tooltip("Penalty per card value at round end (penalty = penaltyRate × Σ card value).")]
    public int penaltyRate = 10;

    [Header("Options")]
    public bool uniqueAiNames   = true;
    public bool uniqueAiAvatars = true;

    public static GameSession I { get; private set; }

    [System.NonSerialized]
    private List<Player> _players = new();
    public List<Player> Players => _players;
    public int ActiveIndex { get; private set; } = -1;

    /// <summary>
    /// Shared Prší rule state (forced suit, draw penalty, skip) owned by the session.
    /// The running game moves physical cards/UI; Core owns the rules via this context.
    /// </summary>
    public GameContext Rules { get; private set; }

    public event Action<Player,int> ActivePlayerChanged;
    public event Action SessionChanged;

    const string PREF_NAME = "p_name";
    const string PREF_AVI  = "p_avatar_idx";

    public Player Human => Players.Count > 0 ? Players[0] : null;
    

    void Awake()
    {
        if (I != null && I != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        I = this;

        EnsurePlayers();
        ApplyHumanPrefsIfAny();
        RandomizeAI();

        // Build the shared rule context over the live players (Player : IPlayerData).
        Rules = new GameContext(new List<IPlayerData>(_players));

    }

    void Start()
    {
        // Check CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameSession] CardManager not found in the scene!");
        }
        
        // broadcast initial state after other Start() calls (UI is already bound)
        StartCoroutine(BroadcastInitialStateNextFrame());
        
        // Wire AI players to UI components
        ConnectAIHandsToPlayers();
    }

    System.Collections.IEnumerator BroadcastInitialStateNextFrame()
    {
        yield return null;
        
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        
        for (int i = 0; i < playerUIs.Length && i < _players.Count; i++)
        {
            playerUIs[i].Bind(_players[i]);
        }
        
        foreach (var p in _players) p.NotifyChanged();
        SessionChanged?.Invoke();
    }

    // === New: start new game / pot bet ===
    // Raise SessionChanged event
    public void NotifySessionChanged()
    {
        SessionChanged?.Invoke();
    }

    /// Reset money for a fresh tournament: everyone back to startingCash (re-shows
    /// bars hidden after elimination). Called when a new game starts from the menu.
    public void ResetTournament()
    {
        foreach (var p in _players) p.SetCash(startingCash);
        SessionChanged?.Invoke();
    }

    public void ApplyHumanFromMenu(string name, Sprite avatar, int avatarIndex)
    {
        // New game from the menu = fresh tournament: reset everyone's money.
        ResetTournament();

        if (Human != null)
        {
            Human.SetName(name);
            Human.SetAvatar(avatar, avatarIndex);
            Human.NotifyChanged();
        }

        PlayerPrefs.SetString(PREF_NAME, Human?.Name ?? "Player");
        PlayerPrefs.SetInt(PREF_AVI, avatarIndex);
        PlayerPrefs.Save();

        SessionChanged?.Invoke();
    }

    public void SetActiveIndex(int i)
    {
        // Deactivate all players
        for (int j = 0; j < _players.Count; j++)
        {
            if (_players[j].IsActive)
            {
                _players[j].IsActive = false;
                // Sound for end of player's turn
                AudioEvents.TriggerPlayerTurnEnded();
            }
        }

        ActiveIndex = i;

        // Activate the new player
        if (ActiveIndex >= 0 && ActiveIndex < _players.Count)
        {
            _players[ActiveIndex].IsActive = true;
            // Sound for start of new player's turn
            AudioEvents.TriggerPlayerTurnStarted();
        }

        var active = (ActiveIndex >= 0 && ActiveIndex < _players.Count) ? _players[ActiveIndex] : null;
        if (active != null) GameLog.Record("ACTIVE", active.Name);
        ActivePlayerChanged?.Invoke(active, ActiveIndex);
    }
    
    /// Gets the index of the next player in order
    public int GetNextPlayerIndex() => NextPlayableAfter(ActiveIndex);

    /// Next player after the given index who can still play; -1 if none.
    int NextPlayableAfter(int index)
    {
        if (_players.Count == 0) return -1;

        int nextIndex = (index + 1) % _players.Count;
        int attempts = 0;
        while (attempts < _players.Count)
        {
            if (_players[nextIndex].CanPlay) return nextIndex;
            nextIndex = (nextIndex + 1) % _players.Count;
            attempts++;
        }
        return -1;
    }

    /// Activates the next player in order. Ace skip is NOT consumed here —
    /// GameplayState handles it so the skipped player briefly "stands" with a UI message first.
    public void ActivateNextPlayer()
    {
        int nextIndex = NextPlayableAfter(ActiveIndex);
        if (nextIndex >= 0) SetActiveIndex(nextIndex);
    }

    // === internal helper methods ===
    void EnsurePlayers()
    {
        int need = 4 - _players.Count;
        
        for (int i = 0; i < need; i++)
        {
            int id = _players.Count;
            bool isHuman = (id == 0);
            string name = isHuman ? "Player" : $"AI Player {id}";
            _players.Add(new Player(id, name, isHuman));
        }

        // default economy for human (when not set from the scene)
        if (_players[0].Cash <= 0) _players[0].SetCash(startingCash);
    }

    void ApplyHumanPrefsIfAny()
    {
        if (Human == null) return;

        if (PlayerPrefs.HasKey(PREF_NAME))
            Human.SetName(PlayerPrefs.GetString(PREF_NAME, "Player"));

        int idx = PlayerPrefs.GetInt(PREF_AVI, -1);
        if (idx >= 0) Human.AvatarIndex = idx; // sprite supplied in ApplyHumanFromMenu
    }

    void RandomizeAI()
    {
        var usedNameIdx   = new HashSet<int>();
        var usedAvatarIdx = new HashSet<int>();


        for (int i = 1; i < _players.Count; i++)
        {
            
            // name
            string aiName = _players[i].Name;
            if (aiNames != null && aiNames.Length > 0)
            {
                int ni = PickRandomIndex(aiNames.Length, uniqueAiNames ? usedNameIdx : null);
                aiName = aiNames[ni];
            }
            _players[i].SetName(aiName);

            // avatar
            if (aiAvatars != null && aiAvatars.Length > 0)
            {
                int ai = PickRandomIndex(aiAvatars.Length, uniqueAiAvatars ? usedAvatarIdx : null);
                var sprite = aiAvatars[ai];
                _players[i].SetAvatar(sprite, ai);
            }
        }
    }
    
    // Wire AI players to AIHand UI components
    void ConnectAIHandsToPlayers()
    {
        // Find CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameSession] CardManager not found!");
            return;
        }
        
        // Find all AIHand components in the scene
        AIHand[] aiHands = FindObjectsOfType<AIHand>();
        
        // If none found, try finding by name
        if (aiHands.Length == 0)
        {
            // Find all objects in the scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            List<AIHand> foundHands = new List<AIHand>();
            
            foreach (var obj in allObjects)
            {
                // Look for objects whose name contains "EnemyBar", "Enemy" or "AI"
                if (obj.name.Contains("EnemyBar") || obj.name.Contains("Enemy") || obj.name.Contains("AI"))
                {
                    AIHand aiHand = obj.GetComponent<AIHand>();
                    if (aiHand != null)
                    {
                        foundHands.Add(aiHand);
                    }
                }
            }
            
            aiHands = foundHands.ToArray();
        }
        
        // Wire AI players to AIHand components
        for (int i = 1; i < _players.Count && i-1 < aiHands.Length; i++) // i=1 because 0 is human
        {
            var aiPlayer = _players[i];
            var aiHand = aiHands[i-1];
            
            aiHand.SetAIPlayer(aiPlayer);
            
            // Wire to the matching CardStack from CardManager
            if (cardManager.aiHands != null && i-1 < cardManager.aiHands.Length)
            {
                CardStack aiCardStack = cardManager.aiHands[i-1];
                if (aiCardStack != null)
                {
                    aiHand.SetCardStack(aiCardStack);
                }
                else
                {
                    Debug.LogError($"[GameSession] CardStack {i-1} is null!");
                }
            }
            else
            {
                Debug.LogError($"[GameSession] CardManager has no CardStack set for AI {i-1}: aiHands is {(cardManager.aiHands == null ? "null" : "not null")}, length is {(cardManager.aiHands == null ? "N/A" : cardManager.aiHands.Length.ToString())}");
            }
            
            // Update UI immediately
            aiHand.UpdateHand(aiPlayer);
        }
        
 
    }

    int PickRandomIndex(int maxExclusive, HashSet<int> usedOrNull)
    {
        if (usedOrNull == null || usedOrNull.Count >= maxExclusive)
            return UnityEngine.Random.Range(0, maxExclusive);

        int tries = 0;
        int idx;
        do {
            idx = UnityEngine.Random.Range(0, maxExclusive);
            tries++;
        } while (usedOrNull.Contains(idx) && tries < 50);

        usedOrNull.Add(idx);
        return idx;
    }
}
