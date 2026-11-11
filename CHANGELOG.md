# Changelog

All notable changes to **Town of Us: Mira - AmonRa Edition** are documented in this file.
Every version has a Polish translation below its English notes (*Po polsku*).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions look like
`<upstream version>-are.<number>`, e.g. `1.7.9-are.2` is the second AmonRa Edition release based on Town of Us: Mira 1.7.9.
Changes of Town of Us: Mira itself are described [upstream](https://github.com/AU-Avengers/TOU-Mira/releases).

## [Unreleased]

### Added

- **Workaholic** (new Crewmate Support role): after finishing all of their own tasks, the Workaholic can take over
  tasks of other crewmates with the "Take Task" button, one at a time. The task is always taken from the crewmate with
  the most tasks left (dead crewmates too, if ghosts do tasks), only tasks nobody has started yet are taken, and never
  the Snitch's. The crewmate learns that someone took over their task, sees it greyed out and can't do it; once the
  Workaholic finishes it, it counts as the crewmate's task. If the Workaholic dies, the task goes back to its owner.
  - Lobby options: cooldown after a finished task (10 s by default), a limit of taken tasks per game (no limit by
    default) and whether the crewmate sees the helper's name (off by default: just "someone").
  - Tasks that use up objects on the map (towels, records, guns, vegetables, ore, shells) are never taken.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Pracoholik** (Workaholic, nowa rola Crewmate Support): po wykonaniu wszystkich swoich zadań Pracoholik może
  przyciskiem „Weź zadanie” przejmować zadania innych członków załogi, po jednym naraz. Zadanie jest zawsze brane od
  osoby, która ma najwięcej zadań do zrobienia (także od martwych, jeśli duchy robią zadania), tylko takie, którego nikt
  jeszcze nie zaczął, i nigdy od Snitcha. Właściciel dowiaduje się, że ktoś przejął jego zadanie, widzi je wyszarzone i
  nie może go robić. Gdy Pracoholik je skończy, liczy się jako zadanie właściciela. Jeśli Pracoholik zginie, zadanie
  wraca do właściciela.
  - Opcje lobby: cooldown po wykonanym zadaniu (domyślnie 10 s), limit przejętych zadań na grę (domyślnie bez limitu)
    i czy właściciel widzi nick pomocnika (domyślnie nie: tylko „ktoś”).
  - Zadania, które zużywają przedmioty na mapie (ręczniki, dokumenty, broń, warzywa, ruda, muszle), nie są przejmowane.

</details>

## [1.7.9-are.4] - 2026-11-11

### Added

- **Gambler** (new Neutral Evil role): in every meeting, within a limited betting time, the Gambler secretly bets on
  who gets ejected, or that no one is (skip or tie): during the voting they click a player (or Skip Vote) and pick the
  "Bet" button next to Confirm (like the Prosecutor's), before casting their own vote. A correct
  bet gives a point, a wrong one takes a point away (never below zero). Betting that no one is ejected can win only
  once per game by default, because early meetings usually end with a skip. If the player they bet on dies during the
  meeting, the bet is cancelled. Collecting enough points while alive wins the game.
  - The points, the betting time left and the bet are shown under the meeting timer, and the player the Gambler bet on
    gets a ♦ next to their name (only for the Gambler).
  - Betting time: by default the first 20 seconds of the voting, so the Gambler can't just wait until everyone agreed on
    a vote. Can be changed to the whole voting.
  - When the Gambler gets close to winning, everyone sees a flash and a warning, so the win doesn't come out of
    nowhere. Optionally the Gambler's role is also revealed on their name.
  - Lobby options: betting time and its limit, points needed to win (4 by default), wrong bet takes a point, how many "no ejection" bets can win, win ends the game (or the
    Gambler wins together with the winners at the end), can use the emergency button, warn others when close to
    winning (off / alert / alert and reveal) and at how many points left.
- **Auditor** (new Crewmate Investigative role): secretly audits a nearby player for 25 seconds, even after walking
  away. The report in the next meeting shows the tasks the target completed, the task stations where they stood without
  finishing anything (typical for faking tasks, but also for the first step of a longer task) and one number of
  irregularities: vents, kills and role abilities, crewmate abilities included. Verdict: verified, unverified,
  irregularities or inconclusive. Auditing counts as an interaction, so a Veteran on alert kills the Auditor.
  - Starts with one audit and gets another for every 3 completed tasks (up to 3 per game).
  - Public audit: once per game, in a meeting, the Auditor can show the latest report to everyone, which reveals them.
  - Optional: the target is told it is being audited, and the Auditor can once per game freeze the vote of a player
    whose audit found irregularities (off by default).
- **Rigger** (new Impostor Support role): rigs real task stations (up to 2 at once). When a crewmate keeps a rigged
  task open for a moment (1.5 seconds by default) without finishing it, the trap fires: the impostors get an alert
  with an arrow to the station, and the task jams and closes (no task progress is lost, the station just doesn't work
  for a few seconds). A crewmate who finishes the task sooner sets nothing off and the station stays rigged. Rigged
  stations spark up close, so careful crewmates can notice them. Rigs are cleared at every meeting. Everything is
  configurable (cooldown, number of rigs, who gets the alert, trigger time, jam, sparks).
  - Optional lethal traps (off by default): the crewmate the trap fires on dies, while the Rigger is alive, up to a
    number of trap kills per game. Shields protect as usual.
- **Insider** (new Impostor Concealing role): an impostor who can really do the tasks from their list. Everyone sees
  them being done (visual tasks too, and the Auditor reports them as verified). Every completed task permanently lowers
  the Insider's kill cooldown a little (1 s by default, down to a minimum of 10 s). By default their tasks also count
  as crew progress (added to the completed tasks, not to the total), so working for a faster kill also brings the crew
  closer to a task win.

### Changed

- The Freeplay "Settings" button is now also available to the host in multiplayer games with the "Freeplay Mode" host
  option, so options can be changed without making a new lobby.
- Your Discord status now shows "Town of Us: Mira - AmonRa Edition" with our own icon (before: "Town of Us Mira").

### Fixed

- The AmonRa modifier icon in the settings is now the same size as the other icons.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Hazardzista** (Gambler, nowa rola Neutral Evil): na każdym spotkaniu, w ograniczonym czasie, Hazardzista
  potajemnie obstawia, kto zostanie wyrzucony, albo że nikt (skip lub remis): w czasie głosowania klika gracza (albo
  Skip Vote) i wybiera przycisk „Zakład” obok „Potwierdź” (jak u Prosecutora), zanim odda własny głos. Trafny zakład daje punkt, chybiony zabiera punkt (nigdy poniżej zera). Zakład, że nikt nie wyleci, domyślnie może wygrać
  tylko raz na grę, bo pierwsze spotkania zwykle kończą się skipem. Jeśli obstawiony gracz zginie
  w trakcie spotkania, zakład przepada bez straty. Zebranie wystarczającej liczby punktów za życia oznacza wygraną.
  - Punkty, pozostały czas na zakład i sam zakład widać pod zegarem spotkania, a obstawiony gracz ma przy nicku ♦
    (widzi to tylko Hazardzista).
  - Czas na zakład: domyślnie pierwsze 20 sekund głosowania, żeby Hazardzista nie mógł po prostu poczekać, aż wszyscy
    uzgodnią głos. Można zmienić na całe głosowanie.
  - Gdy Hazardzista jest blisko wygranej, wszyscy widzą błysk ekranu i ostrzeżenie, więc wygrana nie spada znikąd.
    Opcjonalnie jego rola zostaje też ujawniona przy nicku.
  - Opcje w lobby: czas na zakład i jego limit, punkty potrzebne do wygranej (domyślnie 4), chybiony zakład zabiera punkt, ile zakładów „nikt nie wyleci” może wygrać,
    wygrana kończy grę (albo Hazardzista wygrywa razem ze zwycięzcami na końcu), może użyć przycisku spotkania,
    ostrzeganie innych przed wygraną (wyłączone / alert / alert i ujawnienie) i przy ilu brakujących punktach.
- **Audytor** (Auditor, nowa rola Crewmate Investigative): potajemnie audytuje pobliskiego gracza przez 25 sekund,
  nawet po odejściu. Raport na najbliższym spotkaniu pokazuje ukończone przez cel zadania, stanowiska, przy których stał
  bez ukończenia czegokolwiek (typowe dla udawania zadań, ale też dla pierwszego etapu dłuższego zadania), i jedną
  liczbę nieprawidłowości: wenty, zabójstwa i zdolności ról, także załogantów. Werdykt: zweryfikowany,
  niezweryfikowany, nieprawidłowości albo nierozstrzygnięty. Audyt liczy się jako interakcja, więc Veteran na alercie
  zabija Audytora.
  - Na start jeden audyt, kolejny za każde 3 ukończone zadania (maks. 3 na grę).
  - Audyt publiczny: raz na grę, na spotkaniu, Audytor może pokazać wszystkim ostatni raport, co go ujawnia.
  - Opcjonalnie: cel wie, że jest audytowany, a Audytor może raz na grę zamrozić głos gracza, u którego audyt wykazał
    nieprawidłowości (domyślnie wyłączone).
- **Monter** (Rigger, nowa rola Impostor Support): przerabia prawdziwe stanowiska zadań (do 2 naraz). Gdy załogant
  trzyma przerobione zadanie otwarte przez chwilę (domyślnie 1,5 sekundy) i go nie kończy, pułapka się odpala:
  Impostorzy dostają alarm ze strzałką do stanowiska, a zadanie zacina się i zamyka (postęp zadań nie przepada,
  stanowisko po prostu przez kilka sekund nie działa). Kto skończy zadanie wcześniej, niczego nie uruchamia, a
  stanowisko zostaje przerobione. Przerobione stanowiska iskrzą z bliska, więc uważni załoganci mogą je zauważyć.
  Przeróbki znikają na każdym spotkaniu. Wszystko jest konfigurowalne (cooldown, liczba przeróbek, kto dostaje alarm,
  czas do odpalenia, zacięcie, iskry).
  - Opcjonalnie zabójcze pułapki (domyślnie wyłączone): załogant, na którym pułapka się odpali, ginie, jeśli Monter
    żyje, do limitu zabójstw z pułapek na grę. Tarcze chronią jak zwykle.
- **Wtyczka** (Insider, nowa rola Impostor Concealing): Impostor, który naprawdę może robić zadania ze swojej listy.
  Wszyscy widzą, że są wykonywane (także zadania wizualne, a Audytor raportuje go jako zweryfikowanego). Każde ukończone
  zadanie na stałe trochę skraca cooldown zabójstwa Wtyczki (domyślnie o 1 s, do minimum 10 s). Domyślnie jej zadania
  liczą się też do postępu załogi (doliczane do ukończonych, nie do wszystkich), więc praca na szybsze zabójstwo
  przybliża też załogę do wygranej na zadania.

**Zmieniono**

- Przycisk „Ustawienia” z Freeplay jest teraz dostępny także dla hosta w grze wieloosobowej z opcją hosta
  „Freeplay Mode”, więc opcje można zmieniać bez zakładania nowego lobby.
- Status w Discordzie pokazuje teraz „Town of Us: Mira - AmonRa Edition” z naszą ikoną (wcześniej „Town of Us Mira”).

**Naprawiono**

- Ikona modyfikatora AmonRa w ustawieniach ma teraz ten sam rozmiar co pozostałe ikony.

</details>

## [1.7.9-are.3] - 2026-11-10

### Added

- **Freeplay:** a "Settings" button next to "Set Roles" / "Set Modifiers" opens the lobby settings menu, so you can
  change options (e.g. AmonRa speed) without going back to a lobby. Changes are saved like in the lobby.
- **Updates:** an "Update" button in the main menu reopens the update progress window after it was closed.

### Changed

- **Main menu:** "Source Code" opens the AmonRa Edition repository and "Discord Server" opens our Discord server
  instead of the Town of Us: Mira ones.

### Fixed

- **Updates:** the update no longer fails with "being used by another process" on the first start after the mod files
  were replaced (BepInEx kept them open). If the files are still locked, the downloaded update is kept and installed
  after a restart without downloading it again. Works for updates from this version on.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Freeplay:** przycisk „Ustawienia” obok „Set Roles” / „Set Modifiers” otwiera menu ustawień lobby, więc opcje
  (np. prędkość AmonRa) można zmieniać bez wracania do lobby. Zmiany zapisują się tak jak w lobby.
- **Aktualizacje:** przycisk „Aktualizacja” w menu głównym ponownie otwiera okno postępu aktualizacji po jego zamknięciu.

**Zmieniono**

- **Menu główne:** „Source Code” otwiera repozytorium AmonRa Edition, a „Discord Server” nasz serwer Discord zamiast
  tych od Town of Us: Mira.

**Naprawiono**

- **Aktualizacje:** aktualizacja nie kończy się już błędem „being used by another process” przy pierwszym uruchomieniu
  po podmianie plików moda (BepInEx trzymał je otwarte). Jeśli pliki nadal są zablokowane, pobrana aktualizacja zostaje
  na dysku i instaluje się po ponownym uruchomieniu bez ponownego pobierania. Działa przy aktualizacjach od tej wersji.

</details>

## [1.7.9-are.2] - 2026-11-10

### Added

- **AmonRa:** the big rear wheel turns while rolling.
- **AmonRa:** rolling sound (placeholder for now), louder and higher-pitched the faster you go. Lobby options
  "Rolling Sound" and "Others Hear The Wheelchair Nearby" (off by default; when on, only while the chair is visible).

### Changed

- **AmonRa: new steering that rewards good driving.** The chair turns quickly when slow but needs wide turns at full
  speed. Smooth steering keeps the momentum through rooms and corridors; yanking the controls far away from the
  current direction makes it skid and lose speed. Sharp turns at full speed cost noticeably more speed than smooth
  ones. The "Turning Grip" option was replaced by "Turn Speed At Top Speed" (150°/s by default).
- **AmonRa:** bigger rear wheel placed further back and a leg rest plate in front, so the legs no longer peek out.
- **AmonRa:** listed as a *Universal Passive* modifier (like Drunk) instead of *Universal Visibility*.
- **Startup screen:** the Innersloth, AU Avengers and AmonRa Edition logos form a centered triangle.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **AmonRa:** duże tylne koło obraca się podczas jazdy.
- **AmonRa:** dźwięk toczenia (na razie tymczasowy), głośniejszy i wyższy przy większej prędkości. Opcje w lobby
  „Dźwięk toczenia” i „Inni słyszą wózek w pobliżu” (domyślnie wyłączone; po włączeniu tylko wtedy, gdy wózek widać).

**Zmieniono**

- **AmonRa: nowe sterowanie, które nagradza dobrą jazdę.** Wolno jadący wózek skręca szybko, ale przy pełnej prędkości
  potrzebuje szerokich łuków. Płynne prowadzenie zachowuje rozpęd w pomieszczeniach i korytarzach, a szarpnięcie
  w kierunku dalekim od kierunku jazdy powoduje poślizg i utratę prędkości. Ostry skręt przy pełnej prędkości kosztuje
  wyraźnie więcej prędkości niż płynny. Opcję „Przyczepność na zakrętach” zastąpiła „Szybkość skrętu przy pełnej
  prędkości” (domyślnie 150°/s).
- **AmonRa:** większe tylne koło przesunięte do tyłu i osłona na nogi z przodu, dzięki czemu nie widać już nóg.
- **AmonRa:** kategoria *Universal Passive* (jak Drunk) zamiast *Universal Visibility*.
- **Ekran startowy:** loga Innersloth, AU Avengers i AmonRa Edition tworzą wyśrodkowany trójkąt.

</details>

## [1.7.9-are.1] - 2026-11-10

First release of AmonRa Edition. Based on Town of Us: Mira 1.7.9 (`dev` branch), Among Us 2026.9.29.

### Added

- **AmonRa modifier** (universal, anyone can get it): the player rides a wheelchair.
  - Movement with inertia: gradual acceleration, keeps rolling after letting go of the controls, slides in turns.
  - Higher top speed on long straight lines (1.3× normal speed by default).
  - Visible wheelchair and a "sitting" animation instead of running, without footstep sounds. The wheelchair is hidden
    during camouflage (comms sabotage) by default.
  - Vents, ladders, ziplines and tasks work as usual.
  - Lobby options: amount and chance, top speed, acceleration time, rolling time, turning grip, show wheelchair,
    hide wheelchair during camouflage.
- **Automatic updates**: on startup the game checks for the latest AmonRa Edition release, downloads it in the background
  (verifying its SHA-256 checksum) and asks you to restart the game. Can be turned off in the config
  (`[Updater] AutoUpdate`).
- **Full installation package** for Steam / Epic Games / Microsoft Store (x64) with instructions in Polish.
- **AmonRa Edition logo** on the startup screen, under the Innersloth and AU Avengers logos (placeholder for now).

### Changed

- Mod name: "Town of Us: Mira - AmonRa Edition" (mod list, options menu "TOU Mira ARE", Discord status).
- Main menu announcements are loaded from the mod instead of the upstream repository.
- The installation package uses `version.dll` instead of `winhttp.dll`. On some systems (Windows 11 Insider builds)
  BepInEx with `winhttp.dll` closed the game on the first start.

<details>
<summary><i>Po polsku</i></summary>

Pierwsze wydanie AmonRa Edition. Bazuje na Town of Us: Mira 1.7.9 (branch `dev`), Among Us 2026.9.29.

**Dodano**

- **Modyfikator AmonRa** (uniwersalny, może trafić do każdego): postać jeździ na wózku.
  - Ruch z bezwładnością: stopniowe rozpędzanie, toczenie się po puszczeniu klawiszy, ślizg na zakrętach.
  - Na długich prostych wyższa prędkość maksymalna (domyślnie 1,3× normalnej).
  - Widoczny wózek i „siedząca” animacja zamiast biegu, bez dźwięku kroków. Wózek domyślnie znika podczas
    kamuflażu (sabotaż komunikacji).
  - Wenty, drabiny, zipline i taski działają normalnie.
  - Opcje w lobby: liczba i szansa, prędkość maksymalna, czas rozpędzania, czas toczenia, przyczepność na zakrętach,
    pokazywanie wózka, ukrywanie wózka w kamuflażu.
- **Automatyczne aktualizacje**: przy starcie gra sprawdza najnowsze wydanie AmonRa Edition, pobiera je w tle
  (z weryfikacją sumy SHA-256) i prosi o ponowne uruchomienie gry. Można to wyłączyć w configu (`[Updater] AutoUpdate`).
- **Pełna paczka instalacyjna** dla Steam / Epic Games / Microsoft Store (x64) z instrukcją po polsku.
- **Logo AmonRa Edition** na ekranie startowym gry, pod logami Innersloth i AU Avengers (na razie tymczasowe).

**Zmieniono**

- Nazwa moda: „Town of Us: Mira - AmonRa Edition” (lista modów, menu opcji „TOU Mira ARE”, status Discord).
- Ogłoszenia w menu gry są wczytywane z moda, a nie z repozytorium upstreamu.
- Paczka instalacyjna używa `version.dll` zamiast `winhttp.dll`. Na części systemów (Windows 11 Insider) BepInEx
  z `winhttp.dll` zamykał grę przy pierwszym uruchomieniu.

</details>

[Unreleased]: https://github.com/michalekjarmark/TOU-Mira-ARE/compare/v1.7.9-are.4...are
[1.7.9-are.4]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.4
[1.7.9-are.3]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.3
[1.7.9-are.2]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.2
[1.7.9-are.1]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.1
