using UnityEngine;
using Prsi.Core.Cards;
using Prsi.Core.Game;

public class CardManager : MonoBehaviour
{
    // The draw deck is the Core Deck (seeded shuffle/draw/reshuffle, unit-tested).
    // Discard and hands stay as scene CardStacks because the UI renders from them.
    Deck deck;

    [Header("Deck")]
    [Tooltip("Shuffle seed; 0 = random each run. A fixed value makes the deal reproducible.")]
    public int deckSeed = 0;

    [Header("Card Stacks")]
    public CardStack discard;
    public CardStack playerHand;
    
    [Header("UI Components")]
    public PlayerHand playerHandUI; // UI component for displaying the player's hand
    public AIHand[] aiHandUIs; // UI components for displaying AI hands
    
    [Header("AI Hand Stacks")]
    public CardStack[] aiHands; // Array of AI hand stacks
    
    [Header("Dealing Settings")]
    public int cardsPerPlayer = 5;
    public float dealDelay = 0.3f;
    
    // Events
    public System.Action<Player> OnPlayerWon;
    
    void Awake()
    {
        GameLog.Cards = this;

        // Check references
        if (discard == null) Debug.LogError("[CardManager] Discard is not assigned!");
        if (playerHand == null) Debug.LogError("[CardManager] PlayerHand is not assigned!");
        
        // Automatically find PlayerHand UI component
        if (playerHandUI == null)
        {
            playerHandUI = FindObjectOfType<PlayerHand>();
            if (playerHandUI == null)
                Debug.LogWarning("[CardManager] PlayerHand UI component not found automatically");
        }

        // Automatically find AIHand UI components
        if (aiHandUIs == null || aiHandUIs.Length == 0)
        {
            AIHand[] foundAIHands = FindObjectsOfType<AIHand>();
            if (foundAIHands.Length > 0)
                aiHandUIs = foundAIHands;
            else
                Debug.LogWarning("[CardManager] AIHand UI components not found automatically");
        }
        
        // Check AI hand stacks
        if (aiHands == null || aiHands.Length == 0)
        {
            Debug.LogError("[CardManager] AI hand stacks are not set!");
        }
        else
        {
            for (int i = 0; i < aiHands.Length; i++)
            {
                if (aiHands[i] == null)
                {
                    Debug.LogError($"[CardManager] AI hand {i} is null! Assign a CardStack object to slot {i}");
                }
            }
        }
    }
    
    void InitializeDeck()
    {
        // Fixed seed = reproducible deal; 0 = random run.
        deck = deckSeed != 0 ? new Deck(deckSeed) : new Deck();

        // Build the 32-card mariáš deck of UI Cards and let Core shuffle it.
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                deck.AddCard(new Card(suit, rank));

        deck.Shuffle();
    }
    
    public void DealCardsToPlayers()
    {
        if (playerHand == null) { Debug.LogError("[CardManager] PlayerHand is null!"); return; }

        GameLog.Clear();

        // Build a fresh 32-card deck for the new hand. Done here (not in Awake) so a
        // new round without a scene reload starts full, and so CardSpriteManager has
        // already loaded its sprites by the time the Cards are created.
        InitializeDeck();

        // Reset transient rule state so a Seven/Queen/Ace effect can't leak into a new hand.
        GameSession.I.Rules?.ResetRoundState();

        // Clear hands and discard pile
        playerHand.Clear();
        discard.Clear();
        
        // Clear AI hand stacks
        if (aiHands != null)
        {
            foreach (var aiHand in aiHands)
            {
                if (aiHand != null) aiHand.Clear();
            }
        }
        
        // Clear all players' hands
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Deal cards
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            // One card per player (human included)
            for (int j = 0; j < GameSession.I.Players.Count; j++)
            {
                Card card = deck.DrawCard() as Card;
                if (card != null)
                {
                    var player = GameSession.I.Players[j];
                    player.AddCard(card);
                    
                    // For human player also add to playerHand stack for UI
                    if (j == 0) // Human player
                    {
                        playerHand.AddCard(card);
                    }
                    // For AI player add to matching AI hand stack
                    else if (aiHands != null && j-1 < aiHands.Length && aiHands[j-1] != null)
                    {
                        aiHands[j-1].AddCard(card);
                    }
                    else
                    {
                        Debug.LogError($"[CardManager] Cannot add card to AI hand {j-1}: aiHands is {(aiHands == null ? "null" : "not null")}, length is {(aiHands == null ? "N/A" : aiHands.Length.ToString())}");
                    }
                }
                else
                {
                    Debug.LogError($"[CardManager] Error: cannot deal a card for player {j}");
                }
            }
        }
        
        // After dealing, flip one card face-up onto the discard pile
        Card topCard = deck.DrawCard() as Card;
        if (topCard != null)
        {
            discard.AddCard(topCard);
        }
        else
        {
            Debug.LogError("[CardManager] Error: cannot flip a card face-up - the deck is empty!");
        }

        GameLog.Record("DEAL", "", topCard != null ? topCard.ToString() : "");
#if UNITY_EDITOR
        AssertCardCount("deal");
#endif
    }
    
    // Player played a card
    public void PlayCard(Player player, Card card)
    {
        // Check whether player has the card in hand
        if (!player.hand.Contains(card))
        {
            Debug.LogWarning($"[CardManager] Player {player.Name} doesn't have card {card} in hand!");
            return;
        }
        
        // Check whether the card is playable
        if (!CanPlayCard(card))
        {
            Debug.LogWarning($"[CardManager] Card {card} is not playable!");
            return;
        }
        
        // For human player first remove from playerHand stack and UI
        if (player.IsHuman)
        {
            playerHand.RemoveCard(card);
            // Also remove from UI component
            if (playerHandUI != null)
            {
                playerHandUI.RemoveCard(card);
            }
            else
            {
                Debug.LogWarning("[CardManager] playerHandUI is null - card was not removed from UI!");
            }
        }
        // For AI player first remove from matching AI hand stack
        else
        {
            int aiIndex = player.Id - 1; // AI players have IDs 1, 2, 3...
            if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
            {
                aiHands[aiIndex].RemoveCard(card);
            }
        }
        
        // Play the card
        player.RemoveCard(card);
        discard.AddCard(card);


        GameLog.Record("PLAY", player.Name, card.ToString());

        // Apply the card's rule effect to the shared context before advancing the turn:
        // Seven → draw penalty, Ace → skip, Queen → forced suit, Regular → clears forced suit.
        BaseCard domainCard = CardFactory.Create(card.suit, card.rank);

        // A human Queen lets the player pick the forced suit via a modal; defer the
        // turn until a suit is chosen. Everyone else (and AI Queens) resolve inline.
        if (domainCard is QueenCard humanQueen && player.IsHuman && SuitSelectionModal.Instance != null)
        {
            SuitSelectionModal.Instance.Show(chosenSuit =>
            {
                humanQueen.SelectSuit(chosenSuit);
                humanQueen.OnPlay(GameSession.I.Rules, player);
                FinishPlay(player);
            });
            return;
        }

        if (domainCard is QueenCard aiQueen)
            aiQueen.SelectSuit(ChooseAiSuit(player));

        domainCard.OnPlay(GameSession.I.Rules, player);
        FinishPlay(player);
    }

    // Win check + advance, shared by the inline and deferred (Queen modal) play paths.
    void FinishPlay(Player player)
    {
#if UNITY_EDITOR
        AssertCardCount("play");
#endif
        if (player.HasWon)
            OnPlayerWon?.Invoke(player);
        else
            GameSession.I.ActivateNextPlayer();
    }

    // AI picks the suit it holds most of (falls back to Hearts on an empty hand).
    Suit ChooseAiSuit(Player player)
    {
        var counts = new int[4];
        foreach (Card c in player.hand) counts[(int)c.suit]++;

        int best = 0;
        for (int i = 1; i < counts.Length; i++)
            if (counts[i] > counts[best]) best = i;

        return (Suit)best;
    }
    
    // AI plays a card (deprecated — GameplayState is used now)
    public void PlayAICard(Player aiPlayer)
    {
        if (aiPlayer.hand.Count > 0)
        {
            // Find valid card
            Card cardToPlay = FindPlayableCard(aiPlayer);
            if (cardToPlay != null)
            {
                PlayCard(aiPlayer, cardToPlay);
            }
            else
            {
            }
        }
    }
    
    // Card playability is decided by the Prsi.Core rule engine (special cards included).
    public bool CanPlayCard(Card cardToPlay)
    {
        Card topCard = GetTopDiscardCard();
        if (topCard == null || cardToPlay == null) return false;

        return CardRules.CanPlay(cardToPlay, topCard, GameSession.I.Rules);
    }
    
    // Gets top card from discard pile
    public Card GetTopDiscardCard()
    {
        if (discard?.cards != null && discard.cards.Count > 0)
        {
            return discard.cards[discard.cards.Count - 1];
        }
        return null;
    }
    
    // Finds a playable card in the player's hand
    public Card FindPlayableCard(Player player)
    {
        foreach (Card card in player.hand)
        {
            if (CanPlayCard(card))
            {
                return card;
            }
        }
        return null;
    }
    
    // Draws for a player, honouring an active Seven draw penalty: pulls
    // PendingDrawCount cards (and clears it) when set, otherwise a single card.
    public Card DrawCardForPlayer(Player player)
    {
        int count = 1;
        var rules = GameSession.I.Rules;
        if (rules != null && rules.PendingDrawCount > 0)
        {
            count = rules.PendingDrawCount;
            rules.ClearDrawPenalty();
        }

        // Show "+N" in the drawing player's bar (Seven penalty = many, normal = +1).
        PlayerEffectDisplay.Instance?.ShowDraw(player, count);

        Card last = null;
        for (int i = 0; i < count; i++) last = DrawOneForPlayer(player);
#if UNITY_EDITOR
        AssertCardCount("draw");
#endif
        return last;
    }

    // Draws one card for the player
    Card DrawOneForPlayer(Player player)
    {
        // When deck runs out, refill from discard (except top card).
        if (deck.IsEmpty)
        {
            ReshuffleDiscardIntoDeck();
        }

        Card drawnCard = deck.DrawCard() as Card;
        GameLog.Record("DRAW", player.Name, drawnCard != null ? drawnCard.ToString() : "(none)");

        if (drawnCard != null)
        {
            // For human player first add to UI with CardIn animation
            if (player.IsHuman)
            {
                // Add card to UI with CardIn animation BEFORE data
                if (playerHandUI != null)
                {
                    playerHandUI.AddCardToHand(drawnCard);
                }
                else
                {
                    Debug.LogWarning("[CardManager] playerHandUI is null - CardIn animation will not run");
                }
                
                // Then add to data
                playerHand.AddCard(drawnCard);
            }
            // For AI player add to matching AI hand stack
            else
            {
                int aiIndex = player.Id - 1; // AI players have IDs 1, 2, 3...
                if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
                {
                    aiHands[aiIndex].AddCard(drawnCard);
                }
                
                // Add card to AI UI with CardIn animation
                if (aiHandUIs != null && aiIndex >= 0 && aiIndex < aiHandUIs.Length && aiHandUIs[aiIndex] != null)
                {
                    aiHandUIs[aiIndex].AddCard(drawnCard);
                }
                else
                {
                    Debug.LogWarning($"[CardManager] aiHandUIs[{aiIndex}] is null - CardIn animation will not run");
                }
            }
            
            // Add card to player data (all players)
            player.AddCard(drawnCard);
            
        }
        else
        {
            // Deck empty and discard had nothing to recycle (only its top card):
            // a legal late-game exhaustion, not an error.
            Debug.LogWarning("[CardManager] Both deck and discard are exhausted - cannot draw");
        }

        return drawnCard;
    }
    
    // Recycle the discard pile (minus its top card) back into the Core deck (S6.1).
    void ReshuffleDiscardIntoDeck()
    {
        if (discard.cards.Count <= 1)
        {
            Debug.LogWarning("[CardManager] Cannot reshuffle - discard pile has only one or no card");
            return;
        }

        // Keep the top card in play, recycle the rest.
        Card topCard = discard.cards[discard.cards.Count - 1];
        var recycled = discard.cards.GetRange(0, discard.cards.Count - 1);

        deck.ReshuffleFrom(recycled);

        discard.Clear();
        discard.AddCard(topCard);
    }
    
    public void DiscardPlayerCard(Card card)
    {
        if (playerHand.cards.Contains(card))
        {
            playerHand.cards.Remove(card);
            GameSession.I.Human.RemoveCard(card);
            discard.AddCard(card);
        }
    }
    
    public void ResetForNewRound()
    {
        // Move all cards back to the deck
        deck.AddCards(discard.cards);
        deck.AddCards(playerHand.cards);
        
        // Move cards from all players
        foreach (var player in GameSession.I.Players)
        {
            deck.AddCards(player.hand);
        }
        
        discard.Clear();
        playerHand.Clear();
        
        // Clear players' hands
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Shuffle the deck
        deck.Shuffle();
    }
    
    // Check whether the game is over
    public bool IsGameOver()
    {
        int playersWithCards = 0;
        foreach (var player in GameSession.I.Players)
        {
            if (player.IsInGame)
            {
                playersWithCards++;
            }
        }
        return playersWithCards <= 1; // Game over when at most 1 player has cards left
    }
    
#if UNITY_EDITOR
    // Editor-only instrumentation: every physical card must live in exactly one
    // place (deck, discard, or a hand). If the total drifts from 32, cards are
    // being lost or duplicated — log where so the leak can be traced.
    void AssertCardCount(string where)
    {
        int total = (deck?.RemainingCards ?? 0) + (discard?.cards?.Count ?? 0)
                  + (playerHand?.cards?.Count ?? 0);
        if (aiHands != null)
            foreach (var h in aiHands) total += h?.cards?.Count ?? 0;

        if (total != 32)
            Debug.LogWarning($"[CardManager] Card-count invariant broken after {where}: {total}/32");
    }
#endif

    // Find the winner
    public Player GetWinner()
    {
        foreach (var player in GameSession.I.Players)
        {
            if (player.HasWon)
            {
                return player;
            }
        }
        return null;
    }
}
