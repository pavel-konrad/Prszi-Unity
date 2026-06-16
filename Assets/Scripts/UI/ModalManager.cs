using UnityEngine;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    [Header("Player Turn Modal")]
    public GameObject playerTurnModal;
    public Image playerTurnModalAvatar;
    public Animator playerTurnModalAnimator; // Animator pro player turn modal

    void OnEnable()
    {
        UIEvents.OnPlayerTurnStarted += SetupPlayerTurnModal;
        UIEvents.OnAvatarChanged += UpdateAllAvatars;
    }

    void OnDisable()
    {
        UIEvents.OnPlayerTurnStarted -= SetupPlayerTurnModal;
        UIEvents.OnAvatarChanged -= UpdateAllAvatars;
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
        var human = GameSession.I?.Human;
        if (human != null && playerTurnModalAvatar != null)
            SetupAvatar(playerTurnModalAvatar, human);
    }
}
