using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<Card> cards = new List<Card>();
    
    // Událost pro změnu karet
    public System.Action OnCardsChanged;
    
    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;
    
    public void AddCard(Card card)
    {
        cards.Add(card);
        OnCardsChanged?.Invoke();
    }
    
    public void AddCards(List<Card> newCards)
    {
        cards.AddRange(newCards);
        OnCardsChanged?.Invoke();
    }
    
    public Card DrawCard()
    {
        if (IsEmpty) return null;
        
        Card card = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);
        OnCardsChanged?.Invoke();
        return card;
    }
    
    public List<Card> DrawCards(int count)
    {
        List<Card> drawnCards = new List<Card>();
        for (int i = 0; i < count && !IsEmpty; i++)
        {
            drawnCards.Add(DrawCard());
        }
        return drawnCards;
    }
    
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        OnCardsChanged?.Invoke();
    }
    
    public void Clear()
    {
        cards.Clear();
        OnCardsChanged?.Invoke();
    }
    
    public void RemoveCard(Card card)
    {
        if (cards.Remove(card))
        {
            OnCardsChanged?.Invoke();
        }
    }
}
