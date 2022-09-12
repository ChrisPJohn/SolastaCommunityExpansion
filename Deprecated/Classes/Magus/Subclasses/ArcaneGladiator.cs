﻿using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Classes.Magus.Subclasses;

public static class ArcaneGladiator
{
    private static FeatureDefinitionPower BuildArcaneDueler()
    {
        var shamefulRunawayCombatAffinity = FeatureDefinitionCombatAffinityBuilder
            .Create("ClassMagusArcaneGladiatorShamefulRunawayCombatAffinity", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .AddToDB();
        shamefulRunawayCombatAffinity.autoCritical = true;

        var shamefulRunawayCondition = ConditionDefinitionBuilder
            .Create("ClassMagusArcaneGladiatorShamefulRunawayCondition", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Condition)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(shamefulRunawayCombatAffinity)
            .SetSpecialInterruptions(RuleDefinitions.ConditionInterruption.Damaged)
            .AddToDB();

        var disadvantageAgainstNonChallenger =
            FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder
                .Create("ClassMagusArcaneGladiatorDisadvantageAgainstNonChallenger", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(Category.Feature)
                .SetConditionName("ClassMagusArcaneGladiatorDuelled")
                .AddToDB();

        var conditionDuelled = ConditionDefinitionBuilder
            .Create("ClassMagusArcaneGladiatorDuelled", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature,
                DatabaseHelper.ConditionDefinitions.ConditionHeraldOfBattle.guiPresentation.SpriteReference)
            .SetFeatures(disadvantageAgainstNonChallenger)
            .AddToDB();

        var cullTheWeak = FeatureDefinitionOnAttackEffectBuilder
            .Create("ClassMagusArcaneGladiatorCullTheWeak", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetOnAttackDelegates((attacker, defender, outcome, mode) =>
            {
                var character = defender.RulesetCharacter;
                if (!character.HasConditionOfType(conditionDuelled))
                {
                    return;
                }

                var battleManager = ServiceRepository.GetService<IGameLocationBattleService>();
                if (!AttacksOfOpportunity.MovingCharactersCache.TryGetValue(defender.Guid, out var movement) ||
                    !battleManager.CanPerformOpportunityAttackOnCharacter(attacker, defender, movement.Item1,
                        movement.Item2,
                        out _))
                {
                    return;
                }

                character.AddConditionOfCategory(AttributeDefinitions.TagCombat,
                    RulesetCondition.CreateActiveCondition(character.Guid,
                        shamefulRunawayCondition,
                        RuleDefinitions.DurationType.Round,
                        0,
                        RuleDefinitions.TurnOccurenceType.StartOfTurn,
                        attacker.Guid,
                        string.Empty
                    ));
            }, null)
            .AddToDB();

        var conditionDueling = ConditionDefinitionBuilder
            .Create("ClassMagusArcaneGladiatorDueling", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Condition,
                DatabaseHelper.ConditionDefinitions.ConditionHeraldOfBattle.guiPresentation.SpriteReference)
            .SetFeatures(cullTheWeak)
            .SetCustomSubFeatures(AttacksOfOpportunity.CanIgnoreDisengage)
            .AddToDB();

        var effect = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(RuleDefinitions.Side.Enemy, RuleDefinitions.RangeType.Distance, 24,
                RuleDefinitions.TargetType.IndividualsUnique)
            .SetDurationData(RuleDefinitions.DurationType.Minute, 1)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionDuelled, ConditionForm.ConditionOperation.Add)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionDueling, ConditionForm.ConditionOperation.Add, true, false)
                    .Build()
            )
            .Build();


        return FeatureDefinitionPowerBuilder
            .Create("ClassMagusArcaneGladiatorArcaneDuel", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Subclass,
                DatabaseHelper.SpellDefinitions.DispelEvilAndGood.guiPresentation.SpriteReference)
            .SetEffectDescription(effect)
            .SetUsesProficiency()
            .SetCostPerUse(1)
            .SetRechargeRate(RuleDefinitions.RechargeRate.LongRest)
            .SetActivationTime(RuleDefinitions.ActivationTime.BonusAction)
            .AddToDB();
    }

    private static FeatureDefinitionOnAttackEffect BuildHeavyWeaponMastery()
    {
        return FeatureDefinitionOnAttackEffectBuilder
            .Create("ClassMagusArcaneGladiatorHeavyWeaponMastery", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Subclass)
            .SetOnAttackDelegates((attacker, _, outcome, mode) =>
            {
                if (mode == null || attacker == null ||
                    WeaponValidators.IsOneHanded(
                        attacker.RulesetCharacter.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand)) ||
                    !CharacterValidators.MainHandIsMeleeWeapon(attacker.RulesetCharacter))
                {
                    return;
                }

                mode.toHitBonus += 2;
                outcome.AttacktoHitTrends.Add(new RuleDefinitions.TrendInfo(2,
                    RuleDefinitions.FeatureSourceType.CharacterFeature, "HeavyWeaponMastery", null));
            }, null)
            .AddToDB();
    }

    internal static CharacterSubclassDefinition Build()
    {
        // grant bonus attack after casting a spell
        // the attack is similar to the frenzy barb
        var conditionSpellStrikeBonusAttack = ConditionDefinitionBuilder
            .Create("ClassMagusArcaneGladiatorConditionSpellStrikeBonusAttack", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Condition,
                DatabaseHelper.ConditionDefinitions.ConditionBerserkerFrenzy.guiPresentation.SpriteReference)
            .AddFeatures(DatabaseHelper.FeatureDefinitionAttackModifiers.AttackModifierBerserkerFrenzy)
            .AddToDB();

        var spellStrikeBonusAttackEffect = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(RuleDefinitions.Side.Enemy, RuleDefinitions.RangeType.Self, 0,
                RuleDefinitions.TargetType.Self)
            .SetDurationData(RuleDefinitions.DurationType.Round, 0, false)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionSpellStrikeBonusAttack, ConditionForm.ConditionOperation.Add)
                    .Build()
            )
            .Build();
        spellStrikeBonusAttackEffect.canBePlacedOnCharacter = true;
        spellStrikeBonusAttackEffect.targetExcludeCaster = false;

        var spellStrikeBonusAttack = FeatureDefinitionPowerBuilder
            .Create("ClassMagusArcaneGladiatorPowerSpellStrikeBonusAttack", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Subclass)
            .SetRechargeRate(RuleDefinitions.RechargeRate.AtWill)
            .SetEffectDescription(spellStrikeBonusAttackEffect)
            .SetActivationTime(RuleDefinitions.ActivationTime.OnSpellCast)
            .AddToDB();

        return CharacterSubclassDefinitionBuilder
            .Create("MagusSubclassArcaneGladiator", DefinitionBuilder.CENamespaceGuid)
            .SetOrUpdateGuiPresentation(Category.Subclass,
                DatabaseHelper.CharacterSubclassDefinitions.OathOfTheMotherland.GuiPresentation.SpriteReference)
            .AddFeaturesAtLevel(1, DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyDomainLifeArmor)
            .AddFeaturesAtLevel(3, BuildHeavyWeaponMastery())
            .AddFeaturesAtLevel(7, BuildArcaneDueler())
            .AddFeaturesAtLevel(11, spellStrikeBonusAttack)
            .AddToDB();
    }
}
