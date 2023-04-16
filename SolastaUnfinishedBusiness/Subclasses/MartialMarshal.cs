﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using UnityEngine;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.DecisionPackageDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionConditionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMoveModes;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSummoningAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using Resources = SolastaUnfinishedBusiness.Properties.Resources;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class MartialMarshal : AbstractSubclass
{
    private const string FeatureSetMarshalKnowYourEnemyName = "FeatureSetMarshalKnowYourEnemy";

    private const string MarshalCoordinatedAttackName = "ReactToAttackFinishedMarshalCoordinatedAttack";

    private const string EternalComradeName = "MarshalEternalComrade";

    private const string ConditionMarshalKnowledgeableDefenseACName = "ConditionMarshalKnowledgeableDefenseAC";

    internal MartialMarshal()
    {
        BuildEternalComradeMonster();

        var conditionEncourage = BuildEncourageCondition();
        var powerMarshalEncourage = BuildEncourage(conditionEncourage);
        var powerMarshalImprovedEncourage = BuildImprovedEncourage(conditionEncourage, powerMarshalEncourage);

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("MartialMarshal")
            .SetGuiPresentation(Category.Subclass,
                Sprites.GetSprite("MartialMarshal", Resources.MartialMarshal, 256))
            .AddFeaturesAtLevel(3,
                BuildMarshalCoordinatedAttack(),
                BuildFeatureSetMarshalKnowYourEnemyFeatureSet(),
                BuildStudyEnemyPower())
            .AddFeaturesAtLevel(7,
                BuildFeatureSetMarshalEternalComrade())
            .AddFeaturesAtLevel(10,
                BuildFeatureSetMarshalFearlessCommander(),
                powerMarshalEncourage)
            .AddFeaturesAtLevel(15,
                powerMarshalImprovedEncourage)
            .AddFeaturesAtLevel(18,
                BuildKnowledgeableDefense())
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private static int GetKnowledgeLevelOfEnemy(RulesetCharacter enemy)
    {
        return ServiceRepository.GetService<IGameLoreService>().Bestiary.TryGetBestiaryEntry(enemy, out var entry)
            ? entry.KnowledgeLevelDefinition.Level
            : 0;
    }

    private static FeatureDefinitionFeatureSet BuildFeatureSetMarshalKnowYourEnemyFeatureSet()
    {
        var onComputeAttackModifierMarshalKnowYourEnemy = FeatureDefinitionBuilder
            .Create("OnComputeAttackModifierMarshalKnowYourEnemy")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(new OnComputeAttackModifierMarshalKnowYourEnemy())
            .AddToDB();

        return FeatureDefinitionFeatureSetBuilder
            .Create(FeatureSetMarshalKnowYourEnemyName)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                onComputeAttackModifierMarshalKnowYourEnemy,
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
                CommonBuilders.AdditionalDamageMarshalFavoredEnemyHumanoid
            )
            .AddToDB();
    }

    private static FeatureDefinitionPower BuildStudyEnemyPower()
    {
        var sprite = Sprites.GetSprite("PowerStudyYourEnemy", Resources.PowerStudyYourEnemy, 256, 128);

        return FeatureDefinitionPowerBuilder
            .Create("PowerMarshalStudyYourEnemy")
            .SetGuiPresentation(Category.Feature, sprite)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest, 1, 2)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(IdentifyCreatures.EffectDescription)
                .SetDurationData(DurationType.Instantaneous)
                .ClearRestrictedCreatureFamilies()
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(ConditionDefinitionBuilder
                        .Create("ConditionMarshalStudyYourEnemy")
                        .SetGuiPresentationNoContent(true)
                        .SetCustomSubFeatures(new StudyYourEnemy())
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .AddToDB(), ConditionForm.ConditionOperation.Add)
                    .Build())
                .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.Individuals)
                .Build())
            .AddToDB();
    }

    private static FeatureDefinition BuildMarshalCoordinatedAttack()
    {
        return FeatureDefinitionBuilder
            .Create(MarshalCoordinatedAttackName)
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(new ReactToAttackFinishedMarshalCoordinatedAttack())
            .AddToDB();
    }

    private static void BuildEternalComradeMonster()
    {
        _ = MonsterDefinitionBuilder
            .Create(SuperEgo_Servant_Hostile, EternalComradeName)
            .SetOrUpdateGuiPresentation(Category.Monster)
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
            .SetAttackIterations(new MonsterAttackIteration(
                MonsterAttackDefinitionBuilder
                    .Create(MonsterAttackDefinitions.Attack_Generic_Guard_Longsword,
                        "MonsterAttackMarshalEternalComrade")
                    .SetToHitBonus(5)
                    .SetEffectDescription(EffectDescriptionBuilder
                        .Create()
                        .SetAnimationMagicEffect(AnimationDefinitions.AnimationMagicEffect.Count)
                        .SetEffectForms(EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeSlashing, 2, DieType.D6, 2)
                            .Build())
                        .Build())
                    .SetMagical()
                    .AddToDB(),
                1))
            .SetArmorClass(16)
            .SetAlignment(MonsterDefinitionBuilder.NeutralAlignment)
            .SetCharacterFamily(CharacterFamilyDefinitions.Undead.Name)
            .SetCreatureTags(EternalComradeName)
            .SetDefaultBattleDecisionPackage(DefaultMeleeWithBackupRangeDecisions)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction(FactionDefinitions.Party)
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .AddToDB();
    }

    private static FeatureDefinitionFeatureSet BuildFeatureSetMarshalEternalComrade()
    {
        var sprite = Sprites.GetSprite("PowerMarshalSummonEternalComrade",
            Resources.PowerMarshalSummonEternalComrade, 256, 128);

        var powerMarshalSummonEternalComrade = FeatureDefinitionPowerBuilder
            .Create("PowerMarshalSummonEternalComrade")
            .SetGuiPresentation(Category.Feature, sprite)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest, 2)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(ConjureAnimalsOneBeast.EffectDescription)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetSummonCreatureForm(1, EternalComradeName)
                        .Build())
                .Build())
            .AddToDB();

        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.Familiar, powerMarshalSummonEternalComrade);

        var hpBonus = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierMarshalEternalComradeHP")
            .SetGuiPresentationNoContent(true)
            .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddConditionAmount,
                AttributeDefinitions.HitPoints)
            .AddToDB();

        var conditionMarshalEternalComradeAc = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondAC, "ConditionMarshalEternalComradeAC")
            .SetGuiPresentationNoContent(true)
            .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var conditionMarshalEternalComradeSavingThrow = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondSavingThrows, "ConditionMarshalEternalComradeSavingThrow")
            .SetGuiPresentationNoContent(true)
            .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var conditionMarshalEternalComradeDamage = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondMeleeDamage, "ConditionMarshalEternalComradeDamage")
            .SetGuiPresentationNoContent(true)
            .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var conditionMarshalEternalComradeHit = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondMeleeAttack, "ConditionMarshalEternalComradeHit")
            .SetGuiPresentationNoContent(true)
            .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
            .AddToDB();

        var conditionMarshalEternalComradeHp = ConditionDefinitionBuilder
            .Create(ConditionKindredSpiritBondHP, "ConditionMarshalEternalComradeHP")
            .SetGuiPresentationNoContent(true)
            .SetAmountOrigin(ExtraOriginOfAmount.SourceClassLevel, FighterClass)
            .SetFeatures(hpBonus, hpBonus) // 2 HP per level
            .AddToDB();

        var summoningAffinityMarshalEternalComrade = FeatureDefinitionSummoningAffinityBuilder
            .Create(SummoningAffinityKindredSpiritBond, "SummoningAffinityMarshalEternalComrade")
            .ClearEffectForms()
            .SetRequiredMonsterTag(EternalComradeName)
            .SetAddedConditions(
                conditionMarshalEternalComradeAc,
                conditionMarshalEternalComradeSavingThrow,
                conditionMarshalEternalComradeDamage,
                conditionMarshalEternalComradeHit,
                conditionMarshalEternalComradeHp)
            .AddToDB();

        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetMarshalEternalComrade")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(summoningAffinityMarshalEternalComrade, powerMarshalSummonEternalComrade)
            .AddToDB();
    }

    private static FeatureDefinitionFeatureSet BuildFeatureSetMarshalFearlessCommander()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetMarshalFearlessCommander")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(ConditionAffinityFrightenedImmunity)
            .AddToDB();
    }

    private static ConditionDefinition BuildEncourageCondition()
    {
        return ConditionDefinitionBuilder
            .Create("ConditionMarshalEncouraged")
            .SetGuiPresentation("PowerMarshalEncouragement", Category.Feature, ConditionBlessed)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionCombatAffinitys.CombatAffinityBlessed,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityConditionBlessed)
            .AddToDB();
    }

    private static FeatureDefinitionPower BuildEncourage(ConditionDefinition conditionEncourage)
    {
        return FeatureDefinitionPowerBuilder
            .Create("PowerMarshalEncouragement")
            .SetGuiPresentation(Category.Feature, Bless)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Cube, 5)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionEncourage, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetShowCasting(false)
            .AddToDB();
    }

    private static FeatureDefinitionPower BuildImprovedEncourage(
        ConditionDefinition conditionEncourage, FeatureDefinitionPower overridenPower)
    {
        return FeatureDefinitionPowerBuilder
            .Create("PowerMarshalImprovedEncouragement")
            .SetGuiPresentation(Category.Feature, Bless)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Cube, 13)
                .SetDurationData(DurationType.Permanent)
                .SetRecurrentEffect(
                    RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionEncourage, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetOverriddenPower(overridenPower)
            .SetShowCasting(false)
            .AddToDB();
    }

    private static FeatureDefinition BuildKnowledgeableDefense()
    {
        for (var i = 1; i <= 3; i++)
        {
            _ = ConditionDefinitionBuilder
                .Create($"ConditionMarshalKnowledgeableDefenseAC{i}")
                .SetGuiPresentationNoContent(true)
                .SetSpecialDuration(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
                .AddFeatures(
                    FeatureDefinitionAttributeModifierBuilder
                        .Create($"AttributeModifierKnowledgeableDefenseAC{i}")
                        .SetGuiPresentation(GuiPresentationBuilder.EmptyString,
                            $"Feature/&AttributeModifierACPlus{i}Description")
                        .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                            AttributeDefinitions.ArmorClass, i)
                        .AddToDB())
                .AddToDB();
        }

        var featureMarshalKnowledgeableDefense = FeatureDefinitionBuilder
            .Create("FeatureMarshalKnowledgeableDefense")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureMarshalKnowledgeableDefense.SetCustomSubFeatures(
            new DefenderBeforeAttackHitConfirmedKnowledgeableDefense(featureMarshalKnowledgeableDefense));

        return featureMarshalKnowledgeableDefense;
    }

    private sealed class ReactToAttackFinishedMarshalCoordinatedAttack : IReactToMyAttackFinished
    {
        public IEnumerator HandleReactToMyAttackFinished(
            GameLocationCharacter me,
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
                    || !rulesetCharacterMonster.MonsterDefinition.CreatureTags.Contains(EternalComradeName))
                {
                    continue;
                }

                if (!rulesetCharacterMonster.TryGetConditionOfCategoryAndType(AttributeDefinitions.TagConjure,
                        RuleDefinitions.ConditionConjuredCreature,
                        out var activeCondition)
                    || activeCondition.SourceGuid != me.Guid)
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
                .Where(partyCharacter =>
                    partyCharacter.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) == 0 &&
                    !partyCharacter.Prone)
                .Where(partyCharacter => partyCharacter != me));

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
                    attackParams.FillForPhysicalRangeAttack(partyCharacter, partyCharacter.LocationPosition,
                        allAttackMode,
                        defender, defender.LocationPosition, actionModifierBefore);
                }
                else
                {
                    attackParams.FillForPhysicalReachAttack(partyCharacter, partyCharacter.LocationPosition,
                        allAttackMode,
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
                    StringParameter2 = MarshalCoordinatedAttackName, BoolParameter4 = !canAttack
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

            yield return battleManager.WaitForReactions(me, actionService, count);
        }
    }

    private sealed class OnComputeAttackModifierMarshalKnowYourEnemy : IOnComputeAttackModifier
    {
        public void ComputeAttackModifier(
            RulesetCharacter myself,
            RulesetCharacter defender,
            BattleDefinitions.AttackProximity attackProximity,
            RulesetAttackMode attackMode,
            ref ActionModifier attackModifier)
        {
            if (attackProximity != BattleDefinitions.AttackProximity.PhysicalRange &&
                attackProximity != BattleDefinitions.AttackProximity.PhysicalReach)
            {
                return;
            }

            var knowledgeLevelOfEnemy = GetKnowledgeLevelOfEnemy(defender);

            attackModifier.attackRollModifier += knowledgeLevelOfEnemy;
            attackModifier.attackToHitTrends.Add(new TrendInfo(knowledgeLevelOfEnemy,
                FeatureSourceType.CharacterFeature, "Feature/&FeatureSetMarshalKnowYourEnemyTitle", null));
        }
    }

    private sealed class StudyYourEnemy : ICustomConditionFeature
    {
        public void ApplyFeature(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            var gameLoreService = ServiceRepository.GetService<IGameLoreService>();
            var gameLocationCharacter = GameLocationCharacter.GetFromActor(target);

            if (gameLocationCharacter.RulesetCharacter is not RulesetCharacterMonster creature)
            {
                return;
            }

            var sourceCharacter = EffectHelpers.GetCharacterByGuid(rulesetCondition.sourceGuid);

            if (sourceCharacter == null)
            {
                return;
            }

            if (!gameLoreService.Bestiary.TryGetBestiaryEntry(creature, out var entry)
                && (creature.MonsterDefinition.BestiaryEntry == BestiaryDefinitions.BestiaryEntry.Full
                    || (creature.MonsterDefinition.BestiaryEntry == BestiaryDefinitions.BestiaryEntry.Reference &&
                        creature.MonsterDefinition.BestiaryReference.BestiaryEntry ==
                        BestiaryDefinitions.BestiaryEntry.Full)))
            {
                entry = gameLoreService.Bestiary.AddNewMonsterEntry(creature.MonsterDefinition);
                gameLoreService.MonsterKnowledgeChanged?.Invoke(creature.MonsterDefinition,
                    entry.KnowledgeLevelDefinition);
            }

            if (entry == null)
            {
                return;
            }

            var checkModifier = new ActionModifier();
            var roller = GameLocationCharacter.GetFromActor(sourceCharacter);

            roller.RollAbilityCheck(AttributeDefinitions.Wisdom, SkillDefinitions.Survival,
                10 + Mathf.FloorToInt(entry.MonsterDefinition.ChallengeRating), AdvantageType.None, checkModifier,
                false, -1, out var outcome, out _, true);

            const int MAX_KNOWLEDGE_LEVEL = 4;

            var level = entry.KnowledgeLevelDefinition.Level;

            // rollback one level to at least get a nice message on UI and not get a null after we call Invoke
            if (level == MAX_KNOWLEDGE_LEVEL)
            {
                level--;
                entry.KnowledgeLevelDefinition.level--;
            }

            var newLevel = level;

            if (outcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
            {
                var learned = outcome == RollOutcome.Success ? 1 : 2;

                newLevel = Mathf.Min(entry.KnowledgeLevelDefinition.Level + learned, 4);

                gameLoreService.LearnMonsterKnowledge(entry.MonsterDefinition,
                    gameLoreService.Bestiary.SortedKnowledgeLevels[newLevel]);
            }

            gameLocationCharacter.RulesetCharacter.MonsterIdentificationRolled?.Invoke(
                gameLocationCharacter.RulesetCharacter, entry.MonsterDefinition, outcome, level, newLevel);
        }

        public void RemoveFeature(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
        }
    }

    private class DefenderBeforeAttackHitConfirmedKnowledgeableDefense : IDefenderBeforeAttackHitConfirmed
    {
        private readonly FeatureDefinition _featureDefinition;

        public DefenderBeforeAttackHitConfirmedKnowledgeableDefense(FeatureDefinition featureDefinition)
        {
            _featureDefinition = featureDefinition;
        }

        public IEnumerator DefenderBeforeAttackHitConfirmed(
            GameLocationBattleManager battle,
            GameLocationCharacter attacker,
            GameLocationCharacter me,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget)
        {
            var gameLoreService = ServiceRepository.GetService<IGameLoreService>();

            if (gameLoreService == null)
            {
                yield break;
            }

            var rulesetMe = me.RulesetCharacter;
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetMe == null || rulesetAttacker == null)
            {
                yield break;
            }

            if (!gameLoreService.Bestiary.TryGetBestiaryEntry(rulesetAttacker, out var gameBestiaryEntry) ||
                gameBestiaryEntry.KnowledgeLevelDefinition == null)
            {
                yield break;
            }

            var level = Math.Min(gameBestiaryEntry.KnowledgeLevelDefinition.Level, 3);
            var condition = GetDefinition<ConditionDefinition>($"{ConditionMarshalKnowledgeableDefenseACName}{level}");

            var rulesetCondition = RulesetCondition.CreateActiveCondition(
                rulesetMe.Guid,
                condition,
                condition.DurationType,
                condition.DurationParameter,
                condition.TurnOccurence,
                rulesetMe.Guid,
                rulesetMe.CurrentFaction.Name);

            rulesetMe.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
            GameConsoleHelper.LogCharacterUsedFeature(rulesetMe, _featureDefinition);
        }
    }
}
