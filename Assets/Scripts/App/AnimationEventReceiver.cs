using UnityEngine;

/// <summary>
/// SPRÁVNÁ PIPELINE PRO UI ZVUKY:
/// 1. UI animace → 2. Animation Event → 3. AnimationEventReceiver → 4. AudioEvents
/// 
/// Tento přístup zajišťuje:
/// - Synchronizaci zvuku s vizuální animací
/// - Lepší UX (zvuk přesně odpovídá vizuálnímu efektu)
/// - Snadnější údržbu (zvuky jsou v animacích, ne v kódu)
/// - Flexibilitu (můžeme snadno měnit timing zvuků v animačním editoru)
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    // Statická metoda pro automatické přidání komponenty
    public static AnimationEventReceiver EnsureComponent(GameObject target)
    {
        var receiver = target.GetComponent<AnimationEventReceiver>();
        if (receiver == null)
        {
            receiver = target.AddComponent<AnimationEventReceiver>();
        }
        return receiver;
    }
    
    // === ZÁKLADNÍ UI ZVUKY ===
    
    /// <summary>
    /// Zvuk pro pop animaci (obecný UI zvuk)
    /// </summary>
    public void OnPopAnimationStart()
    {
        AudioEvents.TriggerPopAnimation();
    }
    
    /// <summary>
    /// Zvuk pro hover efekt (stejný jako pop)
    /// </summary>
    public void OnHoverStart()
    {
        AudioEvents.TriggerPopAnimation(); // Stejný zvuk jako Pop
    }
    
    /// <summary>
    /// Zvuk pro focus efekt (stejný jako pop)
    /// </summary>
    public void OnFocusStart()
    {
        AudioEvents.TriggerPopAnimation(); // Stejný zvuk jako Pop
    }
    
    // === HRACÍ ZVUKY ===
    
    /// <summary>
    /// Zvuk pro umístění sázky
    /// </summary>
    public void OnBetPlacedStart()
    {
        AudioEvents.TriggerBetPlaced();
    }
    
    /// <summary>
    /// Zvuk pro rozdání karty
    /// </summary>
    public void OnCardDealtStart()
    {
        AudioEvents.TriggerCardDealt();
    }
    
    /// <summary>
    /// Zvuk pro odehrání karty
    /// </summary>
    public void OnCardPlayedStart()
    {
        AudioEvents.TriggerCardPlayed(); // Zvuk pro kliknutí na kartu
    }
    
    // === KARTA INTERAKCE ===
    
    /// <summary>
    /// Zvuk pro stisknutí karty (lehčí než kliknutí)
    /// </summary>
    public void OnCardPressStart()
    {
        AudioEvents.TriggerPopAnimation(); // Lehký zvuk pro stisknutí
    }
    
    /// <summary>
    /// Zvuk pro uvolnění karty
    /// </summary>
    public void OnCardReleaseStart()
    {
        // Tichý nebo žádný zvuk pro uvolnění
    }
    
    /// <summary>
    /// Zvuk pro výběr karty
    /// </summary>
    public void OnCardSelectStart()
    {
        AudioEvents.TriggerPopAnimation(); // Zvuk pro výběr karty
    }
    
    /// <summary>
    /// Zvuk pro zrušení výběru karty
    /// </summary>
    public void OnCardDeselectStart()
    {
        // Tichý nebo žádný zvuk pro zrušení výběru
    }
    
    /// <summary>
    /// Zvuk pro kliknutí na kartu
    /// </summary>
    public void OnCardClickStart()
    {
        AudioEvents.TriggerPopAnimation(); // Zvuk pro kliknutí
    }
    
    /// <summary>
    /// Zvuk pro nabití karty (vysunutí)
    /// </summary>
    public void OnCardChargedStart()
    {
        AudioEvents.TriggerCardSelected(); // Použít stejný zvuk jako pro výběr
    }
    
    /// <summary>
    /// Zvuk pro vybití karty (zajetí zpět)
    /// </summary>
    public void OnCardDischargedStart()
    {
        AudioEvents.TriggerCardDeselected(); // Použít stejný zvuk jako pro zrušení výběru
    }
    
    /// <summary>
    /// Zvuk pro swipe karty nahoru (do discard)
    /// </summary>
    public void OnCardSwipedUpStart()
    {
        AudioEvents.TriggerCardSwiped(); // Nový specifický zvuk pro swipe
    }
    
    /// <summary>
    /// Dokončení animace swipu karty - karta se může odstranit
    /// </summary>
    public void OnCardSwipedUpComplete()
    {
        // Tato metoda se volá z Animation Event na konci animace swipu
        // Karta se může nyní bezpečně odstranit z ruky
        Debug.Log("[AnimationEventReceiver] Animace swipu dokončena - karta se může odstranit");
        
        // Vyvolat událost dokončení animace
        var cardUI = GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.OnCardSwipedUpComplete?.Invoke(cardUI.card);
        }
    }
    
    // === AVATAR ZVUKY ===
    
    /// <summary>
    /// Zvuk pro aktivaci hráče
    /// </summary>
    public void OnPlayerActiveStart()
    {
        AudioEvents.TriggerPopAnimation(); // Použít existující zvuk pro aktivaci
    }
    
    // === MODAL ZVUKY ===
    
    /// <summary>
    /// Zvuk pro umístění sázky v modalu
    /// </summary>
    public void OnModalBetPlacedStart()
    {
        AudioEvents.TriggerBetPlaced();
    }
    
    /// <summary>
    /// Zvuk pro otevření modalu
    /// </summary>
    public void OnModalOpenStart()
    {
        AudioEvents.TriggerModalOpen(); // Specifický zvuk pro otevření modalu
    }
    
    /// <summary>
    /// Zvuk pro zavření modalu
    /// </summary>
    public void OnModalCloseStart()
    {
        AudioEvents.TriggerModalClose(); // Specifický zvuk pro zavření modalu
    }
    
    /// <summary>
    /// Zvuk pro kliknutí na tlačítko v modalu
    /// </summary>
    public void OnModalButtonClickStart()
    {
        AudioEvents.TriggerModalButtonClick(); // Specifický zvuk pro kliknutí na tlačítko
    }
    
    /// <summary>
    /// Zvuk pro výběr sázky v modalu
    /// </summary>
    public void OnModalBetSelectedStart()
    {
        AudioEvents.TriggerBetPlaced(); // Bet zvuk při výběru sázky
    }
    
    // === PENÍZE A CASH ===

    /// <summary>
    /// Zvuk pro přepočítávání peněz (cash/bet animace)
    /// </summary>
    public void OnCashInDecreaseStart()
    {
        AudioEvents.TriggerCashIncreased(0.5f); // Zvuk pro animaci peněz
    }
    
    /// <summary>
    /// Zvuk pro zvýšení cash (přibývání peněz)
    /// </summary>
    public void OnCashIncreasedStart()
    {
        AudioEvents.TriggerCashIncreased(0.5f);
    }
    
    /// <summary>
    /// Zvuk pro snížení cash (odečítání peněz)
    /// </summary>
    public void OnCashDecreasedStart()
    {
        AudioEvents.TriggerCashDecreased(0.5f);
    }
    
    // === HRACÍ STAVY ===
    
    /// <summary>
    /// Zvuk pro start hry
    /// </summary>
    public void OnGameStartStart()
    {
        AudioEvents.TriggerGameStart();
    }
    
    // Funkce pro zavření modalu po animaci
    public void OnModalCloseComplete()
    {
        // Najít a zavřít modal
        var modalManager = FindObjectOfType<ModalManager>();
        if (modalManager != null)
        {
            // Zavřít modal po dokončení animace
            if (modalManager.betModal != null)
            {
                modalManager.betModal.SetActive(false);
            }
        }
    }
}
