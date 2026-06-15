# SPEC — Prší (herní pravidla jako testovatelné klauzule)

> Kontrakt domény. Každý test mapuje na jednu klauzuli (`S3.1`). Vrstva v `[]`.
> `RED` = klauzule odhaluje **chybějící/špatný kód**, ne jen chybějící test.
> Zdroj pravdy pro chování = `Assets/Scripts/Core/` (polymorfní karty + `GameContext`).

Karty: 4 barvy (Hearts, Diamonds, Clubs, Spades) × 8 hodnot (7,8,9,10,J,Q,K,A) = **32** (mariášový balíček).
Speciální: **7** (lízni +2, kumulativně), **eso** (přeskoč), **dáma** (na cokoliv, mění barvu).

## S1 — Balíček (`Deck`) [EditMode]

- **S1.1** Nový balíček má přesně 32 karet (4×8), každá kombinace barva/hodnota právě jednou.
- **S1.2** `Shuffle(seed)` je **deterministický**: stejný seed → stejné pořadí. `RED` — dnes `UnityEngine.Random`, bez seedu, nedeterministické.
- **S1.3** `DrawCard()` odebere a vrátí vrchní kartu; `RemainingCards` se sníží o 1.
- **S1.4** `DrawCard()` na prázdném balíčku vrátí `null` (nehodí výjimku).

## S2 — Hratelnost, základ (`CanPlayOn`) [EditMode]

- **S2.1** Karta je hratelná, když má **stejnou barvu NEBO hodnotu** jako vrchní.
- **S2.2** Na prázdný odhazovací balíček (`topCard == null`) je hratelná libovolná ne-penalizační karta.
- **S2.3** Při aktivní penalizaci (`PendingDrawCount > 0`) **běžná karta hratelná není**.

## S3 — Speciální karty [EditMode]

- **S3.1** Sedma přidá `+2` do `PendingDrawCount`; opakované sedmy se **kumulují** (2→4→6).
- **S3.2** Při aktivní penalizaci je hratelná **jen sedma**, a to pouze pokud je vrchní karta sedma (obrana).
- **S3.3** Eso nastaví `SkipNextPlayer = true`. Eso **nelze** zahrát při aktivní penalizaci.
- **S3.4** Dáma je hratelná na **cokoliv** (když není penalizace).
- **S3.5** Dáma `OnPlay` nastaví vynucenou barvu na `SelectedSuit` (nebo vlastní barvu, když není vybráno).
- **S3.6** Dáma **nelze** zahrát při aktivní penalizaci.
- **S3.7** Při vynucené barvě (`ForcedSuit`) je běžná karta/eso/sedma hratelná jen při shodě barvy.
- **S3.8** Zahrání běžné karty/esa/sedmy **zruší** vynucenou barvu (`ForcedSuit → null`).

## S4 — Pořadí tahů (`GameContext.AdvanceToNextPlayer`) [EditMode]

- **S4.1** Normálně se posune na `(current + 1) % count`.
- **S4.2** Při `SkipNextPlayer` se posune o **2**, flag se vynuluje a vystřelí `OnPlayerSkipped`.
- **S4.3** `RED` — wrap kolem konce seznamu při skipu (modulo) ověřit exaktně (off-by-one při count=2/3).

## S5 — Konec hry [EditMode]

- **S5.1** `SetWinner(p)` nastaví `Winner = p`, `IsGameOver = true`, vystřelí `OnPlayerWon`.
- **S5.2** `RED` — vítězství = hráč má 0 karet v ruce; tato podmínka dnes není v `Core/` (řeší ji starý `CardManager`).

## S6 — Přehození odhazovacího balíčku [EditMode]

- **S6.1** `RED` — když dojde `Deck`, odhazovací balíček (kromě vrchní karty) se vrátí do balíčku. Dnes jen ve starém `GameplayState`, ne v `Core/`.

## S7 — Tah po líznutí [EditMode]

- **S7.1** `RED` — v Prší po líznutí karty tah končí, i kdyby karta byla hratelná. Logika dnes mimo `Core/`.

## S8 — Boot smoke [PlayMode]

- **S8.1** `RED` — studený start hlavní scény dosáhne menu/rozdání bez chyby v konzoli (zero console errors). Až po sjednocení wiringu.

---

## Pokrytí (stav)

**Pokryto EditMode testy (23 testů, zelené):**
- S1.1–S1.4 — `DeckTests` (32 unikátních karet, deterministický shuffle, draw, prázdný balíček)
- S2.1–S2.2, S3.1–S3.8 — `CardRulesTests` (Strategy: pravidla + speciální karty)
- S4.1–S4.3, S5.1 — `GameFlowTests` (pořadí tahů, skip, wrap, vítěz)

**Vyřešeno (bylo RED):** S1.2 — deterministický seedovaný shuffle (commit „S1.2 red→green").

**Zbývá RED (blokováno migrací na `Core/`):**
- S5.2 — vítězství = 0 karet (logika dnes ve starém `CardManager`)
- S6.1 — přehození odhazovacího balíčku (ve starém `GameplayState`)
- S7.1 — tah končí po líznutí (mimo `Core/`)
- S8.1 — PlayMode boot smoke (po sjednocení wiringu)
