# Elimination Tournament — Implementation Plan

> Executed inline (Unity MCP). Steps are TDD where Core logic allows; UI/scene steps
> are verified by compile + playtest. Spec: `TOURNAMENT.md`.

**Goal:** Replace one-off rounds with an elimination tournament: losers pay card-value
penalties, broke players drop out, last player standing wins.

**Architecture:** Pure Core `TournamentRules` (EditMode-tested) drives the economy and
elimination; `RoundEndState`/`GameOverState` drive flow; two modals show results. The
old betting system is deleted.

---

## Phase A — Core economy (TDD, EditMode)

### Task A1 — `ITournamentPlayer` + `TournamentRules.PenaltyFor`
- Create `Assets/Scripts/Core/Game/ITournamentPlayer.cs`: read/write player money view
  used by Core (`int Cash { get; }`, `void Pay(int amount)`, `void Receive(int amount)`,
  `IReadOnlyList<ICardData> Hand`, `bool IsHuman`, `string Name`).
- Create `Assets/Scripts/Core/Game/TournamentRules.cs`: static `PenaltyFor(IReadOnlyList<ICardData> hand, int baseRate)`
  = `baseRate * Σ (int)card.Rank`.
- Test `Assets/Tests/EditMode/TournamentRulesTests.cs`:
  - T1.1 empty hand → 0
  - T1.2 {A,7} @10 → 210
- Red → Green → commit.

### Task A2 — `SettleRound`
- Add `TournamentRules.SettleRound(IReadOnlyList<ITournamentPlayer> players, ITournamentPlayer winner, int baseRate)`:
  each non-winner pays `min(PenaltyFor(hand), Cash)`; winner receives the sum of amounts actually paid.
- Tests (fake `ITournamentPlayer`):
  - T2.1 loser cash drops by exactly its penalty
  - T2.2 winner receives sum of penalties
  - T2.3 loser with insufficient cash pays only Cash (→0); winner gets only what was paid
- Red → Green → commit.

### Task A3 — elimination + winner helpers
- Add `IsEliminated(p) => p.Cash == 0`, `RemainingPlayers(players)` (Cash>0),
  `OverallWinner(players)` (the single Cash>0 player, else null).
- Tests:
  - T3.1 IsEliminated == (Cash==0)
  - T3.2 OverallWinner returns the one with Cash>0; null if 0 or >1
  - T3.3 eliminated excluded from RemainingPlayers
- Red → Green → commit.

## Phase B — wire Player + delete betting

### Task B1 — `Player : ITournamentPlayer`
- Modify `Assets/Scripts/Game/Player.cs`: implement `ITournamentPlayer`
  (`Pay(a) => SetCash(Cash-a)`, `Receive(a) => SetCash(Cash+a)`; Hand/IsHuman/Name already present).
- Compile check, tests stay green, commit.

### Task B2 — delete the betting system
- Delete `Assets/Scripts/State/BetSelectionState.cs` (+ remove `GameDirector` registration/`OnBetButtonClicked`).
- `Player.cs`: remove `Staked`/`PlaceBet`/`ResetStake`/`RefundStake`/`Payout` (+ bet-related).
- `GameSession.cs`: remove `Pot`/`AddToPot`/`PayoutToWinner`/`StartNewRound`/`PotChanged`.
- `DealingState.cs`: remove the AI betting loop; keep deal + `SetActiveIndex` flow; after deal go to `GameplayState` (not `BetSelectionState`).
- Remove bet modal wiring from `ModalManager` (betModal fields/handlers) and `UIEvents` bet-modal events.
- Compile check (fix fallout), commit.

## Phase C — tournament flow (states)

### Task C1 — `RoundEndState`
- Create `Assets/Scripts/State/RoundEndState.cs`: on Enter, find round winner (the player with 0 cards),
  call `TournamentRules.SettleRound` over the live players (baseRate from a config field), reset `IsActive` of all,
  hide eliminated players' bars (event/callback to UI), show the inter-round modal with winner + scores.
  On "Next round": if `RemainingPlayers > 1` → `Dealing`, else → `GameOver`.
- Wire baseRate (default 10) via `GameDirector` serialized field passed into the state.
- Compile check, commit.

### Task C2 — `GameOverState`
- Create `Assets/Scripts/State/GameOverState.cs`: show final modal with `OverallWinner`; "Do menu" → `MenuState`.
- Register both states in `GameDirector`; `GameplayState.OnPlayerWon` → `_fsm.Go<RoundEndState>()` (drop PayoutToWinner).
- Compile check, commit.

### Task C3 — glow reset
- Ensure `RoundEndState`/`DealingState` reset `IsActive=false` for all players (kills the stuck active-glow).
- Playtest note, commit.

## Phase D — UI modals (MCP)

### Task D1 — inter-round modal
- Build via MCP `execute_code`: overlay panel under Canvas, title "Winner: X", a score list
  (name + cash, "OUT" if eliminated), "Next round" button → callback into `RoundEndState`.
- Component `Assets/Scripts/UI/RoundEndModal.cs` (passive view: `Show(winnerName, rows, onNext)` / `Hide`).
- Wire in scene via MCP. Compile + save scene, commit.

### Task D2 — final modal
- Component `Assets/Scripts/UI/GameOverModal.cs` (`Show(winnerName, onMenu)` / `Hide`); build + wire via MCP.
- Compile + save scene, commit.

## Phase E — playtest

- New game → play round → inter-round modal shows winner + scores → next round.
- Broke player eliminated (bar hidden; user adds row grid for reflow).
- Down to one → final modal → menu.
- Card-count invariant stays 32; no stuck glow.

## SPEC clauses to add to SPEC.md
T1.1–T1.2 (penalty), T2.1–T2.3 (settle), T3.1–T3.3 (elimination/winner).
