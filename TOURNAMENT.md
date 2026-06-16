# Design: eliminační turnaj

> Stav: návrh odsouhlasen, čeká na implementační plán. Větev `refaktor`.
> Doplňuje `SPEC.md` (pravidla karet) o ekonomiku a turnajovou smyčku.

## Cíl

Z jednorázového kola Prší udělat **eliminační turnaj**: hraje se kolo za kolem,
poražení platí podle karet v ruce, kdo přijde o všechny peníze vypadává, a hraje
se dokud nezbude jeden hráč s penězi = celkový vítěz.

## Ekonomika (nahrazuje sázky před kolem)

- **Žádné sázky/pot před kolem.** Ruší se `BetSelectionState`, `Player.PlaceBet`,
  pot (`AddToPot`/`PayoutToWinner`/`Pot`), bet modal a sázky AI v `DealingState`.
- Kolo se hraje jako dnes → první hráč, který vyloží všechny karty = **vítěz kola**
  (0 karet v ruce).
- **Penalta poraženého** = `baseRate × Σ hodnota(karta)` přes zbylé karty v ruce,
  kde `hodnota = (int)Rank` (7…10, J=11, Q=12, K=13, A=14). `baseRate` default **10**
  (konfigurovatelné). Příklad: ruka {A, 7} = (14+7)×10 = 210.
- Penalta se odečte z `Cash`. Když hráč nemá dost, zaplatí jen co má (`Cash → 0`).
- **Vítěz kola inkasuje součet skutečně zaplacených penalt.**
- **Cash == 0 → eliminace.** Hráč v dalších kolech nehraje a jeho bar se v UI skryje.
- Turnaj končí, když zbývá **jediný** hráč s `Cash > 0` = **celkový vítěz**.

Startovní cash zůstává 1000 (doladí se později).

## Architektura

### Core — `TournamentRules` (čistá, testovatelná logika, EditMode)
Bez UnityEngine, nad `IPlayerData` + hodnotami karet. Odpovědnosti:
- `PenaltyFor(hand, baseRate)` → částka za zbylé karty.
- `SettleRound(players, winner, baseRate)` → odečte penalty poraženým, připíše
  vítězi součet zaplaceného; vrátí přehled (kdo kolik zaplatil/dostal).
- `IsEliminated(player)` → `Cash == 0`.
- `RemainingPlayers(players)` / `OverallWinner(players)` → kdo zbývá / jediný s penězi.

To je nová várka red→green testů (SPEC klauzule níže).

> Pozn.: `IPlayerData` je dnes read-only (Cash getter). Pro zápis penalt zavede
> `TournamentRules` malé zapisovací rozhraní nad hráčem (např. `ITournamentPlayer`
> s `Cash`/`PayPenalty`/`Receive`), které `Player` implementuje — drží Core čistý
> a testovatelný s fake hráčem.

### State machine
Zrušit `BetSelectionState`. Tok:
`Menu → Dealing → Gameplay → RoundEnd → (Dealing | GameOver)`
- **`RoundEndState`** — vyhodnotí kolo přes `TournamentRules`, skryje bary
  eliminovaných, ukáže mezikolový modal. Po „Další kolo": pokud zbývá >1 hráč →
  `Dealing`, jinak → `GameOver`.
- **`GameOverState`** — finální modal (celkový vítěz), tlačítko „Do menu" → `Menu`.

### UI — dva modaly
- **Mezikolový modal:** vítěz kola + score všech (jméno, cash, případně „OUT").
  Tlačítko „Další kolo".
- **Finální modal:** celkový vítěz turnaje. Tlačítko „Do menu".
- **Eliminace:** `RoundEndState` skryje bar (SetActive false). Přeskupení řeší
  **uživatel ve scéně přes Unity UI row grid** (layout group) — kód jen skrývá.

### Glow bug (řeší se mimochodem)
Zaseklý „active glow" na Player One po restartu = neresetovaný `IsActive` mezi
koly. `RoundEnd`/`Dealing` čistě resetuje `IsActive` všech hráčů → glow zmizí.

## SPEC klauzule (testovatelné, doplnit do SPEC.md)

- **T1.1** `PenaltyFor` prázdné ruky = 0.
- **T1.2** `PenaltyFor({A,7}, 10)` = 210 (exaktně, hodnota×sazba).
- **T2.1** `SettleRound`: poražení zaplatí svou penaltu, cash klesne přesně o ni.
- **T2.2** Vítěz dostane součet zaplacených penalt.
- **T2.3** Poražený bez dostatku cash zaplatí jen `Cash` (→ 0), vítěz dostane jen reálně zaplacené.
- **T3.1** `IsEliminated` = `Cash == 0`.
- **T3.2** `OverallWinner` vrací hráče, když právě jeden má `Cash > 0`; jinak null.
- **T3.3** Eliminovaný hráč se nepočítá mezi `RemainingPlayers`.

## Co se ruší / mění

- Smazat: `BetSelectionState`, bet modal (UI + ModalManager části), `Player.PlaceBet`/
  `Staked`/`ResetStake`/`RefundStake`/`Payout`, `GameSession.Pot`/`AddToPot`/`PayoutToWinner`/
  `StartNewRound`, sázecí smyčka v `DealingState`.
- `GameplayState.OnPlayerWon` → místo `PayoutToWinner` + `Go<MenuState>` přejde do `RoundEndState`.

## Mimo scope (zatím)

- Vyvážení ekonomiky (sazba/cash) — doladí se po hraní.
- Přeskupení barů — řeší Unity row grid ve scéně (uživatel).
