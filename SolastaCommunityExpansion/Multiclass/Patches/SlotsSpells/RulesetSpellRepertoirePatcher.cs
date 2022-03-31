﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SolastaCommunityExpansion;
using SolastaModApi.Infrastructure;
using SolastaMulticlass.Models;

namespace SolastaMulticlass.Patches.SlotsSpells
{
    internal static class RulesetSpellRepertoirePatcher
    {
        //
        // the following 4 patches also exist in CE. The ones in CE get disabled in favor of these
        //

        // ensures MC Warlocks are treated before SC ones
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetMaxSlotsNumberOfAllLevels")]
        internal static class RulesetSpellRepertoireGetMaxSlotsNumberOfAllLevels
        {
            internal static bool Prefix(
                RulesetSpellRepertoire __instance,
                ref int __result)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return true;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return true;
                }

                if (!SharedSpellsContext.IsWarlock(__instance.SpellCastingClass))
                {
                    return true;
                }

                if (SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
                {
                    return true;
                }

                // SC Warlock
                __result = SharedSpellsContext.GetWarlockMaxSlots(heroWithSpellRepertoire);

                return false;
            }
        }

        // ensures MC Warlocks are treated before SC ones
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetRemainingSlotsNumberOfAllLevels")]
        internal static class RulesetSpellRepertoireGetRemainingSlotsNumberOfAllLevels
        {
            internal static bool Prefix(
                RulesetSpellRepertoire __instance, 
                ref int __result, 
                Dictionary<int, int> ___usedSpellsSlots)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return true;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return true;
                }

                if (!SharedSpellsContext.IsWarlock(__instance.SpellCastingClass))
                {
                    return true;
                }

                if (SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
                {
                    return true;
                }

                // SC Warlock
                var max = SharedSpellsContext.GetWarlockMaxSlots(heroWithSpellRepertoire);

                ___usedSpellsSlots.TryGetValue(1, out var used); // using 1 here as this is the tab for SC Warlocks
                __result = max - used;

                return false;
            }
        }

        // handles all different scenarios to determine slots numbers
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "GetSlotsNumber")]
        internal static class RulesetSpellRepertoireGetSlotsNumber
        {
            internal static bool Prefix(
                RulesetSpellRepertoire __instance,
                Dictionary<int, int> ___usedSpellsSlots,
                Dictionary<int, int> ___spellsSlotCapacities,
                int spellLevel, 
                ref int remaining, 
                ref int max)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return true;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return true;
                }

                if (!SharedSpellsContext.IsWarlock(__instance.SpellCastingClass))
                {
                    return true;
                }

                max = 0;
                remaining = 0;

                // handles MC Warlock
                if (SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
                {
                    ___spellsSlotCapacities.TryGetValue(spellLevel, out max);
                    ___usedSpellsSlots.TryGetValue(spellLevel, out var used);
                    remaining = max - used;
                }
                // handles SC Warlock
                else if (spellLevel <= __instance.MaxSpellLevelOfSpellCastingLevel)
                {
                    max = SharedSpellsContext.GetWarlockMaxSlots(heroWithSpellRepertoire);
                    ___usedSpellsSlots.TryGetValue(1, out var used); // using 1 here as this is the tab for SC Warlocks
                    remaining = max - used;
                }

                return false;
            }
        }

        // handles all different scenarios of spell slots consumption (casts, smites, point buys)
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "SpendSpellSlot")]
        internal static class RulesetSpellRepertoireSpendSpellSlot
        {
            internal static bool Prefix(RulesetSpellRepertoire __instance, int slotLevel)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return true;
                }

                if (slotLevel == 0)
                {
                    return true;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return true;
                }

                if(!SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
                {
                    // handles SC Warlock
                    if (SharedSpellsContext.IsWarlock(__instance.SpellCastingClass))
                    {
                        SpendWarlockSlots(__instance, heroWithSpellRepertoire, slotLevel);

                        return false;
                    }

                    // handles SC
                    return true;
                }

                var warlockSpellRepertoire = SharedSpellsContext.GetWarlockSpellRepertoire(heroWithSpellRepertoire);

                // handles MC non-Warlock
                if (warlockSpellRepertoire == null)
                {
                    foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                        .Where(x => x.SpellCastingRace == null))
                    {
                        var usedSpellsSlots = spellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("usedSpellsSlots");

                        if (!usedSpellsSlots.ContainsKey(slotLevel))
                        {
                            usedSpellsSlots.Add(slotLevel, 0);
                        }

                        usedSpellsSlots[slotLevel]++;
                        spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                    }
                }

                // handles MC Warlock
                else
                {
                    var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(heroWithSpellRepertoire);
                    var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
                    var warlockMaxSlots = SharedSpellsContext.GetWarlockMaxSlots(heroWithSpellRepertoire);
                    var usedSpellsSlotsWarlock = warlockSpellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("usedSpellsSlots");

                    usedSpellsSlotsWarlock.TryGetValue(SharedSpellsContext.PACT_MAGIC_SLOT_TAB_INDEX, out var warlockUsedSlots);
                    __instance.GetSlotsNumber(slotLevel, out var sharedRemainingSlots, out var sharedMaxSlots);

                    var sharedUsedSlots = sharedMaxSlots - sharedRemainingSlots;

                    sharedMaxSlots -= warlockMaxSlots;
                    sharedUsedSlots -= warlockUsedSlots;

                    var canConsumeShortRestSlot = warlockUsedSlots < warlockMaxSlots && slotLevel <= warlockSpellLevel;
                    var canConsumeLongRestSlot = sharedUsedSlots < sharedMaxSlots && slotLevel <= sharedSpellLevel;
                    var forceLongRestSlotUI = canConsumeLongRestSlot && SharedSpellsContext.ForceLongRestSlot;

                    // uses short rest slots across all repertoires
                    if (canConsumeShortRestSlot && !forceLongRestSlotUI)
                    {
                        foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires)
                        {
                            if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                            {
                                continue;
                            }

                            SpendWarlockSlots(spellRepertoire, heroWithSpellRepertoire, slotLevel);
                        }
                    }

                    // otherwise uses long rest slots across all repertoires
                    else
                    {
                        foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires)
                        {
                            if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Race)
                            {
                                continue;
                            }

                            var usedSpellsSlots = spellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("usedSpellsSlots");

                            if (!usedSpellsSlots.ContainsKey(slotLevel))
                            {
                                usedSpellsSlots.Add(slotLevel, 0);
                            }

                            usedSpellsSlots[slotLevel]++;
                            spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                        }
                    }
                }

                return false;
            }

            private static void SpendWarlockSlots(RulesetSpellRepertoire rulesetSpellRepertoire, RulesetCharacterHero heroWithSpellRepertoire, int slotLevel)
            {
                var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
                var usedSpellsSlots = rulesetSpellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("usedSpellsSlots");

                for (var i = SharedSpellsContext.PACT_MAGIC_SLOT_TAB_INDEX; i <= warlockSpellLevel; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    if (!usedSpellsSlots.ContainsKey(i))
                    {
                        usedSpellsSlots.Add(i, 0);
                    }

                    usedSpellsSlots[i]++;
                }

                rulesetSpellRepertoire.RepertoireRefreshed?.Invoke(rulesetSpellRepertoire);
            }
        }

        //
        // patches exclusive to MC
        //

        // handles all different scenarios to determine max spell level (must be a postfix)
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "MaxSpellLevelOfSpellCastingLevel", MethodType.Getter)]
        internal static class RulesetSpellRepertoireMaxSpellLevelOfSpellCastingLevelGetter
        {
            internal static void Postfix(RulesetSpellRepertoire __instance, ref int __result)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return;
                }

                if (!SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
                {
                    return;
                }

                __result = SharedSpellsContext.GetCombinedSpellLevel(heroWithSpellRepertoire);
            }
        }

        // handles Arcane Recovery granted spells on short rests
        [HarmonyPatch(typeof(RulesetSpellRepertoire), "RecoverMissingSlots")]
        internal static class RulesetSpellRepertoireRecoverMissingSlots
        {
            internal static bool Prefix(RulesetSpellRepertoire __instance, Dictionary<int, int> recoveredSlots)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return true;
                }

                var heroWithSpellRepertoire = SharedSpellsContext.GetHero(__instance.CharacterName);

                if (heroWithSpellRepertoire == null)
                {
                    return true;
                }

                foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires)
                {
                    foreach (var recoveredSlot in recoveredSlots)
                    {
                        var key = recoveredSlot.Key;
                        var usedSpellsSlots = spellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("usedSpellsSlots");
                        var spellsSlotCapacities = spellRepertoire.GetField<RulesetSpellRepertoire, Dictionary<int, int>>("spellsSlotCapacities");

                        if (usedSpellsSlots.ContainsKey(key)
                            && spellsSlotCapacities.ContainsKey(key)
                            && usedSpellsSlots[key] > 0)
                        {
                            usedSpellsSlots[key] = UnityEngine.Mathf.Max(0, usedSpellsSlots[key] - recoveredSlot.Value);
                        }

                        spellRepertoire.RepertoireRefreshed?.Invoke(__instance);
                    }
                }

                return false;
            }
        }
    }
}
