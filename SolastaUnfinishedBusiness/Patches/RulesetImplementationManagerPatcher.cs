﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

internal static class RulesetImplementationManagerPatcher
{
    //PATCH: Applies custom effect forms
    [HarmonyPatch(typeof(RulesetImplementationManager), "ApplyEffectForms")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ApplyEffectForms_Patch
    {
        public static void Postfix(
            List<EffectForm> effectForms,
            RulesetImplementationDefinitions.ApplyFormsParams formsParams,
            List<string> effectiveDamageTypes,
            bool retargeting,
            bool proxyOnly,
            bool forceSelfConditionOnly,
            RuleDefinitions.EffectApplication effectApplication,
            List<EffectFormFilter> filters)
        {
            foreach (var customEffect in effectForms.OfType<CustomEffectForm>())
            {
                customEffect.ApplyForm(formsParams, retargeting, proxyOnly, forceSelfConditionOnly, effectApplication,
                    filters);
            }
        }
    }

    //PATCH:
    // Call parts of the stuff `RulesetImplementationManagerLocation` does for `RulesetImplementationManagerCampaign`
    // This makes light and item effects correctly terminate when resting during world travel
    // The code is prettified decompiled code from `RulesetImplementationManagerLocation`
    [HarmonyPatch(typeof(RulesetImplementationManager), "TerminateEffect")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class TerminateEffect_Patch
    {
        internal static void Postfix(RulesetImplementationManager __instance, RulesetEffect activeEffect)
        {
            if (__instance is not RulesetImplementationManagerCampaign)
            {
                return;
            }

            if (activeEffect is { TrackedLightSourceGuids.Count: > 0 })
            {
                var service = ServiceRepository.GetService<IGameLocationVisibilityService>();
                foreach (var trackedLightSourceGuid in activeEffect.TrackedLightSourceGuids)
                {
                    var rulesetLightSource = (RulesetLightSource)null;
                    ref var local = ref rulesetLightSource;

                    if (!RulesetEntity.TryGetEntity(trackedLightSourceGuid, out local) || rulesetLightSource == null)
                    {
                        continue;
                    }

                    rulesetLightSource.LightSourceExtinguished -= activeEffect.LightSourceExtinguished;
                    RulesetCharacter bearer;
                    if (rulesetLightSource.TargetItemGuid != 0UL &&
                        RulesetEntity.TryGetEntity(rulesetLightSource.TargetItemGuid, out RulesetItem rulesetItem))
                    {
                        if (RulesetEntity.TryGetEntity(rulesetItem.BearerGuid, out bearer) &&
                            bearer is { CharacterInventory: { } })
                        {
                            bearer.CharacterInventory.ItemAltered?.Invoke(bearer.CharacterInventory,
                                bearer.CharacterInventory.FindSlotHoldingItem(rulesetItem), rulesetItem);
                        }

                        var fromActor = GameLocationCharacter.GetFromActor(bearer);
                        service?.RemoveCharacterLightSource(fromActor, rulesetItem.RulesetLightSource);
                        rulesetItem.RulesetLightSource?.Unregister();
                        rulesetItem.RulesetLightSource = null;
                    }
                    else if (rulesetLightSource.TargetGuid != 0UL &&
                             RulesetEntity.TryGetEntity(rulesetLightSource.TargetGuid, out bearer))
                    {
                        var fromActor = GameLocationCharacter.GetFromActor(bearer);
                        service?.RemoveCharacterLightSource(fromActor, rulesetLightSource);
                        if (rulesetLightSource.UseSpecificLocationPosition)
                        {
                            if (bearer is RulesetCharacterEffectProxy proxy)
                            {
                                proxy.RemoveAdditionalPersonalLightSource(rulesetLightSource);
                            }
                        }
                        else if (bearer != null)
                        {
                            bearer.PersonalLightSource = null;
                        }
                    }
                }

                activeEffect.TrackedLightSourceGuids.Clear();
            }

            if (activeEffect is not { TrackedItemPropertyGuids.Count: > 0 })
            {
                return;
            }

            foreach (var itemPropertyGuid in activeEffect.TrackedItemPropertyGuids)
            {
                var rulesetItemProperty = (RulesetItemProperty)null;
                ref var local = ref rulesetItemProperty;
                if (!RulesetEntity.TryGetEntity(itemPropertyGuid, out local) || rulesetItemProperty == null)
                {
                    continue;
                }

                if (!RulesetEntity.TryGetEntity(rulesetItemProperty.TargetItemGuid,
                        out RulesetItem rulesetItem) || rulesetItem == null)
                {
                    continue;
                }

                rulesetItem.ItemPropertyRemoved -= activeEffect.ItemPropertyRemoved;
                rulesetItem.RemoveDynamicProperty(rulesetItemProperty);

                if (!RulesetEntity.TryGetEntity(rulesetItem.BearerGuid,
                        out RulesetCharacter rulesetItemBearer) || rulesetItemBearer == null)
                {
                    continue;
                }

                var characterInventory = rulesetItemBearer.CharacterInventory;

                characterInventory?.ItemAltered?.Invoke(characterInventory,
                    characterInventory.FindSlotHoldingItem(rulesetItem),
                    rulesetItem);

                rulesetItemBearer.RefreshAll();
            }
        }
    }

    //PATCH: handles Sorcerer wildshape scenarios / enforces sorcerer class level / correctly handle slots recovery scenarios
    [HarmonyPatch(typeof(RulesetImplementationManager), "ApplySpellSlotsForm")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetImplementationManager_ApplySpellSlotsForm
    {
        internal static bool Prefix(EffectForm effectForm,
            RulesetImplementationDefinitions.ApplyFormsParams formsParams)
        {
            var originalHero = formsParams.sourceCharacter as RulesetCharacterHero;
            var substituteHero =
                originalHero ?? formsParams.sourceCharacter.OriginalFormCharacter as RulesetCharacterHero;

            // this shouldn't happen so passing the problem back to original game code
            if (substituteHero == null)
            {
                return true;
            }

            // patch is only required for Wildshape Heroes or Multiclassed ones
            if (originalHero != null && !SharedSpellsContext.IsMulticaster(originalHero))
            {
                return true;
            }

            var spellSlotsForm = effectForm.SpellSlotsForm;

            switch (spellSlotsForm.Type)
            {
                case SpellSlotsForm.EffectType.RecoverHalfLevelUp
                    when SharedSpellsContext.RecoverySlots.TryGetValue(formsParams.activeEffect.Name,
                        out var invokerClass) && invokerClass is CharacterClassDefinition characterClassDefinition:
                {
                    foreach (var spellRepertoire in substituteHero.SpellRepertoires)
                    {
                        var currentValue = 0;

                        if (spellRepertoire.SpellCastingClass == characterClassDefinition)
                        {
                            currentValue = substituteHero.ClassesAndLevels[characterClassDefinition];
                        }
                        else if (spellRepertoire.SpellCastingSubclass != null)
                        {
                            var characterClass = substituteHero.ClassesAndSubclasses
                                .FirstOrDefault(x => x.Value == spellRepertoire.SpellCastingSubclass).Key;

                            if (characterClass == characterClassDefinition)
                            {
                                currentValue = substituteHero.ClassesAndLevels[characterClassDefinition];
                            }
                        }

                        if (currentValue <= 0)
                        {
                            continue;
                        }

                        var slotsCapital = (currentValue % 2) + (currentValue / 2);

                        Gui.GuiService.GetScreen<SlotRecoveryModal>()
                            .ShowSlotRecovery(substituteHero, formsParams.activeEffect.SourceDefinition.Name,
                                spellRepertoire, slotsCapital, spellSlotsForm.MaxSlotLevel);

                        break;
                    }

                    break;
                }
                //
                // handles Sorcerer and Wildshape scenarios slots / points scenarios
                //
                case SpellSlotsForm.EffectType.CreateSpellSlot or SpellSlotsForm.EffectType.CreateSorceryPoints:
                {
                    var spellRepertoire = substituteHero.SpellRepertoires.Find(sr => sr.SpellCastingClass == Sorcerer);

                    Gui.GuiService.GetScreen<FlexibleCastingModal>().ShowFlexibleCasting(substituteHero,
                        spellRepertoire,
                        spellSlotsForm.Type == SpellSlotsForm.EffectType.CreateSpellSlot);
                    break;
                }
                case SpellSlotsForm.EffectType.GainSorceryPoints:
                    formsParams.sourceCharacter.GainSorceryPoints(spellSlotsForm.SorceryPointsGain);
                    break;
                case SpellSlotsForm.EffectType.RecovererSorceryHalfLevelUp:
                {
                    var currentValue = substituteHero.ClassesAndLevels[Sorcerer];
                    var sorceryPointsGain = (currentValue % 2) + (currentValue / 2);

                    formsParams.sourceCharacter.GainSorceryPoints(sorceryPointsGain);
                    break;
                }
                case SpellSlotsForm.EffectType.RechargePower when formsParams.targetCharacter is RulesetCharacter:
                {
                    foreach (var usablePower in substituteHero.UsablePowers.Where(usablePower =>
                                 usablePower.PowerDefinition == spellSlotsForm.PowerDefinition))
                    {
                        usablePower.Recharge();
                    }

                    break;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetImplementationManager), "IsValidContextForRestrictedContextProvider")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsValidContextForRestrictedContextProvider_Patch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: support for shield counting as melee
            //replaces calls to ItemDefinition's isWeapon and Wea[ponDescription getter with custom ones that account for shield
            return ShieldAttack.MakeShieldCountAsMelee(instructions);
        }
    }
}
