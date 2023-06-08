using System.Collections;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Classes;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Subclasses;

public static class InnovationVivisectionist
{
    private const string Name = "InnovationVivisectionist";

    public static CharacterSubclassDefinition Build()
    {
        //
        // MAIN
        //

        // LEVEL 03

        // Auto Prepared Spells

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(InventorClass.Class)
            .SetAutoTag("InventorVivisectionist")
            .AddPreparedSpellGroup(3, Bless, InflictWounds)
            .AddPreparedSpellGroup(5, EnhanceAbility, LesserRestoration)
            .AddPreparedSpellGroup(9, RemoveCurse, Revivify)
            .AddPreparedSpellGroup(13, DeathWard, IdentifyCreatures)
            .AddPreparedSpellGroup(17, Contagion, RaiseDead)
            .AddToDB();

        // Medical Accuracy

        var additionalDamageMedicalAccuracy = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}MedicalAccuracy")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag("MedicalAccuracy")
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 1, 1, 6, 3)
            .SetRequiredProperty(RestrictedContextRequiredProperty.FinesseOrRangeWeapon)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .AddToDB();

        // Emergency Surgery

        var powerEmergencySurgery = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}EmergencySurgery")
            .SetGuiPresentation(Category.Feature, PowerDomainInsightForeknowledge)
            .SetUsesProficiencyBonus(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Instantaneous)
                    .SetTargetingData(Side.Ally, RangeType.Touch, 0, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetHealingForm(
                                HealingComputation.Dice, 0, DieType.D6, 1, false, HealingCap.MaximumHitPoints)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(new ModifyMagicEffectEmergencySurgery())
            .AddToDB();

        // LEVEL 05

        // Extra Attack

        // Emergency Cure

        var powerEmergencyCure = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}EmergencyCure")
            .SetGuiPresentation(Category.Feature, PowerOathOfJugementPurgeCorruption)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .AddToDB();

        var powerEmergencyCureLesserRestoration = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}EmergencyCureLesserRestoration")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.BonusAction, powerEmergencyCure)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(LesserRestoration)
                    .Build())
            .AddToDB();

        var powerEmergencyCureRemoveCurse = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}EmergencyCureRemoveCurse")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.BonusAction, powerEmergencyCure)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(RemoveCurse)
                    .Build())
            .AddToDB();

        PowerBundle.RegisterPowerBundle(powerEmergencyCure, false,
            powerEmergencyCureLesserRestoration, powerEmergencyCureRemoveCurse);

        // LEVEL 09

        // Stable Surgery

        var dieRollModifierStableSurgery = FeatureDefinitionDieRollModifierBuilder
            .Create($"DieRollModifier{Name}StableSurgery")
            .SetGuiPresentation(Category.Feature)
            .SetModifiers(
                RollContext.HealValueRoll,
                0,
                2,
                0,
                $"Feature/&DieRollModifier{Name}StableSurgeryReroll")
            .AddToDB();

        // Organ Donation

        var powerOrganDonation = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}OrganDonation")
            .SetGuiPresentation(Category.Feature)
            .SetUsesProficiencyBonus(ActivationTime.Reaction, RechargeRate.ShortRest)
            .SetReactionContext(ExtraReactionContext.Custom)
            .AddToDB();

        powerOrganDonation.SetCustomSubFeatures(
            new TargetReducedToZeroHpOrganDonation(powerOrganDonation, powerEmergencySurgery, powerEmergencyCure));

        // LEVEL 15

        // Master Emergency Surgery

        var powerMasterEmergencySurgery = FeatureDefinitionPowerBuilder
            .Create(powerEmergencySurgery, $"Power{Name}MasterEmergencySurgery")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetCustomSubFeatures(new ModifyMagicEffectEmergencySurgery())
            .SetOverriddenPower(powerEmergencySurgery)
            .AddToDB();

        // Master Emergency Cure

        var powerMasterEmergencyCure = FeatureDefinitionPowerBuilder
            .Create(powerEmergencyCure, $"Power{Name}MasterEmergencyCure")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetUsesProficiencyBonus(ActivationTime.NoCost)
            .SetOverriddenPower(powerEmergencyCure)
            .AddToDB();

        var powerMasterEmergencyCureLesserRestoration = FeatureDefinitionPowerSharedPoolBuilder
            .Create(powerEmergencyCureLesserRestoration, $"Power{Name}MasterEmergencyCureLesserRestoration")
            .SetSharedPool(ActivationTime.NoCost, powerMasterEmergencyCure)
            .SetOverriddenPower(powerEmergencyCureLesserRestoration)
            .AddToDB();

        var powerMasterEmergencyCureRemoveCurse = FeatureDefinitionPowerSharedPoolBuilder
            .Create(powerEmergencyCureRemoveCurse, $"Power{Name}MasterEmergencyCureRemoveCurse")
            .SetSharedPool(ActivationTime.NoCost, powerMasterEmergencyCure)
            .SetOverriddenPower(powerEmergencyCureRemoveCurse)
            .AddToDB();

        PowerBundle.RegisterPowerBundle(powerMasterEmergencyCure, false,
            powerMasterEmergencyCureLesserRestoration, powerMasterEmergencyCureRemoveCurse);

        // MAIN

        return CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.InventorVivisectionist, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                additionalDamageMedicalAccuracy,
                powerEmergencySurgery)
            .AddFeaturesAtLevel(5,
                AttributeModifierCasterFightingExtraAttack,
                powerEmergencyCure)
            .AddFeaturesAtLevel(9,
                dieRollModifierStableSurgery,
                powerOrganDonation)
            .AddFeaturesAtLevel(15,
                powerMasterEmergencySurgery,
                powerMasterEmergencyCure)
            .AddToDB();
    }

    private sealed class ModifyMagicEffectEmergencySurgery : IModifyMagicEffect
    {
        public EffectDescription ModifyEffect(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            var intelligence = character.TryGetAttributeValue(AttributeDefinitions.Intelligence);
            var intelligenceModifier = AttributeDefinitions.ComputeAbilityScoreModifier(intelligence);
            var levels = character.GetClassLevel(InventorClass.Class);
            var diceNumber = levels switch
            {
                >= 15 => 3,
                >= 9 => 2,
                _ => 1
            };

            var healingForm = effectDescription.EffectForms[0].HealingForm;

            if (healingForm == null)
            {
                return effectDescription;
            }

            healingForm.diceNumber = diceNumber;
            healingForm.bonusHealing = intelligenceModifier;

            return effectDescription;
        }
    }

    private class TargetReducedToZeroHpOrganDonation : ITargetReducedToZeroHp
    {
        private readonly FeatureDefinitionPower _powerEmergencyCure;
        private readonly FeatureDefinitionPower _powerEmergencySurgery;
        private readonly FeatureDefinitionPower _powerOrganDonation;

        public TargetReducedToZeroHpOrganDonation(
            FeatureDefinitionPower powerOrganDonation,
            FeatureDefinitionPower powerEmergencySurgery,
            FeatureDefinitionPower powerEmergencyCure)
        {
            _powerOrganDonation = powerOrganDonation;
            _powerEmergencySurgery = powerEmergencySurgery;
            _powerEmergencyCure = powerEmergencyCure;
        }

        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.GetRemainingPowerCharges(_powerOrganDonation) <= 0)
            {
                yield break;
            }

            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (gameLocationActionService == null || gameLocationBattleService == null)
            {
                yield break;
            }

            var reactionParams = new CharacterActionParams(attacker, (ActionDefinitions.Id)ExtraActionId.DoNothingFree)
            {
                StringParameter = "Reaction/&CustomReactionOrganDonationReactDescription"
            };
            var previousReactionCount = gameLocationActionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("OrganDonation", reactionParams);

            gameLocationActionService.AddInterruptRequest(reactionRequest);

            yield return gameLocationBattleService.WaitForReactions(
                attacker, gameLocationActionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            rulesetAttacker.UpdateUsageForPower(_powerOrganDonation, _powerOrganDonation.CostPerUse);
            GameConsoleHelper.LogCharacterUsedPower(rulesetAttacker, _powerOrganDonation);
            UsablePowersProvider.Get(_powerEmergencyCure, rulesetAttacker).RepayUse();
            UsablePowersProvider.Get(_powerEmergencySurgery, rulesetAttacker).RepayUse();
        }
    }
}
