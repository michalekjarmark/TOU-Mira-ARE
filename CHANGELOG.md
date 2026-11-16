# Changelog

All notable changes to **Town of Us: Mira - AmonRa Edition** are documented in this file.
Every version has a Polish translation below its English notes (*Po polsku*).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions look like
`<upstream version>-are.<number>`, e.g. `1.7.9-are.2` is the second AmonRa Edition release based on Town of Us: Mira 1.7.9.
Changes of Town of Us: Mira itself are described [upstream](https://github.com/AU-Avengers/TOU-Mira/releases).

## [Unreleased]

### Added

- **Expert** (new Crewmate Investigative role): examines a nearby player and learns their exact role, shown on the
  player's name. With a chance (30% by default) the result is made up: a different role, by default one that can
  spawn in this game. The Expert never knows which results are wrong. By default each player can be examined only
  once, so the first result stays. Examining counts as an interaction. Options: cooldown, examinations per game,
  chance of a wrong result and its growth with every examination, what a wrong result shows (a role in this game /
  any role / a role of another faction), examining the same player again.
- **Moderator** (new Impostor Support role): bans a nearby player from one thing until the next meeting (or for a set
  time). The ban is drawn at random from the types enabled in the lobby: reporting bodies, the meeting button (also the
  Button Barry's), tasks, getting into vents, cams/admin/vitals (also the portable ones), abilities (kill and sabotage
  included) or the vote in the next meeting, which then doesn't count. With an option the Moderator picks the type with
  a second button. The banned player gets a message (option) and the Moderator sees Ø after their name. Sabotage
  repairs, doors and ladders are never blocked, and a player in a vent can always get out. The Cleric's Cleanse
  removes bans. Banning counts as an interaction. Options: cooldown, bans per game and per round, until the meeting or
  a set time, each ban type on/off, the Moderator chooses the type, the banned player gets a message.
- **Chairman** (new Crewmate Support role): orders a nearby player to finish any task within a short time (30 s by
  default). If they do, the Chairman takes the credit: one of their own tasks gets done, the player's vote counts
  twice in the next meeting and the Chairman gets closer to a promotion. If not, the player is slacking and their vote
  doesn't count in the next meeting. Impostors can't do tasks, so an order is also a test, but a crewmate far from
  their tasks slacks too. Promotions keep the earlier perks: Manager (start), Director (call a meeting from anywhere
  once per game), Chairman (see who voted for whom), King (double vote). An order counts as an interaction. Options:
  cooldown, time, orders per game, free task, bonus, reprimand, the player sees the Chairman's name, everyone sees
  promotions, each title on/off and its number of orders.
- **Handyman** (new Crewmate Support role): gets a random job at the start of every round, with an ability for
  that round (one use by default). PC Service: fix every active sabotage from anywhere, like the Engineer. Pizzeria:
  a short sprint (8 s, 1.5x speed by default). Warehouse: the admin map from anywhere (10 s). Stream: the cameras from
  anywhere (10 s, never drawn on maps without cameras). The button shows the job's icon. Options: uses per round,
  cooldown, the same job twice in a row, each job on/off with its time (fix delay, sprint time and speed, admin time
  and moving with the admin map, cameras time).
- **Streamer** (new universal modifier): the cameras are their stream. Whenever someone watches a camera feed with
  the Streamer in it (map cameras, Sentry cameras, the portable ones of the Operative and the Handyman, a Stream Sniper's window; with an option
  also an open admin map), the Streamer sees "LIVE: N viewers" at the top of the screen, without names. "Go offline"
  hides them from the camera feeds for a few seconds (10 s, once per round by default); everyone near them still sees
  them. Comms sabotage switches the stream off. The button takes the modifier button slot, so the Streamer never gets
  another modifier with a button. Options: the admin map counts (and "Go offline" hides from it too), show the number
  of viewers, the "Go offline" button, its time and uses per round.
- **Stream Sniper** (new Impostor Support role): watches another player's "stream". "Watch" opens a small window in
  a corner of the screen with the view around a chosen living player for a few seconds (8 s by default), through walls
  and darkness: where they go, who is near them and whether they are alone. The Stream Sniper moves normally meanwhile.
  By default the player is chosen from a list; with an option only the player marked earlier with "Mark" (next to the
  Stream Sniper, an interaction) can be watched. The watched player knows nothing (option), except a Streamer, who gets
  a viewer, and a Streamer who went offline doesn't show in the window. The window uses the Parasite's window settings.
  Options: cooldown, time, watches per game, view size, who can be watched, mark cooldown, the watched player knows.
- **Fabulist** (new Impostor Concealing role): "Alibi" leaves a copy of the Fabulist (look, name and cosmetics, but
  no skin) standing at a task console for a while (20 s by default), seen by everyone, on the cameras too, while the
  real Fabulist walks away. The copy disappears when another living player comes close to it (about a step), at a
  meeting or when the Fabulist dies; it can't be killed, reported or targeted. The admin map shows the Fabulist where
  they really are. Whoever sees them in two places at once knows everything. Options: cooldown, time, alibis per game
  (3 by default), the distance that breaks the alibi, only at a task console, the admin map counts the copy too.
- **Cryptid** (new Crewmate Afterlife role): a crewmate who died may become the Cryptid right away. "Appear" shows
  the ghost to the living for a few seconds (6 s by default) as a blurry grey figure without a name, on the cameras
  too, so nobody knows who it was. Nobody can get close: when a living player comes near, the figure is gone, and an
  impostor scares the Cryptid off for the rest of the game (option). The dead can't talk, so this is how a ghost points
  at a body, the killer or a room. The Cryptid still haunts and does tasks like any crewmate ghost. Options: cooldown,
  time, sightings per game (3 by default), the distance that makes it vanish, an impostor scares it off.
- **"AmonRa" toggle in the settings search:** the AmonRa icon at the right end of the search box shows only the
  AmonRa-themed roles and modifiers (AmonRa, Comfy, Expert, Moderator, Chairman, Handyman, Streamer, Stream Sniper,
  Fabulist, Cryptid) in the Roles and Modifiers tabs. It works together with the search text.

<details>
<summary><i>Po polsku</i></summary>

**Dodano**

- **Ekspert** (Expert, nowa rola Crewmate Investigative): bada gracza obok i poznaje jego dokładną rolę, widoczną
  potem przy nicku tego gracza. Z pewną szansą (domyślnie 30%) wynik jest zmyślony: to inna rola, domyślnie taka,
  która może wypaść w tej grze. Ekspert nigdy nie wie, które wyniki są błędne. Domyślnie każdego gracza można zbadać
  tylko raz, więc pierwszy wynik zostaje. Ekspertyza liczy się jako interakcja. Opcje: cooldown, liczba ekspertyz na
  grę, szansa na błędny wynik i jej wzrost z każdą ekspertyzą, co pokazuje błędny wynik (rolę z tej gry / dowolną
  rolę / rolę innej frakcji), ponowna ekspertyza tego samego gracza.
- **Moderator** (nowa rola Impostor Support): banuje gracza obok na jedną rzecz do najbliższego spotkania (albo na
  określony czas). Rodzaj bana jest losowany spośród włączonych w lobby: zgłaszanie ciał, przycisk awaryjny (także
  Button Barry), zadania, wchodzenie do wentów, kamery/admin/vitals (także przenośne), umiejętności (razem z zabijaniem
  i sabotażem) albo głos na najbliższym spotkaniu, który wtedy się nie liczy. Z opcją Moderator wybiera rodzaj drugim
  przyciskiem. Zbanowany dostaje komunikat (opcja), a Moderator widzi Ø przy jego nicku. Naprawa sabotażu, drzwi i
  drabiny nigdy nie są blokowane, a gracz w wencie zawsze może z niego wyjść. Oczyszczenie Clerica zdejmuje bany. Ban
  liczy się jako interakcja. Opcje: cooldown, liczba banów na grę i na rundę, do spotkania albo na określony czas, każdy
  rodzaj bana osobno, wybór rodzaju przez Moderatora, komunikat dla zbanowanego.
- **Prezes** (Chairman, nowa rola Crewmate Support): każe graczowi obok skończyć dowolne zadanie w krótkim czasie
  (domyślnie 30 s). Jeśli zdąży, Prezes przypisuje sobie zasługę: jedno jego zadanie kończy się samo, głos gracza na
  najbliższym spotkaniu liczy się podwójnie, a Prezes zbliża się do awansu. Jeśli nie zdąży, gracz się obija i jego
  głos na najbliższym spotkaniu się nie liczy. Impostorzy nie mogą robić zadań, więc zlecenie jest też testem, ale
  crewmate daleko od zadań też się „obija”. Awanse zachowują wcześniejsze przywileje: Kierownik (start), Dyrektor (raz
  na grę spotkanie z dowolnego miejsca), Prezes (widzi, kto na kogo głosował), Król (podwójny głos). Zlecenie liczy
  się jako interakcja. Opcje: cooldown, czas, liczba zleceń na grę, darmowe zadanie, premia, nagana, gracz widzi nick
  Prezesa, wszyscy widzą awanse, każdy tytuł osobno i jego liczba zleceń.
- **Złota Rączka** (Handyman, nowa rola Crewmate Support): na początku każdej rundy losuje etat z umiejętnością na
  tę rundę (domyślnie jedno użycie). Serwis: naprawa wszystkich trwających sabotaży z dowolnego miejsca, jak u
  Inżyniera. Pizzeria: krótki sprint (domyślnie 8 s, prędkość x1,5). Magazyn: mapa admina z dowolnego miejsca (10 s).
  Stream: kamery z dowolnego miejsca (10 s, nie wypada na mapach bez kamer). Przycisk ma ikonę etatu. Opcje: użycia na
  rundę, cooldown, ten sam etat dwa razy z rzędu, każdy etat osobno z jego czasem (opóźnienie naprawy, czas i
  prędkość sprintu, czas admina i ruch z otwartą mapą, czas kamer).
- **Streamer** (nowy modyfikator uniwersalny): kamery są jego streamem. Kiedy ktoś ogląda obraz z kamery, na którym
  jest Streamer (kamery mapy, kamery Sentry, przenośne kamery Operative i Złotej Rączki, okienko Snajpera; z opcją także otwarta mapa
  admina), Streamer widzi u góry ekranu „NA ŻYWO: N widzów”, bez nicków. „Wyłączam streama” ukrywa go na kilka sekund
  przed kamerami (domyślnie 10 s, raz na rundę); gracze obok nadal go widzą. Sabotaż komunikacji wyłącza stream.
  Przycisk zajmuje miejsce przycisku modyfikatora, więc Streamer nie dostanie drugiego modyfikatora z przyciskiem.
  Opcje: mapa admina się liczy (i „Wyłączam streama” ukrywa też przed nią), liczba widzów, przycisk „Wyłączam
  streama”, jego czas i użycia na rundę.
- **Snajper** (Stream Sniper, nowa rola Impostor Support): ogląda „stream” innego gracza. „Podgląd” otwiera w rogu
  ekranu małe okienko z widokiem wokół wybranego żywego gracza na kilka sekund (domyślnie 8 s), przez ściany i
  ciemność: dokąd idzie, kto jest obok i czy jest sam. W tym czasie Snajper rusza się normalnie. Domyślnie gracza
  wybiera się z listy; z opcją można oglądać tylko gracza oznaczonego wcześniej przyciskiem „Oznacz” (obok Snajpera,
  interakcja). Oglądany gracz nic nie wie (opcja), poza Streamerem, któremu przybywa widz, a Streamer z wyłączonym
  streamem nie pokazuje się w okienku. Okienko korzysta z ustawień okienka Parasite. Opcje: cooldown, czas, liczba
  podglądów na grę, rozmiar widoku, kogo można oglądać, cooldown oznaczenia, oglądany gracz wie.
- **Mitoman** (Fabulist, nowa rola Impostor Concealing): „Alibi” zostawia przy konsoli zadania kopię Mitomana
  (wygląd, nick i kosmetyki, ale bez skina), która stoi tam przez chwilę (domyślnie 20 s). Widzą ją wszyscy, także na
  kamerach, a prawdziwy Mitoman w tym czasie odchodzi. Kopia znika, gdy inny żywy gracz podejdzie do niej blisko
  (mniej więcej krok), przy spotkaniu albo gdy Mitoman zginie; nie da się jej zabić, zgłosić ani użyć na niej
  umiejętności. Mapa admina pokazuje Mitomana tam, gdzie naprawdę jest. Kto zobaczy go w dwóch miejscach naraz, wie
  wszystko. Opcje: cooldown, czas, liczba alibi na grę (domyślnie 3), dystans, który psuje alibi, tylko przy konsoli
  zadania, mapa admina liczy też kopię.
- **Kryptyda** (Cryptid, nowa rola Crewmate Afterlife): crewmate może nią zostać zaraz po śmierci. „Pojawienie się”
  pokazuje ducha żywym na kilka sekund (domyślnie 6 s) jako rozmytą szarą postać bez nicku, także na kamerach, więc
  nikt nie wie, kim był. Nikt nie może podejść: gdy żywy gracz się zbliży, postać znika, a impostor przepłasza Kryptydę
  do końca gry (opcja). Martwi nie mówią, więc tak duch wskazuje ciało, zabójcę albo pokój. Poza tym Kryptyda nawiedza
  i robi zadania jak każdy duch załogi. Opcje: cooldown, czas, liczba pojawień na grę (domyślnie 3), dystans, przy
  którym znika, impostor przepłasza.
- **Przełącznik „AmonRa” w wyszukiwarce ustawień:** ikona AmonRa na prawym końcu pola wyszukiwania pokazuje w
  zakładkach Roles i Modifiers tylko role i modyfikatory w klimacie AmonRa (AmonRa, Wygodniś, Ekspert, Moderator,
  Prezes, Złota Rączka, Streamer, Snajper, Mitoman, Kryptyda). Działa razem z wpisanym tekstem.

</details>

## [1.7.9-are.7] - 2026-11-14

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
    the Comfy at the start of the next meeting. An impostor can't do tasks, so offloading is also a test. A task goes
    only to a player who doesn't have an unfinished task of the same type.
  - Every number and rule is a lobby option: lock time, sulk time and cooldown, whether the sulk stops kills and
    abilities, blocks tasks, body reports and the meeting button, whether others see the sulk, offloads per game and
    their cooldown, whether the task comes back and whether the receiver sees the Comfy's name.
- **Lighthouse Keeper** (new Crewmate Support role): places lanterns on the map (3 per game by default), visible to
  everyone. During a lights sabotage, every player within a lantern's radius sees as if the lights were on, like with
  the Torch modifier. Lanterns stay until the end of the game. Options: cooldown, lanterns per game, light radius,
  walls block the light, everyone sees the lanterns, lanterns go out when the Keeper dies.
- **Search in the lobby settings:** a search box (on the left, in place of the tab description) filters the open tab:
  game settings, roles and modifiers. A role also shows up when one of its own settings matches. Polish letters can
  be typed without accents. Ctrl+A, Ctrl+C, Ctrl+X, Ctrl+V and Ctrl+Backspace work in the box.
- Modifier icons in the lobby settings, like the roles have: next to each modifier's amount and chance and in the
  header of its own settings.

### Changed

- Buttons whose effect can be ended at any time (Swooper's "Unswoop", Morphling's "Unmorph", Venerer, Glitch's
  "Unmimic", Medium, Monarch's knighting, Chameleon and the new Possum's "Stand Up") no longer look like they're on
  cooldown while the effect runs: the button is bright, without the grey shading, and the time left is shown in green.
  Effects that can't be ended keep the old look.
- The main menu shows the AmonRa Edition logo. The "Legacy" button (classic Town of Us logo and ability button art) is
  gone and the classic art is turned off.

### Fixed

- Local games: when the host is dead, the meeting goes on by itself after the voting results (a dead host couldn't
  click "Proceed").
- Freeplay with several players: "Set Modifiers" on another player now gives them the modifier in their own game too
  (it only changed the host's game).

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
    początku najbliższego spotkania. Impostor nie może robić zadań, więc zrzucenie jest też testem. Zadanie trafia
    tylko do gracza, który nie ma niezrobionego zadania tego samego typu.
  - Każda liczba i zasada jest opcją w lobby: czas blokady, czas i cooldown focha, czy foch chroni przed zabójstwami i
    umiejętnościami, czy blokuje zadania, zgłaszanie ciał i przycisk spotkania, czy inni widzą focha, liczba i cooldown
    zrzuceń, czy zadanie wraca i czy odbiorca widzi nick Wygodnisia.
- **Latarnik** (Lighthouse Keeper, nowa rola Crewmate Support): stawia latarnie na mapie (domyślnie 3 na grę), widoczne
  dla wszystkich. W trakcie sabotażu świateł każdy gracz w promieniu latarni widzi tak, jakby światła działały, jak z
  modyfikatorem Torch. Latarnie stoją do końca gry. Opcje: cooldown, liczba latarni na grę, promień światła, ściany
  zatrzymują światło, wszyscy widzą latarnie, latarnie gasną po śmierci Latarnika.
- **Wyszukiwarka w ustawieniach lobby:** pole wyszukiwania (po lewej, w miejscu opisu zakładki) filtruje otwartą
  zakładkę: ustawienia gry, role i modyfikatory. Rola pojawia się też wtedy, gdy pasuje któreś z jej ustawień. Polskie
  litery można wpisywać bez ogonków. W polu działają Ctrl+A, Ctrl+C, Ctrl+X, Ctrl+V i Ctrl+Backspace.
- Ikony modyfikatorów w ustawieniach lobby, tak jak przy rolach: przy liczbie i szansie każdego modyfikatora oraz w
  nagłówku jego własnych ustawień.

**Zmieniono**

- Przyciski, których efekt można przerwać w każdej chwili („Unswoop” Swoopera, „Unmorph” Morphlinga, Venerer,
  „Unmimic” Glitcha, Medium, pasowanie na rycerza u Monarchy, Chameleon i „Wstań” nowego Symulanta), w trakcie efektu
  nie wyglądają już jak na cooldownie: przycisk jest jasny, bez szarego zaciemnienia, a pozostały czas jest na
  zielono. Efekty, których nie da się przerwać, wyglądają jak dotąd.
- W menu głównym jest logo AmonRa Edition. Przycisku „Legacy” (klasyczne logo i grafiki przycisków z dawnego Town of
  Us) już nie ma, a klasyczne grafiki są wyłączone.

**Naprawiono**

- Gry lokalne: gdy host nie żyje, spotkanie samo idzie dalej po wynikach głosowania (martwy host nie mógł kliknąć
  „Proceed”).
- Freeplay z kilkoma graczami: „Set Modifiers” na innym graczu daje mu modyfikator także w jego grze (zmieniało tylko
  grę hosta).

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

[Unreleased]: https://github.com/michalekjarmark/TOU-Mira-ARE/compare/v1.7.9-are.7...are
[1.7.9-are.7]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.7
[1.7.9-are.6]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.6
[1.7.9-are.5]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.5
[1.7.9-are.4]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.4
[1.7.9-are.3]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.3
[1.7.9-are.2]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.2
[1.7.9-are.1]: https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.1
