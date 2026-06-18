using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
	public TMP_Text nameText, cashText, betText;
	public Image avatarImage;
	public AvatarStateView stateView;
	public Animator potAnimator; // Animator for pot animation
	public Animator cashAnimator; // Animator for cash animation

	Player bound;
	public Player Bound => bound; // For debug
	int lastCash;
	CanvasGroup canvasGroup;

	void Awake()
	{
		// Used to hide the bar on elimination without deactivating the GameObject
		// (which would stop its coroutines / event refresh).
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
	}

	public void Bind(Player p)
	{
		if (bound != null) bound.Changed -= OnChanged;
		bound = p;
		if (bound != null)
		{
			bound.Changed += OnChanged;
			lastCash = bound.Cash;
		}
		Refresh();
	}

	void OnDestroy()
	{
		if (bound != null) bound.Changed -= OnChanged;
	}

	void OnChanged(Player _) => Refresh();

	void Refresh()
	{
		if (bound == null) return;

		if (nameText) nameText.text = bound.Name;
		
		// Animated Cash changes
		if (cashText && bound.Cash != lastCash)
		{
			
			// Start cash animation IMMEDIATELY (sound via Animation Event)
			if (cashAnimator != null)
			{
				cashAnimator.SetTrigger("CashChanged");
			}
			else
			{
				Debug.LogWarning($"[PlayerUI] CashAnimator is null for {bound.Name}");
			}
			
			// Then animate the number (no sound)
			StartCoroutine(AnimateNumber(cashText, lastCash, bound.Cash, 0.5f));
			lastCash = bound.Cash;
		}
		
		if (avatarImage)
		{
			if (bound.Avatar)
			{
				avatarImage.enabled = true;
				avatarImage.sprite = bound.Avatar;
			}
			else
			{
				avatarImage.enabled = false;
			}
		}

		if (stateView)
		{
			stateView.SetActive(bound.IsActive);
		}

		// Elimination: player with no money disappears (bar hidden, component keeps running).
		if (canvasGroup)
		{
			bool alive = bound.Cash > 0;
			canvasGroup.alpha = alive ? 1f : 0f;
			canvasGroup.blocksRaycasts = alive;
		}
	}

	// Animate number from startValue to endValue
	IEnumerator AnimateNumber(TMP_Text text, int startValue, int endValue, float duration)
	{
		Color originalColor = text.color;
		Color targetColor = endValue > startValue ? Color.green : Color.red;
		
		// Sound plays IMMEDIATELY via Animation Event, not after animation finishes
		
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float progress = elapsed / duration;
			
			// Number animation
			int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, progress));
			text.text = currentValue.ToString();
			
			// Colour animation
			text.color = Color.Lerp(originalColor, targetColor, progress);
			
			yield return null;
		}
		
		text.text = endValue.ToString();
		text.color = originalColor;
		
	}

	// Optional: change avatar directly from UI (does not overwrite Player object)
	public void SetAvatar(Sprite sprite)
	{
		if (avatarImage)
		{
			avatarImage.sprite = sprite;
			avatarImage.enabled = (sprite != null);
		}
	}
}
