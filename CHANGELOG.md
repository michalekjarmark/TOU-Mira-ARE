# Changelog

All notable changes to **Town of Us: Mira - AmonRa Edition** are documented in this file.
Every version has a Polish translation below its English notes (*Po polsku*).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions look like
`<upstream version>-are.<number>`, e.g. `1.7.9-are.2` is the second AmonRa Edition release based on Town of Us: Mira 1.7.9.
Changes of Town of Us: Mira itself are described [upstream](https://github.com/AU-Avengers/TOU-Mira/releases).

## [Unreleased]

### Added

- **Rivals** (new alliance modifier, the opposite of the Lovers): two players know each other as rivals. Both see a
  ≠ after their own name and after their rival's, but not the rival's role. Each of them wins only if the other one
  doesn't survive until the end of the game. This comes on top of the normal win of their team (or of their own role):
  if the rival is still alive at the end, the Rival loses even when their team wins. A rival who left the game counts
  as not surviving. There is no message when the rival dies.
  - Lobby options: Rivals chance (0% by default), chance that one of the rivals is a killer (an Impostor, or a Neutral
    Killing role if neutral roles can be rivals; 20% by default), both rivals can be killers (off by default) and
    neutral roles can be rivals (on by default). For crewmates only, set 0% and turn neutral roles off.
  - In Freeplay, "Set Modifiers" makes two players Rivals, like the Lovers.
- **Possum** (new Impostor Killing role): plays dead. The Possum disappears and their own body lies in their place.
  Reporting that body doesn't start a meeting: the Possum gets up and the reporter is stunned for a few seconds (they
  can't move or use anything), which is the Possum's chance to kill them if their kill is ready. The Possum gets up with
  the button, or on their own when the time runs out or a meeting starts. Other abilities that use bodies (Janitor,
  Undertaker, Amnesiac, Altruist...) don't see the fake body.
  - Lobby options: cooldown, max time playing dead, uses per game and the reporter's stun duration.
- **Bleeder** (new crewmate modifier): after the Bleeder dies, their killer leaves bloody footprints for a while.
  Everyone sees them, but they don't have the killer's color. Indirect kills (fire, bombs, traps) leave no trail, and a
  meeting wipes it. Options: trail time, footprint time, venting ends the trail, the killer is told.
- **Colorblind** (new universal modifier): players further than a few steps away look camouflaged to the Colorblind
  (grey, no name and by default no hat, skin, visor or pet); up close they look normal. Only the Colorblind's view
  changes and meetings look normal. Options: recognition range, also hide cosmetics.
- **Sleepwalker** (new universal modifier): after standing still for a while (20 s by default), the Sleepwalker starts
  walking on their own, slower than normal, in a random direction that changes now and then and at walls. Any movement
  wakes them up. Doing a task, being in a vent or a meeting doesn't count as standing still. Options: time standing
  still, sleepwalking speed, sleepwalking time.
- **Comfy** (new Crewmate Support role, an idea of ours): gets annoyed when a task doesn't go their way. Closing one of
  their own tasks before doing anything in it locks that task for them for a while (20 s by default) and, if their
  sulk is ready, starts it: for a few seconds (5 s) kills and abilities used on the Comfy fail, like on the Cleric's
  barrier, but the Comfy can't do tasks or report bodies. The sulk then needs a cooldown (30 s).
  - "Offload Task": while a task is locked, the Comfy can give it to a player nearby (an interaction, 2 per game by
    default). The receiver gets the task, the Comfy sees whether they did it, and a task that isn't done comes back to
    the Comfy at the start of the next meeting. An impostor can't do tasks, so offloading is also a test.
  - Every number and rule is a lobby option: lock time, sulk time and cooldown, whether the sulk stops kills and
    abilities, blocks tasks, body reports and the meeting button, whether others see the sulk, offloads per game and
    their cooldown, whether the task comes back and whether the receiver sees the Comfy's name.
- **Lighthouse Keeper** (new Crewmate Support role): places lanterns on the map (3 per game by default), visible to
  everyone. During a lights sabotage, every player within a lantern's radius sees as if the lights were on, like with
  the Torch modifier. Lanterns stay until the end of the game. Options: cooldown, lanterns per game, light radius,
  walls block the light, everyone sees the lanterns, lanterns go out when the Keeper dies.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Rywale** (Rivals, nowy modyfikator Alliance, odwrotność Kochanków): dwóch graczy wie, że są rywalami. Obaj widzą ≠
  przy swoim nicku i przy nicku rywala, ale nie znają jego roli. Każdy z nich wygrywa tylko wtedy, gdy drugi nie
  dożyje końca gry. To warunek dodatkowy obok zwykłej wygranej drużyny (albo własnej roli): jeśli rywal żyje na koniec
  gry, Rywal przegrywa, nawet gdy jego drużyna wygra. Rywal, który opuścił grę, liczy się jako ten, który nie przeżył.
  Śmierć rywala nie daje żadnego komunikatu.
  - Opcje lobby: szansa na Rywali (domyślnie 0%), szansa, że jeden z Rywali jest zabójcą (impostorem albo rolą
    Neutral Killing, jeśli neutralni mogą być Rywalami; domyślnie 20%), obaj Rywale mogą być zabójcami (domyślnie nie)
    i neutralni mogą być Rywalami (domyślnie tak). Same osoby z załogi: 0% i bez ról neutralnych.
  - We Freeplay „Set Modifiers” robi z dwóch graczy Rywali, tak jak z Kochanków.
- **Symulant** (Possum, nowa rola Impostor Killing): udaje trupa. Symulant znika, a w jego miejscu leży jego ciało.
  Zgłoszenie tego ciała nie zaczyna spotkania: Symulant wstaje, a zgłaszający jest na kilka sekund ogłuszony (nie może
  się ruszać ani niczego używać). To szansa Symulanta na zabójstwo, jeśli ma je gotowe. Symulant wstaje przyciskiem, a
  sam, gdy skończy się czas albo zacznie się spotkanie. Inne umiejętności, które używają ciał (Janitor, Undertaker,
  Sklerotyk, Altruista...), nie widzą fałszywego ciała.
  - Opcje lobby: cooldown, maks. czas udawania trupa, użycia na grę i czas ogłuszenia zgłaszającego.
- **Krwawiący** (Bleeder, nowy modyfikator załogi): po śmierci Krwawiącego jego zabójca przez chwilę zostawia krwawe
  ślady stóp. Widzą je wszyscy, ale nie mają koloru zabójcy. Zabójstwa pośrednie (ogień, bomby, pułapki) nie zostawiają
  śladu, a spotkanie go zmywa. Opcje: czas krwawienia, czas widoczności śladu, went kończy ślad, komunikat dla zabójcy.
- **Daltonista** (Colorblind, nowy modyfikator uniwersalny): gracze dalej niż kilka kroków wyglądają dla Daltonisty jak
  w kamuflażu (szarzy, bez nicku i domyślnie bez czapki, skina, wizjera i peta), a z bliska normalnie. Zmienia się
  tylko to, co widzi Daltonista, a spotkania wyglądają normalnie. Opcje: zasięg rozpoznawania, ukrywanie kosmetyków.
- **Lunatyk** (Sleepwalker, nowy modyfikator uniwersalny): po dłuższym staniu w miejscu (domyślnie 20 s) Lunatyk zaczyna
  sam chodzić, wolniej niż normalnie, w losowym kierunku, który zmienia się co jakiś czas i przy ścianach. Każdy ruch go
  budzi. Robienie zadania, went i spotkanie nie liczą się jako stanie. Opcje: czas stania, prędkość lunatykowania, czas
  lunatykowania.
- **Wygodniś** (Comfy, nowa rola Crewmate Support, nasz pomysł): denerwuje się, gdy zadanie nie idzie po jego myśli.
  Zamknięcie własnego zadania, zanim cokolwiek się w nim zrobi, blokuje to zadanie na jakiś czas (domyślnie 20 s) i,
  jeśli foch jest gotowy, włącza go: przez kilka sekund (5 s) zabójstwa i umiejętności użyte na Wygodnisiu się nie
  udają, jak przy barierze Kleryka, ale Wygodniś nie może robić zadań ani zgłaszać ciał. Potem foch ma cooldown (30 s).
  - „Zrzuć robotę”: póki zadanie jest zablokowane, Wygodniś może je dać graczowi obok (to interakcja, domyślnie 2 razy
    na grę). Odbiorca dostaje zadanie, Wygodniś widzi, czy zostało zrobione, a niezrobione zadanie wraca do niego na
    początku najbliższego spotkania. Impostor nie może robić zadań, więc zrzucenie jest też testem.
  - Każda liczba i zasada jest opcją w lobby: czas blokady, czas i cooldown focha, czy foch chroni przed zabójstwami i
    umiejętnościami, czy blokuje zadania, zgłaszanie ciał i przycisk spotkania, czy inni widzą focha, liczba i cooldown
    zrzuceń, czy zadanie wraca i czy odbiorca widzi nick Wygodnisia.
- **Latarnik** (Lighthouse Keeper, nowa rola Crewmate Support): stawia latarnie na mapie (domyślnie 3 na grę), widoczne
  dla wszystkich. W trakcie sabotażu świateł każdy gracz w promieniu latarni widzi tak, jakby światła działały, jak z
  modyfikatorem Torch. Latarnie stoją do końca gry. Opcje: cooldown, liczba latarni na grę, promień światła, ściany
  zatrzymują światło, wszyscy widzą latarnie, latarnie gasną po śmierci Latarnika.

</details>

## [1.7.9-are.6] - 2026-11-12

### Added

- **Ventriloquist** (new Impostor Support role): during the round, the Ventriloquist eavesdrops on a nearby player with
  the "Eavesdrop" button (an interaction, so for example a Veteran on alert or a Warden's fortification stops it, and
  the Auditor counts it). At the next meeting they can switch the chat to "Voice: (player)", with the same button as the
  impostor chat, and write a message that everyone sees as a normal message from that player: same name, color and
  look. The player whose voice was used sees the message as their own and can deny it, but doesn't learn who wrote it.
  - The voice works for one meeting only and a new eavesdrop replaces the old one. On the Ventriloquist's screen the
    player gets a ♪ after their name, and the meeting screen shows the voice and the messages left.
  - A voice can't be used while its owner is dead, blackmailed or jailed without access to the public chat.
  - Dead players with "The Dead Know" see who really wrote the message.
  - **Vote steal** (lobby option, by default "instead of chat"): instead of writing, the Ventriloquist can steal that
    player's vote. In the vote popup of any living player (or Skip Vote) there is a "Steal Vote" button, available
    before their own vote. When the votes are counted, every vote of the eavesdropped player goes there (all three of
    a revealed Mayor), also when that player doesn't vote at all. The player votes as usual; with open voting their
    icon shows up at the chosen player in the results. A prosecution can't be stolen: in a meeting where the
    Prosecutor prosecutes, the stolen vote doesn't count. With "with chat" the Ventriloquist can both write and steal
    the vote, with "off" only write.
  - Lobby options: eavesdrop cooldown (20 s by default), eavesdrops per game (no limit by default), messages per
    meeting (1 by default) and vote steal (off / instead of chat / with chat).
- **Vandal** (new Impostor Support role): at a task station, the Vandal can break a task that someone has already
  finished there with the "Break" button. The task goes back to its owner as not done, from the first step, and the
  task bar drops.
  - By default one use breaks the task of one random player who finished it. With the lobby option it breaks it for
    everyone who did.
  - The owner gets a message, by default with the task's name and the room of the station, and has to do the task
    again. A Workaholic can take it over.
  - Only tasks of living players that count toward the task bar can be broken. The Snitch's tasks are safe, and so are
    tasks that use up objects on the map (towels, records, guns and the like). For a task with several stations, such
    as Fix Wiring, any station of that type works.
  - The Auditor counts every use.
  - Lobby options: break cooldown (30 s by default), breaks per game (no limit by default), whose task one use breaks
    (one random player / everyone who did it) and what the owner is told (with the task's name / without it / nothing).

### Fixed

- A vote frozen by the Auditor now also doesn't count when a Tiebreaker breaks a tie or the Monarch's extra votes are
  hidden.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Brzuchomówca** (Ventriloquist, nowa rola Impostor Support): w trakcie rundy Brzuchomówca podsłuchuje gracza obok
  siebie przyciskiem „Podsłuchaj” (to interakcja, więc zatrzyma ją np. Veteran na alercie albo fortyfikacja Wardena,
  a Audytor ją policzy). Na najbliższym spotkaniu może przełączyć czat na „Głos: (gracz)”, tym samym przyciskiem co
  czat impostorów, i napisać wiadomość, którą wszyscy zobaczą jako zwykłą wiadomość od tego gracza: z jego nickiem,
  kolorem i wyglądem. Gracz, którego głosu użyto, widzi tę wiadomość jako swoją i może zaprzeczyć, ale nie wie, kto ją
  napisał.
  - Głos działa tylko na jednym spotkaniu, a nowy podsłuch zastępuje poprzedni. Na ekranie Brzuchomówcy gracz ma ♪
    przy nicku, a na spotkaniu widać głos i liczbę pozostałych wiadomości.
  - Głosu nie da się użyć, gdy jego właściciel nie żyje, jest uciszony przez Szantażystę albo uwięziony przez Klawisza
    bez dostępu do czatu publicznego.
  - Martwi gracze z opcją „The Dead Know” widzą, kto naprawdę napisał wiadomość.
  - **Kradzież głosu** (opcja lobby, domyślnie „zamiast czatu”): zamiast pisać, Brzuchomówca może przejąć głos tego
    gracza. W okienku głosowania każdego żywego gracza (i przy Skip Vote) jest przycisk „Przejmij głos”, dostępny przed
    oddaniem własnego głosu. Przy liczeniu wszystkie głosy podsłuchanego gracza trafiają tam (u ujawnionego Burmistrza
    wszystkie trzy), także wtedy, gdy ten gracz w ogóle nie zagłosuje. Gracz głosuje normalnie, a przy jawnym
    głosowaniu jego ikona pojawia się w wynikach przy wybranym graczu. Oskarżenia nie da się przejąć: na spotkaniu,
    na którym Prokurator oskarża, przejęty głos się nie liczy. Przy „razem z czatem” Brzuchomówca może i pisać,
    i przejąć głos, a przy „wyłączona” tylko pisać.
  - Opcje lobby: cooldown podsłuchu (domyślnie 20 s), podsłuchy na grę (domyślnie bez limitu), wiadomości na spotkanie
    (domyślnie 1) i kradzież głosu (wyłączona / zamiast czatu / razem z czatem).
- **Wandal** (Vandal, nowa rola Impostor Support): przy stanowisku zadania Wandal może przyciskiem „Zepsuj” zepsuć
  zadanie, które ktoś już tam skończył. Zadanie wraca do właściciela jako niezrobione, od pierwszego kroku, a pasek
  zadań spada.
  - Domyślnie jedno użycie psuje zadanie jednej losowej osoby, która je zrobiła. Z opcją lobby psuje je wszystkim,
    którzy je zrobili.
  - Właściciel dostaje komunikat, domyślnie z nazwą zadania i pomieszczeniem stanowiska, i musi zrobić zadanie jeszcze
    raz. Może je też przejąć Pracoholik.
  - Można psuć tylko zadania żywych graczy, które liczą się do paska zadań. Zadania Snitcha są bezpieczne, podobnie jak
    zadania zużywające przedmioty na mapie (ręczniki, dokumenty, broń itp.). Przy zadaniu z kilkoma stanowiskami, jak
    Fix Wiring, wystarczy dowolne stanowisko tego typu.
  - Audytor liczy każde użycie.
  - Opcje lobby: cooldown psucia (domyślnie 30 s), psucia na grę (domyślnie bez limitu), czyje zadanie psuje jedno
    użycie (jednej losowej osoby / wszystkich, którzy je zrobili) i komunikat dla właściciela (z nazwą zadania / bez
    nazwy / brak).

**Naprawiono**

- Głos zamrożony przez Audytora nie liczy się także wtedy, gdy remis rozstrzyga Tiebreaker albo dodatkowe głosy
  Monarchy są ukryte.

</details>

## [1.7.9-are.5] - 2026-11-11

### Added

- **Workaholic** (new Crewmate Support role): after finishing all of their own tasks, the Workaholic can take over
  tasks of other crewmates with the "Take Task" button, one at a time. The task is always taken from the crewmate with
  the most tasks left (dead crewmates too, if ghosts do tasks), only tasks nobody has started yet are taken, and never
  the Snitch's. The crewmate learns that someone took over their task, sees it greyed out and can't do it; once the
  Workaholic finishes it, it counts as the crewmate's task. If the Workaholic dies, the task goes back to its owner.
  - Lobby options: cooldown after a finished task (10 s by default), whether a try that found no task to take also
    starts the cooldown (on by default; the use is never lost), a limit of taken tasks per game (no limit by default)
    and whether the crewmate sees the helper's name (off by default: just "someone").
  - The Auditor sees a task done by the Workaholic as the Workaholic's work, not its owner's.
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
  - Opcje lobby: cooldown po wykonanym zadaniu (domyślnie 10 s), czy nieudana próba (brak zadania do przejęcia) też
    włącza cooldown (domyślnie tak; użycie nigdy nie przepada), limit przejętych zadań na grę (domyślnie bez limitu)
    i czy właściciel widzi nick pomocnika (domyślnie nie: tylko „ktoś”).
  - Audytor widzi zadanie zrobione przez Pracoholika jako pracę Pracoholika, a nie właściciela.
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

[Unreleased]: https://github.com/michalekjarmark/TOU-Mira-ARE/compare/v1.7.9-are.6...are
[1.7.9-are.6]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.6
[1.7.9-are.5]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.5
[1.7.9-are.4]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.4
[1.7.9-are.3]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.3
[1.7.9-are.2]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.2
[1.7.9-are.1]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.1
