﻿using System;
using System.Collections.Generic;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaModApi.Extensions;
using static SolastaModApi.DatabaseHelper;
using static SolastaModApi.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaModApi.DatabaseHelper.ConditionDefinitions;

namespace SolastaCommunityExpansion.Subclasses.Rogue
{
    internal class Opportunist : AbstractSubclass
    {
        private static readonly Guid SubclassNamespace = new("2db81c3c-a9e2-4829-b27b-47af3ba42c76");

        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return FeatureDefinitionSubclassChoices.SubclassChoiceRogueRoguishArchetypes;
        }

        internal override CharacterSubclassDefinition GetSubclass()
        {
            return CreateOpportunist();
        }

        private static void QuickStrikeOnAttackDelegate(GameLocationCharacter attacker, GameLocationCharacter defender, ActionModifier attackModifier, RulesetAttackMode attackerAttackMode)
        {
            // melee attack only
            if (attacker == null || defender == null || attackerAttackMode.Ranged)
            {
                return;
            }

            // grant advatage if attacker is performing an opportunity attack or has higher inititative.
            if (attacker.LastInitiative > defender.LastInitiative || attackerAttackMode.ActionType == ActionDefinitions.ActionType.Reaction && attacker.GetActionStatus(ActionDefinitions.Id.AttackOpportunity, ActionDefinitions.ActionScope.Battle) == ActionDefinitions.ActionStatus.Available)
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, RuleDefinitions.FeatureSourceType.CharacterFeature, "QuickStrike", attacker));
            }
        }

        internal class DebilitatedConditionBuilder : ConditionDefinitionBuilder
        {
            private const string Name = "Debilitated";
            private const string TitleString = "Condition/&DebilitatedConditionTitle";
            private const string DescriptionString = "Condition/&DebilitatedConditionDescription";

            protected DebilitatedConditionBuilder(string name, string guid) : base(ConditionDummy, name, guid)
            {
                Definition.GuiPresentation.Title = TitleString;
                Definition.GuiPresentation.Description = DescriptionString;
            }
            private static ConditionDefinition CreateAndAddToDB(string name, string guid)
            {
                return new DebilitatedConditionBuilder(name, guid).AddToDB();
            }

            internal static readonly ConditionDefinition DebilitatedCondition = CreateAndAddToDB(Name, SubclassNamespace.ToString());
        }

        private static CharacterSubclassDefinition CreateOpportunist()
        {
            var subclassNamespace = new Guid("b217342c-5b1b-46eb-9f2f-86239c3088bf");

            // Grant advantage when attack enemis whos inititative is lower than your
            // or when perform an attack of opportunity.
            var quickStrike = FeatureDefinitionOnAttackEffectBuilder
                .Create("RoguishOppotunistQuickStrike", subclassNamespace)
                .SetGuiPresentation("OpportunistQuickStrike", Category.Feature)
                .SetOnAttackDelegate(QuickStrikeOnAttackDelegate)
                .AddToDB();

            EffectDescriptionBuilder debilitatingStrikeEffectBuilder = new EffectDescriptionBuilder()
                    .SetDurationData(
                        durationType : RuleDefinitions.DurationType.Round,
                        durationParameter: 1,
                        endOfEffect : RuleDefinitions.TurnOccurenceType.EndOfTurn)
                    .SetTargetingData(
                        targetSide : RuleDefinitions.Side.Enemy,
                        rangeType:  RuleDefinitions.RangeType.MeleeHit,
                        rangeParameter: 0, // I think this parameter is irrelevant if range type is meleehit.
                        targetType:  RuleDefinitions.TargetType.Individuals, // allow multiple effect stack ?
                        targetParameter: 0,
                        targetParameter2: 0,
                        itemSelectionType: ActionDefinitions.ItemSelectionType.None)
                    .SetSavingThrowData(
                        hasSavingThrow: true,
                        disableSavingThrowOnAllies: false,
                        savingThrowAbility: SmartAttributeDefinitions.Constitution.name,
                        ignoreCover: true,
                        difficultyClassComputation: RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        savingThrowDifficultyAbility:  SmartAttributeDefinitions.Dexterity.name,
                        fixedSavingThrowDifficultyClass: 20,
                        advantageForEnemies: false,
                        savingThrowAffinitiesBySense: new List<SaveAffinityBySenseDescription>())
                    .AddEffectForm(new EffectFormBuilder()
                        .SetConditionForm(
                            condition: DebilitatedConditionBuilder.DebilitatedCondition,
                            operation: ConditionForm.ConditionOperation.AddRandom,
                            applyToSelf: false,
                            forceOnSelf: false,
                            detrimentalConditions:  new List<ConditionDefinition> { ConditionBlinded, ConditionBaned, ConditionBleeding, ConditionStunned })
                        .HasSavingThrow(RuleDefinitions.EffectSavingThrowType.Negates)
                        .CanSaveToCancel(RuleDefinitions.TurnOccurenceType.EndOfTurn)
                        .Build());

            // Enemies struck by your sneak attack suffered from one of the following condtion (Baned, Blinded, Bleed, Stunned)
            // if they fail a CON save agaisnt the DC of 8 + your DEX mod + your prof.
            var debilitatingStrikePower = FeatureDefinitionConditionalPowerBuilder
                .Create("RoguishOpportunistDebilitatingStrikePower", SubclassNamespace)
                .Configure(
                    1,
                    RuleDefinitions.UsesDetermination.Fixed,
                    AttributeDefinitions.Dexterity,
                    RuleDefinitions.ActivationTime.OnSneakAttackHit,
                    1,
                    RuleDefinitions.RechargeRate.AtWill,
                    false,
                    false,
                    AttributeDefinitions.Dexterity,
                    debilitatingStrikeEffectBuilder.Build()
                )
                .SetGuiPresentation("OpportunistDebilitatingStrike", Category.Feature)
                .AddToDB();

            return CharacterSubclassDefinitionBuilder
                .Create("Opportunist", subclassNamespace)
                .SetGuiPresentation(Category.Subclass, MartialCommander.GuiPresentation.SpriteReference)
                .AddFeatureAtLevel(quickStrike, 3)
                .AddFeatureAtLevel(debilitatingStrikePower, 9)
                //.AddFeatureAtLevel(thugOvercomeCompetition, 13)
                .AddToDB();
        }
    }
}
