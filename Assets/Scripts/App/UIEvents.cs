using UnityEngine;
using UnityEngine.UI;
using System;

public static class UIEvents
{
    // Events for avatars
    public static event Action<Sprite> OnAvatarChanged;
    
    // Events for players
    public static event Action<Player> OnPlayerTurnStarted;
    public static event Action<Player> OnPlayerTurnEnded;
    
    // Events for UI animations
    public static event Action OnPotAnimate; // pot animation
    
    // Events for cross-platform input
    public static event Action<Vector2> OnScreenTapped; // Tap/click on screen
    public static event Action<Vector2> OnScreenPressed; // Press on screen
    public static event Action<Vector2> OnScreenReleased; // Release on screen
    
    // Events for card interactions
    public static event Action<Card> OnCardSelected;
    public static event Action<Card> OnCardDeselected;
    public static event Action<Card> OnCardPressed;
    public static event Action<Card> OnCardReleased;
    public static event Action<Card> OnCardCharged; // Card charged
    public static event Action<Card> OnCardDischarged; // Card discharged
    public static event Action<Card> OnCardSwipedUp; // Card swiped up (to discard)

    // Methods for raising events
    public static void TriggerAvatarChanged(Sprite avatar) => OnAvatarChanged?.Invoke(avatar);
    public static void TriggerPlayerTurnStarted(Player player) => OnPlayerTurnStarted?.Invoke(player);
    public static void TriggerPlayerTurnEnded(Player player) => OnPlayerTurnEnded?.Invoke(player);
    public static void TriggerPotAnimate() => OnPotAnimate?.Invoke();
    
    // Cross-platform input methods
    public static void TriggerScreenTapped(Vector2 position) => OnScreenTapped?.Invoke(position);
    public static void TriggerScreenPressed(Vector2 position) => OnScreenPressed?.Invoke(position);
    public static void TriggerScreenReleased(Vector2 position) => OnScreenReleased?.Invoke(position);
    
    // Card interactions
    public static void TriggerCardSelected(Card card) => OnCardSelected?.Invoke(card);
    public static void TriggerCardDeselected(Card card) => OnCardDeselected?.Invoke(card);
    public static void TriggerCardPressed(Card card) => OnCardPressed?.Invoke(card);
    public static void TriggerCardReleased(Card card) => OnCardReleased?.Invoke(card);
    public static void TriggerCardCharged(Card card) => OnCardCharged?.Invoke(card);
    public static void TriggerCardDischarged(Card card) => OnCardDischarged?.Invoke(card);
    public static void TriggerCardSwipedUp(Card card) => OnCardSwipedUp?.Invoke(card);
}
