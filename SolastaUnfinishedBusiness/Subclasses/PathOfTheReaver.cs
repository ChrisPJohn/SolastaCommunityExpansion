﻿using System.Collections;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Properties;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

// ReSharper disable once IdentifierTypo
internal sealed class PathOfTheReaver : AbstractSubclass
{
    private const string Name = "PathOfTheReaver";

    internal PathOfTheReaver()
    {
        // LEVEL 03

        var featureVoraciousFury = FeatureDefinitionBuilder
            .Create($"Feature{Name}VoraciousFury")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureVoraciousFury.SetCustomSubFeatures(new PhysicalAttackFinishedVoraciousFury(featureVoraciousFury));

        // LEVEL 06

        var featureSetProfaneVitality = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}ProfaneVitality")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonResistance,
                FeatureDefinitionAttributeModifierBuilder
                    .Create($"AttributeModifier{Name}ProfaneVitality")
                    .SetGuiPresentationNoContent(true)
                    .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.HitPointBonusPerLevel, 1)
                    .AddToDB())
            .AddToDB();

        // LEVEL 10

        var powerBloodbath = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Bloodbath")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction, RechargeRate.ShortRest)
            .SetReactionContext(ExtraReactionContext.Custom)
            .AddToDB();

        powerBloodbath.SetCustomSubFeatures(new TargetReducedToZeroHpBloodbath(powerBloodbath));

        // LEVEL 14

        var featureCorruptedBlood = FeatureDefinitionBuilder
            .Create($"Feature{Name}CorruptedBlood")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureCorruptedBlood.SetCustomSubFeatures(new PhysicalAttackFinishedOnMeCorruptedBlood(featureCorruptedBlood));

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PathOfTheReaver, 256))
            .AddFeaturesAtLevel(3, featureVoraciousFury)
            .AddFeaturesAtLevel(6, featureSetProfaneVitality)
            .AddFeaturesAtLevel(10, powerBloodbath)
            .AddFeaturesAtLevel(14, featureCorruptedBlood)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private static void InflictDamage(
        RulesetEntity rulesetAttacker,
        RulesetActor rulesetDefender,
        int totalDamage,
        List<string> attackTags)
    {
        var damageForm = new DamageForm
        {
            DamageType = DamageTypeNecrotic, DieType = DieType.D1, DiceNumber = 0, BonusDamage = totalDamage
        };

        RulesetActor.InflictDamage(
            totalDamage,
            damageForm,
            DamageTypeNecrotic,
            new RulesetImplementationDefinitions.ApplyFormsParams { targetCharacter = rulesetDefender },
            rulesetDefender,
            false,
            rulesetAttacker.Guid,
            false,
            attackTags,
            new RollInfo(DieType.D1, new List<int>(), totalDamage),
            false,
            out _);
    }

    private static void ReceiveHealing(GameLocationCharacter gameLocationCharacter, int totalHealing)
    {
        EffectHelpers.StartVisualEffect(
            gameLocationCharacter, gameLocationCharacter, Heal, EffectHelpers.EffectType.Effect);
        gameLocationCharacter.RulesetCharacter.ReceiveHealing(totalHealing, true, gameLocationCharacter.Guid);
    }

    //
    // Voracious Fury
    //

    private sealed class PhysicalAttackFinishedVoraciousFury : IPhysicalAttackFinished
    {
        private readonly FeatureDefinition _featureVoraciousFury;

        public PhysicalAttackFinishedVoraciousFury(FeatureDefinition featureVoraciousFury)
        {
            _featureVoraciousFury = featureVoraciousFury;
        }

        public IEnumerator OnAttackFinished(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome outcome,
            int damageAmount)
        {
            if (outcome != RollOutcome.Success && outcome != RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker == null)
            {
                yield break;
            }

            if (!IsVoraciousFuryValidContext(rulesetAttacker, attackMode))
            {
                yield break;
            }

            if (attacker.UsedSpecialFeatures.ContainsKey(_featureVoraciousFury.Name) ||
                Gui.Battle == null || Gui.Battle.ActiveContender != attacker)
            {
                yield break;
            }

            GameConsoleHelper.LogCharacterUsedFeature(rulesetAttacker, _featureVoraciousFury);
            attacker.UsedSpecialFeatures.TryAdd(_featureVoraciousFury.Name, 1);

            var multiplier = 1;

            if (outcome is RollOutcome.CriticalSuccess)
            {
                multiplier += 1;
            }

            if (rulesetAttacker.MissingHitPoints > rulesetAttacker.CurrentHitPoints)
            {
                multiplier += 1;
            }

            var rulesetDefender = defender.RulesetCharacter;
            var proficiencyBonus = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            var totalDamageOrHealing = proficiencyBonus * multiplier;

            ReceiveHealing(attacker, totalDamageOrHealing);

            if (rulesetDefender == null || rulesetDefender.IsDeadOrDying)
            {
                yield break;
            }

            EffectHelpers.StartVisualEffect(attacker, defender, VampiricTouch, EffectHelpers.EffectType.Effect);
            InflictDamage(rulesetAttacker, rulesetDefender, totalDamageOrHealing, attackMode.AttackTags);
        }

        private static bool IsVoraciousFuryValidContext(RulesetCharacter rulesetCharacter, RulesetAttackMode attackMode)
        {
            var isValid =
                (ValidatorsWeapon.IsMelee(attackMode) || ValidatorsWeapon.IsUnarmed(rulesetCharacter, attackMode)) &&
                ValidatorsCharacter.DoesNotHaveHeavyArmor(rulesetCharacter) &&
                ValidatorsCharacter.HasAnyOfConditions(ConditionRaging)(rulesetCharacter);

            return isValid;
        }
    }

    //
    // Bloodbath
    //

    private class TargetReducedToZeroHpBloodbath : ITargetReducedToZeroHp
    {
        private readonly FeatureDefinitionPower _featureDefinitionPower;

        public TargetReducedToZeroHpBloodbath(FeatureDefinitionPower featureDefinitionPower)
        {
            _featureDefinitionPower = featureDefinitionPower;
        }

        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (gameLocationActionService == null || gameLocationBattleService == null)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.MissingHitPoints == 0 || !rulesetAttacker.HasConditionOfType(ConditionRaging))
            {
                yield break;
            }

            if (!ValidatorsWeapon.IsMelee(attackMode) && !ValidatorsWeapon.IsUnarmed(rulesetAttacker, attackMode))
            {
                yield break;
            }

            if (rulesetAttacker.GetRemainingPowerCharges(_featureDefinitionPower) <= 0)
            {
                yield break;
            }

            var classLevel = rulesetAttacker.GetClassLevel(CharacterClassDefinitions.Barbarian);
            var totalHealing = 2 * classLevel;
            var reactionParams =
                new CharacterActionParams(attacker, (ActionDefinitions.Id)ExtraActionId.DoNothingFree)
                {
                    StringParameter =
                        Gui.Format("Reaction/&CustomReactionBloodbathDescription", totalHealing.ToString())
                };
            var previousReactionCount = gameLocationActionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("Bloodbath", reactionParams);

            gameLocationActionService.AddInterruptRequest(reactionRequest);

            yield return gameLocationBattleService.WaitForReactions(
                attacker, gameLocationActionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            GameConsoleHelper.LogCharacterUsedPower(rulesetAttacker, _featureDefinitionPower);
            rulesetAttacker.UpdateUsageForPower(_featureDefinitionPower, _featureDefinitionPower.CostPerUse);

            ReceiveHealing(attacker, totalHealing);
        }
    }

    //
    // Corrupted Blood
    //

    private class PhysicalAttackFinishedOnMeCorruptedBlood : IPhysicalAttackFinishedOnMe
    {
        private readonly FeatureDefinition _featureCorruptedBlood;

        public PhysicalAttackFinishedOnMeCorruptedBlood(FeatureDefinition featureCorruptedBlood)
        {
            _featureCorruptedBlood = featureCorruptedBlood;
        }

        public IEnumerator OnAttackFinishedOnMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome outcome,
            int damageAmount)
        {
            if (outcome != RollOutcome.Success && outcome != RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetAttacker == null || rulesetDefender == null || rulesetDefender.IsDeadOrDying)
            {
                yield break;
            }

            var constitution = rulesetDefender.TryGetAttributeValue(AttributeDefinitions.Constitution);
            var totalDamage = AttributeDefinitions.ComputeAbilityScoreModifier(constitution);
            var defenderAttackMode = defender.FindActionAttackMode(ActionDefinitions.Id.AttackMain);

            GameConsoleHelper.LogCharacterUsedFeature(rulesetDefender, _featureCorruptedBlood);
            EffectHelpers.StartVisualEffect(attacker, defender, PowerDomainMischiefStrikeOfChaos);
            InflictDamage(rulesetDefender, rulesetAttacker, totalDamage, defenderAttackMode.AttackTags);
        }
    }
}
