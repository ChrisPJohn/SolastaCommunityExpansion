﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class PathOfTheElements : AbstractSubclass
{
    private const string Name = "PathOfTheElements";
    private const string ElementalBlessing = "ElementalBlessing";
    private const string ElementalBurst = "ElementalBurst";
    private const string ElementalConduit = "ElementalConduit";

    internal PathOfTheElements()
    {
        #region LEVEL 03

        // Elemental Fury

        var ancestryStorm = FeatureDefinitionAncestryBuilder
            .Create($"Ancestry{Name}Storm")
            .SetGuiPresentation(Category.Feature,
                Gui.Format($"Feature/&Ancestry{Name}AllDescription", Gui.Localize("Rules/&DamageLightningTitle")))
            .SetAncestry(ExtraAncestryType.PathOfTheElements)
            .SetDamageType(DamageTypeLightning)
            .AddToDB();

        ancestryStorm.SetCustomSubFeatures(
            new CharacterTurnEndedElementalFury(ancestryStorm, SpellDefinitions.LightningBolt));

        var ancestryBlizzard = FeatureDefinitionAncestryBuilder
            .Create($"Ancestry{Name}Blizzard")
            .SetGuiPresentation(Category.Feature,
                Gui.Format($"Feature/&Ancestry{Name}AllDescription", Gui.Localize("Rules/&DamageColdTitle")))
            .SetAncestry(ExtraAncestryType.PathOfTheElements)
            .SetDamageType(DamageTypeCold)
            .AddToDB();

        ancestryBlizzard.SetCustomSubFeatures(
            new CharacterTurnEndedElementalFury(ancestryBlizzard, SpellDefinitions.RayOfFrost));

        var ancestryWildfire = FeatureDefinitionAncestryBuilder
            .Create($"Ancestry{Name}Wildfire")
            .SetGuiPresentation(Category.Feature,
                Gui.Format($"Feature/&Ancestry{Name}AllDescription", Gui.Localize("Rules/&DamageFireTitle")))
            .SetAncestry(ExtraAncestryType.PathOfTheElements)
            .SetDamageType(DamageTypeFire)
            .AddToDB();

        ancestryWildfire.SetCustomSubFeatures(
            new CharacterTurnEndedElementalFury(ancestryWildfire, SpellDefinitions.FireBolt));

        var featureSetElementalFury = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}ElementalFury")
            .SetGuiPresentation(Category.Feature)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion)
            .AddFeatureSet(
                ancestryStorm,
                ancestryBlizzard,
                ancestryWildfire)
            .SetAncestryType(ExtraAncestryType.PathOfTheElements)
            .AddToDB();

        #endregion

        #region LEVEL 06

        // Storm

        var conditionElementalBlessingStorm = ConditionDefinitionBuilder
            .Create($"Condition{Name}{ElementalBlessing}Storm")
            .SetGuiPresentation($"Condition{Name}{ElementalBlessing}", Category.Condition,
                ConditionDefinitions.ConditionBlessed)
            .SetPossessive()
            .SetSpecialDuration(DurationType.Round, 1)
            .SetSpecialInterruptions(ConditionInterruption.RageStop)
            .SetFeatures(FeatureDefinitionDamageAffinitys.DamageAffinityLightningResistance)
            .AddToDB();

        var powerElementalBlessingStorm = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBlessing}Storm")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 2)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionElementalBlessingStorm, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        var customBehaviorStorm = new CustomRagingAura(powerElementalBlessingStorm, conditionElementalBlessingStorm);

        conditionElementalBlessingStorm.SetCustomSubFeatures(customBehaviorStorm);
        powerElementalBlessingStorm.SetCustomSubFeatures(customBehaviorStorm);

        // Blizzard

        var conditionElementalBlessingBlizzard = ConditionDefinitionBuilder
            .Create($"Condition{Name}{ElementalBlessing}Blizzard")
            .SetGuiPresentation($"Condition{Name}{ElementalBlessing}", Category.Condition,
                ConditionDefinitions.ConditionBlessed)
            .SetPossessive()
            .SetSpecialDuration(DurationType.Round, 1)
            .SetSpecialInterruptions(ConditionInterruption.RageStop)
            .SetFeatures(FeatureDefinitionDamageAffinitys.DamageAffinityColdResistance)
            .AddToDB();

        var powerElementalBlessingBlizzard = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBlessing}Blizzard")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 2)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionElementalBlessingBlizzard, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        var customBehaviorBlizzard =
            new CustomRagingAura(powerElementalBlessingBlizzard, conditionElementalBlessingBlizzard);

        conditionElementalBlessingBlizzard.SetCustomSubFeatures(customBehaviorBlizzard);
        powerElementalBlessingBlizzard.SetCustomSubFeatures(customBehaviorBlizzard);

        // Wildfire

        var conditionElementalBlessingWildfire = ConditionDefinitionBuilder
            .Create($"Condition{Name}{ElementalBlessing}Wildfire")
            .SetGuiPresentation($"Condition{Name}{ElementalBlessing}", Category.Condition,
                ConditionDefinitions.ConditionBlessed)
            .SetPossessive()
            .SetSpecialDuration(DurationType.Round, 1)
            .SetSpecialInterruptions(ConditionInterruption.RageStop)
            .SetFeatures(FeatureDefinitionDamageAffinitys.DamageAffinityFireResistance)
            .AddToDB();

        var powerElementalBlessingWildfire = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBlessing}Wildfire")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 2)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionElementalBlessingWildfire, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        var customBehaviorWildfire =
            new CustomRagingAura(powerElementalBlessingWildfire, conditionElementalBlessingWildfire);

        conditionElementalBlessingWildfire.SetCustomSubFeatures(customBehaviorWildfire);
        powerElementalBlessingWildfire.SetCustomSubFeatures(customBehaviorWildfire);

        // Elemental Blessing

        var featureSetElementalBlessing = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{ElementalBlessing}")
            .SetGuiPresentation(Category.Feature)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.DeterminedByAncestry)
            .SetAncestryType(ExtraAncestryType.PathOfTheElements, DamageTypeCold, DamageTypeLightning, DamageTypeFire)
            .AddFeatureSet(
                powerElementalBlessingBlizzard,
                powerElementalBlessingStorm,
                powerElementalBlessingWildfire)
            .AddToDB();

        #endregion

        #region LEVEL 10

        // Storm

        var conditionElementalBurstStorm = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionShocked, $"Condition{Name}{ElementalBurst}Storm")
            .SetSpecialDuration(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
            .AddToDB();

        var powerElementalBurstStorm = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBurst}Storm")
            .SetGuiPresentation(Category.Feature, PowerDomainElementalLightningBlade)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.Cube, 3)
                    .SetDurationData(DurationType.Instantaneous)
                    .SetParticleEffectParameters(PowerDomainElementalLightningBlade)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Dexterity,
                        true,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        AttributeDefinitions.Constitution)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeLightning, 3, DieType.D10)
                            .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 1, 1, 5, 15)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionElementalBurstStorm, ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(
                new ValidatorsPowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionRaging)))
            .AddToDB();

        // Blizzard

        var powerElementalBurstBlizzard = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBurst}Blizzard")
            .SetGuiPresentation(Category.Feature, PowerDomainElementalIceLance)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.Cube, 3)
                    .SetDurationData(DurationType.Instantaneous)
                    .SetParticleEffectParameters(PowerDomainElementalIceLance)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Strength,
                        true,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        AttributeDefinitions.Constitution)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeCold, 3, DieType.D8)
                            .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 1, 1, 5, 15)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.FallProne)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(
                new ValidatorsPowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionRaging)))
            .AddToDB();

        // Wildfire

        var conditionElementalBurstWildfire = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionOnFire, $"Condition{Name}{ElementalBurst}Wildfire")
            .SetSpecialDuration(DurationType.Minute, 1)
            .AddToDB();

        var powerElementalBurstWildfire = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalBurst}Wildfire")
            .SetGuiPresentation(Category.Feature, PowerDomainElementalFireBurst)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.Cube, 3)
                    .SetDurationData(DurationType.Round, 1)
                    .SetParticleEffectParameters(PowerDomainElementalFireBurst)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Dexterity,
                        true,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        AttributeDefinitions.Constitution)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeFire, 4, DieType.D6)
                            .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 1, 1, 5, 15)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionElementalBurstWildfire, ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(
                new ValidatorsPowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionRaging)))
            .AddToDB();

        // Elemental Burst

        var featureSetElementalBurst = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{ElementalBurst}")
            .SetGuiPresentation(Category.Feature,
                Gui.Format($"Feature/&FeatureSet{Name}{ElementalBurst}Description",
                    Gui.Localize($"Feature/&Power{Name}{ElementalBurst}StormDescription"),
                    Gui.Localize($"Feature/&Power{Name}{ElementalBurst}BlizzardDescription"),
                    Gui.Localize($"Feature/&Power{Name}{ElementalBurst}WildfireDescription")))
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.DeterminedByAncestry)
            .SetAncestryType(ExtraAncestryType.PathOfTheElements, DamageTypeCold, DamageTypeLightning, DamageTypeFire)
            .AddFeatureSet(
                powerElementalBurstBlizzard,
                powerElementalBurstStorm,
                powerElementalBurstWildfire)
            .AddToDB();

        #endregion

        #region LEVEL 14

        // Storm

        var conditionElementalConduitStorm = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionFlyingAdaptive, $"Condition{Name}{ElementalConduit}Storm")
            .SetSpecialInterruptions(ConditionInterruption.RageStop)
            .AddToDB();

        var powerElementalConduitStorm = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{ElementalConduit}Storm")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Permanent)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionElementalConduitStorm, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        // Blizzard

        var featureElementalConduitBlizzard = FeatureDefinitionBuilder
            .Create($"Feature{Name}{ElementalConduit}Blizzard")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureElementalConduitBlizzard.SetCustomSubFeatures(
            new CharacterTurnEndedElementalConduitBlizzard(featureElementalConduitBlizzard));

        // Wildfire

        var featureElementalConduitWildfire = FeatureDefinitionBuilder
            .Create($"Feature{Name}{ElementalConduit}Wildfire")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureElementalConduitWildfire.SetCustomSubFeatures(
            new ReactToAttackOnMeFinishedElementalConduitWildfire(featureElementalConduitWildfire));

        // Elemental Conduit

        var featureSetElementalConduit = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{ElementalConduit}")
            .SetGuiPresentation(Category.Feature,
                Gui.Format($"Feature/&FeatureSet{Name}{ElementalBurst}Description",
                    Gui.Localize($"Feature/&Power{Name}{ElementalConduit}StormDescription"),
                    Gui.Localize($"Feature/&Feature{Name}{ElementalConduit}BlizzardDescription"),
                    Gui.Localize($"Feature/&Feature{Name}{ElementalConduit}WildfireDescription")))
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.DeterminedByAncestry)
            .SetAncestryType(ExtraAncestryType.PathOfTheElements, DamageTypeCold, DamageTypeLightning, DamageTypeFire)
            .AddFeatureSet(
                featureElementalConduitBlizzard,
                powerElementalConduitStorm,
                featureElementalConduitWildfire)
            .AddToDB();

        #endregion

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PathOfTheElements, 256))
            .AddFeaturesAtLevel(3, featureSetElementalFury)
            .AddFeaturesAtLevel(6, featureSetElementalBlessing)
            .AddFeaturesAtLevel(10, featureSetElementalBurst)
            .AddFeaturesAtLevel(14, featureSetElementalConduit)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Elemental Fury
    //

    private sealed class CharacterTurnEndedElementalFury : ICharacterTurnEndListener
    {
        private readonly FeatureDefinitionAncestry _ancestry;
        private readonly IMagicEffect _magicEffect;

        public CharacterTurnEndedElementalFury(
            FeatureDefinitionAncestry ancestry,
            IMagicEffect magicEffect)
        {
            _ancestry = ancestry;
            _magicEffect = magicEffect;
        }

        public void OnCharacterTurnEnded(GameLocationCharacter locationCharacter)
        {
            var battle = Gui.Battle;

            if (battle == null)
            {
                return;
            }

            var rulesetAttacker = locationCharacter.RulesetCharacter;

            if (!rulesetAttacker.HasAnyConditionOfType(ConditionRaging))
            {
                return;
            }

            var gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();

            foreach (var targetLocationCharacter in battle.AllContenders
                         .Where(x =>
                             x.Side != locationCharacter.Side &&
                             gameLocationBattleService.IsWithin1Cell(locationCharacter, x)))
            {
                var rulesetDefender = targetLocationCharacter.RulesetCharacter;
                var classLevel = rulesetAttacker.GetClassLevel(CharacterClassDefinitions.Barbarian);
                int diceNumber;
                DieType dieType;

                switch (classLevel)
                {
                    case < 6:
                        diceNumber = 1;
                        dieType = DieType.D6;
                        break;
                    case < 10:
                        diceNumber = 1;
                        dieType = DieType.D10;
                        break;
                    case < 14:
                        diceNumber = 2;
                        dieType = DieType.D6;
                        break;
                    default:
                        diceNumber = 2;
                        dieType = DieType.D10;
                        break;
                }

                var implementationService = ServiceRepository.GetService<IRulesetImplementationService>();

                var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
                {
                    sourceCharacter = rulesetAttacker,
                    targetCharacter = rulesetDefender,
                    position = targetLocationCharacter.LocationPosition
                };

                var damageForm = new DamageForm
                {
                    DamageType = _ancestry.damageType,
                    DieType = dieType,
                    DiceNumber = diceNumber,
                    BonusDamage = 0,
                    IgnoreCriticalDoubleDice = true
                };

                EffectHelpers.StartVisualEffect(locationCharacter, targetLocationCharacter, _magicEffect);
                implementationService.ApplyEffectForms(
                    new List<EffectForm> { new() { damageForm = damageForm } },
                    applyFormsParams,
                    new List<string> { _ancestry.damageType },
                    out _,
                    out _);

                GameConsoleHelper.LogCharacterUsedFeature(rulesetAttacker, _ancestry);
            }
        }
    }

    //
    // Elemental Conduit - Blizzard
    //

    private sealed class CharacterTurnEndedElementalConduitBlizzard : ICharacterTurnEndListener
    {
        private readonly FeatureDefinition _featureDefinition;

        public CharacterTurnEndedElementalConduitBlizzard(FeatureDefinition featureDefinition)
        {
            _featureDefinition = featureDefinition;
        }

        public void OnCharacterTurnEnded(GameLocationCharacter locationCharacter)
        {
            var battle = Gui.Battle;

            if (battle == null)
            {
                return;
            }

            var rulesetAttacker = locationCharacter.RulesetCharacter;

            if (!rulesetAttacker.HasAnyConditionOfType(ConditionRaging))
            {
                return;
            }

            var gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();

            foreach (var targetLocationCharacter in battle.AllContenders
                         .Where(x =>
                             x.Side != locationCharacter.Side &&
                             gameLocationBattleService.IsWithin1Cell(locationCharacter, x)))
            {
                var rulesetDefender = targetLocationCharacter.RulesetCharacter;
                var modifierTrend = rulesetDefender.actionModifier.savingThrowModifierTrends;
                var advantageTrends = rulesetDefender.actionModifier.savingThrowAdvantageTrends;
                var attackerConModifier =
                    AttributeDefinitions.ComputeAbilityScoreModifier(
                        rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.Constitution));
                var profBonus =
                    AttributeDefinitions.ComputeProficiencyBonus(
                        rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.CharacterLevel));
                var defenderStrModifier =
                    AttributeDefinitions.ComputeAbilityScoreModifier(
                        rulesetDefender.TryGetAttributeValue(AttributeDefinitions.Strength));

                rulesetDefender.RollSavingThrow(0, AttributeDefinitions.Strength, _featureDefinition, modifierTrend,
                    advantageTrends, defenderStrModifier, 8 + profBonus + attackerConModifier, false,
                    out var savingOutcome,
                    out _);

                if (savingOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
                {
                    continue;
                }

                var newCondition = RulesetCondition.CreateActiveCondition(
                    rulesetDefender.Guid,
                    CustomConditionsContext.StopMovement,
                    DurationType.Round,
                    0,
                    TurnOccurenceType.EndOfTurn,
                    rulesetAttacker.Guid,
                    rulesetAttacker.BaseFaction.Name);

                rulesetDefender.AddConditionOfCategory(AttributeDefinitions.TagCombat, newCondition);
            }
        }
    }

    //
    // Elemental Conduit - Wildfire
    //

    private sealed class ReactToAttackOnMeFinishedElementalConduitWildfire : IReactToAttackOnMeFinished
    {
        private readonly FeatureDefinition _featureDefinition;

        public ReactToAttackOnMeFinishedElementalConduitWildfire(FeatureDefinition featureDefinition)
        {
            _featureDefinition = featureDefinition;
        }

        public IEnumerator HandleReactToAttackOnMeFinished(
            GameLocationCharacter attacker,
            GameLocationCharacter me,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode mode,
            ActionModifier modifier)
        {
            if (!me.CanReact())
            {
                yield break;
            }

            if (outcome is RollOutcome.Failure or RollOutcome.CriticalFailure)
            {
                yield break;
            }

            var rulesetAttacker = me.RulesetCharacter;

            if (!rulesetAttacker.HasAnyConditionOfType(ConditionRaging))
            {
                yield break;
            }

            var battleManager =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
            var actionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (actionService == null || battleManager == null || !battleManager.IsBattleInProgress)
            {
                yield break;
            }

            var reactionParams =
                new CharacterActionParams(me, (ActionDefinitions.Id)ExtraActionId.DoNothingReaction)
                {
                    StringParameter = "Reaction/&CustomReactionElementalConduitWildfireDescription"
                };
            var previousReactionCount = actionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("ElementalConduitWildfire", reactionParams);

            actionService.AddInterruptRequest(reactionRequest);

            yield return battleManager.WaitForReactions(me, actionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            var rulesetDefender = attacker.RulesetCharacter;
            var modifierTrend = rulesetDefender.actionModifier.savingThrowModifierTrends;
            var advantageTrends = rulesetDefender.actionModifier.savingThrowAdvantageTrends;
            var attackerConModifier =
                AttributeDefinitions.ComputeAbilityScoreModifier(
                    rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.Constitution));
            var profBonus =
                AttributeDefinitions.ComputeProficiencyBonus(
                    rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.CharacterLevel));
            var defenderDexModifier =
                AttributeDefinitions.ComputeAbilityScoreModifier(
                    rulesetDefender.TryGetAttributeValue(AttributeDefinitions.Dexterity));

            rulesetDefender.RollSavingThrow(0, AttributeDefinitions.Strength, _featureDefinition, modifierTrend,
                advantageTrends, defenderDexModifier, 8 + profBonus + attackerConModifier, false,
                out var savingOutcome,
                out _);

            if (savingOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var classLevel = rulesetAttacker.GetClassLevel(CharacterClassDefinitions.Barbarian);
            var totalDamage = (classLevel + 1) / 2;
            var damageForm = new DamageForm
            {
                DamageType = DamageTypeFire, DieType = DieType.D1, DiceNumber = 0, BonusDamage = totalDamage
            };

            EffectHelpers.StartVisualEffect(me, attacker, SpellDefinitions.HellishRebuke);
            RulesetActor.InflictDamage(
                totalDamage,
                damageForm,
                DamageTypeFire,
                new RulesetImplementationDefinitions.ApplyFormsParams { targetCharacter = rulesetDefender },
                rulesetDefender,
                false,
                rulesetAttacker.Guid,
                false,
                new List<string>(),
                new RollInfo(DieType.D1, new List<int>(), totalDamage),
                true,
                out _);
        }
    }
}
