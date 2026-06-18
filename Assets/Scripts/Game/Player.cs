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

    // New properties for cards
    public List<Card> hand = new List<Card>();
    public bool hasFolded = false; // In Prší: true = player has no cards left (won)

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
    
    // === Card-hand methods ===
    
    /// Adds a card to the player's hand
    public void AddCard(Card card)
    {
        if (card != null)
        {
            hand.Add(card);
            Changed?.Invoke(this);
        }
    }
    
    /// Removes a card from the player's hand
    public void RemoveCard(Card card)
    {
        if (hand.Remove(card))
        {
            Changed?.Invoke(this);
            
            // Check whether the player won
            if (hand.Count == 0)
            {
                SetWon();
            }
        }
    }
    
    /// Clears all cards from the hand
    public void ClearHand()
    {
        if (hand.Count > 0)
        {
            hand.Clear();
            hasFolded = false; // Reset won status
            Changed?.Invoke(this);
        }
    }
    
    /// Marks the player as winner
    public void SetWon()
    {
        hasFolded = true; // hasFolded = won
        Changed?.Invoke(this);
    }
    
    /// Checks whether the player won (has no cards)
    public bool HasWon => hand.Count == 0 || hasFolded;
    
    /// Checks whether the player can play (in game and not folded)
    public bool CanPlay => !hasFolded && hand.Count > 0;
    
    /// Checks whether the player is still in the game
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