<div align="center">
  <img src="TownOfUs/Resources/Custom/AreLogo.png" alt="Town of Us: Mira - AmonRa Edition" width="480"/>
  <h1>Town of Us: Mira - AmonRa Edition</h1>
  <a href="https://github.com/michalekjarmark/TOU-Mira-ARE/releases/latest"><img alt="Najnowsze wydanie" src="https://img.shields.io/github/v/release/michalekjarmark/TOU-Mira-ARE?label=wydanie"></a>
  <a href="https://store.steampowered.com/app/945360/Among_Us"><img alt="Wersja Among Us" src="https://img.shields.io/badge/Among%20Us-2026.9.29-yellow"></a>
  <a href="CHANGELOG.md"><img alt="Changelog" src="https://img.shields.io/badge/changelog-CHANGELOG.md-blue"></a>
  <p><a href="README.md">English</a> | <b>Polski</b></p>
</div>

> [!NOTE]
> To tłumaczenie [README.md](README.md). W razie różnic obowiązuje wersja angielska.

> [!NOTE]
> AmonRa Edition to mała, prywatna edycja moda [Town of Us: Mira](https://github.com/AU-Avengers/TOU-Mira) od AU Avengers,
> robiona do grania ze znajomymi. **Nie** jest powiązana z zespołem Town of Us ani przez niego wspierana, więc błędów
> z tej edycji nie zgłaszaj im. Szukasz oryginalnego moda? Zajrzyj do [oryginalnego repozytorium](https://github.com/AU-Avengers/TOU-Mira).

Mod do [Among Us](https://store.steampowered.com/app/945360/Among_Us) oparty na Town of Salem. AmonRa Edition zawiera
wszystko, co ma Town of Us: Mira, plus nasze własne role, modyfikatory i mechaniki. Aktualizuje się sam, więc po
pierwszej instalacji nic więcej nie trzeba robić.

-----------------------

# Spis treści

- [**Instalacja**](#instalacja)
- [**Aktualizacje**](#aktualizacje)
- [**Dodatki AmonRa Edition**](#dodatki-amonra-edition)
- [**Wersje**](#wersje)
- [**Lista zmian**](CHANGELOG.md)
- [**Problemy**](#problemy)
- [**Dla twórców**](#dla-twórców)
- [**Role i modyfikatory**](#role-i-modyfikatory)
- [**Podziękowania i licencja**](#podziękowania)

-----------------------

# Instalacja

Dla **Steam, Epic Games i Microsoft Store** (Windows, 64-bit).

1. Zaktualizuj Among Us do wersji podanej w tabeli [Wersje](#wersje).
2. Pobierz plik `TOU-Mira-ARE-...-x64-Steam-Epic.zip` z [najnowszego wydania](https://github.com/michalekjarmark/TOU-Mira-ARE/releases/latest)
   i rozpakuj go.
3. Otwórz folder z grą:
   - **Steam:** Biblioteka → prawy przycisk na „Among Us” → Zarządzaj → Przeglądaj pliki lokalne.
   - **Epic Games:** zwykle `C:\Program Files\Epic Games\AmongUs`.
4. Skopiuj **całą zawartość** rozpakowanego folderu (BepInEx, dotnet, CorsacCosmetics, `version.dll` i resztę)
   do folderu z grą. Jeśli Windows zapyta, wybierz „Zamień pliki”.
5. Uruchom grę normalnie. Pierwsze uruchomienie trwa kilka minut. W menu głównym, w lewym górnym rogu,
   powinien pojawić się napis „Town of Us: Mira - AmonRa Edition”.

Wszyscy gracze w lobby muszą mieć **tę samą wersję** moda.

# Aktualizacje

Mod aktualizuje się sam. Przy starcie gry sprawdza, czy jest nowe wydanie, pobiera je w tle (sprawdzając sumę
kontrolną SHA-256) i pokazuje komunikat. Po zakończeniu pobierania wystarczy ponownie uruchomić grę. Co się zmieniło
w każdej wersji, opisuje [lista zmian](CHANGELOG.md) (z polskim tłumaczeniem przy każdej wersji).

Automatyczne aktualizacje można wyłączyć w pliku `BepInEx/config/auavengers.tou.mira.cfg`
(`[Updater]` → `AutoUpdate = false`).

# Dodatki AmonRa Edition

Wszystko poniżej jest nowe w AmonRa Edition. Wszystkie role i modyfikatory Town of Us: Mira są w sekcji
[Role i modyfikatory](#role-i-modyfikatory).

| | Nazwa | Typ | Opis |
|:-:|:--|:--|:--|
| <img src="TownOfUs/Resources/Custom/AmonRaIcon.png" width="48"/> | **AmonRa** | Modyfikator (uniwersalny) | Postać jeździ na wózku z obracającym się kołem i dźwiękiem toczenia. Rozpędza się stopniowo i toczy się po puszczeniu klawiszy. Wolno jadąc, skręca szybko, ale przy pełnej prędkości potrzebuje szerokich łuków: płynna jazda zachowuje rozpęd i jest szybsza od innych, a ostre skręty dają poślizg i utratę prędkości. Wenty, drabiny i taski działają normalnie. |
| <img src="TownOfUs/Resources/Custom/GamblerIcon.png" width="48"/> | **Hazardzista** (Gambler) | Rola (Neutral Evil) | Na każdym spotkaniu, zanim głosy zostaną ujawnione, potajemnie obstawia, kto zostanie wyrzucony (albo że nikt). Trafny zakład daje punkt, chybiony go zabiera. Zebranie wystarczającej liczby punktów za życia oznacza wygraną. Opcjonalnie wszyscy dostają ostrzeżenie (albo Hazardzista zostaje ujawniony), gdy jest blisko wygranej. |
| <img src="TownOfUs/Resources/Custom/AuditorIcon.png" width="48"/> | **Audytor** (Auditor) | Rola (Crewmate Investigative) | Potajemnie audytuje pobliskiego gracza przez jakiś czas: raport na najbliższym spotkaniu pokazuje ukończone zadania, stanowiska, przy których cel stał bez postępu, i jedną zbiorczą liczbę nieprawidłowości (wenty, zabójstwa, zdolności ról, także załogantów). Kolejne audyty zdobywa, robiąc zadania. Raz na grę może pokazać raport wszystkim, ujawniając się. Opcjonalnie cel wie, że jest audytowany, a Audytor może zamrozić jeden głos. |
| <img src="TownOfUs/Resources/Custom/RiggerIcon.png" width="48"/> | **Monter** (Rigger) | Rola (Impostor Support) | Przerabia prawdziwe stanowiska zadań (do 2 naraz). Gdy załogant trzyma przerobione zadanie otwarte przez chwilę i go nie kończy, pułapka się odpala: Impostorzy dostają alarm i strzałkę do stanowiska, a zadanie zacina się i zamyka. Kto skończy zadanie wcześniej, niczego nie uruchamia. Przerobione stanowiska iskrzą z bliska, więc uważni załoganci mogą je zauważyć. Przeróbki znikają na każdym spotkaniu. Opcjonalnie (domyślnie wyłączone) pułapka zabija też załoganta, na którym się odpaliła. |
| <img src="TownOfUs/Resources/Custom/InsiderIcon.png" width="48"/> | **Wtyczka** (Insider) | Rola (Impostor Concealing) | Impostor, który naprawdę może robić zadania ze swojej listy, więc wszyscy (i Audytor) widzą prawdziwą pracę. Każde ukończone zadanie na stałe trochę skraca jego cooldown zabójstwa (domyślnie o 1 s, do minimum). Domyślnie jego zadania liczą się też do postępu załogi, więc szybsze zabójstwo przybliża też załogę do wygranej na zadania. |

Oprócz tego: przycisk „Ustawienia” w trybie Freeplay (zmiana ustawień lobby bez wychodzenia z ćwiczeń), automatyczne
aktualizacje, pełna paczka instalacyjna i własne logo na ekranie startowym.

# Wersje

| Among Us | AmonRa Edition | Bazuje na Town of Us: Mira | Pobierz |
|:--|:--|:--|:--|
| 2026.9.29 | 1.7.9-are.4 | 1.7.9 (`dev`) | [Pobierz](https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.4) |
| 2026.9.29 | 1.7.9-are.3 | 1.7.9 (`dev`) | [Pobierz](https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.3) |
| 2026.9.29 | 1.7.9-are.2 | 1.7.9 (`dev`) | [Pobierz](https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.2) |
| 2026.9.29 | 1.7.9-are.1 | 1.7.9 (`dev`) | [Pobierz](https://github.com/michalekjarmark/TOU-Mira-ARE/releases/tag/v1.7.9-are.1) |

# Problemy

- **Gra uruchamia się bez moda** (brak napisu w menu): w folderze gry zmień nazwę pliku `version.dll` na `winhttp.dll`.
- **Gra zamyka się przy pierwszym uruchomieniu:** sprawdź, czy w folderze gry nie ma jednocześnie `winhttp.dll`
  i `version.dll` (np. po innym modzie). Zostaw tylko `version.dll`.
- **Wyłączenie moda:** usuń (albo zmień nazwę) `version.dll` w folderze gry.
- **Odinstalowanie:** usuń foldery `BepInEx`, `dotnet`, `CorsacCosmetics` i plik `version.dll`, a potem sprawdź
  spójność plików gry w Steam/Epic.
- Błędy z tej edycji zgłaszaj nam, **nie** zespołowi Town of Us. Bardzo pomaga plik `BepInEx/LogOutput.log` z folderu gry.

# Dla twórców

Zobacz [For developers](README.md#for-developers) w wersji angielskiej. Znane pułapki techniczne (po angielsku):
[`CAVEATS.md`](CAVEATS.md).

-----------------------

# Role i modyfikatory

Kliknij ikonę, aby otworzyć jej stronę na wiki Town of Us: Mira (po angielsku). Nasze dodatki prowadzą zamiast tego do
[opisu](#dodatki-amonra-edition): **AmonRa** (wózek, modyfikatory uniwersalne), **Hazardzista** (żeton, Neutral Evil),
**Audytor** (podkładka, Crewmate Investigative), **Monter** (klucz, Impostor Support) i **Wtyczka** (wtyczka,
Impostor Concealing). Kluczowe różnice Town of Us: Mira,
obsługiwane języki i kompatybilność z innymi modami są w [wersji angielskiej](README.md#town-of-us-mira).


<p align="center">
  <img src="Images/Groups/CrewAfterlife.png" align="center" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/afterlife/Haunter"><img width="10%" src="Images/Icons/Haunter.png" /></a>
  <img src="Images/Groups/CrewInvest.png" align="center" />
  <a href="#dodatki-amonra-edition"><img width="10%" src="TownOfUs/Resources/Custom/AuditorIcon.png" alt="Audytor (AmonRa Edition)" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Aurial"><img width="10%" src="Images/Icons/Aurial.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Forensic"><img width="10%" src="Images/Icons/Forensic.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Investigator"><img width="10%" src="Images/Icons/Investigator.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Lookout"><img width="10%" src="Images/Icons/Lookout.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Medium"><img width="10%" src="Images/Icons/Medium.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Mystic"><img width="10%" src="Images/Icons/Mystic.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Seer"><img width="10%" src="Images/Icons/Seer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Snitch"><img width="10%" src="Images/Icons/Snitch.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Sonar"><img width="10%" src="Images/Icons/Sonar.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Spy"><img width="10%" src="Images/Icons/Spy.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/investigative/Trapper"><img width="10%" src="Images/Icons/Trapper.png" /></a>
  <img src="Images/Groups/CrewKilling.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Deputy"><img width="10%" src="Images/Icons/Deputy.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Hunter"><img width="10%" src="Images/Icons/Hunter.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Officer"><img width="10%" src="Images/Icons/Officer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Sheriff"><img width="10%" src="Images/Icons/Sheriff.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Veteran"><img width="10%" src="Images/Icons/Veteran.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/killing/Vigilante"><img width="10%" src="Images/Icons/Vigilante.png" /></a>
  <img src="Images/Groups/CrewPower.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Jailor"><img width="10%" src="Images/Icons/Jailor.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Mayor"><img width="10%" src="Images/Icons/Mayor.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Monarch"><img width="10%" src="Images/Icons/Monarch.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Politician"><img width="10%" src="Images/Icons/Politician.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Prosecutor"><img width="10%" src="Images/Icons/Prosecutor.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Swapper"><img width="10%" src="Images/Icons/Swapper.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/power/Time%20Lord"><img width="10%" src="Images/Icons/TimeLord.png" /></a>
  <img src="Images/Groups/CrewProtect.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Altruist"><img width="10%" src="Images/Icons/Altruist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Cleric"><img width="10%" src="Images/Icons/Cleric.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Medic"><img width="10%" src="Images/Icons/Medic.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Mirrorcaster"><img width="10%" src="Images/Icons/Mirrorcaster.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Oracle"><img width="10%" src="Images/Icons/Oracle.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/protective/Warden"><img width="10%" src="Images/Icons/Warden.png" /></a>
  <img src="Images/Groups/CrewSupport.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Barkeeper"><img width="10%" src="Images/Icons/Barkeeper.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Engineer"><img width="10%" src="Images/Icons/Engineer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Imitator"><img width="10%" src="Images/Icons/Imitator.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Plumber"><img width="10%" src="Images/Icons/Plumber.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Sentry"><img width="10%" src="Images/Icons/Sentry.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/crewmate/support/Transporter"><img width="10%" src="Images/Icons/Transporter.png" /></a>
  <img src="Images/Groups/ImpConcealing.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Eclipsal"><img width="10%" src="Images/Icons/Eclipsal.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Escapist"><img width="10%" src="Images/Icons/Escapist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Grenadier"><img width="10%" src="Images/Icons/Grenadier.png" /></a>
  <a href="#dodatki-amonra-edition"><img width="10%" src="TownOfUs/Resources/Custom/InsiderIcon.png" alt="Wtyczka (AmonRa Edition)" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Morphling"><img width="10%" src="Images/Icons/Morphling.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Swooper"><img width="10%" src="Images/Icons/Swooper.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/concealing/Venerer"><img width="10%" src="Images/Icons/Venerer.png" /></a>
  <img src="Images/Groups/ImpKilling.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/killing/Ambusher"><img width="10%" src="Images/Icons/Ambusher.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/killing/Bomber"><img width="10%" src="Images/Icons/Bomber.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/killing/Parasite"><img width="10%" src="Images/Icons/Parasite.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/killing/Scavenger"><img width="10%" src="Images/Icons/Scavenger.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/killing/Warlock"><img width="10%" src="Images/Icons/Warlock.png" /></a>
  <img src="Images/Groups/ImpPower.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/power/Ambassador"><img width="10%" src="Images/Icons/Ambassador.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/power/Herbalist"><img width="10%" src="Images/Icons/Herbalist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/power/Puppeteer"><img width="10%" src="Images/Icons/Puppeteer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/power/Spellslinger"><img width="10%" src="Images/Icons/Spellslinger.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/power/Traitor"><img width="10%" src="Images/Icons/Traitor.png" /></a>
  <img src="Images/Groups/ImpSupport.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Blackmailer"><img width="10%" src="Images/Icons/Blackmailer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Bootlegger"><img width="10%" src="Images/Icons/Bootlegger.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Hypnotist"><img width="10%" src="Images/Icons/Hypnotist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Janitor"><img width="10%" src="Images/Icons/Janitor.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Miner"><img width="10%" src="Images/Icons/Miner.png" /></a>
  <a href="#dodatki-amonra-edition"><img width="10%" src="TownOfUs/Resources/Custom/RiggerIcon.png" alt="Monter (AmonRa Edition)" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/impostor/support/Undertaker"><img width="10%" src="Images/Icons/Undertaker.png" /></a>

  <img src="Images/Groups/NeutAfterlife.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/afterlife/Spectre"><img width="10%" src="Images/Icons/Spectre.png" /></a>
  <img src="Images/Groups/NeutBenign.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/benign/Amnesiac"><img width="10%" src="Images/Icons/Amnesiac.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/benign/Fairy"><img width="10%" src="Images/Icons/Fairy.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/benign/Mercenary"><img width="10%" src="Images/Icons/Mercenary.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/benign/Survivor"><img width="10%" src="Images/Icons/Survivor.png" /></a>
  <img src="Images/Groups/NeutEvil.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/evil/Doomsayer"><img width="10%" src="Images/Icons/Doomsayer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/evil/Executioner"><img width="10%" src="Images/Icons/Executioner.png" /></a>
  <a href="#dodatki-amonra-edition"><img width="10%" src="TownOfUs/Resources/Custom/GamblerIcon.png" alt="Hazardzista (AmonRa Edition)" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/evil/Jester"><img width="10%" src="Images/Icons/Jester.png" /></a>
  <img src="Images/Groups/NeutKilling.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Arsonist"><img width="10%" src="Images/Icons/Arsonist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Glitch"><img width="10%" src="Images/Icons/Glitch.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Juggernaut"><img width="10%" src="Images/Icons/Juggernaut.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Plaguebearer"><img width="10%" src="Images/Icons/Plaguebearer.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Pestilence"><img width="10%" src="Images/Icons/Pestilence.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Medusa"><img width="10%" src="Images/Icons/Medusa.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Vampire"><img width="10%" src="Images/Icons/Vampire.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/killing/Werewolf"><img width="10%" src="Images/Icons/Werewolf.png" /></a>
  <img src="Images/Groups/NeutOutlier.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/outlier/Chef"><img width="10%" src="Images/Icons/Chef.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/roles/neutral/outlier/Inquisitor"><img width="10%" src="Images/Icons/Inquisitor.png" /></a>

  <img src="Images/Groups/AllianceMods.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/alliance/Crewpostor"><img width="10%" src="Images/Icons/Crewpostor.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/alliance/Egotist"><img width="10%" src="Images/Icons/Egotist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/alliance/Lovers"><img width="10%" src="Images/Icons/Lover.png" /></a>
  <img src="Images/Groups/AssailantMods.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/assailant/Assassin"><img width="10%" src="Images/Icons/Assassin.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/assailant/Double%20Shot"><img width="10%" src="Images/Icons/DoubleShot.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/assailant/Overclocker"><img width="10%" src="Images/Icons/Overclocker.png" /></a>
  <img src="Images/Groups/CrewMods.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Aftermath"><img width="10%" src="Images/Icons/Aftermath.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Bait"><img width="10%" src="Images/Icons/Bait.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Celebrity"><img width="10%" src="Images/Icons/Celebrity.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Diseased"><img width="10%" src="Images/Icons/Diseased.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Frosty"><img width="10%" src="Images/Icons/Frosty.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Investigator"><img width="10%" src="Images/Icons/Investigator.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Multitasker"><img width="10%" src="Images/Icons/Multitasker.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Noisemaker"><img width="10%" src="Images/Icons/Noisemaker.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Operative"><img width="10%" src="Images/Icons/Operative.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Rotting"><img width="10%" src="Images/Icons/Rotting.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Scientist"><img width="10%" src="Images/Icons/Scientist.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Scout"><img width="10%" src="Images/Icons/Scout.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Spy"><img width="10%" src="Images/Icons/Spy.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Taskmaster"><img width="10%" src="Images/Icons/Taskmaster.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/crewmate/Torch"><img width="10%" src="Images/Icons/Torch.png" /></a>
  <img src="Images/Groups/ImpMods.png" />
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Circumvent"><img width="10%" src="Images/Icons/Circumvent.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Deadly%20Quota"><img width="10%" src="Images/Icons/DeadlyQuota.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Disperser"><img width="10%" src="Images/Icons/Disperser.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Saboteur"><img width="10%" src="Images/Icons/Saboteur.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Telepath"><img width="10%" src="Images/Icons/Telepath.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/impostor/Underdog"><img width="10%" src="Images/Icons/Underdog.png" /></a>
  <img src="Images/Groups/UniMods.png" />
  <a href="#dodatki-amonra-edition"><img width="10%" src="TownOfUs/Resources/Custom/AmonRaIcon.png" alt="AmonRa (AmonRa Edition)" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Button%20Barry"><img width="10%" src="Images/Icons/ButtonBarry.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Drunk"><img width="10%" src="Images/Icons/Drunk.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Flash"><img width="10%" src="Images/Icons/Flash.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Giant"><img width="10%" src="Images/Icons/Giant.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Immovable"><img width="10%" src="Images/Icons/Immovable.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Mini"><img width="10%" src="Images/Icons/Mini.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Radar"><img width="10%" src="Images/Icons/Radar.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Satellite"><img width="10%" src="Images/Icons/Satellite.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Shy"><img width="10%" src="Images/Icons/Shy.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Sixth%20Sense"><img width="10%" src="Images/Icons/SixthSense.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Sleuth"><img width="10%" src="Images/Icons/Sleuth.png" /></a>
  <a href="https://au-avengers.github.io/docs.toum.gg/docs/modifiers/universal/Tiebreaker"><img width="10%" src="Images/Icons/Tiebreaker.png" /></a>
</p>

-----------------------

# Podziękowania

AmonRa Edition stoi na barkach **[Town of Us: Mira](https://github.com/AU-Avengers/TOU-Mira)** od
**Among Us Avengers** (Atony, AlchlcSystm, angxlwtf, MyDragonBreath, Nix, Satire, xerminator, XtraCube, whichtwix, Zeo)
oraz wszystkich osób, które przyczyniły się do tego moda. Pełna lista podziękowań:
[Contributions & Credits](README.md#contributions--credits).

# Licencja

Oprogramowanie jest rozpowszechniane na licencji GNU GPLv3. BepInEx jest rozpowszechniany na licencji LGPL-2.1.
Pełna treść: [LICENSE.txt](LICENSE.txt). Mod nie jest powiązany z Among Us ani Innersloth LLC, a jego zawartość
nie jest wspierana ani sponsorowana przez Innersloth LLC. Część materiałów jest własnością Innersloth LLC.
© Innersloth LLC.
