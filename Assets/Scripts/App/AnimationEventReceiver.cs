using UnityEngine;

/// <summary>
/// CORRECT PIPELINE FOR UI SOUNDS:
/// 1. UI animace → 2. Animation Event → 3. AnimationEventReceiver → 4. AudioEvents
/// 
/// This approach ensures:
/// - Sound synchronised with the visual animation
/// - Better UX (sound matches the visual effect exactly)
/// - Easier maintenance (sounds live in animations, not in code)
/// - Flexibility (easy to change sound timing in the animation editor)
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    // Static method to automatically add the component
    public static AnimationEventReceiver EnsureComponent(GameObject target)
    {
        var receiver = target.GetComponent<AnimationEventReceiver>();
        if (receiver == null)
        {
            receiver = target.AddComponent<AnimationEventReceiver>();
        }
        return receiver;
    }
    
    // === BASIC UI SOUNDS ===
    
    /// <summary>
    /// Sound for pop animation (general UI sound)
    /// </summary>
    public void OnPopAnimationStart()
    {
        AudioEvents.TriggerPopAnimation();
    }
    
    /// <summary>
    /// Sound for hover effect (same as pop)
    /// </summary>
    public void OnHoverStart()
    {
        AudioEvents.TriggerPopAnimation(); // Same sound as Pop
    }
    
    /// <summary>
    /// Sound for focus effect (same as pop)
    /// </summary>
    public void OnFocusStart()
    {
        AudioEvents.TriggerPopAnimation(); // Same sound as Pop
    }
    
    // === GAMEPLAY SOUNDS ===
    
    /// <summary>
    /// Sound for placing a bet
    /// </summary>
    public void OnBetPlacedStart()
    {
        AudioEvents.TriggerBetPlaced();
    }
    
    /// <summary>
    /// Sound for dealing a card
    /// </summary>
    public void OnCardDealtStart()
    {
        AudioEvents.TriggerCardDealt();
    }
    
    /// <summary>
    /// Sound for playing a card
    /// </summary>
    public void OnCardPlayedStart()
    {
        AudioEvents.TriggerCardPlayed(); // Sound for clicking a card
    }
    
    // === CARD INTERACTION ===
    
    /// <summary>
    /// Sound for pressing a card (lighter than click)
    /// </summary>
    public void OnCardPressStart()
    {
        AudioEvents.TriggerPopAnimation(); // Light sound for a press
    }
    
    /// <summary>
    /// Sound for releasing a card
    /// </summary>
    public void OnCardReleaseStart()
    {
        // Quiet or no sound for release
    }
    
    /// <summary>
    /// Sound for selecting a card
    /// </summary>
    public void OnCardSelectStart()
    {
        AudioEvents.TriggerPopAnimation(); // Sound for selecting a card
    }
    
    /// <summary>
    /// Sound for deselecting a card
    /// </summary>
    public void OnCardDeselectStart()
    {
        // Quiet or no sound for deselection
    }
    
    /// <summary>
    /// Sound for clicking a card
    /// </summary>
    public void OnCardClickStart()
    {
        AudioEvents.TriggerPopAnimation(); // Sound for a click
    }
    
    /// <summary>
    /// Sound for charging a card (slide out)
    /// </summary>
    public void OnCardChargedStart()
    {
        AudioEvents.TriggerCardSelected(); // Use the same sound as for selection
    }
    
    /// <summary>
    /// Sound for discharging a card (slide back)
    /// </summary>
    public void OnCardDischargedStart()
    {
        AudioEvents.TriggerCardDeselected(); // Use the same sound as for deselection
    }
    
    /// <summary>
    /// Sound for swiping a card up (to discard)
    /// </summary>
    public void OnCardSwipedUpStart()
    {
        AudioEvents.TriggerCardSwiped(); // New specific sound for swipe
    }
    
    /// <summary>
    /// End of card swipe animation — card may be removed
    /// </summary>
    public void OnCardSwipedUpComplete()
    {
        // This method is called from an Animation Event at the end of the swipe
        // Card may now be safely removed from the hand
        
        // Raise animation-complete event
        var cardUI = GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.OnCardSwipedUpComplete?.Invoke(cardUI.card);
        }
    }
    
    // === AVATAR SOUNDS ===
    
    /// <summary>
    /// Sound for player activation
    /// </summary>
    public void OnPlayerActiveStart()
    {
        AudioEvents.TriggerPopAnimation(); // Use existing sound for activation
    }
    
    // === MODAL SOUNDS ===
    
    /// <summary>
    /// Sound for placing a bet in the modal
    /// </summary>
    public void OnModalBetPlacedStart()
    {
        AudioEvents.TriggerBetPlaced();
    }
    
    /// <summary>
    /// Sound for opening the modal
    /// </summary>
    public void OnModalOpenStart()
    {
        AudioEvents.TriggerModalOpen(); // Specific sound for opening the modal
    }
    
    /// <summary>
    /// Sound for closing the modal
    /// </summary>
    public void OnModalCloseStart()
    {
        AudioEvents.TriggerModalClose(); // Specific sound for closing the modal
    }
    
    /// <summary>
    /// Sound for clicking a button in the modal
    /// </summary>
    public void OnModalButtonClickStart()
    {
        AudioEvents.TriggerModalButtonClick(); // Specific sound for a button click
    }
    
    /// <summary>
    /// Sound for selecting a bet in the modal
    /// </summary>
    public void OnModalBetSelectedStart()
    {
        AudioEvents.TriggerBetPlaced(); // Bet sound when selecting a bet
    }
    
    // === MONEY AND CASH ===

    /// <summary>
    /// Sound for counting money (cash/bet animation)
    /// </summary>
    public void OnCashInDecreaseStart()
    {
        AudioEvents.TriggerCashIncreased(0.5f); // Sound for the money animation
    }
    
    /// <summary>
    /// Sound for cash increase (money added)
    /// </summary>
    public void OnCashIncreasedStart()
    {
        AudioEvents.TriggerCashIncreased(0.5f);
    }
    
    /// <summary>
    /// Sound for cash decrease (money deducted)
    /// </summary>
    public void OnCashDecreasedStart()
    {
        AudioEvents.TriggerCashDecreased(0.5f);
    }
    
    // === GAMEPLAY STATES ===
    
    /// <summary>
    /// Sound for game start
    /// </summary>
    public void OnGameStartStart()
    {
        AudioEvents.TriggerGameStart();
    }
    
}
