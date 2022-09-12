﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomDefinitions;
using static FeatureDefinitionAttributeModifier.AttributeModifierOperation;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses.Druid;

internal sealed class CircleOfTheForestGuardian : AbstractSubclass
{
    private const string ForestGuardianName = "CircleOfTheForestGuardian";

    private CharacterSubclassDefinition Subclass;

    internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
    {
        return FeatureDefinitionSubclassChoices.SubclassChoiceDruidCircle;
    }

    internal override CharacterSubclassDefinition GetSubclass()
    {
        return Subclass ??= BuildAndAddSubclass();
    }

    private static CharacterSubclassDefinition BuildAndAddSubclass()
    {
        // Create Auto-prepared Spell list
        var druidForestGuardianMagic = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsForestGuardian", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Shield, FogCloud),
                BuildSpellGroup(3, Blur, FlameBlade),
                BuildSpellGroup(5, ProtectionFromEnergy, DispelMagic),
                BuildSpellGroup(7, FireShield, DeathWard),
                BuildSpellGroup(9, HoldMonster, GreaterRestoration))
            .SetSpellcastingClass(CharacterClassDefinitions.Druid)
            .AddToDB();

        var extraAttack = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierForestGuardianExtraAttack", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetModifier(Additive, AttributeDefinitions.AttacksNumber, 1)
            .AddToDB();

        var sylvanResistance = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierForestGuardianSylvanDurability", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetModifier(Additive, AttributeDefinitions.HitPointBonusPerLevel, 1)
            .AddToDB();

        // Create Sylvan War Magic
        var sylvanWarMagic = FeatureDefinitionMagicAffinityBuilder
            .Create(FeatureDefinitionMagicAffinitys.MagicAffinityBattleMagic,
                "MagicAffinityForestGuardianSylvanWarMagic",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var (barkWard, improvedBarkWard, superiorBarkWard) = CreateBarkWard();

        return CharacterSubclassDefinitionBuilder
            .Create(ForestGuardianName, DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Subclass, MartialMountaineer.GuiPresentation.SpriteReference)
            .AddFeatureAtLevel(druidForestGuardianMagic, 2)
            .AddFeatureAtLevel(sylvanResistance, 2)
            .AddFeatureAtLevel(sylvanWarMagic, 2)
            .AddFeatureAtLevel(barkWard, 2)
            .AddFeatureAtLevel(extraAttack, 6)
            .AddFeatureAtLevel(improvedBarkWard, 10)
            .AddFeatureAtLevel(superiorBarkWard, 14)
            .AddToDB();
    }

    // Create Bark Ward Wild Shape Power (and the two higher variants, improved and superior)
    private static (FeatureDefinitionPowerSharedPool barkWard, FeatureDefinitionPowerSharedPool improvedBarkWard,
        FeatureDefinitionPowerSharedPool superiorBarkWard) CreateBarkWard()
    {
        var tempHpEffect = EffectFormBuilder
            .Create()
            .SetTempHPForm(4, DieType.D1, 0)
            .SetLevelAdvancement(EffectForm.LevelApplianceType.MultiplyBonus, LevelSourceType.ClassLevel)
            .CreatedByCharacter()
            .Build();

        var barkWardBuff = EffectFormBuilder
            .Create()
            .SetConditionForm(CreateConditionForestGuardianBarkWard(), ConditionForm.ConditionOperation.Add, true, true)
            .Build();

        var improvedBarkWardBuff = EffectFormBuilder
            .Create()
            .SetConditionForm(CreateConditionConditionForestGuardianImprovedBarkWard(),
                ConditionForm.ConditionOperation.Add, true,
                true)
            .Build();

        var superiorBarkWardBuff = EffectFormBuilder
            .Create()
            .SetConditionForm(CreateConditionForestGuardianSuperiorBarkWard(), ConditionForm.ConditionOperation.Add,
                true,
                true)
            .Build();

        var barkWardEffectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
            .SetCreatedByCharacter()
            .SetDurationData(DurationType.Minute, 10, TurnOccurenceType.EndOfTurn)
            .AddEffectForm(tempHpEffect)
            .AddEffectForm(barkWardBuff)
            .SetEffectAdvancement(EffectIncrementMethod.None)
            .Build();

        var improvedBarkWardEffectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
            .SetCreatedByCharacter()
            .SetDurationData(DurationType.Minute, 10, TurnOccurenceType.EndOfTurn)
            .AddEffectForm(tempHpEffect)
            .AddEffectForm(improvedBarkWardBuff)
            .SetEffectAdvancement(EffectIncrementMethod.None)
            .Build();

        var superiorBarkWardEffectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
            .SetCreatedByCharacter()
            .SetDurationData(DurationType.Minute, 10, TurnOccurenceType.EndOfTurn)
            .AddEffectForm(tempHpEffect)
            .AddEffectForm(superiorBarkWardBuff)
            .SetEffectAdvancement(EffectIncrementMethod.None)
            .Build();

        var barkWard = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolForestGuardianBarkWard", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature, PowerDruidWildShape.GuiPresentation.SpriteReference)
            .Configure(
                PowerDruidWildShape, RechargeRate.ShortRest, ActivationTime.BonusAction, 1, false, false,
                AttributeDefinitions.Wisdom, barkWardEffectDescription, true)
            .AddToDB();

        var improvedBarkWard = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolForestGuardianImprovedBarkWard", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature, PowerDruidWildShape.GuiPresentation.SpriteReference)
            .Configure(
                PowerDruidWildShape, RechargeRate.ShortRest, ActivationTime.BonusAction, 1, false, false,
                AttributeDefinitions.Wisdom, improvedBarkWardEffectDescription, true)
            .SetOverriddenPower(barkWard)
            .AddToDB();

        var superiorBarkWard = FeatureDefinitionPowerSharedPoolBuilder
            .Create("PowerSharedPoolForestGuardianSuperiorBarkWard", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature, PowerDruidWildShape.GuiPresentation.SpriteReference)
            .Configure(
                PowerDruidWildShape, RechargeRate.ShortRest, ActivationTime.BonusAction, 1, false, false,
                AttributeDefinitions.Wisdom, superiorBarkWardEffectDescription, true)
            .SetOverriddenPower(improvedBarkWard)
            .AddToDB();

        return (barkWard, improvedBarkWard, superiorBarkWard);

        static ConditionDefinition CreateConditionForestGuardianBarkWard()
        {
            return ConditionDefinitionBuilder
                .Create(ConditionDefinitions.ConditionBarkskin, "ConditionForestGuardianBarkWard",
                    DefinitionBuilder.CENamespaceGuid)
                .SetOrUpdateGuiPresentation("ConditionForestGuardianBarkWard", Category.Condition)
                .ClearFeatures()
                .SetAllowMultipleInstances(false)
                .SetDuration(DurationType.Minute, 10)
                .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                .AddToDB();
        }

        static ConditionDefinition CreateConditionConditionForestGuardianImprovedBarkWard()
        {
            var damageEffect = EffectFormBuilder
                .Create()
                .SetDamageForm(false, DieType.D8, DamageTypePiercing, 0, DieType.D8, 2)
                .CreatedByCondition()
                .Build();

            var improvedBarkWardRetaliationEffect = EffectDescriptionBuilder
                .Create()
                .AddEffectForm(damageEffect)
                .Build();

            var improvedBarkWardDamageRetaliate = FeatureDefinitionPowerBuilder
                .Create("PowerForestGuardianImprovedBarkWard", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentationNoContent()
                .Configure(
                    0, UsesDetermination.Fixed, AttributeDefinitions.Wisdom, ActivationTime.NoCost,
                    0, RechargeRate.AtWill, false, false, AttributeDefinitions.Wisdom,
                    improvedBarkWardRetaliationEffect, true)
                .AddToDB();

            var improvedBarkWardDamage = FeatureDefinitionDamageAffinityBuilder
                .Create("DamageAffinityForestGuardianImprovedBarkWard", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentationNoContent()
                .SetDamageAffinityType(DamageAffinityType.None)
                .SetDamageType(DamageTypePoison)
                .SetRetaliate(improvedBarkWardDamageRetaliate, 1, true)
                .SetAncestryDefinesDamageType(false)
                .AddToDB();

            return ConditionDefinitionBuilder
                .Create(ConditionDefinitions.ConditionBarkskin, "ConditionForestGuardianImprovedBarkWard",
                    DefinitionBuilder.CENamespaceGuid)
                .SetOrUpdateGuiPresentation(Category.Condition)
                .SetAllowMultipleInstances(false)
                .SetDuration(DurationType.Minute, 10)
                .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                .SetFeatures(improvedBarkWardDamage)
                .AddToDB();
        }

        static ConditionDefinition CreateConditionForestGuardianSuperiorBarkWard()
        {
            var damageEffect = EffectFormBuilder
                .Create()
                .SetDamageForm(false, DieType.D8, DamageTypePiercing, 0, DieType.D8, 3)
                .CreatedByCondition()
                .Build();

            var superiorBarkWardRetaliationEffect = EffectDescriptionBuilder
                .Create()
                .AddEffectForm(damageEffect)
                .Build();

            var powerSuperiorBarkWardRetaliatePower = FeatureDefinitionPowerBuilder
                .Create("PowerForestGuardianSuperiorBarkWard", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentationNoContent()
                .Configure(
                    0, UsesDetermination.Fixed, AttributeDefinitions.Wisdom, ActivationTime.NoCost,
                    0, RechargeRate.AtWill, false, false,
                    AttributeDefinitions.Wisdom, superiorBarkWardRetaliationEffect, true)
                .AddToDB();

            var powerSuperiorBarkWardRetaliateDamageAffinity = FeatureDefinitionDamageAffinityBuilder
                .Create("DamageAffinityForestGuardianSuperiorBarkWard", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentationNoContent()
                .SetDamageAffinityType(DamageAffinityType.Immunity)
                .SetDamageType(DamageTypePoison)
                .SetRetaliate(powerSuperiorBarkWardRetaliatePower, 1, true)
                .SetAncestryDefinesDamageType(false)
                .AddToDB();

            return ConditionDefinitionBuilder
                .Create(ConditionDefinitions.ConditionBarkskin, "ConditionForestGuardianSuperiorBarkWard",
                    DefinitionBuilder.CENamespaceGuid)
                .SetOrUpdateGuiPresentation(Category.Condition)
                .SetFeatures(powerSuperiorBarkWardRetaliateDamageAffinity)
                .SetAllowMultipleInstances(false)
                .SetDuration(DurationType.Minute, 10)
                .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                .AddToDB();
        }
    }
}
