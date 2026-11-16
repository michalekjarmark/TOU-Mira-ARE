using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Other;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Expert (AmonRa Edition, Crewmate Investigative): examines a player and learns their exact role, but with a chance
///     (lobby option) the result is made up, and the Expert can't tell which results are wrong. The result stays on the
///     player's name for the Expert (<see cref="ExpertVerdictModifier" />, local only, nothing is synced).
/// </summary>
public sealed class ExpertRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Expert";
    public Color RoleColor => TownOfUsColors.Expert;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public DoomableType DoomHintType => DoomableType.Insight;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.ExpertIcon, "TouMira.Role.Crewmate.Expert", 1.45f),
        Icon = AreAssets.ExpertIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.QuestionSound
    };

    /// <summary>Examinations done this game (Expert's client), used by the "wrong result chance growth" option.</summary>
    public int Examinations { get; set; }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Examine", "Examine"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Examine.WikiDescription"),
            AreAssets.ExpertButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Examinations = 0;
    }

    /// <summary>
    ///     Examines <paramref name="target" /> (Expert's client, after the button click passed the interaction checks):
    ///     picks the real or a made-up role, shows it in a notification and on the target's name.
    /// </summary>
    public void Examine(PlayerControl target)
    {
        var opts = OptionGroupSingleton<ExpertOptions>.Instance;
        var real = target.Data.Role;
        var chance = Math.Clamp(opts.WrongChance + opts.WrongChanceGrowth * Examinations, 0f, 100f);
        Examinations++;

        var shown = real;
        var wrong = Random.Range(0f, 100f) < chance;
        if (wrong)
        {
            shown = PickFakeRole(real, opts.FakeRoleSource) ?? real;
        }

        Message($"Expert: {target.Data.PlayerName} is {real.GetRoleName()}, shown {shown.GetRoleName()} " +
                $"(wrong chance {chance}%, wrong {wrong && shown != real})");

        target.RemoveModifier<ExpertVerdictModifier>();
        target.AddModifier<ExpertVerdictModifier>(shown);

        TouAudio.PlaySound(TouAudio.QuestionSound);
        var text = MiraLocaleManager.Get("TownOfUsMira.Role.ExpertVerdict")
            .Replace("<player>", target.Data.PlayerName)
            .Replace("<role>", $"{shown.TeamColor.ToTextColor()}{shown.GetRoleName()}</color>");
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.ExpertIcon.LoadAsset());
        notif.AdjustNotification();
    }

    /// <summary>
    ///     A made-up role, never the real one. Null when there is nothing else to show (the real role is shown then).
    /// </summary>
    [HideFromIl2Cpp]
    private static RoleBehaviour? PickFakeRole(RoleBehaviour real, ExpertFakeRoleSource source)
    {
        var realTeam = TeamOf(real);
        IEnumerable<RoleBehaviour> pool = source is ExpertFakeRoleSource.AnyRole
            ? MiscUtils.SpawnableRoles
            : MiscUtils.GetPotentialRoles();
        var candidates = pool.Where(x => IsShowable(x) && x.Role != real.Role)
            .GroupBy(x => x.Role)
            .Select(x => x.First())
            .ToList();

        if (source is ExpertFakeRoleSource.OtherFaction)
        {
            var otherFaction = candidates.Where(x => TeamOf(x) != realTeam).ToList();
            if (otherFaction.Count == 0)
            {
                // Nothing of another faction in this game: take any role of another faction.
                otherFaction = MiscUtils.SpawnableRoles
                    .Where(x => IsShowable(x) && TeamOf(x) != realTeam)
                    .GroupBy(x => x.Role)
                    .Select(x => x.First())
                    .ToList();
            }

            candidates = otherFaction;
        }

        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    // Ghost roles (Haunter, Spectre...) and the Spectator are never a living player's role.
    private static bool IsShowable(RoleBehaviour? role)
    {
        return role != null && !role.IsDead && role is not SpectatorRole;
    }

    private static ModdedRoleTeams TeamOf(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.Team;
        }

        return role.IsImpostor ? ModdedRoleTeams.Impostor : ModdedRoleTeams.Crewmate;
    }
}
