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
	int lastCash, lastStaked;

	public void Bind(Player p)
	{
		if (bound != null) bound.Changed -= OnChanged;
		bound = p;
		if (bound != null) 
		{
			bound.Changed += OnChanged;
			lastCash = bound.Cash;
			lastStaked = bound.Staked;
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
		
		// Bet text zobrazuje Staked když hráč už vsadil, jinak Bet
		if (betText)
		{
			if (bound.Staked > 0)
			{
				// Hráč už vsadil - zobrazit Staked
				if (bound.Staked != lastStaked)
				{
					// OKAMŽITĚ spustit animaci potu (zvuk se spustí přes Animation Event)
					if (potAnimator != null)
					{
						potAnimator.SetTrigger("PotAnimate");
					}
					else
					{
						Debug.LogWarning($"[PlayerUI] PotAnimator je null pro {bound.Name}");
					}
					
					// Pak spustit animaci čísla (bez zvuku)
					StartCoroutine(AnimateNumber(betText, lastStaked, bound.Staked, 0.5f));
					lastStaked = bound.Staked;
				}
			}
			else
			{
				// Hráč ještě nevsadil - zobrazit Bet
				betText.text = bound.Bet.ToString();
			}
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
