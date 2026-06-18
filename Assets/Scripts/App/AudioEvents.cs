using UnityEngine;
using System;

public static class AudioEvents
{
    // Events for bet sounds
    public static event Action OnBetPlaced;
    public static event Action<float> OnCashDecreased; // with animation duration
    public static event Action<float> OnCashIncreased; // with animation duration
    
    // Events for UI sounds
    public static event Action OnPopAnimation;
    public static event Action OnModalOpen;
    public static event Action OnModalClose;
    public static event Action OnModalButtonClick;
    
    // Events for card sounds
    public static event Action OnCardDealt;
    public static event Action OnCardPlayed;
    public static event Action OnCardSelected;
    public static event Action OnCardDeselected;
    public static event Action OnCardSwiped;  // New event for swipe
    
    // Events for game sounds
    public static event Action OnGameStart;
    public static event Action OnGameWin;
    public static event Action OnGameLose;
    
    // Events for player sounds
    public static event Action OnPlayerTurnEnded;
    public static event Action OnPlayerTurnStarted;

    // Methods for raising events
    public static void TriggerBetPlaced() => OnBetPlaced?.Invoke();
    public static void TriggerCashDecreased(float duration) => OnCashDecreased?.Invoke(duration);
    public static void TriggerCashIncreased(float duration) => OnCashIncreased?.Invoke(duration);
    public static void TriggerPopAnimation() => OnPopAnimation?.Invoke();
    public static void TriggerModalOpen() => OnModalOpen?.Invoke();
    public static void TriggerModalClose() => OnModalClose?.Invoke();
    public static void TriggerModalButtonClick() => OnModalButtonClick?.Invoke();
    
    public static void TriggerCardDealt() => OnCardDealt?.Invoke();
    public static void TriggerCardPlayed() => OnCardPlayed?.Invoke();
    public static void TriggerCardSelected() => OnCardSelected?.Invoke();
    public static void TriggerCardDeselected() => OnCardDeselected?.Invoke();
    public static void TriggerCardSwiped() => OnCardSwiped?.Invoke();  // New trigger for swipe
    
    public static void TriggerGameStart() => OnGameStart?.Invoke();
    public static void TriggerGameWin() => OnGameWin?.Invoke();
    public static void TriggerGameLose() => OnGameLose?.Invoke();
    
    public static void TriggerPlayerTurnEnded() => OnPlayerTurnEnded?.Invoke();
    public static void TriggerPlayerTurnStarted() => OnPlayerTurnStarted?.Invoke();
}
