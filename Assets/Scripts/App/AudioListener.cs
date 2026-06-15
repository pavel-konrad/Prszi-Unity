using UnityEngine;
using System.Collections;

public class AudioListener : MonoBehaviour
{
    [Header("Bet Sounds")]
    public AudioClip betPlaceSound;
    public AudioClip cashDecreaseSound;
    public AudioClip cashIncreaseSound;
    
    [Header("UI Sounds")]
    public AudioClip popSound;
    public AudioClip modalOpenSound;    // Zvuk otevření modalu
    public AudioClip modalCloseSound;   // Zvuk zavření modalu
    public AudioClip modalButtonSound;  // Zvuk kliknutí na tlačítko v modalu
    
    [Header("Card Sounds")]
    public AudioClip cardDealtSound;
    public AudioClip cardPlayedSound;
    public AudioClip cardSelectedSound;  // Nový zvuk pro výběr karet
    public AudioClip cardSwipedSound;    // Nový zvuk pro swipe karet
    
    [Header("Player Turn Sounds")]
    public AudioClip playerTurnEndedSound;
    public AudioClip playerTurnStartedSound;
    
    [Header("Game Sounds")]
    public AudioClip gameStartSound;
    public AudioClip gameWinSound;
    public AudioClip gameLoseSound;
    
    [Header("Settings")]
    public float betSoundVolume = 0.7f;
    public float cashSoundVolume = 0.5f;
    public float cardSoundVolume = 0.6f;
    public float gameSoundVolume = 0.8f;

    // AudioSource komponenty pro různé typy zvuků
    private AudioSource betAudioSource;
    private AudioSource cashAudioSource;
    private AudioSource cardAudioSource;
    private AudioSource gameAudioSource;

    void Awake()
    {
        // Vytvořit AudioSource komponenty
        betAudioSource = gameObject.AddComponent<AudioSource>();
        cashAudioSource = gameObject.AddComponent<AudioSource>();
        cardAudioSource = gameObject.AddComponent<AudioSource>();
        gameAudioSource = gameObject.AddComponent<AudioSource>();
        
        // Nastavit vlastnosti
        betAudioSource.playOnAwake = false;
        cashAudioSource.playOnAwake = false;
        cardAudioSource.playOnAwake = false;
        gameAudioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        // Přihlásit se k událostem
        AudioEvents.OnBetPlaced += PlayBetPlace;
        AudioEvents.OnCashDecreased += PlayCashDecrease;
        AudioEvents.OnCashIncreased += PlayCashIncrease;
        AudioEvents.OnPopAnimation += PlayPopAnimation;
        AudioEvents.OnModalOpen += PlayModalOpen;
        AudioEvents.OnModalClose += PlayModalClose;
        AudioEvents.OnModalButtonClick += PlayModalButtonClick;
        AudioEvents.OnCardDealt += PlayCardDealt;
        AudioEvents.OnCardPlayed += PlayCardPlayed;
        AudioEvents.OnCardSelected += PlayCardSelected;
        AudioEvents.OnCardDeselected += PlayCardDeselected;
        AudioEvents.OnCardSwiped += PlayCardSwiped;  // Nová událost pro swipe
        AudioEvents.OnPlayerTurnEnded += PlayPlayerTurnEnded;
        AudioEvents.OnPlayerTurnStarted += PlayPlayerTurnStarted;
        AudioEvents.OnGameStart += PlayGameStart;
        AudioEvents.OnGameWin += PlayGameWin;
        AudioEvents.OnGameLose += PlayGameLose;
    }

    void OnDisable()
    {
        // Odhlásit se z událostí
        AudioEvents.OnBetPlaced -= PlayBetPlace;
        AudioEvents.OnCashDecreased -= PlayCashDecrease;
        AudioEvents.OnCashIncreased -= PlayCashIncrease;
        AudioEvents.OnPopAnimation -= PlayPopAnimation;
        AudioEvents.OnModalOpen -= PlayModalOpen;
        AudioEvents.OnModalClose -= PlayModalClose;
        AudioEvents.OnModalButtonClick -= PlayModalButtonClick;
        AudioEvents.OnCardDealt -= PlayCardDealt;
        AudioEvents.OnCardPlayed -= PlayCardPlayed;
        AudioEvents.OnCardSelected -= PlayCardSelected;
        AudioEvents.OnCardDeselected -= PlayCardDeselected;
        AudioEvents.OnCardSwiped -= PlayCardSwiped;  // Nová událost pro swipe
        AudioEvents.OnPlayerTurnEnded -= PlayPlayerTurnEnded;
        AudioEvents.OnPlayerTurnStarted -= PlayPlayerTurnStarted;
        AudioEvents.OnGameStart -= PlayGameStart;
        AudioEvents.OnGameWin -= PlayGameWin;
        AudioEvents.OnGameLose -= PlayGameLose;
    }

    void PlayPopAnimation()
    {
        if (popSound != null && betAudioSource != null)
        {
            betAudioSource.clip = popSound;
            betAudioSource.volume = betSoundVolume;
            betAudioSource.Play();
        }
    }

    void PlayModalOpen()
    {
        if (modalOpenSound != null && betAudioSource != null)
        {
            betAudioSource.clip = modalOpenSound;
            betAudioSource.volume = betSoundVolume;
            betAudioSource.Play();
        }
    }

    void PlayModalClose()
    {
        if (modalCloseSound != null && betAudioSource != null)
        {
            betAudioSource.clip = modalCloseSound;
            betAudioSource.volume = betSoundVolume;
            betAudioSource.Play();
        }
    }

    void PlayModalButtonClick()
    {
        if (modalButtonSound != null && betAudioSource != null)
        {
            betAudioSource.clip = modalButtonSound;
            betAudioSource.volume = betSoundVolume;
            betAudioSource.Play();
        }
    }

    void PlayBetPlace()
    {
        if (betPlaceSound != null && betAudioSource != null)
        {
            betAudioSource.clip = betPlaceSound;
            betAudioSource.volume = betSoundVolume;
            betAudioSource.Play();
        }
    }

    void PlayCashDecrease(float duration)
    {
        if (cashDecreaseSound != null && cashAudioSource != null)
        {
            StartCoroutine(PlayCashSoundForDuration(cashDecreaseSound, duration));
        }
    }

    void PlayCashIncrease(float duration)
    {
        if (cashIncreaseSound != null && cashAudioSource != null)
        {
            StartCoroutine(PlayCashSoundForDuration(cashIncreaseSound, duration));
        }
    }

    // Přehrát zvuk pouze po určitou dobu
    IEnumerator PlayCashSoundForDuration(AudioClip clip, float duration)
    {
        cashAudioSource.clip = clip;
        cashAudioSource.volume = cashSoundVolume;
        cashAudioSource.Play();
        
        yield return new WaitForSeconds(duration);
        
        cashAudioSource.Stop();
    }

    void PlayCardDealt()
    {
        if (cardDealtSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = cardDealtSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }

    void PlayCardPlayed()
    {
        if (cardPlayedSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = cardPlayedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }
    
    void PlayCardSelected()
    {
        if (cardSelectedSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = cardSelectedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
        else if (popSound != null && cardAudioSource != null)
        {
            // Fallback na původní zvuk pokud není nastaven nový
            cardAudioSource.clip = popSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }
    
    void PlayCardSwiped()
    {
        if (cardSwipedSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = cardSwipedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
        else if (cardPlayedSound != null && cardAudioSource != null)
        {
            // Fallback na zvuk zahrání karty pokud není nastaven nový
            cardAudioSource.clip = cardPlayedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }
    
    void PlayPlayerTurnEnded()
    {
        if (playerTurnEndedSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = playerTurnEndedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
        else if (popSound != null && cardAudioSource != null)
        {
            // Fallback na popSound pokud není nastaven nový
            cardAudioSource.clip = popSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }
    
    void PlayPlayerTurnStarted()
    {
        if (playerTurnStartedSound != null && cardAudioSource != null)
        {
            cardAudioSource.clip = playerTurnStartedSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
        else if (popSound != null && cardAudioSource != null)
        {
            // Fallback na popSound pokud není nastaven nový
            cardAudioSource.clip = popSound;
            cardAudioSource.volume = cardSoundVolume;
            cardAudioSource.Play();
        }
    }
    
    void PlayCardDeselected()
    {
        // Tichý nebo žádný zvuk pro zrušení výběru
        // Můžeme použít velmi tichý popSound nebo nic
    }

    void PlayGameStart()
    {
        if (gameStartSound != null && gameAudioSource != null)
        {
            gameAudioSource.clip = gameStartSound;
            gameAudioSource.volume = gameSoundVolume;
            gameAudioSource.Play();
        }
    }

    void PlayGameWin()
    {
        if (gameWinSound != null && gameAudioSource != null)
        {
            gameAudioSource.clip = gameWinSound;
            gameAudioSource.volume = gameSoundVolume;
            gameAudioSource.Play();
        }
    }

    void PlayGameLose()
    {
        if (gameLoseSound != null && gameAudioSource != null)
        {
            gameAudioSource.clip = gameLoseSound;
            gameAudioSource.volume = gameSoundVolume;
            gameAudioSource.Play();
        }
    }
}
