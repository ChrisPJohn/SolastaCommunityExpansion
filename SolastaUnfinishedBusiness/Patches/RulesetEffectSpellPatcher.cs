﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class RulesetEffectSpellPatcher
{
    //PATCH: support for `ICustomMagicEffectBasedOnCaster` and `IModifySpellEffect` 
    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.EffectDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static class EffectDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(RulesetEffectSpell __instance, ref EffectDescription __result)
        {
            // allowing to pick and/or tweak spell effect depending on some caster properties
            __result = PowerBundle.ModifySpellEffect(__result, __instance);
        }
    }

    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.SaveDC), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static class SaveDC_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(RulesetEffectSpell __instance, ref int __result)
        {
            //PATCH: allow devices have DC based on user or item summoner stats, instead of static value
            var originItem = __instance.OriginItem;

            if (originItem == null || __result >= 0)
            {
                return;
            }

            var caster = __instance.Caster;
            string className = null;

            if (__result == EffectHelpers.BasedOnItemSummoner)
            {
                caster = EffectHelpers.GetCharacterByEffectGuid(originItem.SourceSummoningEffectGuid) ?? caster;
            }

            var classHolder = originItem.ItemDefinition.GetFirstSubFeatureOfType<IClassHoldingFeature>();

            if (classHolder != null)
            {
                className = classHolder.Class.Name;
            }

            __result = EffectHelpers.CalculateSaveDc(caster, __instance.spellDefinition.effectDescription, className);
        }
    }

    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.MagicAttackBonus), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class MagicAttackBonus_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(RulesetEffectSpell __instance, ref int __result)
        {
            //PATCH: allow devices have magic attack bonus based on user or item summoner stats, instead of static value
            var originItem = __instance.OriginItem;

            if (originItem == null || originItem.UsableDeviceDescription.magicAttackBonus >= 0)
            {
                return;
            }

            var caster = __instance.Caster;
            string className = null;

            if (__result == EffectHelpers.BasedOnItemSummoner)
            {
                caster = EffectHelpers.GetCharacterByEffectGuid(originItem.SourceSummoningEffectGuid) ?? caster;
            }

            var classHolder = originItem.ItemDefinition.GetFirstSubFeatureOfType<IClassHoldingFeature>();

            if (classHolder != null)
            {
                className = classHolder.Class.Name;
            }

            var repertoire = caster.GetClassSpellRepertoire(className);

            if (repertoire != null)
            {
                __result = repertoire.SpellAttackBonus;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.MagicAttackTrends), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class MagicAttackTrends_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(RulesetEffectSpell __instance, ref List<RuleDefinitions.TrendInfo> __result)
        {
            //PATCH: allow devices have magic attack trends based on user or item summoner stats, instead of static value
            var originItem = __instance.OriginItem;

            if (originItem == null || originItem.UsableDeviceDescription.magicAttackBonus >= 0)
            {
                return;
            }

            var caster = __instance.Caster;
            string className = null;

            if (originItem.UsableDeviceDescription.magicAttackBonus == EffectHelpers.BasedOnItemSummoner)
            {
                caster = EffectHelpers.GetCharacterByEffectGuid(originItem.SourceSummoningEffectGuid) ?? caster;
            }

            var classHolder = originItem.ItemDefinition.GetFirstSubFeatureOfType<IClassHoldingFeature>();

            if (classHolder != null)
            {
                className = classHolder.Class.Name;
            }

            var repertoire = caster.GetClassSpellRepertoire(className);

            if (repertoire != null)
            {
                __result = repertoire.MagicAttackTrends;
            }
        }
    }

    //PATCH: Multiclass: enforces cantrips to be cast at character level 
    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.GetClassLevel))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class GetClassLevel_Patch
    {
        [UsedImplicitly]
        public static void Postfix(RulesetEffectSpell __instance, ref int __result, RulesetCharacter character)
        {
            if (character is not RulesetCharacterHero hero)
            {
                return;
            }

            if (__instance.SpellDefinition.SpellLevel == 0)
            {
                __result = hero.TryGetAttributeValue(AttributeDefinitions.CharacterLevel);
                return;
            }

            //PATCH: support for `IClassHoldingFeature`
            var holder = __instance.SpellDefinition.GetFirstSubFeatureOfType<IClassHoldingFeature>();

            if (holder == null)
            {
                return;
            }

            if (hero.ClassesAndLevels.TryGetValue(holder.Class, out var level))
            {
                __result = level;
            }
        }
    }

    //PATCH: enforces cantrips to be cast at character level
    [HarmonyPatch(typeof(RulesetEffectSpell), nameof(RulesetEffectSpell.ComputeTargetParameter))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ComputeTargetParameter_Patch
    {
        [NotNull]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //replaces repertoire's SpellCastingLevel with character level for cantrips
            var spellCastingLevelMethod = typeof(RulesetSpellRepertoire).GetMethod("get_SpellCastingLevel");
            var spellCastingLevel =
                new Func<RulesetSpellRepertoire, RulesetEffectSpell, int>(MulticlassContext.SpellCastingLevel).Method;

            return instructions.ReplaceCalls(spellCastingLevelMethod, "RulesetEffectSpell.ComputeTargetParameter",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, spellCastingLevel));
        }
    }
}
