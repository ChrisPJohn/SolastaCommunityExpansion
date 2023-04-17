﻿using System;
using System.Collections;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomBuilders;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class MartialTactician : AbstractSubclass
{
    internal const string Name = "MartialTactician";
    internal const string MarkDamagedByGambit = "ConditionTacticianDamagedByGambit";
    internal const string TacticalAwareness = "TacticalAwareness";

    private static int _gambitPoolIncreases;

    internal MartialTactician()
    {
        // BACKWARD COMPATIBILITY
        BuildTacticalSurge();

        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolGambitLearn1")
            .SetGuiPresentation(Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.Gambit)
            .AddToDB();
        // END BACKWARD

        var unlearn = BuildUnlearn();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass,
                Sprites.GetSprite(Name, Resources.MartialTactician, 256))
            .AddFeaturesAtLevel(3, BuildEverVigilant(), BuildSharpMind(), GambitsBuilders.GambitPool,
                GambitsBuilders.Learn4Gambit)
            .AddFeaturesAtLevel(7, BuildSharedVigilance(), BuildGambitPoolIncrease(), BuildGambitDieSize(DieType.D8),
                GambitsBuilders.Learn2Gambit, unlearn)
            .AddFeaturesAtLevel(10, BuildStrategicPlan(), BuildGambitDieSize(DieType.D10),
                unlearn)
            .AddFeaturesAtLevel(15, BuildBattleClarity(), BuildGambitPoolIncrease(),
                GambitsBuilders.Learn2Gambit, unlearn)
            .AddFeaturesAtLevel(18, BuildTacticalAwareness(), BuildGambitDieSize(DieType.D12),
                unlearn)
            .AddToDB();

        GambitsBuilders.BuildGambits();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;

    internal override DeityDefinition DeityDefinition => null;

    private static FeatureDefinition BuildSharpMind()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetTacticianSharpMind")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionPointPoolBuilder
                    .Create("PointPoolTacticianSharpMindSkill")
                    .SetGuiPresentationNoContent()
                    .SetPool(HeroDefinitions.PointsPoolType.Skill, 1)
                    .AddToDB(),
                FeatureDefinitionPointPoolBuilder
                    .Create("PointPoolTacticianSharpMindExpertise")
                    .SetGuiPresentationNoContent()
                    .SetPool(HeroDefinitions.PointsPoolType.Expertise, 1)
                    .AddToDB())
            .AddToDB();
    }

    private static FeatureDefinition BuildEverVigilant()
    {
        return FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierTacticianEverVigilant")
            .SetGuiPresentation(Category.Feature)
            .SetModifierAbilityScore(AttributeDefinitions.Initiative, AttributeDefinitions.Intelligence)
            .AddToDB();
    }

    private static FeatureDefinition BuildSharedVigilance()
    {
        return FeatureDefinitionPowerBuilder
            .Create("PowerTacticianSharedVigilance")
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(PowerVisibilityModifier.Hidden)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 6)
                .ExcludeCaster()
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(ConditionDefinitionBuilder
                        .Create("ConditionTacticianSharedVigilance")
                        .SetGuiPresentationNoContent(true)
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .SetAmountOrigin(ExtraOriginOfAmount.SourceAbilityBonus, AttributeDefinitions.Intelligence)
                        .SetFeatures(FeatureDefinitionAttributeModifierBuilder
                            .Create("AttributeModifierTacticianSharedVigilance")
                            .SetGuiPresentation("AttributeModifierTacticianEverVigilant", Category.Feature)
                            .SetAddConditionAmount(AttributeDefinitions.Initiative)
                            .AddToDB())
                        .AddToDB(), ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();
    }

    private static FeatureDefinition BuildBattleClarity()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetTacticianBattleClarity")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfMaraike,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfPakri)
            .AddToDB();
    }

    private static FeatureDefinition BuildGambitPoolIncrease()
    {
        return FeatureDefinitionPowerUseModifierBuilder
            .Create($"PowerUseModifierTacticianGambitPool{_gambitPoolIncreases++:D2}")
            .SetGuiPresentation("PowerUseModifierTacticianGambitPool", Category.Feature)
            .SetFixedValue(GambitsBuilders.GambitPool, 1)
            .AddToDB();
    }

    internal static FeatureDefinition BuildGambitPoolIncrease(int number, string name)
    {
        return FeatureDefinitionPowerUseModifierBuilder
            .Create($"PowerUseModifierTacticianGambitPool{name}")
            .SetGuiPresentation("PowerUseModifierTacticianGambitPool", Category.Feature)
            .SetFixedValue(GambitsBuilders.GambitPool, number)
            .AddToDB();
    }

    private static FeatureDefinition BuildStrategicPlan()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSefTacticianStrategicPlan")
            .SetGuiPresentation(Category.Feature)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion)
            .AddFeatureSet(
                BuildAdaptiveStrategy(),
                BuildImproviseStrategy(),
                BuildOvercomingStrategy())
            .AddToDB();
    }

    private static FeatureDefinition BuildAdaptiveStrategy()
    {
        var feature = FeatureDefinitionBuilder
            .Create("FeatureAdaptiveStrategy")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        feature.SetCustomSubFeatures(new RefundPowerUseAfterCrit(GambitsBuilders.GambitPool, feature));

        return feature;
    }

    private static FeatureDefinition BuildImproviseStrategy()
    {
        var feature = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureImproviseStrategy")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(BuildGambitPoolIncrease(2, "ImproviseStrategy"))
            .AddToDB();

        feature.SetCustomSubFeatures(new RefundPowerUseAfterCrit(GambitsBuilders.GambitPool, feature));

        return feature;
    }

    private static FeatureDefinition BuildOvercomingStrategy()
    {
        var feature = FeatureDefinitionBuilder
            .Create("FeatureOvercomingStrategy")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        feature.SetCustomSubFeatures(new RefundPowerUseAfterKill(GambitsBuilders.GambitPool, feature));

        ConditionDefinitionBuilder
            .Create(MarkDamagedByGambit)
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetCustomSubFeatures(
                new RefundPowerUseWhenTargetWithConditionDies(GambitsBuilders.GambitPool, feature),
                RemoveConditionOnSourceTurnStart.Mark,
                //by default this condition is applied under Effects tag, which is removed right at death - too early for us to detect
                //this feature will add this effect under Combat tag, which is not removed
                new ForceConditionCategory(AttributeDefinitions.TagCombat))
            .SetSpecialDuration(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
            .AddToDB();

        return feature;
    }

    private static FeatureDefinitionCustomInvocationPool BuildUnlearn()
    {
        return CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolGambitUnlearn")
            .SetGuiPresentationNoContent(true)
            .Setup(InvocationPoolTypeCustom.Pools.Gambit, 1, true)
            .AddToDB();
    }

    private static FeatureDefinition BuildGambitDieSize(DieType size)
    {
        //doesn't do anything, just to display to player dice size progression on level up
        return FeatureDefinitionBuilder
            .Create($"FeatureTacticianGambitDieSize{size}")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();
    }

    private static FeatureDefinition BuildTacticalAwareness()
    {
        var additionalDamageTacticalAwareness = FeatureDefinitionAdditionalDamageBuilder
            .Create("AdditionalDamageTacticianTacticalAwareness")
            .SetGuiPresentationNoContent(true)
            .SetNotificationTag("TacticalAwareness")
            .SetDamageValueDetermination(AdditionalDamageValueDetermination.ProficiencyBonus)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .AddToDB();

        var combatAffinityTacticalAwareness = FeatureDefinitionCombatAffinityBuilder
            .Create("CombatAffinityTacticianTacticalAwareness")
            .SetGuiPresentation("FeatureSetTacticianTacticalAwareness", Category.Feature)
            .SetAttackOfOpportunityOnMeAdvantage(AdvantageType.Disadvantage)
            .AddToDB();

        combatAffinityTacticalAwareness.SetCustomSubFeatures(
            new PhysicalAttackInitiatedTacticalAwareness(combatAffinityTacticalAwareness));

        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetTacticianTacticalAwareness")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(additionalDamageTacticalAwareness, combatAffinityTacticalAwareness)
            .AddToDB();
    }

    private static void BuildTacticalSurge()
    {
        const string CONDITION_NAME = "ConditionTacticianTacticalSurge";

        var tick = FeatureDefinitionBuilder
            .Create("FeatureTacticianTacticalSurgeTick")
            .SetGuiPresentation(CONDITION_NAME, Category.Condition)
            .AddToDB();

        tick.SetCustomSubFeatures(new TacticalSurgeTick(GambitsBuilders.GambitPool, tick));

        var feature = FeatureDefinitionBuilder
            .Create("FeatureTacticianTacticalSurge")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var condition = ConditionDefinitionBuilder
            .Create(CONDITION_NAME)
            .SetGuiPresentation(Category.Condition, Sprites.ConditionTacticalSurge)
            .SetPossessive()
            .SetFeatures(tick)
            .AddToDB();

        feature.SetCustomSubFeatures(new TacticalSurge(GambitsBuilders.GambitPool, feature, condition));
    }

    private class RefundPowerUseAfterCrit : IAfterAttackEffect
    {
        private readonly FeatureDefinition feature;
        private readonly FeatureDefinitionPower power;

        public RefundPowerUseAfterCrit(FeatureDefinitionPower power, FeatureDefinition feature)
        {
            this.power = power;
            this.feature = feature;
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            if (outcome is not (RollOutcome.CriticalFailure or RollOutcome.CriticalSuccess))
            {
                Main.Info("AdaptiveStrategy: not critical. exiting.");
                return;
            }

            if (attackMode == null)
            {
                return;
            }

            // once per turn
            if (attacker.UsedSpecialFeatures.ContainsKey("AdaptiveStrategy"))
            {
                Main.Info("AdaptiveStrategy: once per turn. exiting.");
                return;
            }

            var character = attacker.RulesetCharacter;

            if (character == null)
            {
                return;
            }

            if (character.GetRemainingPowerUses(power) >= character.GetMaxUsesForPool(power))
            {
                Main.Info("AdaptiveStrategy: nothing to refuel. exiting.");
                return;
            }

            GameConsoleHelper.LogCharacterUsedFeature(character, feature, indent: true);
            attacker.UsedSpecialFeatures.TryAdd("AdaptiveStrategy", 1);
            character.UpdateUsageForPower(power, -1);
            Main.Info("AdaptiveStrategy: refueled.");
        }
    }

    private class RefundPowerUseAfterKill : ITargetReducedToZeroHp
    {
        private readonly FeatureDefinition feature;
        private readonly FeatureDefinitionPower power;

        public RefundPowerUseAfterKill(FeatureDefinitionPower power, FeatureDefinition feature)
        {
            this.power = power;
            this.feature = feature;
        }

        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            if (downedCreature.RulesetCharacter.HasConditionOfType(MarkDamagedByGambit))
            {
                Main.Info("OvercomingStrategy: enemy is marked. exiting.");
                yield break;
            }

            if (attackMode == null)
            {
                yield break;
            }

            // once per round
            if (attacker.UsedSpecialFeatures.ContainsKey("OvercomingStrategy"))
            {
                Main.Info("OvercomingStrategy: once per round. exiting.");
                yield break;
            }

            var character = attacker.RulesetCharacter;

            if (character == null)
            {
                yield break;
            }

            if (character.GetRemainingPowerUses(power) >= character.GetMaxUsesForPool(power))
            {
                Main.Info("OvercomingStrategy: nothing to refuel. exiting.");
                yield break;
            }

            GameConsoleHelper.LogCharacterUsedFeature(character, feature, indent: true);
            attacker.UsedSpecialFeatures.TryAdd("OvercomingStrategy", 1);
            character.UpdateUsageForPower(power, -1);
            Main.Info("OvercomingStrategy: refueled.");
        }
    }

    private class RefundPowerUseWhenTargetWithConditionDies : INotifyConditionRemoval
    {
        private readonly FeatureDefinition feature;
        private readonly FeatureDefinitionPower power;

        public RefundPowerUseWhenTargetWithConditionDies(FeatureDefinitionPower power, FeatureDefinition feature)
        {
            this.power = power;
            this.feature = feature;
        }

        public void BeforeDyingWithCondition(RulesetActor rulesetActor, RulesetCondition rulesetCondition)
        {
            var character = EffectHelpers.GetCharacterByGuid(rulesetCondition.sourceGuid);


            if (character == null)
            {
                return;
            }

            if (!character.HasAnyFeature(feature))
            {
                return;
            }

            if (character.GetRemainingPowerUses(power) >= character.GetMaxUsesForPool(power))
            {
                return;
            }

            GameConsoleHelper.LogCharacterUsedFeature(character, feature, indent: true);
            character.UpdateUsageForPower(power, -1);
        }

        public void AfterConditionRemoved(RulesetActor removedFrom, RulesetCondition rulesetCondition)
        {
        }
    }

    private class TacticalSurge : IOnAfterActionFeature
    {
        private readonly ConditionDefinition condition;
        private readonly FeatureDefinition feature;
        private readonly FeatureDefinitionPower power;

        public TacticalSurge(FeatureDefinitionPower power, FeatureDefinition feature,
            ConditionDefinition condition)
        {
            this.power = power;
            this.feature = feature;
            this.condition = condition;
        }

        public void OnAfterAction(CharacterAction action)
        {
            if (action is not CharacterActionActionSurge)
            {
                return;
            }

            var character = action.ActingCharacter.RulesetCharacter;
            var charges = character.GetRemainingPowerUses(power) - character.GetMaxUsesForPool(power);
            charges = Math.Max(charges, -2);

            GameConsoleHelper.LogCharacterUsedFeature(character, feature, indent: true);
            if (charges < 0)
            {
                character.UpdateUsageForPower(power, charges);
            }

            character.InflictCondition(condition.Name, DurationType.Minute, 1, TurnOccurenceType.StartOfTurn,
                AttributeDefinitions.TagCombat, character.Guid, character.CurrentFaction.Name, 1, feature.Name, 1, 0,
                0);
        }
    }

    private class TacticalSurgeTick : ICharacterTurnStartListener
    {
        private readonly FeatureDefinition feature;
        private readonly FeatureDefinitionPower power;

        public TacticalSurgeTick(FeatureDefinitionPower power, FeatureDefinition feature)
        {
            this.power = power;
            this.feature = feature;
        }

        public void OnCharacterTurnStarted(GameLocationCharacter locationCharacter)
        {
            var character = locationCharacter.RulesetCharacter;
            var charges = character.GetRemainingPowerUses(power) - character.GetMaxUsesForPool(power);

            charges = Math.Max(charges, -1);

            if (charges >= 0)
            {
                return;
            }

            GameConsoleHelper.LogCharacterUsedFeature(character, feature, indent: true);
            character.UpdateUsageForPower(power, charges);
        }
    }

    private sealed class PhysicalAttackInitiatedTacticalAwareness : IPhysicalAttackInitiated
    {
        private readonly FeatureDefinition _featureDefinition;

        public PhysicalAttackInitiatedTacticalAwareness(FeatureDefinition featureDefinition)
        {
            _featureDefinition = featureDefinition;
        }

        public IEnumerator OnAttackInitiated(
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackerAttackMode)
        {
            if (attackerAttackMode.actionType != ActionDefinitions.ActionType.Reaction &&
                !attackerAttackMode.attackTags.Contains(TacticalAwareness))
            {
                yield break;
            }

            attackModifier.attackAdvantageTrends.Add(
                new TrendInfo(1, FeatureSourceType.CharacterFeature, _featureDefinition.Name, _featureDefinition));
        }
    }
}
