using UnityEngine;
using System;

public static class AudioEvents
{
    // Události pro zvuky sázek
    public static event Action OnBetPlaced;
    public static event Action<float> OnCashDecreased; // s délkou animace
    public static event Action<float> OnCashIncreased; // s délkou animace
    
    // Události pro UI zvuky
    public static event Action OnPopAnimation;
    public static event Action OnModalOpen;
    public static event Action OnModalClose;
    public static event Action OnModalButtonClick;
    
    // Události pro zvuky karet
    public static event Action OnCardDealt;
    public static event Action OnCardPlayed;
    public static event Action OnCardSelected;
    public static event Action OnCardDeselected;
    public static event Action OnCardSwiped;  // Nová událost pro swipe
    
    // Události pro zvuky hry
    public static event Action OnGameStart;
    public static event Action OnGameWin;
    public static event Action OnGameLose;
    
    // Události pro zvuky hráčů
    public static event Action OnPlayerTurnEnded;
    public static event Action OnPlayerTurnStarted;

    // Metody pro vyvolání událostí
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
    public static void TriggerCardSwiped() => OnCardSwiped?.Invoke();  // Nový trigger pro swipe
    
    public static void TriggerGameStart() => OnGameStart?.Invoke();
    public static void TriggerGameWin() => OnGameWin?.Invoke();
    public static void TriggerGameLose() => OnGameLose?.Invoke();
    
    public static void TriggerPlayerTurnEnded() => OnPlayerTurnEnded?.Invoke();
    public static void TriggerPlayerTurnStarted() => OnPlayerTurnStarted?.Invoke();
}
