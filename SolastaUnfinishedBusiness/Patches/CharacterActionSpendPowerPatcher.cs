﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Interfaces;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CharacterActionSpendPowerPatcher
{
    [HarmonyPatch(typeof(CharacterActionSpendPower), nameof(CharacterActionSpendPower.ExecuteImpl))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ExecuteImpl_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            out IEnumerator __result,
            CharacterActionSpendPower __instance)
        {
            __result = ExecuteImpl(__instance);

            return false;
        }

        private static IEnumerator ExecuteImpl(CharacterActionSpendPower __instance)
        {
            var actingCharacter = __instance.ActingCharacter;
            var actionParams = __instance.ActionParams;
            var rulesetEffect = actionParams.RulesetEffect;
            var effectDescription = rulesetEffect.EffectDescription;

            var rulesetImplementationService = ServiceRepository.GetService<IRulesetImplementationService>();

            // Retrieve the target
            __instance.targets.Clear();
            __instance.targets.AddRange(actionParams.TargetCharacters);


            // BEGIN PATCH

            if (rulesetEffect is not RulesetEffectPower { OriginItem: null } activePower)
            {
                yield break;
            }

            // END PATCH

            // Create an active power
            __instance.activePower = activePower;

            var actionModifier = new ActionModifier();

            // Spend the power, if this does not come from an item
            if (__instance.activePower is { OriginItem: null })
            {
                // Fire shield retaliation has no class or race origin
                if (__instance.activePower.UsablePower.OriginClass != null ||
                    __instance.activePower.UsablePower.OriginRace != null)
                {
                    actingCharacter.RulesetCharacter.UsePower(__instance.activePower.UsablePower);
                }

                // BEGIN PATCH

                //PATCH: support for shared pool powers that character got from conditions to properly consume uses when triggered
                if (activePower.UsablePower.powerDefinition.HasSubFeatureOfType<ForcePowerUseInSpendPowerAction>())
                {
                    __instance.ActingCharacter.RulesetCharacter.UsePower(activePower.UsablePower);
                }

                // END PATCH
            }
            else
            {
                if (__instance.activePower != null)
                {
                    actingCharacter.RulesetCharacter.UseDevicePower(
                        __instance.activePower.OriginItem, __instance.activePower.PowerDefinition);
                }
            }

            for (var i = 0; i < __instance.targets.Count; i++)
            {
                var target = __instance.targets[i];

                // These bool information must be store as a class member, as it is passed to HandleFailedSavingThrow
                var hasBorrowedLuck =
                    target.RulesetActor.HasConditionOfTypeOrSubType(RuleDefinitions.ConditionBorrowedLuck);

                if (__instance.activePower != null)
                {
                    __instance.RolledSaveThrow = __instance.activePower.TryRollSavingThrow(
                        actingCharacter.RulesetCharacter, actingCharacter.Side,
                        target.RulesetActor,
                        actionModifier,
                        __instance.activePower.EffectDescription.EffectForms,
                        false,
                        out var saveOutcome,
                        out var saveOutcomeDelta);
                    __instance.SaveOutcome = saveOutcome;
                    __instance.SaveOutcomeDelta = saveOutcomeDelta;

                    if (__instance.RolledSaveThrow)
                    {
                        // Legendary Resistance or Indomitable?
                        var battleService = ServiceRepository.GetService<IGameLocationBattleService>();

                        if (__instance.SaveOutcome == RuleDefinitions.RollOutcome.Failure)
                        {
                            yield return battleService.HandleFailedSavingThrow(
                                __instance, actingCharacter, target, actionModifier, false, hasBorrowedLuck);
                        }

                        // BEGIN PATCH

                        //PATCH: support for `ITryAlterOutcomeSavingThrow`
                        yield return TryAlterOutcomeSavingThrowFromAllyOrEnemy.Handler(
                            battleService as GameLocationBattleManager,
                            __instance, actingCharacter, target, actionModifier, hasBorrowedLuck);

                        // END PATCH
                    }

                    // Apply the forms of the power
                    var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams();

                    applyFormsParams.FillSourceAndTarget(actingCharacter.RulesetCharacter, target.RulesetCharacter);
                    applyFormsParams.FillFromActiveEffect(__instance.activePower);
                    applyFormsParams.FillSpecialParameters(
                        __instance.RolledSaveThrow,
                        0, 0, 0, 0,
                        actionModifier,
                        __instance.SaveOutcome,
                        __instance.SaveOutcomeDelta,
                        false,
                        0,
                        1,
                        null);
                    applyFormsParams.effectSourceType = RuleDefinitions.EffectSourceType.Power;
                    rulesetImplementationService.ApplyEffectForms(
                        effectDescription.EffectForms,
                        applyFormsParams,
                        null,
                        effectApplication: effectDescription.EffectApplication,
                        filters: effectDescription.EffectFormFilters,
                        damageAbsorbedByTemporaryHitPoints: out _,
                        terminateEffectOnTarget: out _);

                    // Impact particles
                    var gameLocationPositioningService =
                        ServiceRepository.GetService<IGameLocationPositioningService>();
                    var impactPoint = new Vector3();
                    var impactRotation = Quaternion.identity;

                    gameLocationPositioningService.ComputeImpactCenterPositionAndRotation(
                        target, ref impactPoint, ref impactRotation);

                    var impactPlanePoint = gameLocationPositioningService.GetImpactPlanePosition(impactPoint);
                    var impactTarget = new ActionDefinitions.MagicEffectCastData
                    {
                        Source = __instance.activePower.Name,
                        EffectDescription = effectDescription,
                        Caster = actingCharacter,
                        Targets = __instance.targets,
                        TargetIndex = i,
                        SubTargets = actionParams.SubTargets,
                        SubTargetIndex = 0,
                        ImpactPoint = impactPoint,
                        ImpactRotation = impactRotation,
                        ImpactPlanePoint = impactPlanePoint,
                        ActionType = __instance.ActionType,
                        ActionId = __instance.ActionId,
                        IsDivertHit = false,
                        ComputedTargetParameter = __instance.activePower.ComputeTargetParameter()
                    };

                    ServiceRepository.GetService<IGameLocationActionService>()?
                        .MagicEffectHitTarget?.Invoke(ref impactTarget);
                }

                if (!__instance.RolledSaveThrow ||
                    __instance.SaveOutcome == RuleDefinitions.RollOutcome.Failure ||
                    __instance.SaveOutcome == RuleDefinitions.RollOutcome.CriticalFailure)
                {
                    yield return __instance.HandlePostSuccessfulExecutionOnTarget();
                }
            }

            __instance.PersistantEffectAction();

            yield return null;
        }
    }
}
