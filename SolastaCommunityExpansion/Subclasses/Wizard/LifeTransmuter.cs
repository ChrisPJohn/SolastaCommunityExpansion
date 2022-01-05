﻿using SolastaCommunityExpansion.CustomFeatureDefinitions;
using SolastaCommunityExpansion.Features;
using SolastaModApi;
using SolastaModApi.BuilderHelpers;
using System;
using System.Collections.Generic;

namespace SolastaCommunityExpansion.Subclasses.Wizard
{
    internal class LifeTransmuter : AbstractSubclass
    {
        private static Guid SubclassNamespace = new Guid("81cdcf44-5f04-4aea-8232-b22a1c264065");
        private readonly CharacterSubclassDefinition Subclass;

        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return DatabaseHelper.FeatureDefinitionSubclassChoices.SubclassChoiceWizardArcaneTraditions;
        }
        internal override CharacterSubclassDefinition GetSubclass()
        {
            return Subclass;
        }

        internal LifeTransmuter()
        {
            CharacterSubclassDefinitionBuilder lifeTransmuter = new CharacterSubclassDefinitionBuilder("LifeTransmuter", GuidHelper.Create(SubclassNamespace, "LifeTransmuter").ToString());
            GuiPresentationBuilder LifeTransmuterPresentation = new GuiPresentationBuilder(
                "Subclass/&TraditionLifeTransmuterDescription",
                "Subclass/&TraditionLifeTransmuterTitle");
            LifeTransmuterPresentation.SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.RoguishDarkweaver.GuiPresentation.SpriteReference);
            lifeTransmuter.SetGuiPresentation(LifeTransmuterPresentation.Build());

            GuiPresentationBuilder LifeTransmuterAffinityGui = new GuiPresentationBuilder(
                "Subclass/&MagicAffinityLifeTransmuterListDescription",
                "Subclass/&MagicAffinityLifeTransmuterListTitle");
            FeatureDefinitionMagicAffinity LifeTransmuterAffinity = BuildMagicAffinityHeightenedList(new List<string>() {
                DatabaseHelper.SpellDefinitions.FalseLife.Name, // necromancy
                DatabaseHelper.SpellDefinitions.MagicWeapon.Name, // transmutation
                DatabaseHelper.SpellDefinitions.Blindness.Name, // necromancy
                DatabaseHelper.SpellDefinitions.Fly.Name, // transmutation
                DatabaseHelper.SpellDefinitions.BestowCurse.Name, // necromancy
                DatabaseHelper.SpellDefinitions.VampiricTouch.Name, // necromancy
                DatabaseHelper.SpellDefinitions.Blight.Name, // necromancy
                DatabaseHelper.SpellDefinitions.CloudKill.Name, // conjuration
            }, 2,
                "MagicAffinityLifeTransmuterHeightened", LifeTransmuterAffinityGui.Build());
            lifeTransmuter.AddFeatureAtLevel(LifeTransmuterAffinity, 2);

            // Add tranmsuter stone like abilities.
            GuiPresentationBuilder LifeTransmuterDivintyGui = new GuiPresentationBuilder(
                "Subclass/&HealingPoolLifeTransmuterListDescription",
                "Subclass/&HealingPoolLifeTransmuterListTitle");

            FeatureDefinitionPower TransmuteForce = new FeatureDefinitionPowerPoolBuilder("AttributeModiferTransmuterHealingPool",
                GuidHelper.Create(SubclassNamespace, "AttributeModiferTransmuterHealingPool").ToString(),
                2, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Intelligence, RuleDefinitions.RechargeRate.LongRest,
                LifeTransmuterDivintyGui.Build()).AddToDB();
            lifeTransmuter.AddFeatureAtLevel(TransmuteForce, 6);

            // Make a power that grants darkvision
            GuiPresentationBuilder TransmuteDarkvision = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteDarkvisionDescription",
                "Subclass/&PowerTransmuteDarkvisionTitle");
            TransmuteDarkvision.SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleDivineWrath.GuiPresentation.SpriteReference);
            GuiPresentationBuilder ConditionDarkvisionGui = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteDarkvisionDescription",
                "Subclass/&PowerTransmuteDarkvisionTitle");
            ConditionDarkvisionGui.SetSpriteReference(DatabaseHelper.ConditionDefinitions.ConditionDarkvision.GuiPresentation.SpriteReference);
            ConditionDefinition superiorDarkvision = BuildCondition(new List<FeatureDefinition>() { DatabaseHelper.FeatureDefinitionSenses.SenseSuperiorDarkvision },
                RuleDefinitions.DurationType.UntilLongRest, 1, "ConditionPowerTransmuteDarkvision", ConditionDarkvisionGui.Build());
            FeatureDefinitionPowerSharedPool PowerDarkvision = BuildActionTransmuteConditionPower(TransmuteForce, RuleDefinitions.RechargeRate.LongRest,
                RuleDefinitions.ActivationTime.BonusAction, 1, RuleDefinitions.RangeType.Touch, 2,
                RuleDefinitions.TargetType.Individuals, ActionDefinitions.ItemSelectionType.None, RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn, AttributeDefinitions.Intelligence, superiorDarkvision, "PowerTransmuteDarkvision", TransmuteDarkvision.Build());
            lifeTransmuter.AddFeatureAtLevel(PowerDarkvision, 6);

            // Make a power that gives resistance to an elemental damage
            GuiPresentationBuilder TransmutePoison = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteElementalResistanceDescription",
                "Subclass/&PowerTransmuteElemnetalResistanceTitle");
            TransmutePoison.SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerDomainElementalFireBurst.GuiPresentation.SpriteReference);
            GuiPresentationBuilder ConditionPoisonResistance = new GuiPresentationBuilder(
                "Subclass/&ConditionTransmutePoisonDescription",
                "Subclass/&ConditionTransmutePoisonTitle");
            ConditionPoisonResistance.SetSpriteReference(DatabaseHelper.ConditionDefinitions.ConditionProtectedFromPoison.GuiPresentation.SpriteReference);
            ConditionDefinition PoisonResistance = BuildCondition(new List<FeatureDefinition>() {
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityPoisonResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityAcidResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityColdResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityFireResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityThunderResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityLightningResistance,
                DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance,
            },
                RuleDefinitions.DurationType.UntilLongRest, 1, "ConditionPowerTransmutePoison", ConditionPoisonResistance.Build());
            FeatureDefinitionPowerSharedPool PowerPoison = BuildActionTransmuteConditionPower(TransmuteForce, RuleDefinitions.RechargeRate.LongRest,
                RuleDefinitions.ActivationTime.BonusAction, 1, RuleDefinitions.RangeType.Touch, 2,
                RuleDefinitions.TargetType.Individuals, ActionDefinitions.ItemSelectionType.None, RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn, AttributeDefinitions.Intelligence, PoisonResistance, "PowerTransmutePoison", TransmutePoison.Build());
            lifeTransmuter.AddFeatureAtLevel(PowerPoison, 6);
            // Make a power that gives proficiency to constitution saves
            GuiPresentationBuilder TransmuteConstitution = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteConstitutionDescription",
                "Subclass/&PowerTransmuteConstitutionTitle");
            GuiPresentationBuilder ConstitutionSaveGui = new GuiPresentationBuilder(
                "Subclass/&ConditionTransmuteConstitutionSaveDescription",
                "Subclass/&ConditionTransmuteConstitutionSaveTitle");
            ConstitutionSaveGui.SetSpriteReference(DatabaseHelper.ConditionDefinitions.ConditionBearsEndurance.GuiPresentation.SpriteReference);
            ConditionDefinition ConstitutionProf = BuildCondition(new List<FeatureDefinition>() { DatabaseHelper.FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfArun },
                RuleDefinitions.DurationType.UntilLongRest, 1, "ConditionPowerTransmuteConstitution", ConstitutionSaveGui.Build());
            TransmuteConstitution.SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerPaladinAuraOfCourage.GuiPresentation.SpriteReference);
            FeatureDefinitionPowerSharedPool PowerConstitution = BuildActionTransmuteConditionPower(TransmuteForce, RuleDefinitions.RechargeRate.LongRest,
                RuleDefinitions.ActivationTime.BonusAction, 1, RuleDefinitions.RangeType.Touch, 2,
                RuleDefinitions.TargetType.Individuals, ActionDefinitions.ItemSelectionType.None, RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn, AttributeDefinitions.Intelligence, ConstitutionProf, "PowerTransmuteConstitution", TransmuteConstitution.Build());
            lifeTransmuter.AddFeatureAtLevel(PowerConstitution, 6);

            GuiPresentationBuilder LifeTransmuterExtraPoolGui = new GuiPresentationBuilder(
                "Subclass/&HealingPoolLifeTransmuterBonusDescription",
                "Subclass/&HealingPoolLifeTransmuterBonusTitle");
            FeatureDefinitionPowerPoolModifier TransmuteForceExtra = new FeatureDefinitionPowerPoolModifierBuilder("AttributeModiferTransmuterHealingPoolExtra",
                GuidHelper.Create(SubclassNamespace, "AttributeModiferTransmuterHealingPoolExtra").ToString(),
                2, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Intelligence, TransmuteForce, LifeTransmuterExtraPoolGui.Build()).AddToDB();
            lifeTransmuter.AddFeatureAtLevel(TransmuteForceExtra, 10);

            GuiPresentationBuilder TransmuteFly = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteFlyDescription",
                "Subclass/&PowerTransmuteFlyTitle");
            TransmuteFly.SetSpriteReference(DatabaseHelper.SpellDefinitions.Fly.GuiPresentation.SpriteReference);
            FeatureDefinitionPowerSharedPool PowerFly = BuildActionTransmuteConditionPower(TransmuteForce, RuleDefinitions.RechargeRate.LongRest,
                RuleDefinitions.ActivationTime.BonusAction, 1, RuleDefinitions.RangeType.Touch, 2,
                RuleDefinitions.TargetType.Individuals, ActionDefinitions.ItemSelectionType.None, RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn, AttributeDefinitions.Intelligence, DatabaseHelper.ConditionDefinitions.ConditionFlying, "PowerTransmuteFly", TransmuteFly.Build());
            lifeTransmuter.AddFeatureAtLevel(PowerFly, 10);

            GuiPresentationBuilder TransmuteHeal = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteHealDescription",
                "Subclass/&PowerTransmuteHealTitle");
            TransmuteHeal.SetSpriteReference(DatabaseHelper.SpellDefinitions.MassHealingWord.GuiPresentation.SpriteReference);
            FeatureDefinitionPowerSharedPool PowerHeal = new FeatureDefinitionPowerSharedPoolBuilder("PowerTransmuteHeal", GuidHelper.Create(SubclassNamespace, "PowerTransmuteHeal").ToString(),
                TransmuteForce, RuleDefinitions.RechargeRate.LongRest, RuleDefinitions.ActivationTime.BonusAction, 1, false, false, AttributeDefinitions.Intelligence,
                DatabaseHelper.SpellDefinitions.MassHealingWord.EffectDescription, TransmuteHeal.Build(), false /* unique instance */).AddToDB();
            lifeTransmuter.AddFeatureAtLevel(PowerHeal, 10);

            GuiPresentationBuilder TransmuteRevive = new GuiPresentationBuilder(
                "Subclass/&PowerTransmuteReviveDescription",
                "Subclass/&PowerTransmuteReviveTitle");
            TransmuteRevive.SetSpriteReference(DatabaseHelper.SpellDefinitions.Revivify.GuiPresentation.SpriteReference);
            FeatureDefinitionPowerSharedPool PowerRevive = new FeatureDefinitionPowerSharedPoolBuilder("PowerTransmuteRevive", GuidHelper.Create(SubclassNamespace, "PowerTransmuteRevive").ToString(),
                TransmuteForce, RuleDefinitions.RechargeRate.LongRest, RuleDefinitions.ActivationTime.BonusAction, 1, false, false, AttributeDefinitions.Intelligence,
                DatabaseHelper.SpellDefinitions.Revivify.EffectDescription, TransmuteRevive.Build(), false /* unique instance */).AddToDB();
            lifeTransmuter.AddFeatureAtLevel(PowerRevive, 10);

            GuiPresentationBuilder LifeTransmuterExtraPoolBonusGui = new GuiPresentationBuilder(
                "Subclass/&HealingPoolLifeTransmuterBonusExtraDescription",
                "Subclass/&HealingPoolLifeTransmuterBonusExtraTitle");
            FeatureDefinitionPowerPoolModifier TransmuteForceExtraBonus = new FeatureDefinitionPowerPoolModifierBuilder("AttributeModiferTransmuterHealingPoolBonus",
                GuidHelper.Create(SubclassNamespace, "AttributeModiferTransmuterHealingPoolBonus").ToString(),
                4, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Intelligence, TransmuteForce, LifeTransmuterExtraPoolBonusGui.Build()).AddToDB();
            lifeTransmuter.AddFeatureAtLevel(TransmuteForceExtraBonus, 14);

            lifeTransmuter.AddFeatureAtLevel(DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance, 14);

            Subclass = lifeTransmuter.AddToDB();
        }

        private static FeatureDefinitionMagicAffinity BuildMagicAffinityHeightenedList(List<string> spellNames, int levelBonus, string name, GuiPresentation guiPresentation)
        {
            FeatureDefinitionMagicAffinityBuilder builder = new FeatureDefinitionMagicAffinityBuilder(name, GuidHelper.Create(SubclassNamespace, name).ToString(),
                guiPresentation).SetWarList(spellNames, levelBonus);
            return builder.AddToDB();
        }

        private static ConditionDefinition BuildCondition(List<FeatureDefinition> conditionFeatures, RuleDefinitions.DurationType durationType,
            int durationParameter, string name, GuiPresentation guiPresentation)
        {
            ConditionDefinitionBuilder builder = new ConditionDefinitionBuilder(name, GuidHelper.Create(SubclassNamespace, name).ToString(),
                conditionFeatures, durationType, durationParameter, false, guiPresentation);
            return builder.AddToDB();
        }

        private static FeatureDefinitionPowerSharedPool BuildActionTransmuteConditionPower(FeatureDefinitionPower poolPower,
            RuleDefinitions.RechargeRate recharge, RuleDefinitions.ActivationTime activationTime, int costPerUse,
            RuleDefinitions.RangeType rangeType, int rangeParameter, RuleDefinitions.TargetType targetType,
            ActionDefinitions.ItemSelectionType itemSelectionType, RuleDefinitions.DurationType durationType, int durationParameter,
            RuleDefinitions.TurnOccurenceType endOfEffect, string abilityScore, ConditionDefinition condition,
            string name, GuiPresentation guiPresentation)
        {

            EffectDescriptionBuilder effectDescriptionBuilder = new EffectDescriptionBuilder();
            effectDescriptionBuilder.SetTargetingData(RuleDefinitions.Side.Ally, rangeType, rangeParameter, targetType, 1, 0, itemSelectionType);
            effectDescriptionBuilder.SetCreatedByCharacter();
            effectDescriptionBuilder.SetDurationData(durationType, durationParameter, endOfEffect);

            EffectFormBuilder effectFormBuilder = new EffectFormBuilder();
            effectFormBuilder.SetConditionForm(condition, ConditionForm.ConditionOperation.Add, false, false, new List<ConditionDefinition>());

            effectFormBuilder.CreatedByCharacter();

            effectDescriptionBuilder.AddEffectForm(effectFormBuilder.Build());
            effectDescriptionBuilder.SetEffectAdvancement(RuleDefinitions.EffectIncrementMethod.None, 1,
                0, 0, 0, 0, 0, 0, 0, 0, RuleDefinitions.AdvancementDuration.None);

            EffectParticleParameters particleParams = new EffectParticleParameters();
            particleParams.Copy(DatabaseHelper.SpellDefinitions.MagicWeapon.EffectDescription.EffectParticleParameters);
            effectDescriptionBuilder.SetParticleEffectParameters(particleParams);

            FeatureDefinitionPowerSharedPoolBuilder builder = new FeatureDefinitionPowerSharedPoolBuilder(name, GuidHelper.Create(SubclassNamespace, name).ToString(),
                poolPower, recharge, activationTime, costPerUse, false, false, abilityScore,
                effectDescriptionBuilder.Build(), guiPresentation, false /* unique instance */);

            return builder.AddToDB();
        }
    }
}
