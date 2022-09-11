﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaCommunityExpansion.Api;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.CustomDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaCommunityExpansion.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaCommunityExpansion.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaCommunityExpansion.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaCommunityExpansion.Models;

internal static class SrdAndHouseRulesContext
{
    internal static void Load()
    {
        AllowTargetingSelectionWhenCastingChainLightningSpell();
        ApplyConditionBlindedShouldNotAllowOpportunityAttack();
        ApplyAcNonStackingRules();
        ApplySrdWeightToFoodRations();
    }

    internal static void ApplyConditionBlindedShouldNotAllowOpportunityAttack()
    {
        // Use the shocked condition affinity which has the desired effect
        if (Main.Settings.BlindedConditionDontAllowAttackOfOpportunity)
        {
            if (!ConditionBlinded.Features.Contains(ActionAffinityConditionShocked))
            {
                ConditionBlinded.Features.Add(ActionAffinityConditionShocked);
            }
        }
        else
        {
            if (ConditionBlinded.Features.Contains(ActionAffinityConditionShocked))
            {
                ConditionBlinded.Features.Remove(ActionAffinityConditionShocked);
            }
        }
    }

    /// <summary>
    ///     Allow the user to select targets when using 'Chain Lightning'.
    /// </summary>
    internal static void AllowTargetingSelectionWhenCastingChainLightningSpell()
    {
        var spell = ChainLightning.EffectDescription;

        if (Main.Settings.AllowTargetingSelectionWhenCastingChainLightningSpell)
        {
            // This is half bug-fix, half houses rules since it's not completely SRD but better than implemented.
            // Spell should arc from target (range 150ft) onto upto 3 extra selectable targets (range 30ft from first).
            // Fix by allowing 4 selectable targets.
            spell.TargetType = RuleDefinitions.TargetType.IndividualsUnique;
            spell.SetTargetParameter(4);
            spell.effectAdvancement.additionalTargetsPerIncrement = 1;

            // TODO: may need to tweak range parameters but it works as is.
        }
        else
        {
            spell.TargetType = RuleDefinitions.TargetType.ArcFromIndividual;
            spell.SetTargetParameter(3);
            spell.effectAdvancement.additionalTargetsPerIncrement = 0;
        }
    }

    private static void ApplyAcNonStackingRules()
    {
        DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierBarbarianUnarmoredDefense
            .SetCustomSubFeatures(ExclusiveArmorClassBonus.Marker);
        DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierMonkUnarmoredDefense
            .SetCustomSubFeatures(ExclusiveArmorClassBonus.Marker);
        DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierSorcererDraconicResilienceAC
            .SetCustomSubFeatures(ExclusiveArmorClassBonus.Marker);
    }

    internal static void ApplySrdWeightToFoodRations()
    {
        var foodSrdWeight = DatabaseHelper.ItemDefinitions.Food_Ration;
        var foodForagedSrdWeight = DatabaseHelper.ItemDefinitions.Food_Ration_Foraged;

        if (Main.Settings.ApplySrdWeightToFoodRations)
        {
            foodSrdWeight.weight = 2.0f;
            foodForagedSrdWeight.weight = 2.0f;
        }
        else
        {
            foodSrdWeight.weight = 3.0f;
            foodForagedSrdWeight.weight = 3.0f;
        }
    }

    //
    //
    //

    internal static void RemoveConcentrationRequirementsFromAnySpell()
    {
        if (!Main.Settings.RemoveConcentrationRequirementsFromAnySpell)
        {
            return;
        }

        foreach (var spell in DatabaseRepository.GetDatabase<SpellDefinition>())
        {
            spell.requiresConcentration = false;
        }
    }

    internal static void RemoveHumanoidFilterOnHideousLaughter()
    {
        if (!Main.Settings.RemoveHumanoidFilterOnHideousLaughter)
        {
            return;
        }

        // Remove Humanoid only filter on Hideous Laughter (as per SRD, any creature can be targeted)
        HideousLaughter.effectDescription.restrictedCreatureFamilies.Clear();
    }

    internal static void RemoveRecurringEffectOnEntangle()
    {
        if (!Main.Settings.RemoveRecurringEffectOnEntangle)
        {
            return;
        }

        // Remove recurring effect on Entangle (as per SRD, any creature is only affected at cast time)
        Entangle.effectDescription.recurrentEffect = RuleDefinitions.RecurrentEffect.OnActivation;
    }


    internal static void MinorFixes()
    {
        // Shows Concentration tag in UI
        BladeBarrier.requiresConcentration = true;

        //
        // BUGFIX: spells durations
        //

        // Use our logic to calculate duration for DominatePerson/Beast/Monster
        DominateBeast.EffectDescription.EffectAdvancement.alteredDuration =
            (RuleDefinitions.AdvancementDuration)ExtraAdvancementDuration.DominateBeast;
        DominatePerson.EffectDescription.EffectAdvancement.alteredDuration =
            (RuleDefinitions.AdvancementDuration)ExtraAdvancementDuration.DominatePerson;

        // Stops upcasting assigning non-SRD durations
        ClearAlteredDuration(ProtectionFromEnergy);
        ClearAlteredDuration(ProtectionFromEnergyAcid);
        ClearAlteredDuration(ProtectionFromEnergyCold);
        ClearAlteredDuration(ProtectionFromEnergyFire);
        ClearAlteredDuration(ProtectionFromEnergyLightning);
        ClearAlteredDuration(ProtectionFromEnergyThunder);
        ClearAlteredDuration(ProtectionFromPoison);

        static void ClearAlteredDuration([NotNull] IMagicEffect spell)
        {
            spell.EffectDescription.EffectAdvancement.alteredDuration = RuleDefinitions.AdvancementDuration.None;
        }
    }

    internal static void UseCubeOnSleetStorm()
    {
        var sleetStormEffect = SleetStorm.EffectDescription;

        if (Main.Settings.ChangeSleetStormToCube)
        {
            // Set to Cube side 8, default height
            sleetStormEffect.targetType = RuleDefinitions.TargetType.Cube;
            sleetStormEffect.targetParameter = 8;
            sleetStormEffect.targetParameter2 = 0;
        }
        else
        {
            // Restore to cylinder radius 4, height 3
            sleetStormEffect.targetType = RuleDefinitions.TargetType.Cylinder;
            sleetStormEffect.targetParameter = 4;
            sleetStormEffect.targetParameter2 = 3;
        }
    }

    internal static void UseHeightOneCylinderEffect()
    {
        // always applicable
        ClearTargetParameter2ForTargetTypeCube();

        ///////////////////////////////////////////////////////////
        // Change SpikeGrowth to be height 1 round cylinder/sphere
        var spikeGrowthEffect = SpikeGrowth.EffectDescription;
        spikeGrowthEffect.targetParameter = 4;

        if (Main.Settings.UseHeightOneCylinderEffect)
        {
            // Set to Cylinder radius 4, height 1
            spikeGrowthEffect.targetType = RuleDefinitions.TargetType.Cylinder;
            spikeGrowthEffect.targetParameter2 = 1;
        }
        else
        {
            // Restore default of Sphere radius 4
            spikeGrowthEffect.targetType = RuleDefinitions.TargetType.Sphere;
            spikeGrowthEffect.targetParameter2 = 0;
        }

        // Spells with TargetType.Cube and defaults values of (tp, tp2)
        // Note that tp2 should be 0 for Cube and is ignored in game.
        // BlackTentacles: (4, 2)
        // Entangle: (4, 1)
        // FaerieFire: (4, 2)
        // FlamingSphere: (3, 2) <- a flaming sphere is a cube?
        // Grease: (2, 2)
        // HypnoticPattern: (6, 2)
        // Slow: (8, 2)
        // Thunderwave: (3, 2)


        ///////////////////////////////////////////////////////////
        // Change Black Tentacles, Entangle, Grease to be height 1 square cylinder/cube
        if (Main.Settings.UseHeightOneCylinderEffect)
        {
            // Setting height switches to square cylinder (if originally cube)
            SetHeight(BlackTentacles, 1);
            SetHeight(Entangle, 1);
            SetHeight(Grease, 1);
        }
        else
        {
            // Setting height to 0 restores original behaviour
            SetHeight(BlackTentacles, 0);
            SetHeight(Entangle, 0);
            SetHeight(Grease, 0);
        }

        static void SetHeight([NotNull] IMagicEffect spellDefinition, int height)
        {
            spellDefinition.EffectDescription.targetParameter2 = height;
        }

        static void ClearTargetParameter2ForTargetTypeCube()
        {
            foreach (var sd in DatabaseRepository
                         .GetDatabase<SpellDefinition>()
                         .Where(sd =>
                             sd.EffectDescription.TargetType is RuleDefinitions.TargetType.Cube
                                 or RuleDefinitions.TargetType.CubeWithOffset))
            {
                // TargetParameter2 is not used by TargetType.Cube but has random values assigned.
                // We are going to use it to create a square cylinder with height so set to zero for all spells with TargetType.Cube.
                sd.EffectDescription.SetTargetParameter2(0);
            }
        }
    }

    internal static void AddBleedingToRestoration()
    {
        var cf = LesserRestoration.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.Condition);

        if (cf != null)
        {
            if (Main.Settings.AddBleedingToLesserRestoration)
            {
                cf.ConditionForm.ConditionsList.TryAdd(ConditionBleeding);
            }
            else
            {
                cf.ConditionForm.ConditionsList.Remove(ConditionBleeding);
            }
        }
        else
        {
            Main.Error("Unable to find form of type Condition in LesserRestoration");
        }

        var cfg = GreaterRestoration.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.Condition);

        if (cfg != null)
        {
            // NOTE: using the same setting as for Lesser Restoration for compatibility
            if (Main.Settings.AddBleedingToLesserRestoration)
            {
                cfg.ConditionForm.ConditionsList.TryAdd(ConditionBleeding);
            }
            else
            {
                cfg.ConditionForm.ConditionsList.Remove(ConditionBleeding);
            }
        }
        else
        {
            Main.Error("Unable to find form of type Condition in GreaterRestoration");
        }
    }
}

internal static class ArmorClassStacking
{
    //replaces call to `RulesetAttributeModifier.BuildAttributeModifier` with custom method that calls base on e and adds extra tags when necessary
    public static void AddCustomTagsToModifierBuilder(List<CodeInstruction> codes)
    {
        var method =
            new Func<AttributeModifierOperation, float, string, string, RulesetAttributeModifier>(RulesetAttributeModifier.BuildAttributeModifier).Method;

        var index = codes.FindIndex(c => c.Calls(method));

        if (index <= 0)
        {
            return;
        }

        var custom =
            new Func<AttributeModifierOperation, float, string, string, FeatureDefinitionAttributeModifier, RulesetAttributeModifier>(CustomBuildAttributeModifier).Method;

        codes[index] = new CodeInstruction(OpCodes.Call, custom); //replace call with custom method
        codes.Insert(index, new CodeInstruction(OpCodes.Ldloc_1)); // load 'feature' as last argument
    }

    private static RulesetAttributeModifier CustomBuildAttributeModifier(
        AttributeModifierOperation operationType,
        float modifierValue,
        string tag,
        string sourceAbility,
        FeatureDefinitionAttributeModifier feature)
    {
        var modifier = RulesetAttributeModifier.BuildAttributeModifier(operationType, modifierValue, tag);
        if (feature.HasSubFeatureOfType<ExclusiveArmorClassBonus>())
        {
            modifier.Tags.Add(ExclusiveArmorClassBonus.Tag);
        }

        return modifier;
    }

    // Replaces calls to `RulesetAttributeModifier.SortAttributeModifiersList` with custom method
    // that removes inactive exclusive modifiers, and then calls `RulesetAttributeModifier.SortAttributeModifiersList`
    public static IEnumerable<CodeInstruction> UnstackACTranspile(IEnumerable<CodeInstruction> instructions)
    {
        var sort = new Action<
            List<RulesetAttributeModifier>
        >(RulesetAttributeModifier.SortAttributeModifiersList).Method;

        var unstack = new Action<
            List<RulesetAttributeModifier>
        >(UnstackAC).Method;

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(sort))
            {
                yield return new CodeInstruction(OpCodes.Call, unstack);
            }
            else
            {
                yield return instruction;
            }
        }
    }
    
    public static void UnstackAC(List<RulesetAttributeModifier> modifiers)
    {
        var attributes = new List<RulesetAttributeModifier>();
        var sets = new List<RulesetAttributeModifier>();

        foreach (var modifier in modifiers)
        {
            if (!modifier.tags.Contains(ExclusiveArmorClassBonus.Tag))
            {
                continue;
            }

            if (modifier.operation == AttributeModifierOperation.Set)
            {
                sets.Add(modifier);
            }

            if (modifier.operation == AttributeModifierOperation.AddAbilityScoreBonus)
            {
                attributes.Add(modifier);
            }
        }

        //sort modifiers so that biggest is first
        attributes.Sort((left, right) => -left.value.CompareTo(right.value));
        sets.Sort((left, right) => -left.value.CompareTo(right.value));

        //get best modifiers
        var bestAttributeBonusMod = attributes.Count > 0 ? attributes[0] : null;
        var bestSetMod = sets.Count > 0 ? sets[0] : null;

        //we have both exclusive attribute (Wise Defense) and exclusive set mods (Dragon Resilience AC bonus)
        //we need to leave only one that grants best results
        if (bestSetMod != null && bestAttributeBonusMod != null)
        {
            if (bestSetMod.value > bestAttributeBonusMod.value + 10)
            {
                //remove biggest set mod, so it will remain in final list
                sets.RemoveAt(0);
            }
            else
            {
                //remove biggest attribute bonus mod, so it will remain in final list
                attributes.RemoveAt(0);
            }

            //remove all exclusive mods
            modifiers.RemoveAll(m => attributes.Contains(m));
            modifiers.RemoveAll(m => sets.Contains(m));
        }

        //sort modifiers
        RulesetAttributeModifier.SortAttributeModifiersList(modifiers);
    }
}

/// <summary>
///     Allow spells that require consumption of a material component (e.g. a gem of value >= 1000gp) use a stack
///     of lesser value components (e.g. 4 x 300gp diamonds).
///     Note that this implementation will only work with identical components - e.g. 'all diamonds', it won't consider
///     combining
///     different types of items with the tag 'gem'.
///     TODO: if anyone requests it we can improve with GroupBy etc...
/// </summary>
public static class StackedMaterialComponent
{
    public static void IsComponentMaterialValid(
        RulesetCharacter character,
        SpellDefinition spellDefinition,
        ref string failure,
        ref bool result)
    {
        if (!Main.Settings.AllowStackedMaterialComponent)
        {
            return;
        }

        if (result)
        {
            return;
        }

        // Repeats the last section of the original method but adds 'approximateCostInGold * item.StackCount'
        var items = new List<RulesetItem>();

        character.CharacterInventory.EnumerateAllItems(items);

        if ((from item in items
                let approximateCostInGold = EquipmentDefinitions.GetApproximateCostInGold(item.ItemDefinition.Costs)
                where item.ItemDefinition.ItemTags.Contains(spellDefinition.SpecificMaterialComponentTag) &&
                      approximateCostInGold * item.StackCount >= spellDefinition.SpecificMaterialComponentCostGp
                select item).Any())
        {
            result = true;
            failure = string.Empty;
        }
    }

    //Modify original code to spend enough of a stack to meet component cost
    public static bool SpendSpellMaterialComponentAsNeeded(RulesetCharacter character, RulesetEffectSpell activeSpell)
    {
        if (!Main.Settings.AllowStackedMaterialComponent)
        {
            return true;
        }

        var spell = activeSpell.SpellDefinition;
        if (spell.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific
            || !spell.SpecificMaterialComponentConsumed
            || string.IsNullOrEmpty(spell.SpecificMaterialComponentTag)
            || spell.SpecificMaterialComponentCostGp <= 0
            || character.CharacterInventory == null)
        {
            return false;
        }

        var items = new List<RulesetItem>();

        character.CharacterInventory.EnumerateAllItems(items);

        var itemToUse = items
            .Where(item => item.ItemDefinition.ItemTags.Contains(spell.SpecificMaterialComponentTag))
            .Select(item => new
            {
                RulesetItem = item,
                // Note original code is "int cost = rulesetItem2.ItemDefinition.Costs[1];" which doesn't agree with IsComponentMaterialValid which
                // uses GetApproximateCostInGold
                Cost = EquipmentDefinitions.GetApproximateCostInGold(item.ItemDefinition.Costs)
            })
            .Select(item => new
            {
                item.RulesetItem,
                item.Cost,
                StackCountRequired = (int)Math.Ceiling(spell.SpecificMaterialComponentCostGp / (double)item.Cost)
            })
            .Where(item => item.StackCountRequired <= item.RulesetItem.StackCount)
            .Select(item => new
            {
                item.RulesetItem,
                item.Cost,
                item.StackCountRequired,
                TotalCost = item.StackCountRequired * item.Cost
            })
            .Where(item => item.TotalCost >= activeSpell.SpellDefinition.SpecificMaterialComponentCostGp)
            .OrderBy(item => item.TotalCost) // min total cost used
            .ThenBy(item => item.StackCountRequired) // min items used
            .FirstOrDefault();

        if (itemToUse == null)
        {
            Main.Log("Didn't find item.");

            return false;
        }

        Main.Log($"Spending stack={itemToUse.StackCountRequired}, cost={itemToUse.TotalCost}");

        var componentConsumed = character.SpellComponentConsumed;

        if (componentConsumed != null)
        {
            for (var i = 0; i < itemToUse.StackCountRequired; i++)
            {
                componentConsumed(character, spell, itemToUse.RulesetItem);
            }
        }

        var rulesetItem = itemToUse.RulesetItem;

        if (rulesetItem.ItemDefinition.CanBeStacked && rulesetItem.StackCount > 1 &&
            itemToUse.StackCountRequired < rulesetItem.StackCount)
        {
            Main.Log($"Spending stack={itemToUse.StackCountRequired}, cost={itemToUse.TotalCost}");

            rulesetItem.SpendStack(itemToUse.StackCountRequired);
        }
        else
        {
            Main.Log("Destroy item");

            character.CharacterInventory.DestroyItem(rulesetItem);
        }

        return false;
    }
}

internal static class UpcastConjureElementalAndFey
{
    private static List<SpellDefinition> _filteredSubspells;

    /**
     * Patch implementation
     * Replaces subspell activation with custom code for upcasted elementals/fey
     */
    internal static bool CheckSubSpellActivated(SubspellSelectionModal __instance, int index)
    {
        if (!Main.Settings.EnableUpcastConjureElementalAndFey || _filteredSubspells is not { Count: > 0 })
        {
            return true;
        }

        if (_filteredSubspells.Count <= index)
        {
            return true;
        }

        __instance.spellCastEngaged?.Invoke(__instance.spellRepertoire, _filteredSubspells[index],
            __instance.slotLevel);

        __instance.Hide();

        _filteredSubspells.Clear();

        return false;
    }

    /**
     * Patch implementation
     * Replaces calls to masterSpell.SubspellsList getter with custom method that adds extra options for upcasted elementals/fey
     */
    internal static IEnumerable<CodeInstruction> ReplaceSubSpellList(IEnumerable<CodeInstruction> instructions)
    {
        var subspellsListMethod = typeof(SpellDefinition).GetMethod("get_SubspellsList");
        var getSpellList = new Func<SpellDefinition, int, List<SpellDefinition>>(SubspellsList);

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(subspellsListMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldarg, 5); // slotLevel
                yield return new CodeInstruction(OpCodes.Call, getSpellList.Method);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [CanBeNull]
    private static List<SpellDefinition> SubspellsList([NotNull] SpellDefinition masterSpell, int slotLevel)
    {
        var subspellsList = masterSpell.SubspellsList;
        var mySlotLevel = masterSpell.Name == ConjureElemental.Name
                          || masterSpell.Name == ConjureFey.Name
            ? slotLevel
            : -1;

        if (!Main.Settings.EnableUpcastConjureElementalAndFey || mySlotLevel < 0 || subspellsList == null ||
            subspellsList.Count == 0)
        {
            return subspellsList;
        }

        var subspellsGroupedAndFilteredByCR = subspellsList
            .Select(s =>
                new
                {
                    SpellDefinition = s,
                    s.EffectDescription
                        .GetFirstFormOfType(EffectForm.EffectFormType.Summon)
                        .SummonForm
                        .MonsterDefinitionName
                }
            )
            .Select(s => new
            {
                s.SpellDefinition,
                s.MonsterDefinitionName,
                ChallengeRating =
                    DatabaseRepository.GetDatabase<MonsterDefinition>()
                        .TryGetElement(s.MonsterDefinitionName, out var monsterDefinition)
                        ? monsterDefinition.ChallengeRating
                        : int.MaxValue
            })
            .GroupBy(s => s.ChallengeRating)
            .Select(g => new
            {
                ChallengeRating = g.Key,
                SpellDefinitions = g.Select(s => s.SpellDefinition)
                    .OrderBy(s => Gui.Localize(s.GuiPresentation.Title))
            })
            .Where(s => s.ChallengeRating <= mySlotLevel)
            .OrderByDescending(s => s.ChallengeRating)
            .ToList();

        var allOrMostPowerful = Main.Settings.OnlyShowMostPowerfulUpcastConjuredElementalOrFey
            ? subspellsGroupedAndFilteredByCR.Take(1).ToList()
            : subspellsGroupedAndFilteredByCR;

        _filteredSubspells = allOrMostPowerful.SelectMany(s => s.SpellDefinitions).ToList();

        _filteredSubspells.ForEach(s => Main.Log($"{Gui.Localize(s.GuiPresentation.Title)}"));

        return _filteredSubspells;
    }
}
