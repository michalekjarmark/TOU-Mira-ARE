# Caveats

Pitfalls found while developing Town of Us: Mira - AmonRa Edition. Read this before writing code. When you hit a new
one, add a short entry: symptom, cause, what to do. Notes about a single feature belong in its code (doc comments).

## IL2CPP and injected roles

- **Never call `base.Method()` in a role's override of an IL2CPP method** (e.g. `base.SpawnTaskHeader`, `base.CanUse`).
  The call dispatches back to the override: endless recursion, and the game closes without any log entry. Use
  `RoleBehaviourStubs.X(this, ...)` (MiraAPI) or write the code yourself. `base` in plain C# classes (buttons,
  `MeetingActionButton`) is fine.
- Roles are injected MonoBehaviours: `public sealed class XRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ...`.
- Members of IL2CPP types that use managed-only types (`List<>`, `Dictionary<>`, `System.Text.StringBuilder`, managed
  delegates, interfaces with default implementations) need `[HideFromIl2Cpp]`.
- Don't mix `Il2CppSystem.*` and `System.*` types without a reason.
- IL2CPP lists don't fully support LINQ: use `.ToArray()` or a loop.
- Coroutines: `Reactor.Utilities.Coroutines.Start(IEnumerator)`. `WaitUntil` needs
  `DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(...)`.

## Networking (Reactor RPC)

- State changed by an ability must go through an RPC (`[MethodRpc((uint)TownOfUsRpc.X)]`), otherwise it changes only
  locally. Handlers check `LobbyBehaviour.Instance` and call `MiscUtils.RunAnticheatWarning`.
- Append new `TownOfUsRpc` values at the end (the id is the position). When two branches both append, keep both entries:
  everyone runs the same build after the merge anyway.
- **Never read an attribute option (`[ModdedToggleOption] public bool X { get; set; } = ...`) directly in a
  `[MethodRpc]` body.** Reactor compiles a copy of the RPC body when the plugin loads, before MiraAPI patches the option
  getters, so the trivial getter gets inlined and the RPC always sees the initializer value (the Rigger's lethal traps
  were always off). Move the body into a `[MethodImpl(MethodImplOptions.NoInlining)]` method (in an IL2CPP role also
  `[HideFromIl2Cpp]`). Option objects (`ModdedNumberOption.Value`) and normal code (buttons, events, coroutines) are fine.
- Kills: `RpcCustomMurder` / `RpcSpecialMurder`, not the vanilla `MurderPlayer`.
- What other clients see: `CompleteTaskEvent`, `AfterMurderEvent`, most `TouAbilityEvent`s, `inVent` and the position.
  `EnterVentEvent` fires only for the player who enters (patch on `RpcEnterVent`). Task steps and who started a sabotage
  are not sent at all.
- Task steps exist only on the task owner's client. To complete a task for another player, let the owner's client send
  it (`owner.RpcCompleteTask(id)`, like the Taskmaster).

## MiraAPI

- Every abstract base class (role, button, modifier, options) needs `[MiraIgnore]`: the reflection scan doesn't skip
  abstract types in plugins.
- **Locale placeholders:** when loading the XML, `MiraLocaleManager` turns every `[` into `<` and every `]` into `>`.
  Write `[player]` in the XML and replace `"<player>"` in code. Replacing `"[player]"` does nothing and players see
  `<player>`.
- Event handlers run from the lowest `[RegisterEvent(priority)]` to the highest.
- **Changing votes in `ProcessVotesEvent`** (host only): MiraAPI recomputes the ejection from the changed votes only
  when no handler set `ExiledPlayer`. The Tiebreaker (on a tie) and the hidden Knighted votes (Monarch option) set it
  at priority 0, the Swapper at 10. A handler that adds, removes or moves votes must run before them (negative
  priority): the Ventriloquist's vote steal is at -20, the Auditor's freeze at -10.
- `MeetingActionButton` visibility (`Enabled()`) is decided once, when the buttons are created at the meeting start.
- `TargetedMeetingButton` in the vote popup (`VoteAreaSelectPatch`): `x = 1.3 - 0.65 * i` over every active
  `PassiveButton`, including hidden buttons of other roles (renderer off, object active). At Skip Vote the third slot
  lands on the Skip button itself. The button is cloned from the player popup's `ConfirmButton`, whose highlight uses
  `maskInteraction = VisibleInsideMask`. Outside the player list mask (Skip Vote) set it to `None`. See
  `GamblerVotePopupPatch`.
- Meeting buttons cloned by hand from `CancelButton` with a sprite from a PNG were clickable but invisible. Use MiraAPI's
  meeting buttons.
- Rarely, MiraAPI's `HudManagerPatches.StartPostfix` throws `KeyNotFoundException: 'ActionButton'` when joining a game.
  The later `HudManager.Start` postfixes don't run (no TOU `HudManagerHelper`), and the log fills with
  `SabotagePatches.RefreshPostfix` NullReferenceExceptions every frame. Not caused by our code: restart that game.

## Game internals

- `PlayerControl.myTasks` also holds list headers (`ImportantTextTask`, e.g. the impostor's "Fake Tasks:" header that
  stays after a role change) with **id 0**. When looking up a task by id, match only `NormalPlayerTask`. Vanilla
  `CompleteTask(idx)` takes the first task with that id: the game data ends up right, but the real task object can stay
  "not done".
- **On a client that isn't the host, `NetworkedPlayerInfo.Tasks[i].TypeId` is not the real task type** (it was 0 for
  every task, so `ShipStatus.GetTaskById` always gave the first common task). `Id` and `Complete` are synced. Take task
  types from the task objects (`player.myTasks`, which every client has for every player).
- Freeplay dummies have no tasks.
- Tasks that use up objects on the map (towels, records, Airship guns, Fungle vegetables, ore and shells) can't be undone
  or done twice on one client: the objects are gone once the task is done.
- Win conditions aren't checked during `ExileController` (`LogicGameFlowPatches`), and `NeutralRoleWinCondition` beats
  the crew, so a neutral win ends the game right after the ejection animation.
- Kill cooldown: `PlayerControl.GetKillCooldown()` (Underdog and the map difference included). Change it with
  `SetKillTimer` in `AfterMurderEvent` and `RoundStartEvent`, like `UnderdogEvents`.
- Abilities with a player target: `TownOfUsRoleButton<TRole, PlayerControl>`. The click goes through
  `MiraButtonClickEvent`, so the Veteran, Plaguebearer, Lookout etc. work on their own. Don't bypass it.
- Taskmaster and Immovable are disabled on Airship (map 4) and Submerged (map 6, a separate mod; Fungle is 5): both maps
  make players pick a spawn point at every round start.
- The settings menu outside the lobby: vanilla `GameSettingMenu.Update` closes it at once (our prefix returns false for
  our menu). Keep the clone's name: `OnEnable` registers an overlay in `ControllerManager` by object name, and a leftover
  overlay blocks the HUD and movement. UI errors go to Unity's `Player.log`, not to BepInEx's `LogOutput.log`.

## Assets

- TOU role and modifier icons are 1.44 × 1.44 units (144 px at PPU 100). Our 128 px icons load with
  `AreAssets.RoleIconPpu` (128 / 1.44). Ability buttons are about 1.0–1.2 units, the vote popup button 0.82 × 0.86.

## Build and tools

- With `-p:AmongUs=<game folder>` always add `-p:BuildAllPlatforms=false`, otherwise the Android and iOS DLLs land in
  `plugins` too (three plugins with one GUID).
- The `Copy` target creates symlinks when Windows developer mode is on. A running game locks the DLLs: close it before
  building.
- The first build downloads the game libraries and can fail with `IOException ... being used by another process`. Build
  again.
- BepInEx with the doorstop as `winhttp.dll` crashed at the first start on some Windows 11 builds
  (`AccessViolationException` in `WinHttpOpen`). `version.dll` works, and the release package uses it. The updater sets
  `UseProxy = false` to stay away from WinHttp.
- After a DLL swap, BepInEx rebuilds its type cache and keeps plugin dependencies open (`TypeLoader.CecilResolver`). The
  updater disposes it before replacing files (`AreUpdater`).
- Working tree files have CRLF line endings (`autocrlf=true`). In perl patterns use `\r?\n`. After any scripted edit check
  `git diff --stat`: a whole-file diff means mixed line endings.
- Windows PowerShell 5.1 reads files without a BOM as ANSI. Save `.ps1` scripts with non-ASCII characters as UTF-8 with
  BOM, and read text files with `-Encoding UTF8`.

## Testing

- Among Us won't start twice from one folder (`forceSingleInstance` in `globalgamemanagers`). For a second window use a
  copy of the game folder with that flag patched to 0 (one byte; redo it after a game update). The copy's plugins can be
  symlinks to the same build. Start the original first, play over "Local".
- With 2 players vents are disabled (`Game Mechanics → Max Players Alive When Vents Disable`, default 2).
- The host option "Freeplay Mode" keeps the game going and enables the Freeplay buttons: "Set Roles" works on any player.
- Without Steam running, the game is in offline mode and stutters every ~2 s.
