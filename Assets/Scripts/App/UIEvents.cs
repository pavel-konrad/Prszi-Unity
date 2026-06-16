using UnityEngine;
using UnityEngine.UI;
using System;

public static class UIEvents
{
    // Události pro avatary
    public static event Action<Sprite> OnAvatarChanged;
    
    // Události pro hráče
    public static event Action<Player> OnPlayerTurnStarted;
    public static event Action<Player> OnPlayerTurnEnded;
    
    // Události pro UI animace
    public static event Action OnPotAnimate; // animace potu
    
    // Události pro cross-platform input
    public static event Action<Vector2> OnScreenTapped; // Tap/kliknutí na obrazovku
    public static event Action<Vector2> OnScreenPressed; // Stisknutí na obrazovku
    public static event Action<Vector2> OnScreenReleased; // Uvolnění na obrazovce
    
    // Události pro karetní interakce
    public static event Action<Card> OnCardSelected;
    public static event Action<Card> OnCardDeselected;
    public static event Action<Card> OnCardPressed;
    public static event Action<Card> OnCardReleased;
    public static event Action<Card> OnCardCharged; // Karta se nabila
    public static event Action<Card> OnCardDischarged; // Karta se vybila
    public static event Action<Card> OnCardSwipedUp; // Karta byla swipnuta nahoru (do discard)

    // Metody pro vyvolání událostí
    public static void TriggerAvatarChanged(Sprite avatar) => OnAvatarChanged?.Invoke(avatar);
    public static void TriggerPlayerTurnStarted(Player player) => OnPlayerTurnStarted?.Invoke(player);
    public static void TriggerPlayerTurnEnded(Player player) => OnPlayerTurnEnded?.Invoke(player);
    public static void TriggerPotAnimate() => OnPotAnimate?.Invoke();
    
    // Cross-platform input metody
    public static void TriggerScreenTapped(Vector2 position) => OnScreenTapped?.Invoke(position);
    public static void TriggerScreenPressed(Vector2 position) => OnScreenPressed?.Invoke(position);
    public static void TriggerScreenReleased(Vector2 position) => OnScreenReleased?.Invoke(position);
    
    // Karetní interakce
    public static void TriggerCardSelected(Card card) => OnCardSelected?.Invoke(card);
    public static void TriggerCardDeselected(Card card) => OnCardDeselected?.Invoke(card);
    public static void TriggerCardPressed(Card card) => OnCardPressed?.Invoke(card);
    public static void TriggerCardReleased(Card card) => OnCardReleased?.Invoke(card);
    public static void TriggerCardCharged(Card card) => OnCardCharged?.Invoke(card);
    public static void TriggerCardDischarged(Card card) => OnCardDischarged?.Invoke(card);
    public static void TriggerCardSwipedUp(Card card) => OnCardSwipedUp?.Invoke(card);
}
