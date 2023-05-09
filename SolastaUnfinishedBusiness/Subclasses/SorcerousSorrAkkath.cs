﻿using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Properties;
using static AttributeDefinitions;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class SorcerousSorrAkkath : AbstractSubclass
{
    private const string Name = "SorcerousSorrAkkath";
    private const string DeceptiveHeritage = "DeceptiveHeritage";
    private const string SpellSneakAttack = "SpellSneakAttack";
    private const string BloodOfSorrAkkath = "BloodOfSorrAkkath";
    private const string DarknessAffinity = "DarknessAffinity";
    private const string TouchOfDarkness = "TouchOfDarkness";

    internal SorcerousSorrAkkath()
    {
        // LEVEL 01

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation("ExpandedSpells", Category.Feature)
            .SetAutoTag("Origin")
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, HideousLaughter)
            .AddPreparedSpellGroup(3, Invisibility)
            .AddPreparedSpellGroup(5, Fear)
            .AddPreparedSpellGroup(7, GreaterInvisibility)
            .AddPreparedSpellGroup(9, DominatePerson)
            .AddPreparedSpellGroup(11, GlobeOfInvulnerability)
            .AddToDB();

        // Deceptive Heritage

        var bonusCantripsDeceptiveHeritage = FeatureDefinitionBonusCantripsBuilder
            .Create($"BonusCantrips{Name}{DeceptiveHeritage}")
            .SetGuiPresentationNoContent(true)
            .SetBonusCantrips(VenomousSpike)
            .AddToDB();

        var proficiencyDeceptiveHeritage = FeatureDefinitionProficiencyBuilder
            .Create($"Proficiency{Name}{DeceptiveHeritage}")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.SkillOrExpertise, SkillDefinitions.Deception, SkillDefinitions.Stealth)
            .AddToDB();

        var featureSetDeceptiveHeritage = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{DeceptiveHeritage}")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                bonusCantripsDeceptiveHeritage,
                proficiencyDeceptiveHeritage,
                FeatureDefinitionSenses.SenseDarkvision12)
            .AddToDB();

        // Spell Sneak Attack

        var additionalDamageSpellSneakAttack = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}{SpellSneakAttack}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(SpellSneakAttack)
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 2, 1, 6, 5)
            .SetRequiredProperty(RestrictedContextRequiredProperty.SpellWithAttackRoll)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetSavingThrowData(EffectDifficultyClassComputation.SpellCastingFeature, EffectSavingThrowType.None)
            .AddToDB();

        // another odd dice damage progression
        for (var i = 0; i < 4; i++)
        {
            additionalDamageSpellSneakAttack.DiceByRankTable[i].diceNumber = 1;
        }

        // LEVEL 06

        // Blood of Sorr-Akkath

        var conditionBloodOfSorrAkkath = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionPoisoned, $"Condition{Name}{BloodOfSorrAkkath}")
            .SetSpecialDuration(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
            .AddToDB();

        var additionalDamageBloodOfSorrAkkath = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}{BloodOfSorrAkkath}")
            .SetGuiPresentation($"AdditionalDamage{Name}{SpellSneakAttack}", Category.Feature)
            // use flat bonus to allow it to interact correct with spell attack
            .SetDamageValueDetermination(AdditionalDamageValueDetermination.FlatBonus)
            .SetRequiredProperty(RestrictedContextRequiredProperty.SpellWithAttackRoll)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetSavingThrowData()
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    saveOccurence = TurnOccurenceType.EndOfTurn,
                    saveAffinity = EffectSavingThrowType.Negates,
                    operation = ConditionOperationDescription.ConditionOperation.Add,
                    conditionDefinition = conditionBloodOfSorrAkkath
                })
            .AddToDB();

        var featureSetBloodOfSorrAkkath = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{BloodOfSorrAkkath}")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonResistance,
                additionalDamageBloodOfSorrAkkath)
            .AddToDB();

        // LEVEL 14

        // Darkness Affinity

        const string DARKNESS_AFFINITY_NAME = $"FeatureSet{Name}{DarknessAffinity}";

        var attackModifierDarknessAffinity = FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetAttackRollModifier(2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var attributeModifierDarknessAffinity = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetSituationalContext(ExtraSituationalContext.IsNotInBrightLight)
            .SetModifier(AttributeModifierOperation.Additive, ArmorClass, 2)
            .AddToDB();

        var magicAffinityDarknessAffinity = FeatureDefinitionMagicAffinityBuilder
            .Create($"MagicAffinity{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetCastingModifiers(2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var regenerationDarknessAffinity = FeatureDefinitionRegenerationBuilder
            .Create(FeatureDefinitionRegenerations.RegenerationRing, $"Regeneration{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetDuration(DurationType.Round, 1)
            .SetRegenerationDice(DieType.D1, 0, 2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var savingThrowAffinityDarknessAffinity = FeatureDefinitionSavingThrowAffinityBuilder
            .Create($"SavingThrowAffinity{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetModifiers(FeatureDefinitionSavingThrowAffinity.ModifierType.AddDice, DieType.D1, 2, false,
                Charisma, Constitution, Dexterity, Intelligence, Strength, Wisdom)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var featureSetDarknessAffinity = FeatureDefinitionFeatureSetBuilder
            .Create(DARKNESS_AFFINITY_NAME)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                attackModifierDarknessAffinity,
                attributeModifierDarknessAffinity,
                magicAffinityDarknessAffinity,
                regenerationDarknessAffinity,
                savingThrowAffinityDarknessAffinity)
            .AddToDB();

        // LEVEL 18

        // Touch of Darkness

        const string TOUCH_OF_DARKNESS_NAME = $"FeatureSet{Name}{TouchOfDarkness}";

        var effectTouchOfDarkness = EffectDescriptionBuilder
            .Create()
            .SetDurationData(DurationType.Instantaneous)
            .SetParticleEffectParameters(VampiricTouch)
            .SetTargetingData(Side.Enemy, RangeType.MeleeHit, 1, TargetType.Individuals)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeNecrotic, 8, DieType.D8, 0, HealFromInflictedDamage.Half)
                    .Build())
            .Build();

        var powerTouchOfDarknessFixed = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{TouchOfDarkness}Fixed")
            .SetGuiPresentation(TOUCH_OF_DARKNESS_NAME, Category.Feature, VampiricTouch)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest, 1, 3)
            .SetUseSpellAttack()
            .SetEffectDescription(effectTouchOfDarkness)
            .AddToDB();

        powerTouchOfDarknessFixed.SetCustomSubFeatures(
            new PowerUseValidityTouchOfDarknessFixed(powerTouchOfDarknessFixed));

        var powerTouchOfDarknessPoints = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{TouchOfDarkness}Points")
            .SetGuiPresentation(TOUCH_OF_DARKNESS_NAME, Category.Feature, VampiricTouch)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.SorceryPoints, 4, 0)
            .SetUseSpellAttack()
            .SetEffectDescription(effectTouchOfDarkness)
            .AddToDB();

        powerTouchOfDarknessPoints.SetCustomSubFeatures(
            new PowerUseValidityTouchOfDarknessPoints(powerTouchOfDarknessFixed));

        var featureSetTouchOfDarkness = FeatureDefinitionFeatureSetBuilder
            .Create(TOUCH_OF_DARKNESS_NAME)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(powerTouchOfDarknessFixed, powerTouchOfDarknessPoints)
            .AddToDB();

        // BUGFIX

        ChillTouch.EffectDescription.EffectForms[0].savingThrowAffinity = EffectSavingThrowType.None;
        RayOfFrost.EffectDescription.EffectForms[0].savingThrowAffinity = EffectSavingThrowType.None;

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.SorcererSorrAkkath, 256))
            .AddFeaturesAtLevel(1,
                autoPreparedSpells,
                featureSetDeceptiveHeritage,
                additionalDamageSpellSneakAttack)
            .AddFeaturesAtLevel(6,
                featureSetBloodOfSorrAkkath)
            .AddFeaturesAtLevel(14,
                featureSetDarknessAffinity)
            .AddFeaturesAtLevel(18,
                featureSetTouchOfDarkness)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceSorcerousOrigin;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Touch of Darkness Fixed
    //

    private class PowerUseValidityTouchOfDarknessFixed : IPowerUseValidity
    {
        private readonly FeatureDefinitionPower _powerTouchFixed;

        public PowerUseValidityTouchOfDarknessFixed(FeatureDefinitionPower powerTouchFixed)
        {
            _powerTouchFixed = powerTouchFixed;
        }

        public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower featureDefinitionPower)
        {
            var usablePower = UsablePowersProvider.Get(_powerTouchFixed, character);

            return usablePower.RemainingUses > 0;
        }
    }

    //
    // Touch of Darkness Points
    //

    private class PowerUseValidityTouchOfDarknessPoints : IPowerUseValidity
    {
        private readonly FeatureDefinitionPower _powerTouchFixed;

        public PowerUseValidityTouchOfDarknessPoints(FeatureDefinitionPower powerTouchFixed)
        {
            _powerTouchFixed = powerTouchFixed;
        }

        public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower featureDefinitionPower)
        {
            var usablePower = UsablePowersProvider.Get(_powerTouchFixed, character);

            return usablePower.RemainingUses == 0;
        }
    }
}
