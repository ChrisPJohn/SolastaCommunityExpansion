﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterAttackDefinitions;
using static EffectForm;
using SolastaUnfinishedBusiness.CustomBehaviors;
using System.Collections.Generic;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class CircleOfTheNight : AbstractSubclass
{
    private const string CircleOfTheNightName = "CircleOfTheNight";

    internal CircleOfTheNight()
    {
        // 3rd level
        // Combat Wildshape 
        // Official rules are CR = 1/3 of druid level. However in solasta the selection of beasts is greatly reduced
        var featureSetCircleOfTheNightWildShapeCombat = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetCircleOfTheNightWildShapeCombat")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var powerCircleOfTheNightWildShapeCombat = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerCircleOfTheNightWildShapeCombat")
            .SetOverriddenPower(PowerDruidWildShape)
            .SetGuiPresentation(Category.Feature, hidden: true)
            .SetSharedPool(ActivationTime.Action, PowerDruidWildShape)
            .SetEffectDescription(BuildCombatWildShapeEffectDescription())
            .AddToDB();

        // Combat Wild Shape Healing
        // While wild shaped, you can use a bonus action to heal yourself for 1d8 hit points.
        // You can use this feature a number of times equal to your Proficiency Modifier per form per long rest
        var powerCircleOfTheNightWildShapeHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing())
            .SetCustomSubFeatures(CanUseCombatHealing())
            .AddToDB();

        // 6th Level
        // Primal Strike
        // Starting at 6th level, your attacks in beast form count as magical for the purpose of overcoming resistance
        // and immunity to non magical attacks and damage.
        // NOTE: (BUG)This also affects attacks with regular weapons
        var powerCircleOfTheNightPrimalStrike = FeatureDefinitionAttackModifierBuilder
            .Create("PowerCircleOfTheNightPrimalStrike")
            .SetGuiPresentation(Category.Feature)
            .SetMagicalWeapon()
            //.SetRequiredProperty(RestrictedContextRequiredProperty.Unarmed)
            .AddToDB();

        // Improved Combat Healing
        // At 6th level, your combat healing improves to 2d8 + 2
        var powerCircleOfTheNightWildShapeImprovedHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeImprovedHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing(2))
            .SetCustomSubFeatures(CanUseCombatHealing())
            .SetOverriddenPower(powerCircleOfTheNightWildShapeHealing)
            .AddToDB();

        // 10th Level
        // Superior Combat Healing
        // At 10th level, your combat healing improves to 3d8 + 6
        var powerCircleOfTheNightWildShapeSuperiorHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeSuperiorHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing(3, DieType.D8, 6))
            .SetCustomSubFeatures(CanUseCombatHealing())
            .SetOverriddenPower(powerCircleOfTheNightWildShapeImprovedHealing)
            .AddToDB();

        // Elemental Forms
        var featureSetCircleOfTheNightElementalForms = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetCircleOfTheNightElementalForms")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();


        Subclass = CharacterSubclassDefinitionBuilder
            .Create(CircleOfTheNightName)
            .SetGuiPresentation(Category.Subclass, PathClaw)
            .AddFeaturesAtLevel(2,
                featureSetCircleOfTheNightWildShapeCombat,
                powerCircleOfTheNightWildShapeCombat,
                powerCircleOfTheNightWildShapeHealing)
            .AddFeaturesAtLevel(6,
                powerCircleOfTheNightPrimalStrike,
                powerCircleOfTheNightWildShapeImprovedHealing)
            .AddFeaturesAtLevel(10,
                featureSetCircleOfTheNightElementalForms,
                powerCircleOfTheNightWildShapeSuperiorHealing)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceDruidCircle;


    // custom wild shapes

    /**
     * based on MM Cave Bear
     * */
    private static MonsterDefinition HBWildShapeDireBear()
    {
        // attacks
        // Bite
        // TODO Bump damage mod from +4 to +5
        var biteAttack = new MonsterAttackIteration
        {
            monsterAttackDefinition = MonsterAttackDefinitionBuilder
                .Create(Attack_Wildshape_BrownBear_Bite, "Attack_Wildshape_DireBear_Bite")
                .SetToHitBonus(7)
                .AddToDB()
        };

        // Claw
        var clawAttack = new MonsterAttackIteration
        {
            monsterAttackDefinition = MonsterAttackDefinitionBuilder
                .Create(Attack_Wildshape_BrownBear_Claw, "Attack_Wildshape_DireBear_Claw")
                .SetToHitBonus(7)
                .AddToDB()
        };

        var shape = MonsterDefinitionBuilder.Create(WildshapeBlackBear, "WildShapeDireBear")
            // STR, DEX, CON, INT, WIS, CHA
            .SetAbilityScores(20, 10, 16, 2, 13, 7)
            .SetArmorClass(14)
            .SetStandardHitPoints(42)
            .SetHitDice(DieType.D10, 5)
            .SetChallengeRating(2)
            .SetOrUpdateGuiPresentation(Category.Monster, WildshapeBlackBear)
            .SetAttackIterations(biteAttack, clawAttack)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HBWildShapeAirElemental()
    {
        var shape = MonsterDefinitionBuilder.Create(Air_Elemental, "WildShapeAirElemental")
            // STR, DEX, CON, INT, WIS, CHA
            .SetAbilityScores(14, 20, 14, 6, 10, 6)
            .SetArmorClass(15)
            .SetStandardHitPoints(90)
            .SetHitDice(DieType.D10, 12)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HBWildShapeFireElemental()
    {
        var shape = MonsterDefinitionBuilder.Create(Fire_Elemental, "WildShapeFireElemental")
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HBWildShapeEarthElemental()
    {
        var shape = MonsterDefinitionBuilder.Create(Earth_Elemental, "WildShapeEarthElemental")
            .AddToDB();

        return shape;
    }

#if false
    private static MonsterDefinition HBWildShapeWaterElemental()
    {
        var shape = MonsterDefinitionBuilder.Create(Ice_Elemental, "WildShapeWaterElemental")
            .AddToDB();

        return shape;
    }
#endif

    private static ShapeOptionDescription ShapeBuilder(int level, MonsterDefinition monster)
    {
        var shape = new ShapeOptionDescription { requiredLevel = level, substituteMonster = monster };
        return shape;
    }

    //TODO: use builders here
    private static EffectDescription BuildCombatWildShapeEffectDescription()
    {
        var wildShapeEffect = EffectDescriptionBuilder
            .Create(PowerDruidWildShape.effectDescription)
            .SetEffectAdvancement(EffectIncrementMethod.None)
            .Build();
        /*
        wildShapeEffect.targetType = TargetType.Self;
        wildShapeEffect.recurrentEffect = RecurrentEffect.No;
        wildShapeEffect.durationType = DurationType.HalfClassLevelHours;
        wildShapeEffect.effectAIParameters = PowerDruidWildShape.effectDescription.effectAIParameters;
        wildShapeEffect.effectParticleParameters = PowerDruidWildShape.effectDescription.effectParticleParameters;
        */
        var effectForm = new EffectForm
        {
            formType = EffectFormType.ShapeChange,
            addBonusMode = AddBonusMode.None,
            applyLevel = LevelApplianceType.No,
            levelType = LevelSourceType.ClassLevel,
            levelMultiplier = 0,
            createdByCharacter = true,
            createdByCondition = false,
            hasSavingThrow = false,
            savingThrowAffinity = EffectSavingThrowType.None,
            dcModifier = 0,
            canSaveToCancel = false,
            saveOccurence = TurnOccurenceType.StartOfTurn,
            hasFilterId = false,
            filterId = 0,
            shapeChangeForm = new ShapeChangeForm
            {
                shapeChangeType = ShapeChangeForm.Type.ClassLevelListSelection,
                keepMentalAbilityScores = true,
                specialSubstituteCondition = ConditionDefinitions.ConditionWildShapeSubstituteForm,
                shapeOptions = new List<ShapeOptionDescription>
                {
                    ShapeBuilder(2, WildShapeBadlandsSpider),
                    ShapeBuilder(2, WildshapeDirewolf),
                    ShapeBuilder(2, WildShapeBrownBear),
                    ShapeBuilder(4, WildshapeDeepSpider),
                    ShapeBuilder(4, HBWildShapeDireBear()),
                    ShapeBuilder(6, WildShapeApe),
                    // flying
                    ShapeBuilder(8, WildshapeTiger_Drake),
                    ShapeBuilder(8, WildShapeGiant_Eagle),
                    // Elementals
                    // According to the rules, transforming into an elemental should cost 2 Wild Shape Charges
                    // However elementals in this game are nerfed, since they don't have special attacks, such as Whirlwind
                    //TODO: Create a new feature for elemental transformation.
                    //TODO: Add special attacks to elemental forms (whirlwind, Whelm, Earth Glide maybe)
                    ShapeBuilder(10, HBWildShapeAirElemental()),
                    ShapeBuilder(10, HBWildShapeFireElemental()),
                    ShapeBuilder(10, HBWildShapeEarthElemental()),
                    // don't use future features
                    // ShapeBuilder(10, HBWildShapeWaterElemental())
                    // ShapeBuilder(10, WildShapeTundraTiger)
                }
            }
        };

        wildShapeEffect.effectForms.Clear();
        wildShapeEffect.effectForms.Add(effectForm);

        return wildShapeEffect;
    }

    private static EffectDescription CombatHealing(
        int diceNumber = 1,
        DieType dieType = DieType.D8,
        int bonusHealing = 0)
    {
        var healingForm = EffectFormBuilder.Create()
            .SetHealingForm(
                HealingComputation.Dice,
                bonusHealing,
                dieType,
                diceNumber,
                false,
                HealingCap.MaximumHitPoints)
            .Build();

        var effectDescription = EffectDescriptionBuilder.Create()
            .SetRequiredCondition(ConditionDefinitions.ConditionWildShapeSubstituteForm)
            .SetDurationData(DurationType.Instantaneous)
            .SetEffectForms(healingForm)
            .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
            .Build();

        return effectDescription;
    }

    private static ValidatorsPowerUse CanUseCombatHealing()
    {
        return new ValidatorsPowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionDefinitions.ConditionWildShapeSubstituteForm));
    }
}
