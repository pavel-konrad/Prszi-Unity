using UnityEngine;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    [Header("Bet Modal")]
    public GameObject betModal;
    public Image betModalAvatar;
    public Animator betModalAnimator; // Animator pro bet modal
    
    [Header("Player Turn Modal")]
    public GameObject playerTurnModal;
    public Image playerTurnModalAvatar;
    public Animator playerTurnModalAnimator; // Animator pro player turn modal
    
    [Header("Game Over Modal")]
    public GameObject gameOverModal;
    public Image gameOverModalAvatar;
    public Animator gameOverModalAnimator; // Animator pro game over modal
    
    [Header("Settings")]
    public bool autoSetupAvatars = true;

    void OnEnable()
    {
        // Přihlásit se k událostem
        UIEvents.OnBetModalOpened += SetupBetModal;
        UIEvents.OnBetModalClosed += CloseBetModal;
        UIEvents.OnPlayerTurnStarted += SetupPlayerTurnModal;
        UIEvents.OnAvatarChanged += UpdateAllAvatars;
    }

    void OnDisable()
    {
        // Odhlásit se z událostí
        UIEvents.OnBetModalOpened -= SetupBetModal;
        UIEvents.OnBetModalClosed -= CloseBetModal;
        UIEvents.OnPlayerTurnStarted -= SetupPlayerTurnModal;
        UIEvents.OnAvatarChanged -= UpdateAllAvatars;
    }

    void SetupBetModal(Player player)
    {
        if (betModal != null)
        {
            betModal.SetActive(true);
            SetupAvatar(betModalAvatar, player);
            
            // Spustit animaci otevření
            if (betModalAnimator != null)
            {
                betModalAnimator.SetTrigger("Open");
            }
        }
    }

    void CloseBetModal()
    {
        if (betModal != null)
        {
            // Spustit animaci zavření
            if (betModalAnimator != null)
            {
                betModalAnimator.SetTrigger("Close");
                // Modal se zavře po dokončení animace přes Animation Event
            }
            else
            {
                betModal.SetActive(false);
            }
        }
    }

    void SetupPlayerTurnModal(Player player)
    {
        if (playerTurnModal != null)
        {
            playerTurnModal.SetActive(true);
            SetupAvatar(playerTurnModalAvatar, player);
        }
    }

    void SetupAvatar(Image avatarImage, Player player)
    {
        if (avatarImage != null && player != null)
        {
            if (player.Avatar != null)
            {
                avatarImage.sprite = player.Avatar;
                avatarImage.enabled = true;
            }
            else
            {
                avatarImage.enabled = false;
            }
        }
    }

    void UpdateAllAvatars(Sprite newAvatar)
    {
        // Aktualizovat všechny avatary v modálních oknech
        var human = GameSession.I?.Human;
        if (human != null)
        {
            if (betModalAvatar != null) SetupAvatar(betModalAvatar, human);
            if (playerTurnModalAvatar != null) SetupAvatar(playerTurnModalAvatar, human);
            if (gameOverModalAvatar != null) SetupAvatar(gameOverModalAvatar, human);
        }
    }
}
