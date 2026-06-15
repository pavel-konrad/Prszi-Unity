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
	
	void Awake()
	{
		Debug.Log($"[PlayerUI] Awake - potAnimator: {(potAnimator != null ? "OK" : "NULL")}, cashAnimator: {(cashAnimator != null ? "OK" : "NULL")}");
		if (potAnimator != null)
		{
			Debug.Log($"[PlayerUI] PotAnimator Controller: {(potAnimator.runtimeAnimatorController != null ? "OK" : "NULL")}");
		}
		if (cashAnimator != null)
		{
			Debug.Log($"[PlayerUI] CashAnimator Controller: {(cashAnimator.runtimeAnimatorController != null ? "OK" : "NULL")}");
		}
	}

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
		Debug.Log($"[PlayerUI] Refresh pro {bound.Name}: Cash={bound.Cash}, lastCash={lastCash}");

		if (nameText) nameText.text = bound.Name;
		
		// Animované změny pro Cash
		if (cashText && bound.Cash != lastCash)
		{
			Debug.Log($"[PlayerUI] Cash změna pro {bound.Name}: {lastCash} -> {bound.Cash}");
			Debug.Log($"[PlayerUI] cashText: {(cashText != null ? "OK" : "NULL")}");
			
			// OKAMŽITĚ spustit animaci cash (zvuk se spustí přes Animation Event)
			if (cashAnimator != null)
			{
				Debug.Log($"[PlayerUI] OKAMŽITĚ spouštím CashChanged trigger pro {bound.Name}");
				Debug.Log($"[PlayerUI] CashAnimator Controller: {(cashAnimator.runtimeAnimatorController != null ? "OK" : "NULL")}");
				
				// Zkontrolovat dostupné parametry
				Debug.Log($"[PlayerUI] CashAnimator parametry:");
				foreach (var param in cashAnimator.parameters)
				{
					Debug.Log($"[PlayerUI] - {param.name} ({param.type})");
				}
				
				cashAnimator.SetTrigger("CashChanged");
				Debug.Log($"[PlayerUI] CashChanged trigger OKAMŽITĚ spuštěn pro {bound.Name}");
				
				// Zkontrolovat aktuální stav
				Debug.Log($"[PlayerUI] Aktuální stav: {cashAnimator.GetCurrentAnimatorStateInfo(0).IsName("CashChanged")}");
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
						Debug.Log($"[PlayerUI] OKAMŽITĚ spouštím PotAnimate trigger pro {bound.Name}");
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
			Debug.Log($"[PlayerUI] Setting {bound.Name} stateView.SetActive({bound.IsActive})");
			stateView.SetActive(bound.IsActive);
		}
	}

	// Animace čísla od startValue do endValue
	IEnumerator AnimateNumber(TMP_Text text, int startValue, int endValue, float duration)
	{
		Debug.Log($"[PlayerUI] AnimateNumber začíná: {startValue} -> {endValue}, duration: {duration}");
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
		
		Debug.Log($"[PlayerUI] AnimateNumber dokončeno");
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
