# Changelog

All notable changes to **Town of Us: Mira - AmonRa Edition** are documented in this file.
Every version has a Polish translation below its English notes (*Po polsku*).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions look like
`<upstream version>-are.<number>`, e.g. `1.7.9-are.2` is the second AmonRa Edition release based on Town of Us: Mira 1.7.9.
Changes of Town of Us: Mira itself are described [upstream](https://github.com/AU-Avengers/TOU-Mira/releases).

## [Unreleased]

### Added

- **Gambler** (new Neutral Evil role): in every meeting, within a limited betting time, the Gambler secretly bets on
  who gets ejected, or that no one is (skip or tie): during the voting they click a player (or Skip Vote) and pick the
  "Bet" chip next to Confirm, before casting their own vote. A correct
  bet gives a point, a wrong one takes a point away (never below zero). Betting that no one is ejected can win only
  once per game by default, because early meetings usually end with a skip. If the player they bet on dies during the
  meeting, the bet is cancelled. Collecting enough points while alive wins the game.
  - Betting time: by default the first 20 seconds of the voting, so the Gambler can't just wait until everyone agreed on
    a vote. Can be changed to the whole voting.
  - When the Gambler gets close to winning, everyone sees a flash and a warning, so the win doesn't come out of
    nowhere. Optionally the Gambler's role is also revealed on their name.
  - Lobby options: betting time and its limit, points needed to win (4 by default), wrong bet takes a point, how many "no ejection" bets can win, win ends the game (or the
    Gambler wins together with the winners at the end), can use the emergency button, warn others when close to
    winning (off / alert / alert and reveal) and at how many points left.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Hazardzista** (Gambler, nowa rola Neutral Evil): na każdym spotkaniu, w ograniczonym czasie, Hazardzista
  potajemnie obstawia, kto zostanie wyrzucony, albo że nikt (skip lub remis): w czasie głosowania klika gracza (albo
  Skip Vote) i wybiera żeton „Zakład” obok „Potwierdź”, zanim odda własny głos. Trafny zakład daje punkt, chybiony zabiera punkt (nigdy poniżej zera). Zakład, że nikt nie wyleci, domyślnie może wygrać
  tylko raz na grę, bo pierwsze spotkania zwykle kończą się skipem. Jeśli obstawiony gracz zginie
  w trakcie spotkania, zakład przepada bez straty. Zebranie wystarczającej liczby punktów za życia oznacza wygraną.
  - Czas na zakład: domyślnie pierwsze 20 sekund głosowania, żeby Hazardzista nie mógł po prostu poczekać, aż wszyscy
    uzgodnią głos. Można zmienić na całe głosowanie.
  - Gdy Hazardzista jest blisko wygranej, wszyscy widzą błysk ekranu i ostrzeżenie, więc wygrana nie spada znikąd.
    Opcjonalnie jego rola zostaje też ujawniona przy nicku.
  - Opcje w lobby: czas na zakład i jego limit, punkty potrzebne do wygranej (domyślnie 4), chybiony zakład zabiera punkt, ile zakładów „nikt nie wyleci” może wygrać,
    wygrana kończy grę (albo Hazardzista wygrywa razem ze zwycięzcami na końcu), może użyć przycisku spotkania,
    ostrzeganie innych przed wygraną (wyłączone / alert / alert i ujawnienie) i przy ilu brakujących punktach.

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

[Unreleased]: https://github.com/michalekjarmark/TOU-Mira-ARE/compare/v1.7.9-are.3...are
[1.7.9-are.3]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.3
[1.7.9-are.2]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.2
[1.7.9-are.1]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.1
