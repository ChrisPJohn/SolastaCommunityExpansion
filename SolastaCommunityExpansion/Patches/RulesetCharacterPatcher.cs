﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaCommunityExpansion.Api;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.Api.Infrastructure;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaCommunityExpansion.CustomInterfaces;
using SolastaCommunityExpansion.Models;
using static SolastaCommunityExpansion.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaCommunityExpansion.Models.SpellsSlotsContext;

namespace SolastaCommunityExpansion.Patches;

internal static class RulesetCharacterPatcher
{
    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniquePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class TerminateMatchingUniquePower_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, FeatureDefinitionPower powerDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniquePower(__instance, powerDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniqueSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class TerminateMatchingUniqueSpell_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, SpellDefinition spellDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniqueSpell(__instance, spellDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionAdded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class OnConditionAdded_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is applied 
            activeCondition.ConditionDefinition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.ApplyFeature(__instance));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionRemoved")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class OnConditionRemoved_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is removed 
            activeCondition.ConditionDefinition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.RemoveFeature(__instance));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentSomaticValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsComponentSomaticValid_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition spellDefinition, ref string failure)
        {
            //PATCH: Allows valid Somatic component if specific material component is held in main hand or off hand slots
            // allows casting somatic spells with full hands if one of the hands holds material component for the spell
            if (__result || spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spellDefinition.SpecificMaterialComponentTag;
            var inventorySlotsByName = __instance.CharacterInventory.InventorySlotsByName;
            var mainHand = inventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
            var offHand = inventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;
            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();

            mainHand?.FillTags(tagsMap, __instance, true);
            offHand?.FillTags(tagsMap, __instance, true);

            if (!tagsMap.ContainsKey(materialTag))
            {
                return;
            }

            __result = true;
            failure = string.Empty;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentMaterialValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsComponentMaterialValid_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition spellDefinition, ref string failure)
        {
            //PATCH: Allow spells to satisfy material components by using stack of equal or greater value
            StackedMaterialComponent.IsComponentMaterialValid(__instance, spellDefinition, ref failure, ref __result);

            //TODO: move to separate file
            //PATCH: Allows spells to satisfy specific material components by actual active tags on an item that are not directly defined in ItemDefinition (like "Melee")
            //Used mostly for melee cantrips requiring melee weapon to cast
            if (__result || spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spellDefinition.SpecificMaterialComponentTag;
            var requiredCost = spellDefinition.SpecificMaterialComponentCostGp;

            List<RulesetItem> items = new();
            __instance.CharacterInventory.EnumerateAllItems(items);
            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();
            foreach (var rulesetItem in items)
            {
                tagsMap.Clear();
                rulesetItem.FillTags(tagsMap, __instance, true);
                var itemItemDefinition = rulesetItem.ItemDefinition;
                var costInGold = EquipmentDefinitions.GetApproximateCostInGold(itemItemDefinition.Costs);

                if (!tagsMap.ContainsKey(materialTag) || costInGold < requiredCost)
                {
                    continue;
                }

                __result = true;
                failure = string.Empty;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "SpendSpellMaterialComponentAsNeeded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SpendSpellMaterialComponentAsNeeded_Patch
    {
        public static bool Prefix(RulesetCharacter __instance, RulesetEffectSpell activeSpell)
        {
            //PATCH: Modify original code to spend enough of a stack to meet component cost
            return StackedMaterialComponent.SpendSpellMaterialComponentAsNeeded(__instance, activeSpell);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsValidReadyCantrip")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsValidReadyCantrip_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition cantrip)
        {
            //PATCH: Modifies validity of ready cantrip action to include attack cantrips even if they don't have damage forms
            //makes melee cantrips valid for ready action
            if (__result)
            {
                return;
            }

            var effect = CustomFeaturesContext.ModifySpellEffect(cantrip, __instance);
            var hasDamage = effect.HasFormOfType(EffectForm.EffectFormType.Damage);
            var hasAttack = cantrip.HasSubFeatureOfType<IPerformAttackAfterMagicEffectUse>();
            var notGadgets = effect.TargetFilteringMethod != RuleDefinitions.TargetFilteringMethod.GadgetOnly;
            var componentsValid = __instance.AreSpellComponentsValid(cantrip);

            __result = (hasDamage || hasAttack) && notGadgets && componentsValid;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsSubjectToAttackOfOpportunity")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsSubjectToAttackOfOpportunity_Patch
    {
        // ReSharper disable once RedundantAssignment
        internal static void Postfix(RulesetCharacter __instance, ref bool __result, RulesetCharacter attacker,
            float distance)
        {
            //PATCH: allows custom exceptions for attack of opportunity triggering
            //Mostly for Sentinel feat
            __result = AttacksOfOpportunity.IsSubjectToAttackOfOpportunity(__instance, attacker, __result, distance);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "ComputeSaveDC")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ComputeSaveDC_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref int __result)
        {
            //PATCH: support for `IIncreaseSpellDC`
            //Adds extra modifiers to spell DC

            var features = __instance.GetSubFeaturesByType<IIncreaseSpellDC>();
            __result += features.Where(feature => feature != null).Sum(feature => feature.GetSpellModifier(__instance));
        }
    }

    //PATCH: ensures that the wildshape heroes or heroes under rage cannot cast any spells (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "CanCastSpells")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class CanCastSpells_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result)
        {
            // wildshape
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                __result = false;
            }

            // raging
            if (__instance.AllConditions
                .Any(x => x.ConditionDefinition == DatabaseHelper.ConditionDefinitions.ConditionRaging))
            {
                __result = false;
            }
        }
    }

    //PATCH: ensures that the wildshape hero has access to spell repertoires for calculating slot related features (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "SpellRepertoires", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SpellRepertoires_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref List<RulesetSpellRepertoire> __result)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                __result = hero.SpellRepertoires;
            }
        }
    }

    //PATCH: ensures that original character sorcery point pool is in sync with substitute (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "CreateSorceryPoints")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class CreateSorceryPoints_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, int slotLevel, RulesetSpellRepertoire repertoire)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                hero.CreateSorceryPoints(slotLevel, repertoire);
            }
        }
    }

    //PATCH: ensures that original character sorcery point pool is in sync with substitute (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "GainSorceryPoints")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GainSorceryPoints_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, int sorceryPointsGain)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                hero.GainSorceryPoints(sorceryPointsGain);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UsePower_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
        {
            //PATCH: ensures that original character rage pool is in sync with substitute (Multiclass)
            if (usablePower.PowerDefinition == PowerBarbarianRageStart
                && __instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                hero.SpendRagePoint();
            }

            //PATCH: update usage for power pools 
            __instance.UpdateUsageForPowerPool(usablePower, usablePower.PowerDefinition.CostPerUse);
        }
    }

    //PATCH: ensures ritual spells work correctly when MC (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "CanCastAnyRitualSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class CanCastAnyRitualSpell_Patch
    {
        internal static bool Prefix(RulesetCharacter __instance, ref bool __result)
        {
            if (__instance is not RulesetCharacterHero)
            {
                return true;
            }

            RitualSelectionPanelPatcher.Bind_Patch
                .EnumerateUsableRitualSpells(
                    __instance,
                    RuleDefinitions.RitualCasting.None,
                    __instance.usableSpells);
            __result = __instance.usableSpells.Count > 0;

            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RefreshArmorClassInFeatures")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RefreshArmorClassInFeatures_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            //PATCH: support for exclusivity tags in AC modifiers  
            //used to prevent various extra defence feats (like arcane defense or wise defense) from stacking
            //replaces call to `RulesetAttributeModifier.BuildAttributeModifier` with custom method that calls base on e and adds extra tags when necessary
            ArmorClassStacking.AddCustomTagsToModifierBuilder(codes);

            return codes;
        }
    }

    //PATCH: IChangeAbilityCheck
    [HarmonyPatch(typeof(RulesetCharacter), "RollAbilityCheck")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacter_RollAbilityCheck
    {
        internal static void Prefix(
            [NotNull] RulesetCharacter __instance,
            int baseBonus,
            string abilityScoreName,
            string proficiencyName,
            List<RuleDefinitions.TrendInfo> modifierTrends,
            List<RuleDefinitions.TrendInfo> advantageTrends,
            int rollModifier,
            ref int minRoll)
        {
            var features = __instance.EnumerateFeaturesToBrowse<IChangeAbilityCheck>();

            if (features.Count <= 0)
            {
                return;
            }

            var newMinRoll = features
                .Max(x => x.MinRoll(__instance, baseBonus, rollModifier, abilityScoreName, proficiencyName,
                    advantageTrends, modifierTrends));

            if (minRoll < newMinRoll)
            {
                minRoll = newMinRoll;
            }
        }
    }

    //PATCH: IChangeAbilityCheck
    [HarmonyPatch(typeof(RulesetCharacter), "ResolveContestCheck")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacter_ResolveContestCheck
    {
        public static int ExtendedRollDie(
            [NotNull] RulesetCharacter rulesetCharacter,
            RuleDefinitions.DieType dieType,
            RuleDefinitions.RollContext rollContext,
            bool isProficient,
            RuleDefinitions.AdvantageType advantageType,
            out int firstRoll,
            out int secondRoll,
            bool enumerateFeatures,
            bool canRerollDice,
            string skill,
            int baseBonus,
            int rollModifier,
            string abilityScoreName,
            string proficiencyName,
            List<RuleDefinitions.TrendInfo> advantageTrends,
            List<RuleDefinitions.TrendInfo> modifierTrends)
        {
            var result = rulesetCharacter.RollDie(dieType, rollContext, isProficient, advantageType, out firstRoll,
                out secondRoll, enumerateFeatures, canRerollDice, skill);
            var features = rulesetCharacter.EnumerateFeaturesToBrowse<IChangeAbilityCheck>();

            if (features.Count <= 0)
            {
                return result;
            }

            var newMinRoll = features
                .Max(x => x.MinRoll(rulesetCharacter, baseBonus, rollModifier, abilityScoreName, proficiencyName,
                    advantageTrends, modifierTrends));

            if (result < newMinRoll)
            {
                result = newMinRoll;
            }

            return result;
        }

        //
        // there are 2 calls to RollDie on this method
        // we replace them to allow us to compare the die result vs. the minRoll value from any IChangeAbilityCheck feature
        //
        internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var found = 0;
            var rollDieMethod = typeof(RulesetActor).GetMethod("RollDie");
            var extendedRollDieMethod = typeof(RulesetCharacter_ResolveContestCheck).GetMethod("ExtendedRollDie");

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(rollDieMethod))
                {
                    ++found;

                    switch (found)
                    {
                        // first call to roll die checks the initiator
                        case 1:
                            yield return new CodeInstruction(OpCodes.Ldarg, 1); // baseBonus
                            yield return new CodeInstruction(OpCodes.Ldarg, 2); // rollModifier
                            yield return new CodeInstruction(OpCodes.Ldarg, 3); // abilityScoreName
                            yield return new CodeInstruction(OpCodes.Ldarg, 4); // proficiencyName
                            yield return new CodeInstruction(OpCodes.Ldarg, 5); // advantageTrends
                            yield return new CodeInstruction(OpCodes.Ldarg, 6); // modifierTrends

                            break;
                        // second call to roll die checks the opponent
                        case 2:
                            yield return new CodeInstruction(OpCodes.Ldarg, 7); // opponentBaseBonus
                            yield return new CodeInstruction(OpCodes.Ldarg, 8); // opponentRollModifier
                            yield return new CodeInstruction(OpCodes.Ldarg, 9); // opponentAbilityScoreName
                            yield return new CodeInstruction(OpCodes.Ldarg, 10); // opponentProficiencyName
                            yield return new CodeInstruction(OpCodes.Ldarg, 11); // opponentAdvantageTrends
                            yield return new CodeInstruction(OpCodes.Ldarg, 12); // opponentModifierTrends

                            break;
                    }

                    yield return new CodeInstruction(OpCodes.Call, extendedRollDieMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    //PATCH: INotifyConditionRemoval
    [HarmonyPatch(typeof(RulesetCharacter), "Kill")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacter_Kill
    {
        internal static void Prefix(RulesetCharacter __instance)
        {
            foreach (var rulesetCondition in __instance.ConditionsByCategory
                         .SelectMany(keyValuePair => keyValuePair.Value))
            {
                if (rulesetCondition?.ConditionDefinition is INotifyConditionRemoval notifiedDefinition)
                {
                    notifiedDefinition.BeforeDyingWithCondition(__instance, rulesetCondition);
                }
            }
        }
    }

    //PATCH: logic to correctly calculate spell slots under MC (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "RefreshSpellRepertoires")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RefreshSpellRepertoires_Patch
    {
        public static void Postfix(RulesetCharacter __instance)
        {
            if (__instance is not RulesetCharacterHero hero || !SharedSpellsContext.IsMulticaster(hero))
            {
                return;
            }

            var slots = new Dictionary<int, int>();

            // adds features slots
            foreach (var additionalSlot in hero.FeaturesToBrowse
                         .OfType<ISpellCastingAffinityProvider>()
                         .SelectMany(x => x.AdditionalSlots))
            {
                slots[additionalSlot.SlotLevel] = additionalSlot.SlotsNumber;
            }

            // adds spell slots
            var sharedCasterLevel = SharedSpellsContext.GetSharedCasterLevel(hero);
            var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(hero);

            for (var i = 1; i <= sharedSpellLevel; i++)
            {
                slots.TryAdd(i, 0);
                slots[i] += FullCastingSlots[sharedCasterLevel - 1].Slots[i - 1];
            }

            // adds warlock slots
            var warlockCasterLevel = SharedSpellsContext.GetWarlockCasterLevel(hero);
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(hero);

            for (var i = 1; i <= warlockSpellLevel; i++)
            {
                slots.TryAdd(i, 0);
                slots[i] += WarlockCastingSlots[warlockCasterLevel - 1].Slots[i - 1];
            }

            // reassign slots back to repertoires except for race ones
            foreach (var spellRepertoire in hero.SpellRepertoires
                         .Where(x => x.SpellCastingRace == null))
            {
                spellRepertoire.spellsSlotCapacities = slots.DeepCopy();
                spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RechargePowersForTurnStart")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RechargePowersForTurnStart_Patch
    {
        internal static void Postfix(RulesetCharacter __instance)
        {
            //PATCH: support for powers that recharge on turn start
            foreach (var usablePower in __instance.UsablePowers)
            {
                if (usablePower?.PowerDefinition is not IStartOfTurnRecharge startOfTurnRecharge ||
                    usablePower.RemainingUses >= usablePower.MaxUses)
                {
                    continue;
                }

                usablePower.Recharge();

                if (!startOfTurnRecharge.IsRechargeSilent && __instance.PowerRecharged != null)
                {
                    __instance.PowerRecharged(__instance, usablePower);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RepayPowerUse")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RepayPowerUse_Patch
    {
        public static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
        {
            //PATCH: update usage for power pools
            __instance.UpdateUsageForPowerPool(usablePower, -usablePower.PowerDefinition.CostPerUse);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "GrantPowers")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GrantPowers_Patch
    {
        public static void Postfix(RulesetCharacter __instance)
        {
            //PATCH: update usage for power pools
            CustomFeaturesContext.RechargeLinkedPowers(__instance, RuleDefinitions.RestType.LongRest);
        }
    }

    //
    //TODO: Consolidate below
    //

    [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ApplyRest_Patch
    {
        internal static void Postfix(
            RulesetCharacter __instance, RuleDefinitions.RestType restType, bool simulate)
        {
            //PATCH: update usage for power pools
            if (!simulate)
            {
                CustomFeaturesContext.RechargeLinkedPowers(__instance, restType);
            }

            // The player isn't recharging the shared pool features, just the pool.
            // Hide the features that use the pool from the UI.
            foreach (var feature in __instance.RecoveredFeatures.Where(f => f is IPowerSharedPool).ToArray())
            {
                __instance.RecoveredFeatures.Remove(feature);
            }
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: Makes powers that have their max usage extended by pool modifiers show up correctly during rest
            //replace calls to MaxUses getter to custom method that accounts for extended power usage
            var codes = instructions.ToList();
            var maxUses =
                new Func<RulesetUsablePower, RulesetCharacter, int>(CustomFeaturesContext.GetMaxUsesForPool).Method;

            var bind = typeof(RulesetUsablePower).GetMethod("get_MaxUses", BindingFlags.Public | BindingFlags.Instance);

            var bindIndex = codes.FindIndex(x => x.Calls(bind));

            if (bindIndex <= 0)
            {
                return codes.AsEnumerable();
            }

            codes[bindIndex] = new CodeInstruction(OpCodes.Call, maxUses);
            codes.Insert(bindIndex, new CodeInstruction(OpCodes.Ldarg_0));

            return codes.AsEnumerable();
        }
    }

    //PATCH: Correctly solve short rests for Warlocks under MC (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ApplyRest_Patch_2
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var restoreAllSpellSlotsMethod = typeof(RulesetSpellRepertoire).GetMethod("RestoreAllSpellSlots");
            var myRestoreAllSpellSlotsMethod = typeof(ApplyRest_Patch_2).GetMethod("RestoreAllSpellSlots");

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(restoreAllSpellSlotsMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // rulesetCharacter
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // restType
                    yield return new CodeInstruction(OpCodes.Call, myRestoreAllSpellSlotsMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static void RestoreAllSpellSlots(RulesetSpellRepertoire __instance, RulesetCharacter rulesetCharacter,
            RuleDefinitions.RestType restType)
        {
            if (restType == RuleDefinitions.RestType.LongRest
                || rulesetCharacter is not RulesetCharacterHero hero
                || !SharedSpellsContext.IsMulticaster(hero))
            {
                rulesetCharacter.RestoreAllSpellSlots();

                return;
            }

            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(hero);
            var warlockUsedSlots = SharedSpellsContext.GetWarlockUsedSlots(hero);

            foreach (var spellRepertoire in hero.SpellRepertoires
                         .Where(x => x.SpellCastingRace == null))
            {
                for (var i = -1; i <= warlockSpellLevel; i++)
                {
                    if (spellRepertoire.usedSpellsSlots.ContainsKey(i))
                    {
                        spellRepertoire.usedSpellsSlots[i] -= warlockUsedSlots;
                    }
                }

                spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
            }
        }
    }
}
