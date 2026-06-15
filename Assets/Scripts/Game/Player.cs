using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public int Id;
    public string Name;
    public bool IsHuman;

    public Sprite Avatar;
    public int AvatarIndex;

    public int Cash = 1000;
    public int Bet  = 25;

    // kolik má právě vsazeno v aktuální hře
    public int Staked { get; private set; }
    
    // Nové properties pro karty
    public List<Card> hand = new List<Card>();
    public bool hasFolded = false; // V prší: true = hráč už nemá karty (vyhrál)

    bool isActive;
    public bool IsActive {
        get => isActive;
        set { 
            if (isActive == value) return; 
            Debug.Log($"[Player] {Name} IsActive: {isActive} → {value}");
            isActive = value; 
            Changed?.Invoke(this); 
        }
    }

    public event Action<Player> Changed;

    public Player(int id, string name, bool isHuman = false) 
    { 
        Id = id; 
        Name = name; 
        IsHuman = isHuman; 
        Debug.Log($"[Player] Konstruktor: Vytvořen hráč ID={id}, Name={name}, IsHuman={isHuman}");
    }

    public void SetName(string n){ Name = string.IsNullOrWhiteSpace(n) ? "Player" : n.Trim(); Changed?.Invoke(this); }
    public void SetAvatar(Sprite s, int idx){ Avatar=s; AvatarIndex=idx; Changed?.Invoke(this); }
    public void SetCash(int v){ 
        Debug.Log($"[Player] SetCash pro {Name}: {Cash} -> {Mathf.Max(0,v)}");
        Cash=Mathf.Max(0,v); 
        Changed?.Invoke(this); 
    }
    public void SetBet(int v){ Bet =Mathf.Max(0,v); Changed?.Invoke(this); }
    public void NotifyChanged(){ Changed?.Invoke(this); }

    public bool CanAffordBet() => Bet > 0 && Cash >= Bet;

    /// Odečte sázku z Cash a uloží ji do Staked. Vrací skutečně vsazenou částku.
    public int PlaceBet()
    {
        if (Bet <= 0) return 0;
        int amount = Mathf.Min(Bet, Cash);
        if (amount <= 0) return 0;

        Debug.Log($"[Player] PlaceBet pro {Name}: Cash {Cash} -> {Cash - amount}, Staked {Staked} -> {Staked + amount}");
        Cash  -= amount;
        Staked += amount;
        Changed?.Invoke(this);
        return amount;
    }

    /// Vynuluje vsazenou částku (např. po vyplacení nebo refundu).
    public void ResetStake()
    {
        if (Staked == 0) return;
        Staked = 0;
        Changed?.Invoke(this);
    }

    /// Vrátí vsazené peníze zpět (např. při zrušení hry)
    public void RefundStake()
    {
        if (Staked <= 0) return;
        Debug.Log($"[Player] RefundStake pro {Name}: Cash {Cash} -> {Cash + Staked}, Staked {Staked} -> 0");
        Cash += Staked;
        Staked = 0;
        Changed?.Invoke(this);
    }

    /// Vyplatí výhru (přičte do Cash) – bez vztahu ke Staked (počítej zvlášť).
    public void Payout(int amount)
    {
        if (amount <= 0) return;
        Debug.Log($"[Player] Payout pro {Name}: Cash {Cash} -> {Cash + amount}");
        Cash += amount;
        Changed?.Invoke(this);
    }
    
    public void SetAvatar(Sprite s) {
        Avatar = s;
        Changed?.Invoke(this);
    }
    
    // === Metody pro práci s kartami ===
    
    /// Přidá kartu do ruky hráče
    public void AddCard(Card card)
    {
        if (card != null)
        {
            hand.Add(card);
            Changed?.Invoke(this);
        }
    }
    
    /// Odebere kartu z ruky hráče
    public void RemoveCard(Card card)
    {
        if (hand.Remove(card))
        {
            Changed?.Invoke(this);
            
            // Kontrola, zda hráč vyhrál
            if (hand.Count == 0)
            {
                SetWon();
            }
        }
    }
    
    /// Vyčistí všechny karty z ruky
    public void ClearHand()
    {
        if (hand.Count > 0)
        {
            hand.Clear();
            hasFolded = false; // Reset won status
            Changed?.Invoke(this);
        }
    }
    
    /// Označí hráče jako vítěze
    public void SetWon()
    {
        hasFolded = true; // hasFolded = vyhrál
        Changed?.Invoke(this);
    }
    
    /// Zkontroluje zda hráč vyhrál (nemá žádné karty)
    public bool HasWon => hand.Count == 0 || hasFolded;
    
    /// Zkontroluje zda hráč může hrát (je ve hře a není folded)
    public bool CanPlay => !hasFolded && hand.Count > 0;
    
    /// Zkontroluje zda je hráč stále ve hře
    public bool IsInGame => !hasFolded;
}