/*using System.Collections.Generic;
using System.Linq;
using TownOfHost.Extensions;
using TownOfHost.Gamemodes.Standard;
using TownOfHost.Managers;
using TownOfHost.Roles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Utilities;

namespace TownOfHost.Options;

[Localized(Group = "OptionShower")]
public static class OptionShower
{

    [Localized("ActiveRolesList")]
    private static string ActiveRolesList;
    [Localized("NextPage")]
    private static string NextPageString;

    public static int currentPage = 0;
    public static List<string> pages = new();

    public static string GetText()
    {
        /#1#/初期化
        string text = "";
        pages = new()
        {
            //1ページに基本ゲーム設定を格納
            GameOptionsManager.Instance.CurrentGameOptions.ToHudString(GameData.Instance ? GameData.Instance.PlayerCount : 10) + "\n\n"
        };
        // TODO: localize gamemode
        text += $"Gamemode: {Game.CurrentGamemode.GetName()}\n\n";

        //Standardの時のみ実行
        if (Game.CurrentGamemode is StandardGamemode)
        {
            text += ActiveRolesList + "\n";
            foreach (CustomRole role in CustomRoleManager.AllRoles)
            {
                Option matchingHolder = TOHPlugin.OptionManager.PreviewOptions().FirstOrDefault(h => h.Name == role.RoleName);
                string chance = role.Chance + "%";
                string count = role.Count.ToString();

                if (matchingHolder?.Pseudo ?? false)
                {
                    chance = matchingHolder.GetValueAsString();
                    count = matchingHolder.SubOptions[0].GetValueAsString();
                }

                if (chance != "0%" && count != "0")
                    text += $"{role.RoleColor.Colorize(role.RoleName)}: {chance}×{count}\n";
            }

            pages.Add(text + "\n\n");
            text = "";
        }
        //有効な役職と詳細設定一覧
        pages.Add("");
        if (Game.CurrentGamemode.EnabledTabs().Contains(DefaultTabs.GeneralTab))
            text += $"{CustomRoleManager.Special.GM.RoleColor.Colorize("GM")}: {Utils.GetOnOffColored(StaticOptions.EnableGM)}\n";
        HashSet<Option> roleHolders = new();
        foreach (CustomRole role in CustomRoleManager.AllRoles)
        {
            string chance = role.Chance + "%";
            string count = role.Count.ToString();
            Option matchingHolder = TOHPlugin.OptionManager.PreviewOptions().FirstOrDefault(h => h.Name == role.RoleName);
            if (matchingHolder != null) roleHolders.Add(matchingHolder);

            if (matchingHolder?.Pseudo ?? false)
            {
                if (matchingHolder.GetValueAsString() == "0%") continue;
                chance = matchingHolder.GetValueAsString();
                count = matchingHolder.SubOptions[0].GetValueAsString();
            }
            else if (!role.IsEnable() || role is GM) continue;
            text += "\n";
            text += $"{role.RoleColor.Colorize(role.RoleName)}: {chance}×{count}\n";

            if (matchingHolder != null)
                ShowChildren(matchingHolder, ref text, role.RoleColor.ShadeColor(-0.5f), 1);

            string rule = Utils.ColorString(Palette.ImpostorRed.ShadeColor(-0.5f), "┣ ");
            string ruleFooter = Utils.ColorString(Palette.ImpostorRed.ShadeColor(-0.5f), "┗ ");
            /*if (kvp.Key.GetReduxRole().IsMadmate()) //マッドメイトの時に追加する詳細設定
                {
                    text += $"{rule}{OldOptions.MadmateCanFixLightsOut.GetName()}: {OldOptions.MadmateCanFixLightsOut.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateCanFixComms.GetName()}: {OldOptions.MadmateCanFixComms.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateHasImpostorVision.GetName()}: {OldOptions.MadmateHasImpostorVision.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateCanSeeKillFlash.GetName()}: {OldOptions.MadmateCanSeeKillFlash.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateCanSeeOtherVotes.GetName()}: {OldOptions.MadmateCanSeeOtherVotes.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateCanSeeDeathReason.GetName()}: {OldOptions.MadmateCanSeeDeathReason.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateRevengeCrewmate.GetName()}: {OldOptions.MadmateRevengeCrewmate.GetString()}\n";
                    text += $"{rule}{OldOptions.MadmateVentCooldown.GetName()}: {OldOptions.MadmateVentCooldown.GetString()}\n";
                    text += $"{ruleFooter}{OldOptions.MadmateVentMaxTime.GetName()}: {OldOptions.MadmateVentMaxTime.GetString()}\n";
                }#2#
            /*if (kvp.Key.GetReduxRole().CanMakeMadmate()) //シェイプシフター役職の時に追加する詳細設定
                {
                    text += $"{ruleFooter}{OldOptions.CanMakeMadmateCount.GetName()}: {OldOptions.CanMakeMadmateCount.GetString()}\n";
                }#2#
        }

        foreach (Option holder in TOHPlugin.OptionManager.PreviewOptions().Where(o => !roleHolders.Contains(o) && Game.CurrentGamemode.EnabledTabs().Contains(o.Tab)))
        {
            if (holder.Name == "Host GM") continue;
            if (holder.IsHeader) text += "\n";
            text += $"{holder.Name}: {holder.GetValueAsString()}\n";
            if (holder.MatchesPredicate())
                ShowChildren(holder, ref text, Color.white, 1);
        }

        List<string> tmp = new(text.Split("\n\n"));
        for (var i = 0; i < tmp.Count; i++)
        {
            if (pages[^1].Count(c => c == '\n') + 1 + tmp[i].Count(c => c == '\n') + 1 > 35)
                pages.Add(tmp[i] + "\n\n");
            else pages[^1] += tmp[i] + "\n\n";
        }
        if (currentPage >= pages.Count) currentPage = pages.Count - 1; //現在のページが最大ページ数を超えていれば最後のページに修正#1#
        return $"{pages[currentPage]}{NextPageString}({currentPage + 1}/{pages.Count})";
    }
    public static void Next()
    {
        currentPage++;
        if (currentPage >= pages.Count) currentPage = 0; //現在のページが最大ページを超えていれば最初のページに
    }
    private static void ShowChildren(Option option, ref string text, Color color, int deep = 0)
    {
        foreach (var opt in option.SubOptions.Select((v, i) => new { Value = v, Index = i + 1 }))
        {
            if (opt.Value.Name == "Maximum") continue; //Maximumの項目は飛ばす
            text += string.Concat(Enumerable.Repeat(color.Colorize("┃"), deep - 1));
            text += color.Colorize(opt.Index == option.SubOptions.Count ? "┗ " : "┣ ");
            text += $"{opt.Value.Name}: {opt.Value.GetValueAsString()}\n";
            if (opt.Value.MatchesPredicate()) ShowChildren(opt.Value, ref text, color, deep + 1);
        }
    }
}*/