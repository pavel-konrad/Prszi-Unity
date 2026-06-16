# Multiplayer Prší — Backend Roadmap (junior → mid)

> Cíl: postavit server-authoritative multiplayer backend pro Prší a přitom si **do hloubky**
> osahat klíčové koncepty a patterns. Napsat to **sám** (paměť + kód + úsudek).
>
> Klient = Unity (jen view + input). Pravdu o hře drží server.
>
> Stack je **koncept-first**: u každé fáze je primárně Rails a vedle .NET ekvivalent —
> takže roadmapa slouží, ať se rozhodneš pro Rails nebo C#/ASP.NET Core. Patterns jsou
> jazykově nezávislé; mění se jen nástroje.

---

## Jak roadmapu číst

Každá fáze má:

- **Koncept** — co a *proč*
- **Postav (Prší)** — co konkrétně přidat do projektu
- **Hloubka** — kam jít, aby to bylo mid, ne junior
- **Rails / .NET** — mapování nástrojů
- **Reference** — dokumentace + video/článek
- **Ověření** — otázka; když na ni umíš odpovědět z hlavy, fázi máš zvládnutou

Časový odhad: **~6–8 týdnů při 3–4 h/den** (odhad, ne slib — debugging roste s učením).

---

## Fáze 0 — Setup (1–2 dny)

**Koncept.** Kostra projektu, ať máš kam psát.

**Postav.** Rails 8 API-only + Postgres + RSpec + Rubocop + Git. (Nebo: `dotnet new webapi` + EF Core + xUnit.)

**Hloubka.** Pochop, co ti API-only režim vypne (views, asset pipeline, cookies-by-default) a proč to pro backend ke hře nechceš.

**Rails / .NET.** `rails new prsi --api -d postgresql` / `dotnet new webapi`.

**Reference.**
- Rails API-only: https://guides.rubyonrails.org/api_app.html
- ASP.NET Core Web API: https://learn.microsoft.com/aspnet/core/web-api/

**Ověření.** Proč API-only? Co tím ztrácíš a proč ti to nevadí?

---

## Fáze 1 — Doménový port + TDD (cca týden)

**Koncept.** Entity vs Value Object, agregát, business pravidla. Čistá doména bez DB a bez frameworku.

**Postav.** Přepiš pravidla karet (máš je odladěná v `Prsi.Core`): `Card` jako **value object** (immutable suit+rank), hratelnost (`can_play_on?`), efekty sedmy/dámy/esa. Žádný ActiveRecord. TDD: test → kód → zeleně.

**Hloubka.** Proč je karta value object (rovná se hodnotou, ne identitou) a hra entita (má identitu, žije v čase). Immutabilita. Doménu drž mimo persistenci.

> **Trap:** nepiš C# v Ruby. Žádný `CardFactory`, žádné interface — Ruby je duck-typed.
> Naopak v C# **přímo reuse `Prsi.Core`** (žádný port).

**Rails / .NET.** PORO + RSpec / sdílená `Prsi.Core` knihovna + xUnit.

**Reference.**
- Fowler, Value Object: https://martinfowler.com/bliki/ValueObject.html
- Fowler, Domain Model: https://martinfowler.com/eaaCatalog/domainModel.html

**Ověření.** Proč je `Card` value object a `Game` entita? Kde přesně je ta hranice?

---

## Fáze 2 — Persistence (cca týden — nejtěžší nový kus)

**Koncept.** Jak namapovat doménu do DB. Agregát → tabulky. Zdroj pravdy v Postgresu.

**Postav.** Tabulky: `users`, `games`, `players` (členství usera ve hře), `moves`, stav rukou. Rozhodni:
- **A) snapshot** aktuálního stavu (jednoduché), nebo
- **B) seed + log Moves** a stav „přehraj" (event sourcing; tvůj seedovaný deck to umožňuje).
- Doporučení pro učení: **ukládej Moves jako event log _i_ snapshot** pro rychlé čtení = de facto CQRS read model.

**Hloubka.** AR asociace, indexy, transakce, N+1 (`includes`/`joins`/`preload`), jak vůbec perzistovat agregát, aby nešel uložit do nekonzistentního stavu.

**Rails / .NET.** ActiveRecord + migrace / EF Core + migrations.

**Reference.**
- Active Record Basics: https://guides.rubyonrails.org/active_record_basics.html
- Active Record Querying (N+1, includes): https://guides.rubyonrails.org/active_record_querying.html
- EF Core: https://learn.microsoft.com/ef/core/

**Ověření.** Kde je zdroj pravdy o stavu hry? Když server spadne uprostřed tahu, jak se hra obnoví?

---

## Fáze 3 — State machine (3–5 dní)

**Koncept.** Finite state machine, přechody, guards. Lifecycle hry.

**Postav.** `waiting_for_players → playing → finished`. Zakázat nevalidní přechod (`finished → playing` musí vyhodit chybu, ne tiše projít). Tohle jsou tvoje **první testy**.

**Hloubka.** Guards (přejdi do `playing` jen když ≥ 2 hráči), callbacky na přechodu (rozdej karty při startu), proč stav nepatří do controlleru ani do `if`ů roztroušených po kódu.

**Rails / .NET.** Gem **AASM** / knihovna **Stateless** (NuGet) nebo vlastní enum + guardy.

**Reference.**
- AASM (GitHub, README je zdroj pravdy): https://github.com/aasm/aasm
- Praktický guide: https://rubyguides.dev/guides/ruby-state-machine-gem/
- Přehled FSM gemů v Ruby: https://dev.to/gsgermanok/understanding-state-machines-in-ruby-concepts-examples-the-best-gems-4gh9

**Ověření.** Napiš test, že `finished → playing` vyhodí chybu a **nezmění** stav. Proč guard, a ne jen kontrola v controlleru?

---

## Fáze 4 — Service objekty (3–5 dní)

**Koncept.** Aplikační vrstva. Orchestrace logiky mimo model i controller.

**Postav.** `PlayCardService.call(game:, player:, card:)`, `JoinGameService`. Sem vytáhni tu změť, co je teď v Unity `CardManager.PlayCard` (validace + efekt karty + kontrola výhry + posun tahu). Vrať strukturovaný výsledek (success/failure + chyba).

**Hloubka.** Co patří do modelu (pravidla o sobě), co do service (orchestrace více objektů), proč „model nevolá model". Transakce kolem celé akce.

**Rails / .NET.** PORO service + ServiceResult / **MediatR** handler (command pattern).

**Reference.**
- Fowler, Service Layer: https://martinfowler.com/eaaCatalog/serviceLayer.html
- MediatR (.NET): https://github.com/jbogard/MediatR

**Ověření.** Co dělá service a co model? Proč ne všechno nacpat do modelu?

---

## Fáze 5 — Konkurence a zamykání (cca týden — TADY se děje junior → mid)

**Koncept.** Race conditions. Dva hráči táhnou naráz. Idempotence.

**Postav.** Obal tah zámkem. Přidej `idempotency_key` na tah (mobil pošle request 2× kvůli lagu → server nezahraje kartu dvakrát).

**Hloubka.** Optimistic (`lock_version` + retry na `StaleObjectError`) vs pessimistic (`with_lock` / `SELECT … FOR UPDATE`). Izolační úrovně. **Jak race vůbec otestovat** (vlákna / simulace konfliktu) — to je těžké a poučné.

**Rails / .NET.** AR locking / EF Core concurrency tokens (`[ConcurrencyCheck]`, `RowVersion`).

**Reference.**
- Optimistic vs pessimistic (přehled): https://blog.saeloun.com/2022/03/23/rails-7-adds-lock-with/
- API `with_lock`: https://api.rubyonrails.org/classes/ActiveRecord/Locking/Pessimistic.html
- Hloubkový rozbor („lock is not a mutex"): https://baweaver.com/writing/2026/06/05/rails-sharp-parts-lock-is-not-a-mutex/
- EF Core concurrency: https://learn.microsoft.com/ef/core/saving/concurrency

**Ověření.** Co se stane při dvojkliku „zahraj kartu"? Který zámek zvolíš a proč? Jak ten race otestuješ?

---

## Fáze 6 — HTTP API + kontrakt (cca týden)

**Koncept.** REST sémantika. Tvůj zájem o protokoly přímo tady.

**Postav.** Endpointy: vytvoř hru, připoj se, zahraj kartu, lízni, načti stav. Statusy doopravdy: 200 vs 201 vs 422 (nevalidní data) vs 409 (konflikt stavu / není tvůj tah). Auth tokenem.

**Hloubka.** Sémantika metod (PUT idempotentní vs POST), status kódy a *proč*, idempotence, kde žije token (header vs cookie a proč), CORS.

**Rails / .NET.** Action Controller / ASP.NET Core controllers + minimal API.

**Reference.**
- MDN HTTP (zlatý standard protokolu): https://developer.mozilla.org/en-US/docs/Web/HTTP
- Action Controller: https://guides.rubyonrails.org/action_controller_overview.html
- Rails Security (tokeny, CSRF, atd.): https://guides.rubyonrails.org/security.html

**Ověření.** Nevalidní tah → 422, nebo 409? Obhaj to. Proč je `GET` bezpečný a `POST` ne?

---

## Fáze 7 — Event-driven design (3–5 dní)

**Koncept.** Observer pattern, decoupling. Po akci se „něco stane" bez přímého volání.

**Postav.** Po zahrání karty vyvolej event `CardPlayed` → odběratelé: ulož Move, zapiš audit, notifikuj realtime vrstvu. (Tvůj C# `GameContext` má eventy `OnCardPlayed`/`OnPlayerWon` už teď — máš předlohu.)

**Hloubka.** Proč přes event a ne přímým voláním v service (přidání odběratele nemění service). Synchronní vs async event. Kde je hranice, aby z toho nebyl nečitelný „spaghetti přes eventy".

**Rails / .NET.** `ActiveSupport::Notifications` nebo malý EventBus / **MediatR** notifications.

**Reference.**
- Active Support Instrumentation: https://guides.rubyonrails.org/active_support_instrumentation.html
- Fowler, Event-Driven (pojmy): https://martinfowler.com/articles/201701-event-driven.html

**Ověření.** Proč zápis auditu přes event a ne přímo v `PlayCardService`? Co tím získáš?

---

## Fáze 8 — Realtime (cca týden)

**Koncept.** Push „soupeř táhl". WebSocket vs HTTP.

**Postav.** Nejdřív **polling** (Unity se ptá každou 1–2 s) — pochopíš request/response a stav. Pak refactor na **WebSocket**: server pushne nový stav všem u stolu.

**Hloubka.** Čím se liší WS handshake (HTTP upgrade) od běžného requestu. Stavové spojení vs request/response. Pub/sub backend a proč ho potřebuješ přes víc procesů.

**Rails / .NET.** **ActionCable** (pub/sub backend: Solid Cable bez Redisu, nebo Redis adapter) / **SignalR** (volitelně Redis backplane).

**Reference.**
- Action Cable Overview: https://guides.rubyonrails.org/action_cable_overview.html
- GoRails realtime série (placené, kvalitní): https://gorails.com/series/realtime-group-chat-with-hotwire
- SignalR (.NET): https://oneuptime.com/blog/post/2026-01-29-realtime-apps-signalr-dotnet/view
- **Referenční projekt — tahová hra na ASP.NET Core + SignalR + EF Core (rooms, server-side validace):** https://github.com/phuchautea/UniXiangqi

**Ověření.** Čím se WS handshake liší od běžného HTTP requestu? Proč single-process pub/sub nestačí v produkci?

---

## Fáze 9 — Docker + CI/CD (3–5 dní)

**Koncept.** Reprodukovatelné prostředí. Zelený build po každém pushi.

**Postav.** `compose.yaml`: Rails + Postgres jedním příkazem (`docker compose up`). GitHub Actions: install → testy → rubocop po pushi.

**Hloubka.** Multi-stage Dockerfile (build vs runtime), non-root user, proč `host: db` a ne `localhost`, named volume pro perzistenci DB. Service containers v CI.

**Rails / .NET.** Rails generuje Dockerfile od 7.1; Rails 7.2+ generuje i CI workflow / `dotnet publish` + Docker.

**Reference.**
- Docker — Rails guide: https://docs.docker.com/guides/ruby/containerize/
- Rails containerization best practices: https://blog.saeloun.com/2026/05/04/rails-containerization-best-practices/
- Rails + Postgres + Docker (krok za krokem): https://danielabaron.me/blog/rails-postgres-docker/
- GitHub Actions — oficiální Rails starter: https://github.com/actions/starter-workflows/blob/main/ci/rubyonrails.yml

**Ověření.** Spustí nový člověk projekt za 5 minut z čistého stroje? Proč multi-stage build?

---

## Fáze 10 — Bonus (vyber 1)

- **Replay hry** — těží ze seed + Moves (event log). Nejlevnější, pokud jsi šel cestou B v Fázi 2.
- **Leaderboard** — tady přijde **Redis sorted set** (`ZADD`/`ZREVRANGE`). Lekce o specializované datové struktuře.
  - Redis Sorted Sets: https://redis.io/docs/latest/develop/data-types/sorted-sets/
- **AI hráč** — server-side bot (máš logiku v Unity `GameplayState`/`CardManager`).

---

## Joby: Solid Queue vs Sidekiq vs Redis (shrnutí)

Nejsou to konkurenti z jedné police:

| Funkce | DB-based (bez Redisu) | Redis-based |
|---|---|---|
| Background joby | **Solid Queue** (default Rails 8) | Sidekiq |
| Cache | Solid Cache | Redis cache |
| ActionCable pub/sub | Solid Cable | redis adapter |

- **Redis** = úložiště/datové struktury, ne job systém. Přidej ho **až** pro konkrétní důvod (leaderboard sorted set, hot cache).
- Pro tenhle projekt: **Solid Queue stačí, Redis nepotřebuješ** — dokud nechceš leaderboard nebo se Redis učit záměrně.
- Detail k pochopení: Solid Queue polluje DB přes `FOR UPDATE SKIP LOCKED` a umí enqueue **ve stejné transakci** jako data (mizí klasický „job běžel dřív, než se commitlo").

---

## Průběžně (10 min/den) — Martin Fowler

- Event-Driven Architecture: https://martinfowler.com/articles/201701-event-driven.html
- Domain Model: https://martinfowler.com/eaaCatalog/domainModel.html
- CQRS (jen pochopit): https://martinfowler.com/bliki/CQRS.html
- State Machine / pojmy z *Patterns of Enterprise Application Architecture*

---

## Co přeskočit (příští 2 měsíce)

Kubernetes · Kafka · Microservices · React · GraphQL

---

## Výsledek

multiplayer backend · state machine · observer pattern · testy · concurrence/locking ·
HTTP kontrakt · realtime · Docker · CI/CD · portfolio projekt · základ pro JINGI
