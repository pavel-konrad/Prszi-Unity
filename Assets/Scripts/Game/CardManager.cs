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
    public PlayerHand playerHandUI; // UI komponenta pro zobrazení ruky hráče
    public AIHand[] aiHandUIs; // UI komponenty pro zobrazení AI rukou
    
    [Header("AI Hand Stacks")]
    public CardStack[] aiHands; // Pole pro AI hand stacky
    
    [Header("Dealing Settings")]
    public int cardsPerPlayer = 5;
    public float dealDelay = 0.3f;
    
    // Events
    public System.Action<Player> OnPlayerWon;
    
    void Awake()
    {
        // Kontrola referencí
        if (discard == null) Debug.LogError("[CardManager] Discard není přiřazen!");
        if (playerHand == null) Debug.LogError("[CardManager] PlayerHand není přiřazen!");
        
        // Automaticky najít PlayerHand UI komponentu
        if (playerHandUI == null)
        {
            playerHandUI = FindObjectOfType<PlayerHand>();
            if (playerHandUI == null)
                Debug.LogWarning("[CardManager] PlayerHand UI komponenta nebyla nalezena automaticky");
        }

        // Automaticky najít AIHand UI komponenty
        if (aiHandUIs == null || aiHandUIs.Length == 0)
        {
            AIHand[] foundAIHands = FindObjectsOfType<AIHand>();
            if (foundAIHands.Length > 0)
                aiHandUIs = foundAIHands;
            else
                Debug.LogWarning("[CardManager] AIHand UI komponenty nebyly nalezeny automaticky");
        }
        
        // Kontrola AI hand stacků
        if (aiHands == null || aiHands.Length == 0)
        {
            Debug.LogError("[CardManager] AI hand stacky nejsou nastaveny!");
        }
        else
        {
            for (int i = 0; i < aiHands.Length; i++)
            {
                if (aiHands[i] == null)
                {
                    Debug.LogError($"[CardManager] AI hand {i} je null! Přiřaďte CardStack objekt do slotu {i}");
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
        if (playerHand == null) { Debug.LogError("[CardManager] PlayerHand je null!"); return; }

        // Build a fresh 32-card deck for the new hand. Done here (not in Awake) so a
        // new round without a scene reload starts full, and so CardSpriteManager has
        // already loaded its sprites by the time the Cards are created.
        InitializeDeck();

        // Reset transient rule state so a Seven/Queen/Ace effect can't leak into a new hand.
        GameSession.I.Rules?.ResetRoundState();

        // Vyčistit ruce a odhazovací balíček
        playerHand.Clear();
        discard.Clear();
        
        // Vyčistit AI hand stacky
        if (aiHands != null)
        {
            foreach (var aiHand in aiHands)
            {
                if (aiHand != null) aiHand.Clear();
            }
        }
        
        // Vyčistit ruce všech hráčů
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Rozdat karty
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            // Karta pro každého hráče (včetně human)
            for (int j = 0; j < GameSession.I.Players.Count; j++)
            {
                Card card = deck.DrawCard() as Card;
                if (card != null)
                {
                    var player = GameSession.I.Players[j];
                    player.AddCard(card);
                    
                    // Pro human hráče také přidat do playerHand stacku pro UI
                    if (j == 0) // Human player
                    {
                        playerHand.AddCard(card);
                    }
                    // Pro AI hráče přidat do odpovídajícího AI hand stacku
                    else if (aiHands != null && j-1 < aiHands.Length && aiHands[j-1] != null)
                    {
                        aiHands[j-1].AddCard(card);
                    }
                    else
                    {
                        Debug.LogError($"[CardManager] Nemohu přidat kartu do AI hand {j-1}: aiHands je {(aiHands == null ? "null" : "není null")}, délka je {(aiHands == null ? "N/A" : aiHands.Length.ToString())}");
                    }
                }
                else
                {
                    Debug.LogError($"[CardManager] Chyba: nemohu rozdat kartu pro hráče {j}");
                }
            }
        }
        
        // Po rozdání karet otočit jednu kartu nahoru do odhazovacího balíčku
        Card topCard = deck.DrawCard() as Card;
        if (topCard != null)
        {
            discard.AddCard(topCard);
        }
        else
        {
            Debug.LogError("[CardManager] Chyba: nemohu otočit kartu nahoru - balíček je prázdný!");
        }

#if UNITY_EDITOR
        AssertCardCount("deal");
#endif
    }
    
    // Hráč odehrál kartu
    public void PlayCard(Player player, Card card)
    {
        // Kontrola, zda hráč má kartu v ruce
        if (!player.hand.Contains(card))
        {
            Debug.LogWarning($"[CardManager] Hráč {player.Name} nemá kartu {card} v ruce!");
            return;
        }
        
        // Kontrola, zda je karta hratelná
        if (!CanPlayCard(card))
        {
            Debug.LogWarning($"[CardManager] Karta {card} není hratelná!");
            return;
        }
        
        // Pro human hráče nejdřív odebrat z playerHand stacku a UI
        if (player.IsHuman)
        {
            playerHand.RemoveCard(card);
            // Také odebrat z UI komponenty
            if (playerHandUI != null)
            {
                playerHandUI.RemoveCard(card);
            }
            else
            {
                Debug.LogWarning("[CardManager] playerHandUI je null - karta nebyla odebrána z UI!");
            }
        }
        // Pro AI hráče nejdřív odebrat z odpovídajícího AI hand stacku
        else
        {
            int aiIndex = player.Id - 1; // AI hráči mají ID 1, 2, 3...
            if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
            {
                aiHands[aiIndex].RemoveCard(card);
            }
        }
        
        // Odehrát kartu
        player.RemoveCard(card);
        discard.AddCard(card);


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
    
    // AI odehrá kartu (deprecated - nyní se používá GameplayState)
    public void PlayAICard(Player aiPlayer)
    {
        if (aiPlayer.hand.Count > 0)
        {
            // Najít platnou kartu
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
    
    // Získá vrchní kartu z odhazovacího balíčku
    public Card GetTopDiscardCard()
    {
        if (discard?.cards != null && discard.cards.Count > 0)
        {
            return discard.cards[discard.cards.Count - 1];
        }
        return null;
    }
    
    // Najde hratelnou kartu v ruce hráče
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

        Card last = null;
        for (int i = 0; i < count; i++) last = DrawOneForPlayer(player);
#if UNITY_EDITOR
        AssertCardCount("draw");
#endif
        return last;
    }

    // Lízne jednu kartu pro hráče
    Card DrawOneForPlayer(Player player)
    {
        // Když dojde balíček, doplnit ho z odhazovacího (kromě vrchní karty).
        if (deck.IsEmpty)
        {
            ReshuffleDiscardIntoDeck();
        }

        Card drawnCard = deck.DrawCard() as Card;

        if (drawnCard != null)
        {
            // Pro human hráče nejdřív přidat do UI s CardIn animací
            if (player.IsHuman)
            {
                // Přidat kartu do UI s CardIn animací DŘÍV než do dat
                if (playerHandUI != null)
                {
                    playerHandUI.AddCardToHand(drawnCard);
                }
                else
                {
                    Debug.LogWarning("[CardManager] playerHandUI je null - CardIn animace se nespustí");
                }
                
                // Pak přidat do dat
                playerHand.AddCard(drawnCard);
            }
            // Pro AI hráče přidat do odpovídajícího AI hand stacku
            else
            {
                int aiIndex = player.Id - 1; // AI hráči mají ID 1, 2, 3...
                if (aiHands != null && aiIndex >= 0 && aiIndex < aiHands.Length && aiHands[aiIndex] != null)
                {
                    aiHands[aiIndex].AddCard(drawnCard);
                }
                
                // Přidat kartu do AI UI s CardIn animací
                if (aiHandUIs != null && aiIndex >= 0 && aiIndex < aiHandUIs.Length && aiHandUIs[aiIndex] != null)
                {
                    aiHandUIs[aiIndex].AddCard(drawnCard);
                }
                else
                {
                    Debug.LogWarning($"[CardManager] aiHandUIs[{aiIndex}] je null - CardIn animace se nespustí");
                }
            }
            
            // Přidat kartu do dat hráče (pro všechny hráče)
            player.AddCard(drawnCard);
            
        }
        else
        {
            // Deck empty and discard had nothing to recycle (only its top card):
            // a legal late-game exhaustion, not an error.
            Debug.LogWarning("[CardManager] Balíček i odhazovací jsou vyčerpané - nelze líznout");
        }

        return drawnCard;
    }
    
    // Recycle the discard pile (minus its top card) back into the Core deck (S6.1).
    void ReshuffleDiscardIntoDeck()
    {
        if (discard.cards.Count <= 1)
        {
            Debug.LogWarning("[CardManager] Nelze přehodit - odhazovací balíček má jen jednu nebo žádnou kartu");
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
        // Přesunout všechny karty zpět do balíčku
        deck.AddCards(discard.cards);
        deck.AddCards(playerHand.cards);
        
        // Přesunout karty ze všech hráčů
        foreach (var player in GameSession.I.Players)
        {
            deck.AddCards(player.hand);
        }
        
        discard.Clear();
        playerHand.Clear();
        
        // Vyčistit ruce hráčů
        foreach (var player in GameSession.I.Players)
        {
            player.ClearHand();
        }
        
        // Zamíchat balíček
        deck.Shuffle();
    }
    
    // Kontrola, zda je konec hry
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
        return playersWithCards <= 1; // Konec hry když zůstane max 1 hráč s kartami
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

    // Najít vítěze
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
