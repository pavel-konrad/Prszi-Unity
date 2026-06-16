using System;
using System.Collections.Generic;
using UnityEngine;
using Prsi.Core;
using Prsi.Core.Cards;
using Prsi.Core.Game;

[Serializable]
public class Player : IPlayerData, ITournamentPlayer
{
    public int Id;
    public string Name;
    public bool IsHuman;

    public Sprite Avatar;
    public int AvatarIndex;

    public int Cash = 1000;

    // Nové properties pro karty
    public List<Card> hand = new List<Card>();
    public bool hasFolded = false; // V prší: true = hráč už nemá karty (vyhrál)

    bool isActive;
    public bool IsActive {
        get => isActive;
        set { 
            if (isActive == value) return; 
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
    }

    public void SetName(string n){ Name = string.IsNullOrWhiteSpace(n) ? "Player" : n.Trim(); Changed?.Invoke(this); }
    public void SetAvatar(Sprite s, int idx){ Avatar=s; AvatarIndex=idx; Changed?.Invoke(this); }
    public void SetCash(int v){
        Cash=Mathf.Max(0,v);
        Changed?.Invoke(this);
    }
    public void NotifyChanged(){ Changed?.Invoke(this); }

    // ITournamentPlayer money in/out (clamped at 0, fires Changed for the UI).
    public void Pay(int amount)     => SetCash(Cash - amount);
    public void Receive(int amount) => SetCash(Cash + amount);

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

    // === Prsi.Core bridge (explicit interface implementations) ===
    // Map the public fields onto the domain interfaces without renaming fields or
    // touching Unity serialization. HasWon/CanPlay/IsInGame and the public
    // Name/IsHuman/Cash already satisfy the interfaces implicitly.
    int IPlayerData.Id => Id;
    string IPlayerData.Name => Name;
    bool IPlayerData.IsHuman => IsHuman;
    Sprite IPlayerData.Avatar => Avatar;
    int IPlayerData.Cash => Cash;
    IReadOnlyList<ICardData> IPlayerData.Hand => hand;

    string ITournamentPlayer.Name => Name;
    bool ITournamentPlayer.IsHuman => IsHuman;
    int ITournamentPlayer.Cash => Cash;
    IReadOnlyList<ICardData> ITournamentPlayer.Hand => hand;
}