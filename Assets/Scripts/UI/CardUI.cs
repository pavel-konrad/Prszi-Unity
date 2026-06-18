using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.Animations; // For AnimatorControllerParameterType

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Card Display")]
    public Image cardImage;
    public TextMeshProUGUI cardText;
    public TextMeshProUGUI valueText; // For card value (7, 8, 9, 10, J, Q, K, A)
    public TextMeshProUGUI suitText;  // For card suit (Hearts, Diamonds, Clubs, Spades)
    
    [Header("Card Data")]
    public Card card;
    
    [Header("Animation")]
    public Animator cardAnimator; // Animator for card animations
    
    [Header("Interaction")]
    public bool isInteractable = true; // Can we play the card?
    public bool isCharged = false; // Is the card "charged" (slid out)?
    
    [Header("Swipe Detection")]
    public float minSwipeDistance = 80f; // Minimum distance for a swipe (larger for mobile)
    public float maxSwipeTime = 1.0f; // Maximum time for a swipe (longer for mobile)
    public float maxTapDistance = 30f; // Maximum distance for a tap (so it doesn't trigger a swipe)
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.8f, 0.9f, 1f, 1f); // Light blue
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grey
    public Color pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Light grey
    
    // Event for card click
    public System.Action<Card> OnCardClicked;
    public System.Action<Card> OnCardPressed;
    public System.Action<Card> OnCardReleased;
    public System.Action<Card> OnCardCharged; // Card charged
    public System.Action<Card> OnCardDischarged; // Card discharged
    public System.Action<Card> OnCardSwipedUp; // Card swiped up (to discard)
    public System.Action<Card> OnCardSwipedUpComplete; // Swipe animation finished
    
    private bool isPressed = false;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Swipe detection
    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTouching = false;
    private bool wasSwipe = false; // Flag to distinguish tap vs swipe
    private bool wasTap = false; // Flag pro detekci tap
    private bool shouldToggleCharge = true; // Flag for deferred charge toggle
    
    void Awake()
    {
        // If components not assigned, find them
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }
        
        if (cardText == null)
        {
            cardText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Look for specific TextMeshPro components (optional)
        if (valueText == null)
        {
            Transform valueTransform = transform.Find("value");
            if (valueTransform != null)
            {
                valueText = valueTransform.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (suitText == null)
        {
            Transform suitTransform = transform.Find("suit");
            if (suitTransform != null)
            {
                suitText = suitTransform.GetComponent<TextMeshProUGUI>();
            }
        }
        
        // Find or create Animator
        if (cardAnimator == null)
        {
            cardAnimator = GetComponent<Animator>();
            if (cardAnimator == null)
            {
                cardAnimator = gameObject.AddComponent<Animator>();
                Debug.LogWarning("[CardUI] Animator not found, a new one was created. Don't forget to assign an Animator Controller!");
            }
        }
        
        // Add AnimationEventReceiver if missing
        AnimationEventReceiver.EnsureComponent(gameObject);
        
        // Store original values
        originalScale = transform.localScale;
        if (cardImage != null)
        {
            originalColor = cardImage.color;
        }
    }
    
    public void SetCard(Card newCard)
    {
        card = newCard;
        UpdateDisplay();
        
        // Do NOT trigger CardIn here — only AddCardToHand() for new cards
        // SetCard is called on all cards in UpdateHand(), not just new ones
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        UpdateVisualState();
    }
    
    // SetSelected is now an alias for SetCharged (for compatibility)
    public void SetSelected(bool selected)
    {
        SetCharged(selected);
    }
    
    public void SetCharged(bool charged)
    {
        isCharged = charged;
        UpdateVisualState();
        
        // Raise event
        if (charged)
        {
            OnCardCharged?.Invoke(card);
            UIEvents.TriggerCardCharged(card);
        }
        else
        {
            OnCardDischarged?.Invoke(card);
            UIEvents.TriggerCardDischarged(card);
        }
    }
    
    void UpdateDisplay()
    {
        if (card == null) 
        {
            Debug.LogWarning("[CardUI] Card is null in UpdateDisplay");
            return;
        }
        
        
        // Set card sprite
        if (cardImage != null && card.cardSprite != null)
        {
            cardImage.sprite = card.cardSprite;
        }
        else
        {
            Debug.LogWarning($"[CardUI] Cannot set sprite - cardImage: {(cardImage != null ? "OK" : "NULL")}, cardSprite: {(card.cardSprite != null ? "OK" : "NULL")}");
        }
        
        // Set card text (fallback when no sprite)
        if (cardText != null)
        {
            cardText.text = $"{card.rank}\n{card.suit}";
        }
        
        // Set value text
        if (valueText != null)
        {
            valueText.text = card.rank.ToString();
        }
        
        // Set suit text
        if (suitText != null)
        {
            suitText.text = card.suit.ToString();
        }
        
        UpdateVisualState();
    }
    
    void UpdateVisualState()
    {
        if (cardImage == null) return;
        
        Color targetColor = normalColor;
        
        if (!isInteractable)
        {
            targetColor = disabledColor;
        }
        else if (isCharged)
        {
            targetColor = selectedColor; // Charged card is blue
        }
        else if (isPressed)
        {
            targetColor = pressedColor;
        }
        
        cardImage.color = targetColor;
        
        // Set animator parameters only when animator has controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Check parameter exists before setting
            bool hasIsCharged = false;
            foreach (var param in cardAnimator.parameters)
            {
                if (param.name == "IsCharged" && param.type == AnimatorControllerParameterType.Bool)
                {
                    hasIsCharged = true;
                    break;
                }
            }
            
            if (hasIsCharged)
            {
                cardAnimator.SetBool("IsCharged", isCharged);
            }
            else
            {
                Debug.LogWarning("[CardUI] Parameter 'IsCharged' does not exist in the Animator Controller");
            }
        }
        else if (cardAnimator != null && cardAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[CardUI] Animator has no Animator Controller assigned");
        }
        else if (cardAnimator == null)
        {
            Debug.LogWarning("[CardUI] Animator is null!");
        }
    }
    
    // IPointerDownHandler for press (works on web and mobile)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        isPressed = true;
        isTouching = true;
        wasSwipe = false; // Reset swipe flag
        wasTap = false; // Reset tap flag
        shouldToggleCharge = true; // Reset toggle flag
        touchStartPos = eventData.position;
        touchStartTime = Time.time;
        
        UpdateVisualState();
        
        // Play press animation only when animator has controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Check whether trigger exists
            bool hasPressTrigger = false;
            foreach (var param in cardAnimator.parameters)
            {
                if (param.name == "Press" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasPressTrigger = true;
                    break;
                }
            }
            
            if (hasPressTrigger)
            {
                cardAnimator.SetTrigger("Press");
            }
        }
        
        // Raise eventi
        OnCardPressed?.Invoke(card);
        UIEvents.TriggerCardPressed(card);
    }
    
    // IPointerUpHandler for release (works on web and mobile)
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        isPressed = false;
        
        // Check swipe only if we were touching
        if (isTouching)
        {
            CheckForSwipe(eventData.position);
            isTouching = false;
        }
        
        UpdateVisualState();
        
        // Raise eventi
        OnCardReleased?.Invoke(card);
        UIEvents.TriggerCardReleased(card);
    }
    
    // IPointerClickHandler for click/tap (works on web and mobile)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        // If it was a swipe, do not perform tap action
        if (wasSwipe) 
        {
            return;
        }
        
        // On mobile use wasTap flag for better detection
        #if UNITY_ANDROID || UNITY_IOS
        if (!wasTap)
        {
            return;
        }
        #endif
        
        // Play click animation only when animator has controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Check whether trigger exists
            bool hasClickTrigger = false;
            foreach (var param in cardAnimator.parameters)
            {
                if (param.name == "Click" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasClickTrigger = true;
                    break;
                }
            }
            
            if (hasClickTrigger)
            {
                cardAnimator.SetTrigger("Click");
            }
            else
            {
                Debug.LogWarning("[CardUI] Trigger 'Click' does not exist in the Animator Controller");
            }
        }
        
        // Toggle card charge state only when it is not a swipe
        if (shouldToggleCharge)
        {
            SetCharged(!isCharged);
        }
        
        // Play charge/discharge animation only when state actually changes
        if (shouldToggleCharge && cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            if (!isCharged) // About to charge
            {
                // Check whether trigger exists
                bool hasChargedTrigger = false;
                foreach (var param in cardAnimator.parameters)
                {
                    if (param.name == "Charged" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasChargedTrigger = true;
                        break;
                    }
                }
                
                if (hasChargedTrigger)
                {
                    cardAnimator.SetTrigger("Charged");
                }
            }
            else // About to discharge
            {
                // Check whether trigger exists
                bool hasDischargedTrigger = false;
                foreach (var param in cardAnimator.parameters)
                {
                    if (param.name == "Discharged" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasDischargedTrigger = true;
                        break;
                    }
                }
                
                if (hasDischargedTrigger)
                {
                    cardAnimator.SetTrigger("Discharged");
                }
            }
        }
        
        // Raise callback after short delay (wait for animation) only when state actually changes
        if (shouldToggleCharge)
        {
            StartCoroutine(InvokeCardClickedAfterAnimation());
        }
    }
    
    private IEnumerator InvokeCardClickedAfterAnimation()
    {
        // Wait for animation to start
        yield return new WaitForSeconds(0.1f);
        
        // Raise callback
        OnCardClicked?.Invoke(card);
    }
    
    private void CheckForSwipe(Vector2 endPos)
    {
        float swipeTime = Time.time - touchStartTime;
        float swipeDistance = Vector2.Distance(touchStartPos, endPos);
        
        
        // Tap detection (short movement)
        if (swipeDistance <= maxTapDistance && swipeTime <= 0.3f)
        {
            wasTap = true;
            return; // Exit — it is a tap (charge/discharge)
        }
        
        // Swipe detection (longer movement) — only on charged card
        if (swipeDistance >= minSwipeDistance && swipeTime <= maxSwipeTime && isCharged)
        {
            // Check swipe direction (upward)
            Vector2 swipeDirection = (endPos - touchStartPos).normalized;
            float verticalComponent = swipeDirection.y;
            
            
            // Upward swipe (vertical > 0.5 means more up than sideways)
            if (verticalComponent > 0.5f)
            {
                wasSwipe = true; // Mark as swipe
                shouldToggleCharge = false; // Prevent charge toggle in OnPointerClick
                
                // Play swipe animation FIRST
                if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
                {
                    // Check whether trigger exists
                    bool hasSwipedUpTrigger = false;
                    foreach (var param in cardAnimator.parameters)
                    {
                        if (param.name == "SwipedUp" && param.type == AnimatorControllerParameterType.Trigger)
                        {
                            hasSwipedUpTrigger = true;
                            break;
                        }
                    }
                    
                    if (hasSwipedUpTrigger)
                    {
                        cardAnimator.SetTrigger("SwipedUp");
                    }
                }
                
                // Automatically discharge card after swipe
                SetCharged(false);
                
                // Raise events AFTER starting animation
                OnCardSwipedUp?.Invoke(card);
                UIEvents.TriggerCardSwipedUp(card);
                
                return; // Exit method after swipe
            }
        }
    }
    
    // Public methods for programmatic control
    public void SimulateClick()
    {
        if (isInteractable)
        {
            OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
    
    public void SimulatePress()
    {
        if (isInteractable)
        {
            OnPointerDown(new PointerEventData(EventSystem.current));
        }
    }
    
    public void SimulateRelease()
    {
        if (isInteractable)
        {
            OnPointerUp(new PointerEventData(EventSystem.current));
        }
    }
}
