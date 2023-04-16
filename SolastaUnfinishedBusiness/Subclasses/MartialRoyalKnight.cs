﻿using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class MartialRoyalKnight : AbstractSubclass
{
    internal MartialRoyalKnight()
    {
        const string Name = "RoyalKnight";

        // LEVEL 03

        var powerRallyingCry = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}RallyingCry")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerRallyingCry", Resources.PowerRallyingCry, 256, 128))
            .SetUsesAbilityBonus(ActivationTime.BonusAction, RechargeRate.ShortRest, AttributeDefinitions.Charisma)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(PowerDomainLifePreserveLife.EffectDescription)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetHealingForm(
                            HealingComputation.Dice,
                            0,
                            DieType.D1,
                            4,
                            false,
                            HealingCap.MaximumHitPoints,
                            EffectForm.LevelApplianceType.MultiplyBonus)
                        .Build())
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly, TargetFilteringTag.No, 5, DieType.D8)
                .Build())
            .SetOverriddenPower(PowerFighterSecondWind)
            .AddToDB();

        // LEVEL 07

        var abilityCheckAffinityRoyalEnvoy = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}RoyalEnvoy")
            .SetGuiPresentationNoContent()
            .BuildAndSetAffinityGroups(
                CharacterAbilityCheckAffinity.HalfProficiencyWhenNotProficient,
                DieType.D1,
                0,
                (AttributeDefinitions.Charisma, null))
            .AddToDB();

        var featureSetRoyalEnvoy = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}RoyalEnvoy")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                abilityCheckAffinityRoyalEnvoy,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfSolasta)
            .AddToDB();

        // LEVEL 10

        var additionalActionInspiringSurge = FeatureDefinitionAdditionalActionBuilder
            .Create($"AdditionalAction{Name}InspiringSurge")
            .SetGuiPresentationNoContent(true)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .AddToDB();

        var conditionInspiringSurge = ConditionDefinitionBuilder
            .Create($"Condition{Name}InspiringSurge")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(additionalActionInspiringSurge)
            .AddToDB();

        var powerInspiringSurge = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}InspiringSurge")
            .SetGuiPresentation(Category.Feature, SpellDefinitions.Heroism)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Round, 1)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionInspiringSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        // LEVEL 18

        var savingThrowAffinitySpiritedSurge = FeatureDefinitionSavingThrowAffinityBuilder
            .Create($"SavingThrowAffinity{Name}SpiritedSurge")
            .SetGuiPresentationNoContent(true)
            .SetAffinities(CharacterSavingThrowAffinity.Advantage, true,
                AttributeDefinitions.Strength,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var combatAffinitySpiritedSurge = FeatureDefinitionCombatAffinityBuilder
            .Create($"CombatAffinity{Name}SpiritedSurge")
            .SetGuiPresentation($"Power{Name}SpiritedSurge", Category.Feature)
            .SetMyAttackAdvantage(AdvantageType.Advantage)
            .AddToDB();

        var abilityCheckAffinitySpiritedSurge = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}SpiritedSurge")
            .SetGuiPresentation($"Power{Name}SpiritedSurge", Category.Feature)
            .BuildAndSetAffinityGroups(CharacterAbilityCheckAffinity.Advantage,
                AttributeDefinitions.Strength,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var conditionSpiritedSurge = ConditionDefinitionBuilder
            .Create($"Condition{Name}SpiritedSurge")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                abilityCheckAffinitySpiritedSurge,
                additionalActionInspiringSurge,
                combatAffinitySpiritedSurge,
                savingThrowAffinitySpiritedSurge)
            .AddToDB();

        var powerSpiritedSurge = FeatureDefinitionPowerBuilder
            .Create(powerInspiringSurge, $"Power{Name}SpiritedSurge")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique, 2)
                    .SetDurationData(DurationType.Round, 1)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionSpiritedSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .SetOverriddenPower(powerInspiringSurge)
            .AddToDB();

        // LEVEL 18

        const string TEXT = "PowerRoyalKnightInspiringProtection";

        var powerRoyalKnightInspiringProtection = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtection")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction, RechargeRate.LongRest, 1, 3)
            .SetReactionContext(ReactionTriggerContext.None)
            .AddToDB();

        var powerRoyalKnightInspiringProtectionAura = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtectionAura")
            .SetGuiPresentation(TEXT, Category.Feature)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetCustomSubFeatures(PowerVisibilityModifier.Hidden)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 12)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(EffectFormBuilder
                    .Create()
                    .SetConditionForm(ConditionDefinitionBuilder
                        .Create("ConditionRoyalKnightInspiringProtectionAura")
                        .SetGuiPresentationNoContent(true)
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .SetCustomSubFeatures(
                            new InspiringProtection(powerRoyalKnightInspiringProtection, "InventorFlashOfGenius"))
                        .AddToDB(), ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        var featureSetRoyalKnightInspiringProtection = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetRoyalKnightInspiringProtection")
            .SetGuiPresentation(TEXT, Category.Feature)
            .AddFeatureSet(powerRoyalKnightInspiringProtectionAura, powerRoyalKnightInspiringProtection)
            .AddToDB();

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create($"Martial{Name}")
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.MartialRoyalKnight, 256))
            .AddFeaturesAtLevel(3, powerRallyingCry)
            .AddFeaturesAtLevel(7, featureSetRoyalEnvoy)
            .AddFeaturesAtLevel(10, powerInspiringSurge)
            .AddFeaturesAtLevel(15, featureSetRoyalKnightInspiringProtection)
            .AddFeaturesAtLevel(18, powerSpiritedSurge)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private class InspiringProtection : ConditionSourceCanUsePowerToImproveFailedSaveRoll
    {
        internal InspiringProtection(FeatureDefinitionPower power, string reactionName) : base(power, reactionName)
        {
        }

        internal override bool ShouldTrigger(
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier saveModifier,
            bool hasHitVisual,
            bool hasBorrowedLuck,
            RollOutcome saveOutcome,
            int saveOutcomeDelta)
        {
            if (helper.IsOppositeSide(defender.Side))
            {
                return false;
            }

            return helper.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) ==
                ActionDefinitions.ActionStatus.Available && action.RolledSaveThrow;
        }

        internal override bool TryModifyRoll(
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier saveModifier,
            CharacterActionParams reactionParams,
            bool hasHitVisual,
            bool hasBorrowedLuck,
            ref RollOutcome saveOutcome,
            ref int saveOutcomeDelta)
        {
            // ReSharper disable once MergeConditionalExpression
            action.RolledSaveThrow = action.ActionParams.RulesetEffect == null
                ? action.ActionParams.AttackMode.TryRollSavingThrow(attacker.RulesetCharacter, defender.RulesetActor,
                    saveModifier, action.ActionParams.AttackMode.EffectDescription.EffectForms, out saveOutcome,
                    out saveOutcomeDelta)
                : action.ActionParams.RulesetEffect.TryRollSavingThrow(attacker.RulesetCharacter, attacker.Side,
                    defender.RulesetActor, saveModifier, reactionParams.RulesetEffect.EffectDescription.EffectForms,
                    hasHitVisual, out saveOutcome, out saveOutcomeDelta);

            action.SaveOutcome = saveOutcome;
            action.SaveOutcomeDelta = saveOutcomeDelta;

            return true;
        }

        internal override string FormatReactionDescription(
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier saveModifier,
            bool hasHitVisual,
            bool hasBorrowedLuck,
            RollOutcome saveOutcome,
            int saveOutcomeDelta)
        {
            var text = defender == helper
                ? "Reaction/&SpendPowerInventorFlashOfGeniusReactDescriptionSelfFormat"
                : "Reaction/&SpendPowerInventorFlashOfGeniusReactAllyDescriptionAllyFormat";

            return Gui.Format(text, defender.Name, attacker.Name, action.FormatTitle());
        }
    }
}
