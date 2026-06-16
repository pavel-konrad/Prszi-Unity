using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
	public TMP_Text nameText, cashText, betText;
	public Image avatarImage;
	public AvatarStateView stateView;
	public Animator potAnimator; // Animator pro pot animaci
	public Animator cashAnimator; // Animator pro cash animaci

	Player bound;
	public Player Bound => bound; // Pro debug
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
		
		// Animované změny pro Cash
		if (cashText && bound.Cash != lastCash)
		{
			
			// OKAMŽITĚ spustit animaci cash (zvuk se spustí přes Animation Event)
			if (cashAnimator != null)
			{
				cashAnimator.SetTrigger("CashChanged");
			}
			else
			{
				Debug.LogWarning($"[PlayerUI] CashAnimator je null pro {bound.Name}");
			}
			
			// Pak spustit animaci čísla (bez zvuku)
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

		// Eliminace: hráč bez peněz zmizí (bar zneviditelný, ale komponenta běží dál).
		if (canvasGroup)
		{
			bool alive = bound.Cash > 0;
			canvasGroup.alpha = alive ? 1f : 0f;
			canvasGroup.blocksRaycasts = alive;
		}
	}

	// Animace čísla od startValue do endValue
	IEnumerator AnimateNumber(TMP_Text text, int startValue, int endValue, float duration)
	{
		Color originalColor = text.color;
		Color targetColor = endValue > startValue ? Color.green : Color.red;
		
		// Zvuk se spustí OKAMŽITĚ přes Animation Event, ne až po dokončení animace
		
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float progress = elapsed / duration;
			
			// Animace čísla
			int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, progress));
			text.text = currentValue.ToString();
			
			// Animace barvy
			text.color = Color.Lerp(originalColor, targetColor, progress);
			
			yield return null;
		}
		
		text.text = endValue.ToString();
		text.color = originalColor;
		
	}

	// Volitelně: přímá změna avatara z UI (nepřepisuje Player objekt)
	public void SetAvatar(Sprite sprite)
	{
		if (avatarImage)
		{
			avatarImage.sprite = sprite;
			avatarImage.enabled = (sprite != null);
		}
	}
}
