using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.Animations; // Pro AnimatorControllerParameterType

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Card Display")]
    public Image cardImage;
    public TextMeshProUGUI cardText;
    public TextMeshProUGUI valueText; // Pro hodnotu karty (7, 8, 9, 10, J, Q, K, A)
    public TextMeshProUGUI suitText;  // Pro barvu karty (Hearts, Diamonds, Clubs, Spades)
    
    [Header("Card Data")]
    public Card card;
    
    [Header("Animation")]
    public Animator cardAnimator; // Animator pro animace karty
    
    [Header("Interaction")]
    public bool isInteractable = true; // Můžeme kartu hrát?
    public bool isCharged = false; // Je karta "nabitá" (vysunutá)?
    
    [Header("Swipe Detection")]
    public float minSwipeDistance = 80f; // Minimální vzdálenost pro swipe (větší pro mobil)
    public float maxSwipeTime = 1.0f; // Maximální čas pro swipe (delší pro mobil)
    public float maxTapDistance = 30f; // Maximální vzdálenost pro tap (aby se nespustil swipe)
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.8f, 0.9f, 1f, 1f); // Světle modrá
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Šedá
    public Color pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Světle šedá
    
    // Event pro kliknutí na kartu
    public System.Action<Card> OnCardClicked;
    public System.Action<Card> OnCardPressed;
    public System.Action<Card> OnCardReleased;
    public System.Action<Card> OnCardCharged; // Karta se nabila
    public System.Action<Card> OnCardDischarged; // Karta se vybila
    public System.Action<Card> OnCardSwipedUp; // Karta byla swipnuta nahoru (do discard)
    public System.Action<Card> OnCardSwipedUpComplete; // Animace swipu byla dokončena
    
    private bool isPressed = false;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Swipe detection
    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTouching = false;
    private bool wasSwipe = false; // Flag pro rozlišení tap vs swipe
    private bool wasTap = false; // Flag pro detekci tap
    private bool shouldToggleCharge = true; // Flag pro odložené přepnutí nabití
    
    void Awake()
    {
        // Pokud nemáme přiřazené komponenty, najdeme je
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }
        
        if (cardText == null)
        {
            cardText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Hledat specifické TextMeshPro komponenty (volitelné)
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
        
        // Najít nebo vytvořit Animator
        if (cardAnimator == null)
        {
            cardAnimator = GetComponent<Animator>();
            if (cardAnimator == null)
            {
                cardAnimator = gameObject.AddComponent<Animator>();
                Debug.LogWarning("[CardUI] Animator nebyl nalezen, byl vytvořen nový. Nezapomeňte přiřadit Animator Controller!");
            }
        }
        
        // Přidat AnimationEventReceiver pokud neexistuje
        AnimationEventReceiver.EnsureComponent(gameObject);
        Debug.Log($"[CardUI] AnimationEventReceiver zajištěn pro kartu: {gameObject.name}");
        
        // Uložit původní hodnoty
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
        
        // NESPOUŠTĚT CardIn animaci zde - to se spouští jen v AddCardToHand() pro nové karty
        // SetCard se volá na všech kartách při UpdateHand(), ne jen na nových
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        UpdateVisualState();
    }
    
    // SetSelected je nyní alias pro SetCharged (pro kompatibilitu)
    public void SetSelected(bool selected)
    {
        SetCharged(selected);
    }
    
    public void SetCharged(bool charged)
    {
        isCharged = charged;
        UpdateVisualState();
        
        // Vyvolat událost
        if (charged)
        {
            OnCardCharged?.Invoke(card);
            UIEvents.TriggerCardCharged(card);
            Debug.Log($"[CardUI] Karta {card?.rank} of {card?.suit} se NABILA");
        }
        else
        {
            OnCardDischarged?.Invoke(card);
            UIEvents.TriggerCardDischarged(card);
            Debug.Log($"[CardUI] Karta {card?.rank} of {card?.suit} se VYBILA");
        }
    }
    
    void UpdateDisplay()
    {
        if (card == null) 
        {
            Debug.LogWarning("[CardUI] Card je null v UpdateDisplay");
            return;
        }
        
        Debug.Log($"[CardUI] UpdateDisplay pro kartu: {card.rank} of {card.suit}");
        Debug.Log($"[CardUI] cardImage: {(cardImage != null ? "nalezen" : "null")}");
        Debug.Log($"[CardUI] card.cardSprite: {(card.cardSprite != null ? "nalezen" : "null")}");
        
        // Nastavit sprite karty
        if (cardImage != null && card.cardSprite != null)
        {
            cardImage.sprite = card.cardSprite;
            Debug.Log($"[CardUI] Sprite nastaven: {card.cardSprite.name}");
        }
        else
        {
            Debug.LogWarning($"[CardUI] Nelze nastavit sprite - cardImage: {(cardImage != null ? "OK" : "NULL")}, cardSprite: {(card.cardSprite != null ? "OK" : "NULL")}");
        }
        
        // Nastavit text karty (fallback pokud nemáme sprite)
        if (cardText != null)
        {
            cardText.text = $"{card.rank}\n{card.suit}";
            Debug.Log($"[CardUI] Text nastaven: {cardText.text}");
        }
        
        // Nastavit value text
        if (valueText != null)
        {
            valueText.text = card.rank.ToString();
        }
        
        // Nastavit suit text
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
            targetColor = selectedColor; // Nabitá karta má modrou barvu
        }
        else if (isPressed)
        {
            targetColor = pressedColor;
        }
        
        cardImage.color = targetColor;
        
        // Nastavit animator parametry pouze pokud má animator controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Zkontrolovat, jestli parametr existuje před nastavením
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
                Debug.Log($"[CardUI] Animator parametr IsCharged nastaven na: {isCharged}");
            }
            else
            {
                Debug.LogWarning("[CardUI] Parametr 'IsCharged' neexistuje v Animator Controller");
            }
        }
        else if (cardAnimator != null && cardAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[CardUI] Animator nemá přiřazený Animator Controller");
        }
        else if (cardAnimator == null)
        {
            Debug.LogWarning("[CardUI] Animator je null!");
        }
    }
    
    // Implementace IPointerDownHandler pro stisknutí (funguje na webu i mobilu)
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
        
        // Spustit animaci stisknutí pouze pokud má animator controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Zkontrolovat, jestli trigger existuje
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
        
        // Vyvolat události
        OnCardPressed?.Invoke(card);
        UIEvents.TriggerCardPressed(card);
    }
    
    // Implementace IPointerUpHandler pro uvolnění (funguje na webu i mobilu)
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        isPressed = false;
        
        // Zkontrolovat swipe pouze pokud jsme dotýkali
        if (isTouching)
        {
            CheckForSwipe(eventData.position);
            isTouching = false;
        }
        
        UpdateVisualState();
        
        // Vyvolat události
        OnCardReleased?.Invoke(card);
        UIEvents.TriggerCardReleased(card);
    }
    
    // Implementace IPointerClickHandler pro kliknutí/tap (funguje na webu i mobilu)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        // Pokud to byl swipe, neprovádět tap akci
        if (wasSwipe) 
        {
            Debug.Log($"[CardUI] Ignoruji tap - byl to swipe");
            return;
        }
        
        // Na mobilu používáme wasTap flag pro lepší detekci
        #if UNITY_ANDROID || UNITY_IOS
        if (!wasTap)
        {
            Debug.Log($"[CardUI] Ignoruji tap - nebyl detekován jako tap");
            return;
        }
        #endif
        
        // Spustit animaci kliknutí pouze pokud má animator controller
        if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            // Zkontrolovat, jestli trigger existuje
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
                Debug.Log($"[CardUI] Trigger 'Click' spuštěn");
            }
            else
            {
                Debug.LogWarning("[CardUI] Trigger 'Click' neexistuje v Animator Controller");
            }
        }
        
        // Přepnout stav nabití karty pouze pokud to není swipe
        if (shouldToggleCharge)
        {
            SetCharged(!isCharged);
        }
        
        // Spustit animaci nabití/vybití pouze pokud se skutečně mění stav
        if (shouldToggleCharge && cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
        {
            if (!isCharged) // Bude se nabíjet
            {
                // Zkontrolovat, jestli trigger existuje
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
            else // Bude se vybíjet
            {
                // Zkontrolovat, jestli trigger existuje
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
        
        // Vyvolat callback po krátké prodlevě (počkáme na animaci) pouze pokud se skutečně mění stav
        if (shouldToggleCharge)
        {
            StartCoroutine(InvokeCardClickedAfterAnimation());
        }
    }
    
    private IEnumerator InvokeCardClickedAfterAnimation()
    {
        // Počkat na začátek animace
        yield return new WaitForSeconds(0.1f);
        
        // Vyvolat callback
        OnCardClicked?.Invoke(card);
    }
    
    private void CheckForSwipe(Vector2 endPos)
    {
        float swipeTime = Time.time - touchStartTime;
        float swipeDistance = Vector2.Distance(touchStartPos, endPos);
        
        Debug.Log($"[CardUI] Touch check: distance={swipeDistance}, time={swipeTime}, minSwipe={minSwipeDistance}, maxTap={maxTapDistance}, isCharged={isCharged}");
        
        // Detekce tap (krátký pohyb)
        if (swipeDistance <= maxTapDistance && swipeTime <= 0.3f)
        {
            wasTap = true;
            Debug.Log($"[CardUI] Tap detekován - karta se nabije/vybije");
            return; // Ukončit - je to tap (nabití/vybití)
        }
        
        // Detekce swipe (delší pohyb) - pouze na nabité kartě
        if (swipeDistance >= minSwipeDistance && swipeTime <= maxSwipeTime && isCharged)
        {
            // Zkontrolovat směr swipu (nahoru)
            Vector2 swipeDirection = (endPos - touchStartPos).normalized;
            float verticalComponent = swipeDirection.y;
            
            Debug.Log($"[CardUI] Swipe direction: {swipeDirection}, vertical: {verticalComponent}");
            
            // Swipe nahoru (vertical > 0.5 znamená více nahoru než do stran)
            if (verticalComponent > 0.5f)
            {
                Debug.Log($"[CardUI] Swipe nahoru detekován! Karta {card?.rank} of {card?.suit} jde do discard");
                wasSwipe = true; // Označit jako swipe
                shouldToggleCharge = false; // Zabránit přepnutí nabití v OnPointerClick
                
                // Spustit animaci swipu PRVNÍ
                if (cardAnimator != null && cardAnimator.runtimeAnimatorController != null)
                {
                    // Zkontrolovat, jestli trigger existuje
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
                        Debug.Log($"[CardUI] Animace SwipedUp spuštěna pro kartu {card?.rank} of {card?.suit}");
                    }
                }
                
                // Automaticky vybít kartu po swipu
                SetCharged(false);
                
                // Vyvolat události PO spuštění animace
                OnCardSwipedUp?.Invoke(card);
                UIEvents.TriggerCardSwipedUp(card);
                
                return; // Ukončit metodu po swipu
            }
        }
        else if (swipeDistance >= minSwipeDistance && swipeTime <= maxSwipeTime && !isCharged)
        {
            Debug.Log($"[CardUI] Swipe detekován, ale karta není nabitá (isCharged={isCharged}) - ignoruji swipe");
        }
        
        // Pokud to není ani tap ani swipe, nechat kartu nabitou (pokud už je nabitá)
        if (isCharged)
        {
            Debug.Log($"[CardUI] Není ani tap ani swipe - karta zůstává nabitá");
        }
    }
    
    // Veřejné metody pro programové ovládání
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
