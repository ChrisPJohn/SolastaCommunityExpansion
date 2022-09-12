﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionConditionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterAttackDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMoveModes;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSummoningAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.DecisionPackageDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses.Fighter;

internal sealed class MartialMarshal : AbstractSubclass
{
    // ReSharper disable once InconsistentNaming
    private CharacterSubclassDefinition Subclass;

    internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
    {
        return FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
    }

    internal override CharacterSubclassDefinition GetSubclass()
    {
        return Subclass ??= MarshalFighterSubclassBuilder.BuildAndAddSubclass();
    }
}

internal static class FeatureSetKnowYourEnemyBuilder
{
    private static int GetKnowledgeLevelOfEnemy(RulesetCharacter enemy)
    {
        return ServiceRepository.GetService<IGameLoreService>().Bestiary.TryGetBestiaryEntry(enemy, out var entry)
            ? entry.KnowledgeLevelDefinition.Level
            : 0;
    }

    private static void FeatureSetKnowYourEnemyComputeAttackModifier(
        RulesetCharacter myself,
        RulesetCharacter defender,
        RulesetAttackMode attackMode,
        ref ActionModifier attackModifier)
    {
        // no spell attack
        if (attackMode == null || defender == null)
        {
            return;
        }

        var knowledgeLevelOfEnemy = GetKnowledgeLevelOfEnemy(defender);
        attackModifier.attackRollModifier += knowledgeLevelOfEnemy;
        attackModifier.attackToHitTrends.Add(new TrendInfo(knowledgeLevelOfEnemy,
            FeatureSourceType.CharacterFeature, "FeatureSetKnowYourEnemy", null));
    }

    internal static FeatureDefinitionFeatureSet BuildFeatureSetKnowYourEnemyFeatureSet()
    {
        var knowYourEnemiesAttackHitModifier = FeatureDefinitionOnComputeAttackModifierBuilder
            .Create("OnComputeAttackModifierKnowYourEnemy",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation("FighterMarshalFeatureSetKnowYourEnemyFeatureSet", Category.Feature)
            .SetOnComputeAttackModifierDelegate(FeatureSetKnowYourEnemyComputeAttackModifier)
            .AddToDB();

        var additionalDamageRangerFavoredEnemyHumanoid = FeatureDefinitionAdditionalDamageBuilder
            .Create("AdditionalDamageMarshalFavoredEnemyHumanoid",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetNotificationTag("FavoredEnemy")
            .SetTriggerCondition(AdditionalDamageTriggerCondition.SpecificCharacterFamily)
            .SetDamageValueDetermination(AdditionalDamageValueDetermination.TargetKnowledgeLevel)
            .SetAdditionalDamageType(AdditionalDamageType.SameAsBaseDamage)
            .AddToDB();

        additionalDamageRangerFavoredEnemyHumanoid.requiredCharacterFamily = CharacterFamilyDefinitions.Humanoid;

        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetKnowYourEnemy", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation("FighterMarshalFeatureSetKnowYourEnemyFeatureSet", Category.Feature)
            .AddFeatureSet(
                knowYourEnemiesAttackHitModifier,
                AdditionalDamageRangerFavoredEnemyAberration,
                AdditionalDamageRangerFavoredEnemyBeast,
                AdditionalDamageRangerFavoredEnemyCelestial,
                AdditionalDamageRangerFavoredEnemyConstruct,
                AdditionalDamageRangerFavoredEnemyDragon,
                AdditionalDamageRangerFavoredEnemyElemental,
                AdditionalDamageRangerFavoredEnemyFey,
                AdditionalDamageRangerFavoredEnemyFiend,
                AdditionalDamageRangerFavoredEnemyGiant,
                AdditionalDamageRangerFavoredEnemyMonstrosity,
                AdditionalDamageRangerFavoredEnemyOoze,
                AdditionalDamageRangerFavoredEnemyPlant,
                AdditionalDamageRangerFavoredEnemyUndead,
                additionalDamageRangerFavoredEnemyHumanoid
            )
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .AddToDB();
    }
}

internal static class PowerMarshalStudyYourEnemyBuilder
{
    public static FeatureDefinitionPower BuildStudyEnemyPower()
    {
        var effectDescription = IdentifyCreatures.EffectDescription
            .Copy()
            .SetDuration(DurationType.Instantaneous)
            .SetHasSavingThrow(false)
            .SetRange(RangeType.Distance, 12)
            .SetTargetType(TargetType.Individuals)
            .SetTargetSide(Side.Enemy)
            .SetTargetParameter(1)
            .ClearRestrictedCreatureFamilies()
            .SetEffectForms(new StudyEnemyEffectDescription());

        return FeatureDefinitionPowerBuilder
            .Create("PowerMarshalStudyYourEnemy", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature,
                IdentifyCreatures.GuiPresentation.SpriteReference)
            .SetFixedUsesPerRecharge(2)
            .SetCostPerUse(1)
            .SetRechargeRate(RechargeRate.ShortRest)
            .SetActivation(ActivationTime.BonusAction, 0)
            .SetEffectDescription(effectDescription)
            .AddToDB();
    }

    private sealed class StudyEnemyEffectDescription : CustomEffectForm
    {
        public override void ApplyForm(
            RulesetImplementationDefinitions.ApplyFormsParams formsParams,
            bool retargeting,
            bool proxyOnly,
            bool forceSelfConditionOnly,
            EffectApplication effectApplication = EffectApplication.All,
            [CanBeNull] List<EffectFormFilter> filters = null)
        {
            var manager = ServiceRepository.GetService<IGameLoreService>();

            var gameLocationCharacter = GameLocationCharacter.GetFromActor(formsParams.targetCharacter);

            if (gameLocationCharacter.RulesetCharacter is not RulesetCharacterMonster creature)
            {
                return;
            }

            if (!manager.Bestiary.TryGetBestiaryEntry(creature, out var entry)
                && (creature.MonsterDefinition.BestiaryEntry == BestiaryDefinitions.BestiaryEntry.Full
                    || (creature.MonsterDefinition.BestiaryEntry == BestiaryDefinitions.BestiaryEntry.Reference &&
                        creature.MonsterDefinition.BestiaryReference.BestiaryEntry ==
                        BestiaryDefinitions.BestiaryEntry.Full)))
            {
                entry = manager.Bestiary.AddNewMonsterEntry(creature.MonsterDefinition);
                manager.MonsterKnowledgeChanged?.Invoke(creature.MonsterDefinition, entry.KnowledgeLevelDefinition);
            }

            if (entry == null)
            {
                return;
            }

            var checkModifier = new ActionModifier();
            var roller = GameLocationCharacter.GetFromActor(formsParams.sourceCharacter);

            roller.RollAbilityCheck(AttributeDefinitions.Wisdom, SkillDefinitions.Survival,
                10 + Mathf.FloorToInt(entry.MonsterDefinition.ChallengeRating), AdvantageType.None, checkModifier,
                false, -1, out var outcome, out _, true);

            var level = entry.KnowledgeLevelDefinition.Level;
            var num = level;

            if (outcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
            {
                var num2 = outcome == RollOutcome.Success ? 1 : 2;
                num = Mathf.Min(entry.KnowledgeLevelDefinition.Level + num2, 4);
                manager.LearnMonsterKnowledge(entry.MonsterDefinition, manager.Bestiary.SortedKnowledgeLevels[num]);
            }

            gameLocationCharacter.RulesetCharacter.MonsterIdentificationRolled?.Invoke(
                gameLocationCharacter.RulesetCharacter, entry.MonsterDefinition, outcome, level, num);
        }

        public override void FillTags(Dictionary<string, TagsDefinitions.Criticity> tagsMap)
        {
        }
    }
}

internal static class MarshalCoordinatedAttackBuilder
{
    private static IEnumerator MarshalCoordinatedAttackOnAttackHitDelegate(
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        RollOutcome outcome,
        CharacterActionParams actionParams,
        [NotNull] RulesetAttackMode attackMode,
        ActionModifier attackModifier)
    {
        // melee only
        if (attackMode.ranged || outcome is RollOutcome.CriticalFailure or RollOutcome.Failure ||
            actionParams.actionDefinition.Id == ActionDefinitions.Id.AttackOpportunity)
        {
            yield break;
        }

        var characterService = ServiceRepository.GetService<IGameLocationCharacterService>();
        var gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();
        var battleManager = gameLocationBattleService as GameLocationBattleManager;
        var allies = new List<GameLocationCharacter>();

        foreach (var guestCharacter in characterService.GuestCharacters)
        {
            if (guestCharacter.RulesetCharacter is not RulesetCharacterMonster rulesetCharacterMonster
                || !rulesetCharacterMonster.MonsterDefinition.CreatureTags.Contains("MarshalEternalComrade"))
            {
                continue;
            }

            if (!rulesetCharacterMonster.TryGetConditionOfCategoryAndType("17TagConjure",
                    RuleDefinitions.ConditionConjuredCreature,
                    out var activeCondition)
                || activeCondition.SourceGuid != attacker.Guid)
            {
                continue;
            }

            var gameLocationMonster = GameLocationCharacter.GetFromActor(rulesetCharacterMonster);

            if (gameLocationMonster.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) ==
                ActionDefinitions.ActionStatus.Available
                && !gameLocationMonster.Prone)
            {
                allies.Add(gameLocationMonster);
            }
        }

        allies.AddRange(characterService.PartyCharacters
            .Where(partyCharacter => partyCharacter.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) == 0 &&
                                     !partyCharacter.Prone)
            .Where(partyCharacter => partyCharacter != attacker));

        var reactions = new List<CharacterActionParams>();

        foreach (var partyCharacter in allies)
        {
            var allAttackMode = partyCharacter.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
            var attackParams = default(BattleDefinitions.AttackEvaluationParams);
            var actionModifierBefore = new ActionModifier();
            var canAttack = true;

            if (allAttackMode == null)
            {
                continue;
            }

            if (allAttackMode.Ranged)
            {
                attackParams.FillForPhysicalRangeAttack(partyCharacter, partyCharacter.LocationPosition, allAttackMode,
                    defender, defender.LocationPosition, actionModifierBefore);
            }
            else
            {
                attackParams.FillForPhysicalReachAttack(partyCharacter, partyCharacter.LocationPosition, allAttackMode,
                    defender, defender.LocationPosition, actionModifierBefore);
            }

            if (!gameLocationBattleService.CanAttack(attackParams))
            {
                canAttack = false;
                var cantrips = ReactionRequestWarcaster.GetValidCantrips(battleManager, partyCharacter, defender);
                if (cantrips == null || cantrips.Empty())
                {
                    continue;
                }
            }

            var reactionParams = new CharacterActionParams(partyCharacter, ActionDefinitions.Id.AttackOpportunity,
                allAttackMode, defender, actionModifierBefore)
            {
                StringParameter2 = "MarshalCoordinatedAttack", BoolParameter4 = !canAttack
            };

            reactions.Add(reactionParams);
        }

        if (reactions.Empty() || battleManager == null)
        {
            yield break;
        }

        var actionService = ServiceRepository.GetService<IGameLocationActionService>();
        var count = actionService.PendingReactionRequestGroups.Count;

        foreach (var reaction in reactions)
        {
            actionService.ReactForOpportunityAttack(reaction);
        }

        yield return battleManager.WaitForReactions(attacker, actionService, count);
    }

    internal static FeatureDefinition BuildMarshalCoordinatedAttack()
    {
        return FeatureDefinitionBuilder
            .Create("MarshalCoordinatedAttack", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(new ReactToAttackFinished(MarshalCoordinatedAttackOnAttackHitDelegate))
            .AddToDB();
    }
}

internal static class EternalComradeBuilder
{
    private static readonly MonsterDefinition EternalComrade = BuildEternalComradeMonster();

    private static MonsterDefinition BuildEternalComradeMonster()
    {
        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectForms(
                new EffectForm
                {
                    FormType = EffectForm.EffectFormType.Damage,
                    DamageForm = new DamageForm
                    {
                        BonusDamage = 2, DiceNumber = 2, DieType = DieType.D6, DamageType = DamageTypeSlashing
                    }
                }
            )
            .Build();
        effectDescription.SetAnimationMagicEffect(AnimationDefinitions.AnimationMagicEffect.Count);

        var eternalComradeAttack = MonsterAttackDefinitionBuilder
            .Create(
                Attack_Generic_Guard_Longsword,
                "AttackMarshalEternalComrade",
                DefinitionBuilder.CENamespaceGuid)
            .SetEffectDescription(effectDescription)
            .AddToDB();

        eternalComradeAttack.magical = true;

        var marshalEternalComradeAttackInteraction = new MonsterAttackIteration(eternalComradeAttack, 1);

        return MonsterDefinitionBuilder
            .Create(
                SuperEgo_Servant_Hostile,
                "MarshalEternalComrade",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(
                Category.Feature,
                SuperEgo_Servant_Hostile.GuiPresentation.SpriteReference)
            .ClearFeatures()
            .SetFeatures(
                SenseNormalVision,
                SenseDarkvision24,
                MoveModeMove10,
                MoveModeFly10,
                ActionAffinityFightingStyleProtection,
                ConditionAffinityCharmImmunity,
                ConditionAffinityExhaustionImmunity,
                ConditionAffinityFrightenedImmunity,
                ConditionAffinityGrappledImmunity,
                ConditionAffinityParalyzedmmunity,
                ConditionAffinityPetrifiedImmunity,
                ConditionAffinityPoisonImmunity,
                ConditionAffinityProneImmunity,
                ConditionAffinityRestrainedmmunity,
                DamageAffinityColdImmunity,
                DamageAffinityNecroticImmunity,
                DamageAffinityFireResistance,
                DamageAffinityBludgeoningResistance,
                DamageAffinityPiercingResistance,
                DamageAffinitySlashingResistance,
                DamageAffinityFireResistance,
                DamageAffinityAcidResistance,
                DamageAffinityLightningResistance,
                DamageAffinityThunderResistance,
                ConditionAffinityHinderedByFrostImmunity
            )
            .SetAttackIterations(marshalEternalComradeAttackInteraction)
            .SetArmorClass(16)
            .SetAlignment(AlignmentDefinitions.Neutral.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Undead.name)
            .SetCreatureTags("MarshalEternalComrade")
            .SetDefaultBattleDecisionPackage(DefaultMeleeWithBackupRangeDecisions)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .AddToDB();
    }

    internal static FeatureDefinitionFeatureSet BuildFeatureSetMarshalEternalComrade()
    {
        var summonForm = new SummonForm { monsterDefinitionName = EternalComrade.Name };

        var effectForm = new EffectForm
        {
            formType = EffectForm.EffectFormType.Summon, createdByCharacter = true, summonForm = summonForm
        };

        // TODO: make this use concentration and reduce the duration to may be 3 rounds
        // TODO: increase the number of use to 2 and recharge per long rest
        var summonEternalComradePower = FeatureDefinitionPowerBuilder
            .Create("PowerMarshalSummonEternalComrade", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature,
                Bane.GuiPresentation.SpriteReference)
            .SetCostPerUse(1)
            .SetUsesFixed(1)
            .SetFixedUsesPerRecharge(1)
            .SetRechargeRate(RechargeRate.ShortRest)
            .SetActivationTime(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(ConjureAnimalsOneBeast.EffectDescription.Copy())
                    .SetDurationData(DurationType.Round, 10)
                    .ClearEffectForms()
                    .AddEffectForm(effectForm)
                    .Build()
            )
            .AddToDB();
        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.Familiar, summonEternalComradePower);

        var acConditionDefinition = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondAC, "ConditionMarshalEternalComradeAC",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetAmountOrigin((ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var stConditionDefinition = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondSavingThrows, "ConditionMarshalEternalComradeSavingThrow",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetAmountOrigin((ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var damageConditionDefinition = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondMeleeDamage, "ConditionMarshalEternalComradeDamage",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetAmountOrigin((ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var hitConditionDefinition = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondMeleeAttack, "ConditionMarshalEternalComradeHit",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetAmountOrigin((ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var hpConditionDefinition = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondHP, "ConditionMarshalEternalComradeHP",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentationNoContent()
            .SetAmountOrigin((ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceClassLevel)
            .SetAllowMultipleInstances(true)
            .AddToDB();

        // Find a better place to put this in?
        hpConditionDefinition.additionalDamageType = "Fighter";

        var summoningAffinity = FeatureDefinitionSummoningAffinityBuilder
            .Create(
                SummoningAffinityKindredSpiritBond,
                "SummoningAffinityMarshalEternalComrade",
                DefinitionBuilder.CENamespaceGuid)
            .ClearEffectForms()
            .SetRequiredMonsterTag("MarshalEternalComrade")
            .SetAddedConditions(
                acConditionDefinition, stConditionDefinition, damageConditionDefinition,
                hitConditionDefinition, hpConditionDefinition, hpConditionDefinition)
            .AddToDB();

        return FeatureDefinitionFeatureSetBuilder
            .Create(
                "FeatureSetMarshalEternalComrade",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(
                Category.Feature)
            .SetFeatureSet(summoningAffinity, summonEternalComradePower)
            .AddToDB();
    }
}

internal static class FeatureSetMarshalFearlessCommanderBuilder
{
    internal static FeatureDefinitionFeatureSet BuildFeatureSetMarshalFearlessCommander()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetMarshalFearlessCommander", DefinitionBuilder.CENamespaceGuid)
            .SetFeatureSet(ConditionAffinityFrightenedImmunity)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetGuiPresentation(Category.Feature)
            .AddToDB();
    }
}

internal static class EncourageBuilder
{
    internal static FeatureDefinitionPower BuildEncourage()
    {
        var conditionEncouraged = ConditionDefinitionBuilder
            .Create(
                "ConditionMarshalEncouraged",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(
                "PowerMarshalEncouragement",
                Category.Feature,
                ConditionBlessed.GuiPresentation.SpriteReference)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionCombatAffinitys.CombatAffinityBlessed,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityConditionBlessed)
            .SetTurnOccurence(TurnOccurenceType.StartOfTurn)
            .AddToDB();

        conditionEncouraged.ConditionTags.Add("Buff");

        // this allows the condition to still display as a label on character panel
        Global.CharacterLabelEnabledConditions.Add(conditionEncouraged);

        var effect = EffectDescriptionBuilder
            .Create()
            .SetCreatedByCharacter()
            .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Cube, 5, 2)
            .SetDurationData(DurationType.Permanent)
            .SetRecurrentEffect(RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .CreatedByCharacter()
                    .SetConditionForm(conditionEncouraged, ConditionForm.ConditionOperation.Add, false, false)
                    .Build()
            ).Build();

        effect.SetCanBePlacedOnCharacter(true);

        return FeatureDefinitionPowerBuilder
            .Create("PowerMarshalEncouragement", DefinitionBuilder.CENamespaceGuid)
            .Configure(-1, UsesDetermination.Fixed, AttributeDefinitions.Charisma,
                ActivationTime.PermanentUnlessIncapacitated, 1,
                RechargeRate.AtWill, false, false, AttributeDefinitions.Charisma, effect)
            .SetShowCasting(false)
            .SetGuiPresentation(Category.Feature,
                Bless.GuiPresentation.SpriteReference)
            .AddToDB();
    }
}

internal static class MarshalFighterSubclassBuilder
{
    private const string MarshalFighterSubclassName = "MartialMarshal";

    private static readonly FeatureDefinition MarshalCoordinatedAttack =
        MarshalCoordinatedAttackBuilder.BuildMarshalCoordinatedAttack();

    private static readonly FeatureDefinitionFeatureSet KnowYourEnemies =
        FeatureSetKnowYourEnemyBuilder.BuildFeatureSetKnowYourEnemyFeatureSet();

    private static readonly FeatureDefinitionPower PowerMarshalStudyYourEnemy =
        PowerMarshalStudyYourEnemyBuilder.BuildStudyEnemyPower();

    private static readonly FeatureDefinitionFeatureSet FeatureSetMarshalFearlessCommander =
        FeatureSetMarshalFearlessCommanderBuilder.BuildFeatureSetMarshalFearlessCommander();

    private static readonly FeatureDefinitionPower Encourage = EncourageBuilder.BuildEncourage();

    private static readonly FeatureDefinitionFeatureSet EternalComrade =
        EternalComradeBuilder.BuildFeatureSetMarshalEternalComrade();

    internal static CharacterSubclassDefinition BuildAndAddSubclass()
    {
        return CharacterSubclassDefinitionBuilder
            .Create(MarshalFighterSubclassName, DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Subclass, OathOfJugement.GuiPresentation.SpriteReference)
            .AddFeatureAtLevel(MarshalCoordinatedAttack, 3)
            .AddFeatureAtLevel(KnowYourEnemies, 3)
            .AddFeatureAtLevel(PowerMarshalStudyYourEnemy, 3)
            .AddFeatureAtLevel(EternalComrade, 7)
            .AddFeatureAtLevel(FeatureSetMarshalFearlessCommander, 10)
            .AddFeatureAtLevel(Encourage, 10)
            .AddToDB();
    }
}
