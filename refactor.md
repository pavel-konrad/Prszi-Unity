# Refaktor — Prsi UI demo (audit)

> Stav: audit hotový, kód zatím nedotčen. Větev `refaktor`.
> Rozsah projektu: 2D karetní hra Prší, jedna scéna, hráč vs. AI, sólo / učební projekt.
> ~7 900 řádků C#, 66 skriptů.

## Rozhodnutí (potvrzená)

1. **Architektura:** migrovat na čistý svět `Core/` — propojit ho do hry a starou logiku `Game/State` smazat.
2. **Začínáme:** úklid logů + mrtvého kódu (nízké riziko, okamžitý zisk).
3. **SQLite:** zatím NE. Na uložení profilu/nastavení stačí JSON (skill `unity-persistence`).

---

## Cílová konvence: inspirace `legends-of-bohemia` (ne kopie)

Referenční repo `legends-of-bohemia-master` = čistá, zdokumentovaná (`ARCHITECTURE.md`) implementace
celé `unity-*` rodiny. Bereme z něj **vzory a názvosloví**, ale filtrujeme přes realitu 2D karetní hry —
LoB je real-time grid akce, Prší je tahová UI hra. Ne vše se přenáší.

### Co z LoB VZÍT (sedí na Prší)
| Vzor | LoB tvar | Prší (cíl) |
|---|---|---|
| Observer | `IObserver<T>`/`ISubject<T>` + `*EventManager` + `*Event` | nahradí statické `UIEvents`/`AudioEvents` + `GameContext` C# eventy → jeden konzistentní mechanismus |
| MVP | `*View` (pasivní settery) + `*Presenter : IObserver<*Event>`, self-wiring přes `Registry` | `PlayerUI` → `PlayerView` + `PlayerPresenter`; `CardUI` → `CardView`+presenter |
| Registry | `PlayerRegistry` (List + `OnPlayerAdded`) | `PlayerRegistry` (4 hráči) — zabíjí name-matching v `GameSession` |
| Factory | `*Factory.Create()` → `Instantiate` + strategy + `Initialize()` | `CardFactory` (už je) + `PlayerFactory` |
| Strategy | `CharacterBehavior` + Race overrides | polymorfní karty `BaseCard`→`Ace/Seven/Queen/Regular` (už hotové ✓) |
| Composition root | `GameManager` (prázdný, vše self-wiruje) | nahradí `GameSession.I` singleton + `FindObjectOfType` |
| Bridges | `UIAudioObserver`, `UIAnimationObserver` | `AudioListener`→`UIAudioObserver`; `AnimationEventReceiver` zůstává anim→audio bridge |

### Co z LoB NEBRAT (LoB-specifické / pro Prší overkill)
- **Grid/PathFinder/NavMesh/Combat** — Prší nemá prostor ani pohyb.
- **Per-frame tick eventy** (`ChargeTick`/`DurationTick`) — Prší je tahové, animace řeší stav karty (viz níže).
- **Spawn coroutiny v intervalech** — karty se rozdají jednou, ne streamují.

### Vědomé odchylky (cíl = showcase + testovatelnost, ne 1:1 LoB)
1. **asmdef** — LoB nemá žádné. Prší MÍT BUDE: EditMode test assembly + hranice Core↔UI to vyžadují. Nutnost.
2. **Event payload** — LoB `*Event` je `class` (alokuje při notify). Pro tahovou hru OK (nízký rate); jen kdyby vznikl per-frame event, udělat `struct` (zero-GC).
3. **Namespace vs jen složky** — viz otevřené rozhodnutí níže.

## Hlavní problém: dvě paralelní architektury, ta čistá je mrtvý kód

V `Assets/Scripts/` existují **dva souběžné kódy**:

| | Svět A — *běží* | Svět B — *osiřelý* |
|---|---|---|
| Umístění | `Game/` + `State/` + `UI/` + `Core/GameSession.cs` | `Core/Game/`, `Core/Cards/`, `Core/Services/`, `Core/Interfaces/` |
| Model karty | `Card` (plochý: suit + rank) | `BaseCard`→`AceCard`/`SevenCard`/`QueenCard` (polymorfní efekty) |
| Stav hry | rozházený po polích | `GameContext` (vynucená barva, draw penalty, skip) |
| Pravidla | **speciální karty NEimplementované** — `GameplayState` testuje jen `suit==suit \|\| rank==rank` | sedma / eso / dáma plně namodelované |
| Používá hra? | ano | **ne — ověřeno, nic mimo `Core/` to nereferencuje** |

`Scripts/Core/` (~1 400 řádků) + `Core/Tests/` (361 řádků) je *správný* návrh — čistý polymorfismus,
reálná pravidla Prší, pořádný `GameContext`. A je **úplně odpojený.** Hra běží na nepořádném světě,
který ani neimplementuje speciální karty. Tenhle rozštěp je zdroj pocitu „zběsilosti“ — víc než animace.

→ **Cílový stav:** zmigrovat na `Core/`, starou rules-logiku v `Game/State` smazat.

---

## Co je konkrétně špatně (po oblastech)

### Provázání / coupling
- **17× `Find*ObjectOfType`** v 10 souborech.
- `GameSession.ConnectAIHandsToPlayers()` hledá AI ruce **podle jména GameObjectu** (`"EnemyBar"`/`"Enemy"`/`"AI"`)
  a v záloze skenuje *všechny* objekty ve scéně.
- Provázání běží **dvakrát** — `GameSession` i `GameRootBinder` bindují `PlayerUI`.
- Hack `BroadcastInitialStateNextFrame` (coroutine „o snímek později“) obchází pořadí inicializace.
- → přesně tohle má zabít `Game` service-locator (`unity-controllers`) + factory (`unity-factory`).

### Animace („zběsilé“)
- Pipeline Animation-Event → `AudioEvents` (`AnimationEventReceiver`) je sama o sobě v pořádku (návrh zvuku).
- Skutečný problém je **sekvencování**: timing rozdání / zahrání / lízání je rozházený mezi
  `CardManager`, `CardSpriteManager`, `CardUI`, `PlayerHand` a `GameplayState`, slepený coroutinami
  a magickými `WaitForSeconds(2.0f)/(0.5f)`. **Nikdo nevlastní sekvenci tahu.**
- → orchestrační mezera, ne chybějící FSM (i když malý sekvencer tahu sem sedí).

### Výkon
- **360× `Debug.Log`**, žádný nezabalený do `#if UNITY_EDITOR` (74 jen v `CardSpriteManager`).
  String interpolace běží i v buildu.
- Per-frame náklady NEjsou problém (jen 2× `Update()`, oba ošetřené — `GameStateMachine`, `PortraitFit`).
- → práce na výkonu = stripnout/zabalit logy, pak proměřit dávku rozdávání na GC alloc. Ne optimalizovat naslepo.

### Mrtvý kód
- Zakomentované bloky (`StartNewGame`, řetěz TODO v `OnBetSelected`).
- Osiřelý svět `Core/` + 2 test soubory, které testují osiřelý kód.

---

## Patterny — které využít, které jsou zbytečné

Důležité: **smyslem není přidávat patterny, ale konečně použít ty, co už máš rozdělané** —
a přestat je obcházet `Find`em a name-matchingem. Přidat *další* (Lua, …) by byl overkill.

### Použít (dávají smysl, většinu už máš v `Core/`)

| Pattern | Kde je / kam patří | Proč |
|---|---|---|
| **State Machine** (GoF State) | `GameStateMachine` už existuje | Rozšířit tok hry: Menu→Bet→Dealing→Gameplay→RoundEnd. Skill `unity-state-machine`. |
| **Service Locator** (`Game`) | chybí — zatím `Find*` | Nahradit 17× `FindObjectOfType` jedním přístupovým bodem. Skill `unity-controllers`. |
| **Factory** | `CardFactory` už je v `Core/Cards/` | Vytváření + provázání karet a UI místo ručního `Instantiate`. Skill `unity-factory`. |
| **Observer / Events** | částečně: `AudioEvents`, `UIEvents`, eventy v `GameContext` | Formalizovat signál „animace dokončena“ + změny stavu, zbavit se zbytku `Find`ů. Skill `unity-events`. |
| **Strategy / polymorfismus** | `BaseCard`→`Ace/Seven/Queen` v `Core/Cards/` | Chování karet bez `switch`e na typ. Tohle je jádro migrace na `Core/`. Skill `unity-strategy`. |
| **MVP** (View/Presenter) | `PlayerUI`/`AIHand`/`CardUI` už ten tvar mají | Pasivní View + Presenter, který pozoruje doménu. Skill `unity-ui`. |
| **ScriptableObject** (data) | `AvatarDB`/`CardThemeDB`/`EnemyDB` už používáš | OK, jen projít/zkonsolidovat. Skill `unity-scriptable-objects`. |

### Nepoužívat (overkill pro tenhle rozsah)

| Pattern / skill | Proč zbytečné |
|---|---|
| **SQLite** (`unity-database`) | Persistuješ ~5 hodnot (jméno, avatar, cash, nastavení). Stačí JSON / `PlayerPrefs`. SQLite má smysl **jen** když přidáš historii zápasů / statistiky / žebříčky. |
| **NavMesh postavy** (`unity-characters`) | AI je logika výběru karty, ne pohybující se agent. |
| **Procedurální generování** (`unity-procedural-generation`) | Žádné levely/bludiště. |
| **Lua scripting** (`unity-scripting`) | Žádný modding. |
| **Additivní scény** (`unity-scenes`) | Fakticky jedna scéna. |
| ~~**asmdef**~~ | **PŘEHODNOCENO → potřeba.** Jakmile chceš unit testy + vynucené hranice (Core nesmí vidět UI) + showcase repo, asmdef je *mechanismus*, ne overkill — přesně signál ze scope gate `unity-core`. Viz Testovací strategie. |

---

## Sjednocení eventů + MVP

### Problém: čtyři překrývající se event systémy

| Systém | Typ | Kdo poslouchá | Účel |
|---|---|---|---|
| `Player.Changed` | C# `event` na instanci | `PlayerUI` (přes `Bind`) | doménový stav → View (**MVP páteř**) |
| `UIEvents` | statický hub | jen `ModalManager` | mix: input + karty + tah |
| `AudioEvents` | statický hub | jen `AudioListener` | zvuky |
| `GameContext` eventy | C# eventy (osiřelé) | nikdo | doménové eventy v `Core/` |

Překryv: `OnPlayerTurnStarted/Ended` je **současně v `UIEvents` i `AudioEvents`** (jeden koncept, dva huby).
`GameContext.OnCardPlayed/OnPlayerWon` nikdo nečte.

### Pravidlo: dva kanály podle ÚČELU (ne podle náhody)

Skilly `unity-ui` + `unity-events`: existují dva druhy komunikace, každý má jiný kanál. Míchat je = chyba.

**Kanál 1 — odraz stavu (stav → UI): MVP observer.**
Doménový objekt (`Player`, `GameContext`) vystaví typovaný change-event → **Presenter** ho pozoruje →
tlačí do **pasivního View**. Už to dělá `Player.Changed → PlayerUI`. Udělat z toho pravidlo pro vše,
co zrcadlí stav (cash, jméno, aktivní hráč, obsah ruky). **Nikdy přes statický hub.**

**Kanál 2 — jednorázové notifikace („něco se stalo“): jeden event hub.**
Zvuk, one-shot efekty. Sem patří `AudioEvents`, ale **jeden zdroj pravdy** každého eventu.
Sloučit `UIEvents` + `AudioEvents`, zrušit duplikáty (`OnPlayerTurnStarted` ať existuje jen jednou).

### `PlayerUI` porušuje MVP — rozdělit

Teď je to View, ale zároveň rozhoduje „cash se změnil → animator + coroutine + zvuk“. Správně:

- **View** (`PlayerView`) = jen hloupé settery: `SetName()`, `SetCash()`, `PlayCashChange()`.
  → `AvatarStateView` už JE tenhle ideál (čistý pasivní View, jen `SetSelected/SetActive/SetDisabled` + `Apply`).
- **Presenter** (`PlayerPresenter`) = pozoruje `Player`/`IPlayerData`, rozhoduje *kdy* animovat, volá View.
- Zvuk/animace = malé **observer bridge** komponenty, ne nacpané do View (`unity-ui` to říká výslovně).
- Hledání entity → **Registry** (`unity-ui`), ne `Find`/`Bind` ze `Session`.

## Interfacy — v obou světech obráceně

Pravidlo `unity-core`: interface jen při 2+ implementacích nebo pro inverzi závislosti (testovatelnost).

- **Běžící hra: skoro žádné** — reálně použitý jen `IGameState` (4 impl ✓). `PlayerUI.Bind(Player)` bere konkrétní třídu.
- **Osiřelý `Core/`: až moc** — `ICardData` (Ace/Seven/Queen ✓ správně = polymorfismus), ale
  `ICardStateProvider`, `ICardThemeProvider`, `IPlayerProvider` mají **jednu implementaci = předčasná abstrakce (YAGNI)**.

Akce:
- **Nech:** `IGameState`, `ICardData`.
- **Přidej smysluplně:** `PlayerView`/`Presenter` ať závisí na `IPlayerData` (read-only pohled), ne na konkrétním `Player` → legitimní dependency inversion + testovatelný View.
- **Zahoď:** `*Provider` interfacy s jednou implementací, dokud nepřijde druhá.

## Testovací strategie (cíl: profesionální, testovatelný, CI/CD repo)

Spec-driven TDD (skill `testing` + `jingi-testing`), přeloženo do Unity Test Framework.
**Pravda:** běžící hru (`FindObjectOfType` + `GameSession.I` + logika v MonoBehaviourech) unit-testovat **nejde**.
EditMode NUnit běží bez scény → testuje jen čisté C#. Jediné čisté C# je `Core/`.
→ **„Udělat to testovatelné" = dotáhnout migraci do `Core/`.** Není to oddělený úkol.

### Vrstvy

| Vrstva | Co dokazuje | Kde | Stav |
|---|---|---|---|
| EditMode | doménové kontrakty (SPEC S1–S7), čisté C#, bez enginu | `Assets/Tests/EditMode/` | postavit |
| PlayMode | boot smoke (S8): studený start → menu, zero console errors | `Assets/Tests/PlayMode/` | po sjednocení wiringu |
| Manual | feel, UX, vizuál | protokol | před demo buildy |

### Blokery testovatelnosti (vyřešit PŘED prvním testem)

1. **Enum knot:** `Card.Suit`/`Card.Rank` jsou ve starém `Scripts/Game/Card.cs` (`using UnityEngine`, `Sprite`,
   konstruktor volá `CardSpriteManager.EnsureInstance()` MonoBehaviour). Celý `Core/` na nich závisí.
   → **Vytáhnout `Suit`/`Rank` do `Prsi.Core.Cards` jako čisté enumy** (bez UnityEngine). Tím se Core stane skutečně čistým.
2. **Nedeterministický shuffle:** `Deck.Shuffle()` = `UnityEngine.Random`, bez seedu (S1.2 RED).
   → injektovat RNG (`System.Random` se seedem) → reprodukovatelné testy + předvídatelnost.
3. **asmdef:** bez asmdef je vše v `Assembly-CSharp` a nejde izolovat. Test asmdef nemůže referencovat předdefinované assembly.
   → `Prsi.Core.asmdef` (Core) + `Prsi.Tests.EditMode.asmdef` (ref Core + nunit). `Assembly-CSharp` automaticky referencuje asmdefy, takže staré UI dál kompiluje.

### CI brána (bez ní „musí projít testy" = jen zvyk)

- `.github/workflows/tests.yml` → `game-ci/unity-test-runner@v4`, na každý push.
- `checkout` (LFS až bude potřeba), cache `Library`, `unityVersion: 6000.1.0f1`, licence `UNITY_LICENSE` (Personal `.ulf` → repo secret).
- **Red `main` = stop-the-line.** Práce na větvích, červená neblokuje vývoj, ale blokuje merge.

## Doporučené pořadí prací (testing-first)

0. **SPEC.md** ✅ — kontrakt hotový (číslované klauzule).
1. **Odblokovat testovatelnost** — vytáhnout `Suit`/`Rank` do čistého Core; injektovat seedovaný RNG do `Deck`.
2. **asmdef + Test Framework** — `Prsi.Core.asmdef`, `Prsi.Tests.EditMode.asmdef`, instalace `com.unity.test-framework`.
3. **Reálné EditMode testy** — přepsat `Debug.Log` demo na NUnit `[Test]` s **exaktními** asserty, Red→Green per SPEC klauzule. RED klauzule (S1.2, S4.3, S5.2, S6.1, S7.1) odhalují chybějící kód → dopsat.
4. **GameCI workflow** — zelená brána na GitHubu.
5. **Migrace na `Core/`** — propojit do hry pod zelenou sítí (`CardManager`/`GameSession`/`Player`/`GameplayState`), smazat starou rules-logiku.
6. **Stripnout/zabalit 360 `Debug.Log`** + smazat mrtvý kód.
7. **Zabít `Find*ObjectOfType` + name-matching** → `Game` locator + factory + Registry.
8. **Sjednotit eventy + MVP** — dva kanály, rozdělit `PlayerUI` na View+Presenter, pročistit interfacy.
9. **Animace přes stav karty** — sekvencer řízený eventy „animace dokončena".
10. **PlayMode boot smoke** (S8) + **JSON save** + **profiling** dávky rozdávání.

---

## Migrace na `Core/` — co zbývá (z REFACTORING_STATUS)

**Hotovo (v `Core/`, ale odpojené):** SO databáze (`AvatarDB`/`EnemyDB`/`CardThemeDB`) + modely,
interfacy, services (`CardTheme/State/Deck`), polymorfní karty (`BaseCard`→`Ace/Seven/Queen/Regular` + `CardFactory`),
`GameContext`/`Deck`/`DiscardPile`, testy.

**Zbývá propojit / smazat staré:**
- [ ] `Card` — oddělit data od logiky (nebo nahradit `BaseCard`)
- [ ] `CardManager` — použít services + `GameContext` (dnes drží logiku sám)
- [ ] `CardSpriteManager` — použít `CardThemeDB` (dnes 74 logů, 588 řádků)
- [ ] `GameSession` — použít `EnemyDB`
- [ ] `Player` — data z DB; zvážit `BasePlayer`→`HumanPlayer`/`AIPlayer` + `PlayerFactory`
- [ ] `GameplayState` — doplnit speciální karty přes polymorfní `OnPlay` (sedma/eso/dáma) — dnes chybí
- [ ] po propojení smazat starou rules-logiku v `Game/State`
- [ ] `*Provider` interfacy s jednou impl. zvážit zahodit (YAGNI)

Pozn.: `PlayerService`/`CardData`/`PlayerData`/`CardDataAdapter` ze starého PLANu už **neexistují** (nahrazeny polymorfismem).

## Animace přes stav karty (z INTEGRATION_GUIDE — odpověď na „zběsilé animace")

Místo coroutin + magických `WaitForSeconds` rozházených po 5 souborech: **animaci řídí změna stavu (umístění) karty.**

```
CardLocation { Deck, PlayerHand, EnemyHand, Discard, Drawn, Playing }
```

- `CardStateService.SetCardLocation(card, location)` → vyvolá `CardState.LocationChanged`
- `CardUI` poslouchá `LocationChanged` a podle přechodu spustí animaci:
  - `Deck → PlayerHand` ⇒ trigger `CardIn`
  - `PlayerHand → Discard` ⇒ trigger `CardPlayed`
  - `→ Drawn` ⇒ trigger `CardDrawn`
  - `→ EnemyHand` ⇒ zobrazit rub
- Animation Event v klipu → `AnimationEventReceiver` → `AudioEvents` (zvuk synchronní s vizuálem)
- Dokončení animace (`OnCardSwipedUpComplete`) → notifikace zpět do logiky (sekvencer čeká na tenhle signál, ne na `WaitForSeconds`)

**Speciální karty (polymorfně, ne `switch`):** `card.OnPlay(context, player)` →
- `SevenCard` ⇒ `context.NotifyDrawPenalty(2)` → UI „Líznout X karet"
- `AceCard` ⇒ `context.SkipNextPlayer = true` → animace přeskočení
- `QueenCard` ⇒ `SuitSelectionDialog` → `context.NotifySuitChanged(suit)` → indikátor vynucené barvy

→ Tohle je „state machine pro animace", po kterém jsi volal: **stav karty = zdroj pravdy, UI jen reaguje.**
Sedí na Kanál 1 (MVP observer) výše. Chybí dodělat: `CardUI` na `BaseCard`, `GameUI` na `GameContext` eventy, `SuitSelectionDialog`, indikátory `ForcedSuit`/`PendingDrawCount`.

## Mapování skillů

- **Potřeba:** `unity-core`, `unity-controllers`, `unity-factory`, `unity-state-machine`, `unity-events`, `unity-ui`, `unity-profiling`, `unity-persistence`
- **Už používáš:** `unity-scriptable-objects`, `unity-strategy` (v `Core/`)
- **Overkill teď:** `unity-database`, `unity-characters`, `unity-procedural-generation`, `unity-scripting`, `unity-scenes`, `unity-asmdef`
