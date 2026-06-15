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
    public int defaultBet   = 25;

    [Header("Options")]
    public bool uniqueAiNames   = true;
    public bool uniqueAiAvatars = true;
    public bool randomizeAiBets = true;

    public static GameSession I { get; private set; }

    [System.NonSerialized]
    private List<Player> _players = new();
    public List<Player> Players => _players;
    public int ActiveIndex { get; private set; } = -1;

    public int Pot { get; private set; } = 0;

    /// <summary>
    /// Shared Prší rule state (forced suit, draw penalty, skip) owned by the session.
    /// The running game moves physical cards/UI; Core owns the rules via this context.
    /// </summary>
    public GameContext Rules { get; private set; }

    public event Action<Player,int> ActivePlayerChanged;
    public event Action SessionChanged;
    public event Action<int> PotChanged;

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
        Rules = new GameContext(null, null, new List<IPlayerData>(_players));

        // Debug: zkontrolovat finální stav po všech inicializacích
        Debug.Log($"[GameSession] Po všech inicializacích: Human player (ID {Human.Id}, Name {Human.Name}) IsHuman: {Human.IsHuman}");
    }

    void Start()
    {
        // Kontrola CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameSession] CardManager nebyl nalezen ve scéně!");
        }
        
        // rozpošli počáteční stav po Start() ostatních (UI už je nabindované)
        StartCoroutine(BroadcastInitialStateNextFrame());
        
        // Propojit AI hráče s UI komponentami
        ConnectAIHandsToPlayers();
    }

    System.Collections.IEnumerator BroadcastInitialStateNextFrame()
    {
        yield return null;
        
        // Debug: zkontrolovat PlayerUI komponenty a automaticky je propojit
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        Debug.Log($"[GameSession] Nalezeno {playerUIs.Length} PlayerUI komponent");
        
        for (int i = 0; i < playerUIs.Length && i < _players.Count; i++)
        {
            Debug.Log($"[GameSession] PlayerUI {i}: {playerUIs[i].name}, před bindingem: {(playerUIs[i].Bound?.Name ?? "NULL")}");
            Debug.Log($"[GameSession] PlayerUI {i} animátory - potAnimator: {(playerUIs[i].potAnimator != null ? "OK" : "NULL")}, cashAnimator: {(playerUIs[i].cashAnimator != null ? "OK" : "NULL")}");
            playerUIs[i].Bind(_players[i]);
            Debug.Log($"[GameSession] PlayerUI {i}: {playerUIs[i].name}, po bindingu: {(playerUIs[i].Bound?.Name ?? "NULL")}");
        }
        
        foreach (var p in _players) p.NotifyChanged();
        SessionChanged?.Invoke();
    }

    // === Nové: start nové hry / vsazení potu ===
    public void StartNewRound()
    {
        // Vynuluj pot a stake
        Pot = 0;
        foreach (var p in _players) p.ResetStake();
        
        PotChanged?.Invoke(Pot);
        SessionChanged?.Invoke();
    }

    // Přidat sázku do potu
    public void AddToPot(int amount)
    {
        Pot += amount;
        PotChanged?.Invoke(Pot);
    }

    // Vyvolat SessionChanged událost
    public void NotifySessionChanged()
    {
        SessionChanged?.Invoke();
    }

    // Vyplacení banku vítězi (a vynulování stake)
    public void PayoutToWinner(int winnerIndex)
    {
        if (winnerIndex < 0 || winnerIndex >= _players.Count) return;
        _players[winnerIndex].Payout(Pot);
        Pot = 0;
        foreach (var p in _players) p.ResetStake();

        PotChanged?.Invoke(Pot);
        SessionChanged?.Invoke();
    }

    // Start nové hry - resetuje stav kola
    // public void StartNewGame() // Odstraněno
    // {
    //     isRoundStarted = false;
    //     StartNewRound();
    // }

    public void ApplyHumanFromMenu(string name, Sprite avatar, int avatarIndex, int? chosenBet = null)
    {
        if (Human != null)
        {
            Human.SetName(name);
            Human.SetAvatar(avatar, avatarIndex);

            if (Human.Cash <= 0) Human.SetCash(startingCash);

            if (chosenBet.HasValue)
                Human.SetBet(BetRules.ClampToPreset(chosenBet.Value));
            else if (Human.Bet <= 0)
                Human.SetBet(BetRules.ClampToPreset(defaultBet));

            Human.NotifyChanged();
        }

        PlayerPrefs.SetString(PREF_NAME, Human?.Name ?? "Player");
        PlayerPrefs.SetInt(PREF_AVI, avatarIndex);
        PlayerPrefs.Save();

        SessionChanged?.Invoke();
    }

    public void SetActiveIndex(int i)
    {
        // Deaktivovat všechny hráče
        for (int j = 0; j < _players.Count; j++)
        {
            if (_players[j].IsActive)
            {
                _players[j].IsActive = false;
                // Zvuk pro konec tahu hráče
                AudioEvents.TriggerPlayerTurnEnded();
            }
        }

        ActiveIndex = i;

        // Aktivovat nového hráče
        if (ActiveIndex >= 0 && ActiveIndex < _players.Count)
        {
            _players[ActiveIndex].IsActive = true;
            // Zvuk pro začátek tahu nového hráče
            AudioEvents.TriggerPlayerTurnStarted();
        }

        var active = (ActiveIndex >= 0 && ActiveIndex < _players.Count) ? _players[ActiveIndex] : null;
        ActivePlayerChanged?.Invoke(active, ActiveIndex);
    }
    
    /// Získá index dalšího hráče v pořadí
    public int GetNextPlayerIndex()
    {
        if (_players.Count == 0) return -1;
        
        int nextIndex = (ActiveIndex + 1) % _players.Count;
        
        // Najdi dalšího hráče, který může hrát
        int attempts = 0;
        while (attempts < _players.Count)
        {
            if (_players[nextIndex].CanPlay)
            {
                return nextIndex;
            }
            nextIndex = (nextIndex + 1) % _players.Count;
            attempts++;
        }
        
        // Pokud žádný hráč nemůže hrát, vrať -1
        return -1;
    }
    
    /// Aktivuje dalšího hráče v pořadí
    public void ActivateNextPlayer()
    {
        int nextIndex = GetNextPlayerIndex();
        if (nextIndex >= 0)
        {
            SetActiveIndex(nextIndex);
        }
    }

    // === interní pomocné metody ===
    void EnsurePlayers()
    {
        int need = 4 - _players.Count;
        
        for (int i = 0; i < need; i++)
        {
            int id = _players.Count;
            bool isHuman = (id == 0);
            string name = isHuman ? "Player" : $"AI Player {id}";
            _players.Add(new Player(id, name, isHuman));
            Debug.Log($"[GameSession] Vytvořen hráč: ID={id}, Name={name}, IsHuman={isHuman}");
        }

        // výchozí ekonomika pro humana (když nebyla nastavena ze scény)
        if (_players[0].Cash <= 0) _players[0].SetCash(startingCash);
        if (_players[0].Bet  <= 0) _players[0].SetBet(BetRules.ClampToPreset(defaultBet));
        
        // Debug: zkontrolovat stav po vytvoření
        Debug.Log($"[GameSession] Po EnsurePlayers: Human player (ID {Human.Id}, Name {Human.Name}) IsHuman: {Human.IsHuman}");
    }

    void ApplyHumanPrefsIfAny()
    {
        if (Human == null) return;

        if (PlayerPrefs.HasKey(PREF_NAME))
            Human.SetName(PlayerPrefs.GetString(PREF_NAME, "Player"));

        int idx = PlayerPrefs.GetInt(PREF_AVI, -1);
        if (idx >= 0) Human.AvatarIndex = idx; // sprite dodáme při ApplyHumanFromMenu
    }

    void RandomizeAI()
    {
        var usedNameIdx   = new HashSet<int>();
        var usedAvatarIdx = new HashSet<int>();

        Debug.Log($"[GameSession] RandomizeAI: Začínám s {_players.Count} hráči");
        Debug.Log($"[GameSession] RandomizeAI: Human player před randomizací: ID={Human.Id}, Name={Human.Name}, IsHuman={Human.IsHuman}");

        for (int i = 1; i < _players.Count; i++)
        {
            Debug.Log($"[GameSession] RandomizeAI: Zpracovávám AI hráče {i}: ID={_players[i].Id}, Name={_players[i].Name}, IsHuman={_players[i].IsHuman}");
            
            // jméno
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

            // AI bet
            if (randomizeAiBets)
                _players[i].SetBet(BetRules.RandomAffordable(_players[i].Cash));
        }
        
        Debug.Log($"[GameSession] RandomizeAI: Human player po randomizaci: ID={Human.Id}, Name={Human.Name}, IsHuman={Human.IsHuman}");
    }
    
    // Propojit AI hráče s AIHand UI komponentami
    void ConnectAIHandsToPlayers()
    {
        // Najít CardManager
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("[GameSession] CardManager nebyl nalezen!");
            return;
        }
        
        // Najít všechny AIHand komponenty ve scéně
        AIHand[] aiHands = FindObjectsOfType<AIHand>();
        
        // Pokud nenajdeme žádné, zkusit najít podle názvu
        if (aiHands.Length == 0)
        {
            // Najdeme všechny objekty ve scéně
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            List<AIHand> foundHands = new List<AIHand>();
            
            foreach (var obj in allObjects)
            {
                // Hledáme objekty s názvem obsahujícím "EnemyBar", "Enemy" nebo "AI"
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
        
        // Propojit AI hráče s AIHand komponentami
        for (int i = 1; i < _players.Count && i-1 < aiHands.Length; i++) // i=1 protože 0 je human
        {
            var aiPlayer = _players[i];
            var aiHand = aiHands[i-1];
            
            aiHand.SetAIPlayer(aiPlayer);
            
            // Propojit s odpovídajícím CardStack z CardManager
            if (cardManager.aiHands != null && i-1 < cardManager.aiHands.Length)
            {
                CardStack aiCardStack = cardManager.aiHands[i-1];
                if (aiCardStack != null)
                {
                    aiHand.SetCardStack(aiCardStack);
                }
                else
                {
                    Debug.LogError($"[GameSession] CardStack {i-1} je null!");
                }
            }
            else
            {
                Debug.LogError($"[GameSession] CardManager nemá nastavený CardStack pro AI {i-1}: aiHands je {(cardManager.aiHands == null ? "null" : "není null")}, délka je {(cardManager.aiHands == null ? "N/A" : cardManager.aiHands.Length.ToString())}");
            }
            
            // Okamžitě aktualizovat UI
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
