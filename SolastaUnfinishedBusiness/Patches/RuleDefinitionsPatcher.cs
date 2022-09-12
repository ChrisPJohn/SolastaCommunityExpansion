﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

internal static class RuleDefinitionsPatcher
{
    //PATCH: Apply SRD setting `UseOfficialAdvantageDisadvantageRules`
    [HarmonyPatch(typeof(RuleDefinitions), "ComputeAdvantage")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ComputeAdvantage_Patch
    {
        public static void Postfix([NotNull] List<TrendInfo> trends, ref AdvantageType __result)
        {
            if (!Main.Settings.UseOfficialAdvantageDisadvantageRules)
            {
                return;
            }

            var hasAdvantage = trends.Any(t => t.value > 0);
            var hasDisadvantage = trends.Any(t => t.value < 0);

            if (!(hasAdvantage ^ hasDisadvantage))
            {
                __result = AdvantageType.None;
            }
            else if (hasAdvantage)
            {
                __result = AdvantageType.Advantage;
            }
            else
            {
                __result = AdvantageType.Disadvantage;
            }
        }
    }
}
