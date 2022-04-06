﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolastaCommunityExpansion.FightingStyles;

namespace SolastaCommunityExpansion.Models
{
    internal static class FightingStyleContext
    {
        private static Dictionary<FightingStyleDefinition, List<FeatureDefinitionFightingStyleChoice>> FightingStylesChoiceList { get; set; } = new();

        internal static HashSet<FightingStyleDefinition> FightingStyles { get; private set; } = new();

        internal static void Load()
        {
            LoadStyle(new BlindFighting());
            LoadStyle(new Pugilist());

            FightingStyles = FightingStyles.OrderBy(x => x.FormatTitle()).ToHashSet();
        }

        private static void LoadStyle(AbstractFightingStyle styleBuilder)
        {
            var style = styleBuilder.GetStyle();

            if (!FightingStyles.Contains(style))
            {
                FightingStylesChoiceList.Add(style, styleBuilder.GetChoiceLists());
                FightingStyles.Add(style);
            }

            UpdateStyleVisibility(style);
        }

        private static void UpdateStyleVisibility(FightingStyleDefinition fightingStyleDefinition)
        {
            var name = fightingStyleDefinition.Name;
            var choiceLists = FightingStylesChoiceList[fightingStyleDefinition];

            foreach (var fightingStyles in choiceLists.Select(cl => cl.FightingStyles))
            {
                if (Main.Settings.FightingStyleEnabled.Contains(name))
                {
                    fightingStyles.TryAdd(name);
                }
                else
                {
                    fightingStyles.Remove(name);
                }
            }
        }

        internal static void Switch(FightingStyleDefinition fightingStyleDefinition, bool active)
        {
            if (!FightingStyles.Contains(fightingStyleDefinition))
            {
                return;
            }

            var name = fightingStyleDefinition.Name;

            if (active)
            {
                Main.Settings.FightingStyleEnabled.TryAdd(name);
            }
            else
            {
                Main.Settings.FightingStyleEnabled.Remove(name);
            }

            UpdateStyleVisibility(fightingStyleDefinition);
        }

#if DEBUG
        public static string GenerateFightingStyleDescription()
        {
            var outString = new StringBuilder();

            foreach (var style in FightingStyles)
            {
                outString.Append("\n[*][b]");
                outString.Append(style.FormatTitle());
                outString.Append("[/b]: ");
                outString.Append(style.FormatDescription());
            }

            return outString.ToString();
        }
#endif
    }
}
