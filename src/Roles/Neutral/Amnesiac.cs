using AmongUs.GameOptions;
using TownOfHost.Extensions;
using TownOfHost.Factions;
using TownOfHost.Managers;
using TownOfHost.Options;
using VentLib.Options;
using TownOfHost.Roles.Internals;
using TownOfHost.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Logging;

namespace TownOfHost.Roles;

public class Amnesiac : CustomRole {
    private bool stealExactRole;

    [RoleAction(RoleActionType.AnyReportedBody)]
    public void AmnesiacRememberAction(PlayerControl reporter, GameData.PlayerInfo reported, ActionHandle handle)
    {
        VentLogger.Old($"Reporter: {reporter.GetRawName()} | Reported: {reported.GetNameWithRole()} | Self: {MyPlayer.GetRawName()}", "");

        if (reporter.PlayerId != MyPlayer.PlayerId) return;
        CustomRole newRole = reported.GetCustomRole();
        if (!stealExactRole)
        {
            if (newRole.SpecialType == SpecialType.NeutralKilling) { }
            else if (newRole.SpecialType == SpecialType.Neutral)
                newRole = CustomRoleManager.Static.Opportunist;
            else if (newRole.IsCrewmate())
                newRole = CustomRoleManager.Static.Sheriff;
            else
                newRole = Ref<Traitor>();
        }

        Game.AssignRole(MyPlayer, newRole);

        CustomRole role = MyPlayer.GetCustomRole();
        role.DesyncRole = RoleTypes.Impostor;

        handle.Cancel();
    }

    protected override OptionBuilder RegisterOptions(OptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub.Name("Steals Exact Role")
                .Bind(v => stealExactRole = (bool)v)
                .AddOnOffValues(false).Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.51f, 0.87f, 0.99f)).DesyncRole(RoleTypes.Impostor).Factions(Faction.Solo);
}