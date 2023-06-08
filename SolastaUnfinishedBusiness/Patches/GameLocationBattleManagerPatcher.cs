﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using TA;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class GameLocationBattleManagerPatcher
{
    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.CanCharacterUsePower))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class CanCharacterUsePower_Patch
    {
        [UsedImplicitly]
        public static void Postfix(
            ref bool __result,
            RulesetCharacter caster,
            RulesetUsablePower usablePower)
        {
            //PATCH: support for `IPowerUseValidity` when trying to react with power 
            if (!caster.CanUsePower(usablePower.PowerDefinition))
            {
                __result = false;
            }

            //PATCH: support for `IReactionAttackModeRestriction`
            if (__result)
            {
                __result = RestrictReactionAttackMode.CanCharacterReactWithPower(usablePower);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.CanPerformReadiedActionOnCharacter))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class CanPerformReadiedActionOnCharacter_Patch
    {
        [NotNull]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: Makes only preferred cantrip valid if it is selected and forced
            var customBindMethod =
                new Func<List<SpellDefinition>, SpellDefinition, bool>(CustomReactionsContext.CheckAndModifyCantrips)
                    .Method;

            //PATCH: allows to ready non-standard ranged attacks (like Armorer's Lightning Launcher)
            var customFindMethod =
                new Func<
                        GameLocationCharacter, // character,
                        ActionDefinitions.Id, // actionId,
                        bool, // getWithMostAttackNb,
                        bool, // onlyIfRemainingUses,
                        bool, // onlyIfCanUseAction
                        ActionDefinitions.ReadyActionType, // readyActionType
                        RulesetAttackMode //result
                    >(FindActionAttackMode)
                    .Method;

            return instructions
                .ReplaceCall(
                    "Contains",
                    -1,
                    "GameLocationBattleManager.CanPerformReadiedActionOnCharacter.Contains",
                    new CodeInstruction(OpCodes.Call, customBindMethod)
                )
                .ReplaceCall(
                    "FindActionAttackMode",
                    -1,
                    "GameLocationBattleManager.CanPerformReadiedActionOnCharacter.FindActionAttackMode",
                    new CodeInstruction(OpCodes.Call, customFindMethod)
                );
        }

        private static RulesetAttackMode FindActionAttackMode(
            GameLocationCharacter character,
            ActionDefinitions.Id actionId,
            bool getWithMostAttackNb,
            bool onlyIfRemainingUses,
            bool onlyIfCanUseAction,
            ActionDefinitions.ReadyActionType readyActionType
        )
        {
            var attackMode =
                character.FindActionAttackMode(actionId, getWithMostAttackNb, onlyIfRemainingUses, onlyIfCanUseAction,
                    readyActionType);

            if (readyActionType != ActionDefinitions.ReadyActionType.Ranged)
            {
                return attackMode;
            }

            if (attackMode != null && (attackMode.Ranged || attackMode.Thrown))
            {
                return attackMode;
            }

            return character.GetFirstRangedModeThatCanBeReadied();
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.IsValidAttackForReadiedAction))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class IsValidAttackForReadiedAction_Patch
    {
        [UsedImplicitly]
        public static void Postfix(
            ref bool __result,
            BattleDefinitions.AttackEvaluationParams attackParams)
        {
            //PATCH: Checks if attack cantrip is valid to be cast as readied action on a target
            // Used to properly check if melee cantrip can hit target when used for readied action

            if (!DatabaseHelper.TryGetDefinition<SpellDefinition>(attackParams.effectName, out var cantrip))
            {
                return;
            }

            var canAttack = cantrip.GetFirstSubFeatureOfType<IAttackAfterMagicEffect>()?.CanAttack;

            if (canAttack != null)
            {
                __result = canAttack(attackParams.attacker, attackParams.defender);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.HandleCharacterMoveStart))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterMoveStart_Patch
    {
        [UsedImplicitly]
        public static void Prefix(
            GameLocationCharacter mover,
            int3 destination)
        {
            //PATCH: support for Polearm Expert AoO
            //Stores character movements to be processed later
            AttacksOfOpportunity.ProcessOnCharacterMoveStart(mover, destination);
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.HandleCharacterMoveEnd))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterMoveEnd_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter mover)
        {
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            //PATCH: support for Polearm Expert AoO. processes saved movement to trigger AoO when appropriate
            // ReSharper disable once InvertIf
            if (Gui.Battle != null &&
                mover.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var extraEvents = AttacksOfOpportunity.ProcessOnCharacterMoveEnd(__instance, mover);

                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.PrepareBattleEnd))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class PrepareBattleEnd_Patch
    {
        [UsedImplicitly]
        public static void Prefix()
        {
            //PATCH: support for Polearm Expert AoO
            //clears movement cache on battle end

            AttacksOfOpportunity.CleanMovingCache();
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleBardicInspirationForAttack))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleBardicInspirationForAttack_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter target,
            ActionModifier attackModifier)
        {
            //PATCH: support for IAlterAttackOutcome
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (action.BardicDieRoll > 0)
            {
                action.AttackSuccessDelta += action.BardicDieRoll;
            }

            // ReSharper disable once InvertIf
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                target.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                foreach (var extraEvents in attacker.RulesetCharacter
                             .GetSubFeaturesByType<IPhysicalAttackTryAlterOutcome>()
                             .TakeWhile(_ =>
                                 action.AttackRollOutcome == RuleDefinitions.RollOutcome.Failure &&
                                 action.AttackSuccessDelta < 0)
                             .Select(feature =>
                                 feature.OnAttackTryAlterOutcome(__instance, action, attacker, target, attackModifier)))
                {
                    while (extraEvents.MoveNext())
                    {
                        yield return extraEvents.Current;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.HandleCharacterAttackFinished))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterAttackFinished_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackerAttackMode,
            RulesetEffect rulesetEffect,
            int damageAmount
        )
        {
            //PATCH: support for Sentinel feat - allows reaction attack on enemy attacking ally 
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (rulesetEffect != null && rulesetEffect.EffectDescription.RangeType is
                    not (RuleDefinitions.RangeType.MeleeHit or RuleDefinitions.RangeType.RangeHit))
            {
                yield break;
            }

            //PATCH: support for Sentinel Fighting Style - allows attacks of opportunity on enemies attacking allies
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var extraEvents =
                    AttacksOfOpportunity.ProcessOnCharacterAttackFinished(__instance, attacker, defender);

                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }

            //PATCH: support for Defensive Strike Power - allows adding Charisma modifier and chain reactions
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var defensiveEvents =
                    DefensiveStrikeAttack.ProcessOnCharacterAttackFinished(__instance, attacker, defender);

                while (defensiveEvents.MoveNext())
                {
                    yield return defensiveEvents.Current;
                }
            }


            //PATCH: support for Aura of the Guardian power - allows swapping hp on enemy attacking ally
            // ReSharper disable once InvertIf
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var guardianEvents =
                    GuardianAuraHpSwap.ProcessOnCharacterAttackHitFinished(
                        __instance, attacker, defender, attackerAttackMode, rulesetEffect, damageAmount);

                while (guardianEvents.MoveNext())
                {
                    yield return guardianEvents.Current;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleCharacterAttackHitConfirmed))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterAttackHitConfirmed_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            RuleDefinitions.AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget)
        {
            //PATCH: support for `IPhysicalAttackBeforeHitConfirmedOnEnemy`
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var rulesetAttacker = attacker.RulesetCharacter;

                foreach (var extra in rulesetAttacker
                             .GetSubFeaturesByType<IPhysicalAttackBeforeHitConfirmedOnEnemy>()
                             .Select(feature => feature.OnAttackBeforeHitConfirmedOnEnemy(
                                 __instance,
                                 attacker,
                                 defender,
                                 attackModifier,
                                 attackMode,
                                 rangedAttack,
                                 advantageType,
                                 actualEffectForms,
                                 rulesetEffect,
                                 criticalHit,
                                 firstTarget)))
                {
                    while (extra.MoveNext())
                    {
                        yield return extra.Current;
                    }
                }
            }

            //PATCH: support for `IPhysicalAttackBeforeHitConfirmedOnMe`
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                var rulesetDefender = defender.RulesetCharacter;

                foreach (var extra in rulesetDefender
                             .GetSubFeaturesByType<IPhysicalAttackBeforeHitConfirmedOnMe>()
                             .Select(feature => feature.OnAttackBeforeHitConfirmed(
                                 __instance,
                                 attacker,
                                 defender,
                                 attackModifier,
                                 attackMode,
                                 rangedAttack,
                                 advantageType,
                                 actualEffectForms,
                                 rulesetEffect,
                                 criticalHit,
                                 firstTarget)))
                {
                    while (extra.MoveNext())
                    {
                        yield return extra.Current;
                    }
                }
            }

            while (values.MoveNext())
            {
                yield return values.Current;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleCharacterAttackHitPossible))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterAttackHitPossible_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect,
            ActionModifier attackModifier,
            int attackRoll
        )
        {
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                //PATCH: Support for Spiritual Shielding feature - allows reaction before hit confirmed
                var blockEvents =
                    BlockAttacks.ProcessOnCharacterAttackHitConfirm(__instance, attacker, defender, attackMode,
                        rulesetEffect, attackModifier, attackRoll);

                while (blockEvents.MoveNext())
                {
                    yield return blockEvents.Current;
                }
            }

            // ReSharper disable once InvertIf
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                //PATCH: support for 'IAttackHitPossible'
                foreach (var extra in defender.RulesetCharacter
                             .GetSubFeaturesByType<IAttackHitPossible>()
                             .Select(feature => feature.DefenderAttackHitPossible(
                                 __instance,
                                 attacker,
                                 defender,
                                 attackMode,
                                 rulesetEffect,
                                 attackModifier,
                                 attackRoll)))
                {
                    while (extra.MoveNext())
                    {
                        yield return extra.Current;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleAttackerTriggeringPowerOnCharacterAttackHitConfirmed))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleAttackerTriggeringPowerOnCharacterAttackHitConfirmed_Patch
    {
        [UsedImplicitly]
        public static void Prefix(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode)
        {
            //PATCH: support for `IReactionAttackModeRestriction`
            RestrictReactionAttackMode.ReactionContext = (attacker, defender, attackMode);
        }

        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values)
        {
            //PATCH: support for `IReactionAttackModeRestriction`
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            RestrictReactionAttackMode.ReactionContext = (null, null, null);
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleDefenderBeforeDamageReceived))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleDefenderBeforeDamageReceived_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect,
            ActionModifier attackModifier,
            bool rolledSavingThrow,
            bool saveOutcomeSuccess
        )
        {
            //PATCH: support for features that trigger when defender gets hit, like `FeatureDefinitionReduceDamage` 
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var defenderCharacter = defender.RulesetCharacter;

            if (defenderCharacter is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            // Not actually used currently, but may be useful for future features.
            // var selfDamage = attacker.RulesetCharacter == defenderCharacter;

            // Not actually used currently, but may be useful for future features.
            // var canPerceiveAttacker = selfDamage
            //                           || defender.PerceivedFoes.Contains(attacker)
            //                           || defender.PerceivedAllies.Contains(attacker);

            foreach (var feature in defenderCharacter
                         .GetFeaturesByType<FeatureDefinitionReduceDamage>())
            {
                var isValid = defenderCharacter.IsValid(feature.GetAllSubFeaturesOfType<IsCharacterValidHandler>());

                if (!isValid)
                {
                    continue;
                }

                var canReact = defender.CanReact();

                //TODO: add ability to specify whether this feature can reduce magic damage
                var damageTypes = feature.DamageTypes;
                var damage = attackMode?.EffectDescription?.FindFirstDamageFormOfType(damageTypes);

                // In case of a ruleset effect, check that it shall apply damage forms, otherwise don't proceed (e.g. CounterSpell)
                if (rulesetEffect?.EffectDescription != null)
                {
                    var canForceHalfDamage = false;

                    if (rulesetEffect is RulesetEffectSpell activeSpell)
                    {
                        canForceHalfDamage = attacker.RulesetCharacter.CanForceHalfDamage(activeSpell.SpellDefinition);
                    }

                    var effectDescription = rulesetEffect.EffectDescription;

                    if (rolledSavingThrow)
                    {
                        damage = saveOutcomeSuccess
                            ? effectDescription.FindFirstNonNegatedDamageFormOfType(canForceHalfDamage, damageTypes)
                            : effectDescription.FindFirstDamageFormOfType(damageTypes);
                    }
                    else
                    {
                        damage = effectDescription.FindFirstDamageFormOfType(damageTypes);
                    }
                }

                if (damage == null)
                {
                    continue;
                }

                var totalReducedDamage = 0;

                switch (feature.TriggerCondition)
                {
                    // Can I always reduce a fixed damage amount (i.e.: Heavy Armor Feat)
                    case RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive:
                        totalReducedDamage = feature.ReducedDamage;
                        break;

                    // Can I reduce the damage consuming slots? (i.e.: Blade Dancer)
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpendSpellSlot:
                    {
                        if (!canReact)
                        {
                            continue;
                        }

                        var repertoire = defenderCharacter.SpellRepertoires
                            .Find(x => x.spellCastingClass == feature.SpellCastingClass);

                        if (repertoire == null)
                        {
                            continue;
                        }

                        if (!repertoire.AtLeastOneSpellSlotAvailable())
                        {
                            continue;
                        }

                        var actionService = ServiceRepository.GetService<IGameLocationActionService>();
                        var previousReactionCount = actionService.PendingReactionRequestGroups.Count;
                        var reactionParams = new CharacterActionParams(defender, ActionDefinitions.Id.SpendSpellSlot)
                        {
                            IntParameter = 1,
                            StringParameter = feature.NotificationTag,
                            SpellRepertoire = repertoire
                        };

                        actionService.ReactToSpendSpellSlot(reactionParams);

                        yield return __instance.WaitForReactions(defender, actionService, previousReactionCount);

                        if (!reactionParams.ReactionValidated)
                        {
                            continue;
                        }

                        totalReducedDamage = feature.ReducedDamage * reactionParams.IntParameter;
                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpecificCharacterFamily:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasConditionCreatedByMe:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasCondition:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetIsWounded:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasSenseType:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasCreatureTag:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.RangeAttackFromHigherGround:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.EvocationSpellDamage:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetDoesNotHaveCondition:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamagesTarget:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamageMatchesSourceAncestry:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.CriticalHit:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.RagingAndTargetIsSpellcaster:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.Raging:
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.NotWearingHeavyArmor:
                        break;
                    default:
                        throw new ArgumentException("feature.TriggerCondition");
                }

                var trendInfo = new RuleDefinitions.TrendInfo(totalReducedDamage,
                    RuleDefinitions.FeatureSourceType.CharacterFeature, feature.FormatTitle(), feature);

                damage.bonusDamage -= totalReducedDamage;
                damage.DamageBonusTrends.Add(trendInfo);
                defenderCharacter.DamageReduced(defenderCharacter, feature, totalReducedDamage);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.CanAttack))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class CanAttack_Patch
    {
        [UsedImplicitly]
        public static void Postfix(
            BattleDefinitions.AttackEvaluationParams attackParams,
            bool __result,
            GameLocationBattleManager __instance
        )
        {
            //PATCH: support for features removing ranged attack disadvantage
            RangedAttackInMeleeDisadvantageRemover.CheckToRemoveRangedDisadvantage(attackParams);

            //PATCH: add modifier or advantage/disadvantage for physical and spell attack
            ApplyCustomModifiers(attackParams, __result);

            //PATCH: add a rule to grant adv to attacks that the target unable to see
            PatchIlluminationBasedAdvantage(__instance, __result, attackParams);
        }

        //TODO: move this somewhere else and maybe split?
        private static void ApplyCustomModifiers(BattleDefinitions.AttackEvaluationParams attackParams, bool __result)
        {
            if (!__result)
            {
                return;
            }

            var attacker = attackParams.attacker.RulesetCharacter;
            var defender = attackParams.defender.RulesetCharacter;

            if (attacker == null || defender == null)
            {
                return;
            }

            var attackModifiers = attacker.GetSubFeaturesByType<IAttackComputeModifier>();

            foreach (var feature in attackModifiers)
            {
                feature.OnAttackComputeModifier(
                    attacker,
                    defender,
                    attackParams.attackProximity,
                    attackParams.attackMode,
                    ref attackParams.attackModifier);
            }
        }

        private static void PatchIlluminationBasedAdvantage(
            GameLocationBattleManager __instance,
            bool __result,
            BattleDefinitions.AttackEvaluationParams attackParams)
        {
            if (!__result || !Main.Settings.AttackersWithDarkvisionHaveAdvantageOverDefendersWithout)
            {
                return;
            }

            var attackerLoc = attackParams.attacker;
            var defenderLoc = attackParams.defender;
            var attackerChr = attackerLoc.RulesetCharacter;
            var defenderChr = defenderLoc.RulesetCharacter;

            if (attackerChr == null || defenderChr == null)
            {
                return;
            }

            // It seems that we don't need to find the controller of the attacker or the defender
            //RulesetCharacterEffectProxy rulesetCharacterEffectProxy;
            //if ((rulesetCharacterEffectProxy = (attackerLoc.RulesetCharacter as RulesetCharacterEffectProxy)) != null)
            //{
            //    RulesetActor rulesetActor = null;
            //    if (RulesetEntity.TryGetEntity<RulesetActor>(rulesetCharacterEffectProxy.ControllerGuid, out rulesetActor))
            //    {
            //        attackerLoc = GameLocationCharacter.GetFromActor(rulesetActor);
            //    }
            //}

            var attackerGravityCenter =
                __instance.gameLocationPositioningService.ComputeGravityCenterPosition(attackerLoc);
            var defenderGravityCenter =
                __instance.gameLocationPositioningService.ComputeGravityCenterPosition(defenderLoc);

            IIlluminable attacker = attackerLoc;
            var lightingState = attackerLoc.LightingState;
            var distance = (defenderGravityCenter - attackerGravityCenter).magnitude;
            var flag = defenderLoc.RulesetCharacter.SenseModes
                .Where(senseMode => distance <= senseMode.SenseRange)
                .Any(senseMode => SenseMode.ValidForLighting(senseMode.SenseType, lightingState));

            if (flag)
            {
                return;
            }

            attackParams.attackModifier.AttackAdvantageTrends.Add(
                new RuleDefinitions.TrendInfo(1, RuleDefinitions.FeatureSourceType.Lighting, lightingState.ToString(),
                    attacker.TargetSource, (string)null));
            attackParams.attackModifier.AbilityCheckAdvantageTrends.Add(
                new RuleDefinitions.TrendInfo(1, RuleDefinitions.FeatureSourceType.Lighting, lightingState.ToString(),
                    attacker.TargetSource, (string)null));
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleAdditionalDamageOnCharacterAttackHitConfirmed))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleAdditionalDamageOnCharacterAttackHitConfirmed_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            GameLocationBattleManager __instance,
            out IEnumerator __result,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            RuleDefinitions.AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget)
        {
            //PATCH: Completely replace this method to support several features. Modified method based on TA provided sources.
            __result = GameLocationBattleManagerTweaks.HandleAdditionalDamageOnCharacterAttackHitConfirmed(__instance,
                attacker, defender, attackModifier, attackMode, rangedAttack, advantageType, actualEffectForms,
                rulesetEffect, criticalHit, firstTarget);

            return false;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.ComputeAndNotifyAdditionalDamage))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ComputeAndNotifyAdditionalDamage_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            IAdditionalDamageProvider provider,
            List<EffectForm> actualEffectForms,
            CharacterActionParams reactionParams,
            RulesetAttackMode attackMode,
            bool criticalHit)
        {
            //PATCH: Completely replace this method to support several features. Modified method based on TA provided sources.
            GameLocationBattleManagerTweaks.ComputeAndNotifyAdditionalDamage(__instance, attacker, defender, provider,
                actualEffectForms, reactionParams, attackMode, criticalHit);

            return false;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.HandleTargetReducedToZeroHP))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static class HandleTargetReducedToZeroHP_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode rulesetAttackMode,
            RulesetEffect activeEffect)
        {
            //PATCH: INotifyConditionRemoval
            var rulesetDownedCreature = downedCreature.RulesetCharacter;

            foreach (var rulesetCondition in
                     RulesetCharacterMonsterPatcher.HandleDeathForEffectConditions_Patch.ConditionsBeforeDeath)
            {
                if (rulesetCondition.ConditionDefinition == null)
                {
                    continue;
                }

                foreach (var notifyConditionRemoval in rulesetCondition.ConditionDefinition
                             .GetAllSubFeaturesOfType<INotifyConditionRemoval>())
                {
                    notifyConditionRemoval.BeforeDyingWithCondition(rulesetDownedCreature, rulesetCondition);
                }
            }

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (__instance.battle == null)
            {
                yield break;
            }

            //PATCH: Support for `ITargetReducedToZeroHP` feature
            foreach (var extraEvents in attacker.RulesetActor.GetSubFeaturesByType<ITargetReducedToZeroHp>()
                         .Select(x =>
                             x.HandleCharacterReducedToZeroHp(attacker, downedCreature, rulesetAttackMode,
                                 activeEffect)))
            {
                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }

            if (__instance.battle == null)
            {
                yield break;
            }

            //PATCH: Support for `ISourceReducedToZeroHP` feature
            foreach (var extraEvents in downedCreature.RulesetActor.GetSubFeaturesByType<ISourceReducedToZeroHp>()
                         .Select(x =>
                             x.HandleSourceReducedToZeroHp(attacker, downedCreature, rulesetAttackMode,
                                 activeEffect)))
            {
                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleCharacterMagicalAttackHitConfirmed))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterMagicalAttackHitConfirmed_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier magicModifier,
            RulesetEffect rulesetEffect,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                //call all before handlers
                foreach (var feature in attacker.RulesetActor.GetSubFeaturesByType<IMagicalAttackInitiated>())
                {
                    yield return feature.OnMagicalAttackInitiated(attacker, defender, magicModifier, rulesetEffect,
                        actualEffectForms, firstTarget, criticalHit);
                }
            }

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            // ReSharper disable once InvertIf
            if (Gui.Battle != null &&
                attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } &&
                defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            {
                //call all after handlers
                foreach (var feature in attacker.RulesetActor.GetSubFeaturesByType<IMagicalAttackFinished>())
                {
                    yield return feature.OnMagicalAttackFinished(attacker, defender, magicModifier, rulesetEffect,
                        actualEffectForms, firstTarget, criticalHit);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), nameof(GameLocationBattleManager.HandleFailedSavingThrow))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleFailedSavingThrow_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier saveModifier,
            bool hasHitVisual,
            bool hasBorrowedLuck
        )
        {
            //PATCH: allow source character of a condition to use power to augment failed save roll
            //used mainly for Inventor's `Quick Wit`
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var saveOutcome = action.SaveOutcome;

            if (!IsFailed(saveOutcome))
            {
                yield break;
            }

            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var rulesService = ServiceRepository.GetService<IRulesetImplementationService>();

            var allConditions = new List<RulesetCondition>();

            rulesetDefender.GetAllConditions(allConditions);

            foreach (var condition in allConditions)
            {
                var feature = condition.ConditionDefinition
                    .GetFirstSubFeatureOfType<ConditionSourceCanUsePowerToImproveFailedSaveRoll>();

                if (feature == null)
                {
                    continue;
                }

                if (!RulesetEntity.TryGetEntity<RulesetCharacter>(condition.SourceGuid, out var helper))
                {
                    continue;
                }

                var locHelper = GameLocationCharacter.GetFromActor(helper);

                if (locHelper == null)
                {
                    continue;
                }

                if (!feature.ShouldTrigger(action, attacker, defender, locHelper, saveModifier, hasHitVisual,
                        hasBorrowedLuck, saveOutcome, action.saveOutcomeDelta))
                {
                    continue;
                }

                var power = feature.Power;

                if (!helper.CanUsePower(power))
                {
                    continue;
                }

                var usablePower = UsablePowersProvider.Get(power, helper);

                var reactionParams = new CharacterActionParams(locHelper, ActionDefinitions.Id.SpendPower)
                {
                    StringParameter = feature.ReactionName,
                    StringParameter2 = feature.FormatReactionDescription(action, attacker, defender, locHelper,
                        saveModifier, hasHitVisual, hasBorrowedLuck, saveOutcome, action.saveOutcomeDelta),
                    RulesetEffect = rulesService.InstantiateEffectPower(helper, usablePower, false)
                };

                var count = actionService.PendingReactionRequestGroups.Count;

                actionService.ReactToSpendPower(reactionParams);

                yield return __instance.WaitForReactions(locHelper, actionService, count);

                if (reactionParams.ReactionValidated)
                {
                    GameConsoleHelper.LogCharacterUsedPower(helper, power, indent: true);
                    rulesetDefender.UsePower(usablePower);

                    action.RolledSaveThrow = feature.TryModifyRoll(action, attacker, defender, locHelper, saveModifier,
                        reactionParams, hasHitVisual, hasBorrowedLuck, ref saveOutcome, ref action.saveOutcomeDelta);
                    action.SaveOutcome = saveOutcome;
                }

                reactionParams.RulesetEffect.Terminate(true);

                if (!IsFailed(saveOutcome))
                {
                    yield break;
                }
            }

            if (__instance.Battle == null)
            {
                yield break;
            }

            //PATCH: Allow attack of opportunity on target that failed saving throw
            //Process other participants of the battle
            foreach (var unit in __instance.Battle.AllContenders
                         .Where(x => x is { RulesetCharacter.IsDeadOrDyingOrUnconscious: false })
                         .ToList())
            {
                if (unit == defender || unit == attacker)
                {
                    continue;
                }

                foreach (var feature in unit.RulesetCharacter.GetSubFeaturesByType<IOnDefenderFailedSavingThrow>())
                {
                    yield return feature.OnDefenderFailedSavingThrow(
                        __instance, action, unit, defender, saveModifier, hasHitVisual, hasBorrowedLuck);
                }
            }
        }

        private static bool IsFailed(RuleDefinitions.RollOutcome outcome)
        {
            return outcome is RuleDefinitions.RollOutcome.Failure or RuleDefinitions.RollOutcome.CriticalFailure;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleCharacterPhysicalAttackInitiated))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterPhysicalAttackInitiated_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackerAttackMode)
        {
            //PATCH: registers which weapon types were used so far on attacks

            ValidatorsCharacter.RegisterWeaponTypeUsed(attacker, attackerAttackMode);

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            //PATCH: allow custom behavior when physical attack initiates

            if (__instance.battle == null)
            {
                yield break;
            }

            foreach (var attackInitiated in attacker.RulesetCharacter.GetSubFeaturesByType<IPhysicalAttackInitiated>())
            {
                yield return attackInitiated.OnAttackInitiated(
                    __instance, action, attacker, defender, attackModifier, attackerAttackMode);
            }

            //PATCH: allow custom behavior when physical attack initiates on me

            if (__instance.battle == null)
            {
                yield break;
            }

            foreach (var attackInitiated in
                     defender.RulesetCharacter.GetSubFeaturesByType<IPhysicalAttackInitiatedOnMe>())
            {
                yield return attackInitiated.OnAttackInitiatedOnMe(
                    __instance, action, attacker, defender, attackModifier, attackerAttackMode);
            }

            //PATCH: allow custom behavior when physical attack initiates on me or ally

            if (__instance.battle == null)
            {
                yield break;
            }

            foreach (var attackInitiated in __instance.battle.GetOpposingContenders(attacker.Side)
                         .SelectMany(x =>
                             x.RulesetCharacter.GetSubFeaturesByType<IPhysicalAttackInitiatedOnMeOrAlly>()))
            {
                yield return attackInitiated.OnAttackInitiatedOnMeOrAlly(
                    __instance, action, attacker, defender, attackModifier, attackerAttackMode);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleCharacterPhysicalAttackFinished))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class HandleCharacterPhysicalAttackFinished_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction attackAction,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackerAttackMode,
            RuleDefinitions.RollOutcome attackRollOutcome,
            int damageAmount)
        {
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (attacker.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } && __instance.Battle != null)
            {
                //PATCH: allow custom behavior when physical attack finished
                foreach (var feature in attacker.RulesetCharacter.GetSubFeaturesByType<IPhysicalAttackFinished>())
                {
                    yield return feature.OnAttackFinished(
                        __instance, attackAction, attacker, defender, attackerAttackMode, attackRollOutcome,
                        damageAmount);
                }
            }

            // ReSharper disable once InvertIf
            if (defender.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false } && __instance.Battle != null)
            {
                //PATCH: allow custom behavior when physical attack finished on defender
                foreach (var feature in defender.RulesetCharacter.GetSubFeaturesByType<IPhysicalAttackFinishedOnMe>())
                {
                    yield return feature.OnAttackFinishedOnMe(
                        __instance, attackAction, attacker, defender, attackerAttackMode, attackRollOutcome,
                        damageAmount);
                }
            }
        }
    }
}
