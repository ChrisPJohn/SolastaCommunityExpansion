﻿using HarmonyLib;
using SolastaCommunityExpansion.Builders.Features;
using SolastaModApi.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace SolastaCommunityExpansion.Patches.PowerSharedPool
{
    internal static class RulesetCharacterPatcher
    {
        //
        // this patch shouldn't be protected
        //
        [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class RulesetCharacter_UsePower
        {
            public static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
            {
                __instance.UpdateUsageForPowerPool(usablePower, usablePower.PowerDefinition.CostPerUse);
            }
        }

        //
        // this patch shouldn't be protected
        //
        [HarmonyPatch(typeof(RulesetCharacter), "RepayPowerUse")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class RulesetCharacter_RepayPowerUse
        {
            public static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
            {
                __instance.UpdateUsageForPowerPool(usablePower, -usablePower.PowerDefinition.CostPerUse);
            }
        }

        public static void UpdateUsageForPowerPool(this RulesetCharacter character, RulesetUsablePower modifiedPower, int poolUsage)
        {
            if (!(modifiedPower.PowerDefinition is IPowerSharedPool))
            {
                return;
            }
            IPowerSharedPool sharedPoolPower = (IPowerSharedPool)modifiedPower.PowerDefinition;
            foreach (RulesetUsablePower poolPower in character.UsablePowers)
            {
                if (poolPower.PowerDefinition == sharedPoolPower.GetUsagePoolPower())
                {
                    int maxUses = GetMaxUsesForPool(poolPower, character);
                    int remainingUses = Mathf.Clamp(poolPower.RemainingUses - poolUsage, 0, maxUses);
                    poolPower.SetField("remainingUses", remainingUses);
                    AssignUsesToSharedPowersForPool(character, poolPower, remainingUses, maxUses);
                    return;
                }
            }
        }

        //
        // this patch shouldn't be protected
        //
        [HarmonyPatch(typeof(RulesetCharacter), "GrantPowers")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class RulesetCharacter_GrantPowers
        {
            public static void Postfix(RulesetCharacter __instance)
            {
                RechargeLinkedPowers(__instance, RuleDefinitions.RestType.LongRest);
            }
        }

        //
        // this patch shouldn't be protected
        //
        [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class RulesetCharacter_ApplyRest
        {
            internal static void Postfix(
                RulesetCharacter __instance, RuleDefinitions.RestType restType, bool simulate)
            {
                if (!simulate)
                {
                    RechargeLinkedPowers(__instance, restType);
                }

                // The player isn't recharging the shared pool features, just the pool.
                // Hide the features that use the pool from the UI.
                foreach (FeatureDefinition feature in __instance.RecoveredFeatures.Where(f => f is IPowerSharedPool).ToArray())
                {
                    __instance.RecoveredFeatures.Remove(feature);
                }
            }
        }

        public static void RechargeLinkedPowers(RulesetCharacter character, RuleDefinitions.RestType restType)
        {
            List<FeatureDefinitionPower> pointPoolPowerDefinitions = new List<FeatureDefinitionPower>();
            foreach (RulesetUsablePower usablePower in character.UsablePowers)
            {
                if (usablePower.PowerDefinition is IPowerSharedPool pool)
                {
                    FeatureDefinitionPower pointPoolPower = pool.GetUsagePoolPower();

                    // Only add to recharge here if it (recharges on a short rest and this is a short or long rest) or 
                    // it recharges on a long rest and this is a long rest.
                    if (!pointPoolPowerDefinitions.Contains(pointPoolPower)
                        && ((pointPoolPower.RechargeRate == RuleDefinitions.RechargeRate.ShortRest &&
                            (restType == RuleDefinitions.RestType.ShortRest || restType == RuleDefinitions.RestType.LongRest)) ||
                            (pointPoolPower.RechargeRate == RuleDefinitions.RechargeRate.LongRest && restType == RuleDefinitions.RestType.LongRest)))
                    {
                        pointPoolPowerDefinitions.Add(pointPoolPower);
                    }
                }
            }

            // Find the UsablePower of the point pool powers.
            foreach (RulesetUsablePower poolPower in character.UsablePowers)
            {
                if (pointPoolPowerDefinitions.Contains(poolPower.PowerDefinition))
                {
                    int poolSize = GetMaxUsesForPool(poolPower, character);
                    poolPower.SetField("remainingUses", poolSize);

                    AssignUsesToSharedPowersForPool(character, poolPower, poolSize, poolSize);
                }
            }
        }

        private static void AssignUsesToSharedPowersForPool(RulesetCharacter character, RulesetUsablePower poolPower, int remainingUses, int totalUses)
        {
            // Find powers that rely on this pool
            foreach (RulesetUsablePower usablePower in character.UsablePowers)
            {
                if (usablePower.PowerDefinition is IPowerSharedPool pool)
                {
                    FeatureDefinitionPower pointPoolPower = pool.GetUsagePoolPower();
                    if (pointPoolPower == poolPower.PowerDefinition)
                    {
                        usablePower.SetField("maxUses", totalUses / usablePower.PowerDefinition.CostPerUse);
                        usablePower.SetField("remainingUses", remainingUses / usablePower.PowerDefinition.CostPerUse);
                    }
                }
            }
        }

        private static int GetMaxUsesForPool(RulesetUsablePower poolPower, RulesetCharacter character)
        {
            int totalPoolSize = poolPower.MaxUses;

            foreach (RulesetUsablePower modifierPower in character.UsablePowers)
            {
                if (modifierPower.PowerDefinition is IPowerPoolModifier modifier && modifier.GetUsagePoolPower() == poolPower.PowerDefinition)
                {
                    totalPoolSize += modifierPower.MaxUses;
                }
            }

            return totalPoolSize;
        }
    }
}
