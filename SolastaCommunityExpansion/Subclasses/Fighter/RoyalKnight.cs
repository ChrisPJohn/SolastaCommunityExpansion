﻿using System;
using SolastaCommunityExpansion.Builders;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Subclasses.Fighter
{
    internal class RoyalKnight : AbstractSubclass
    {
        private static readonly Guid SubclassNamespace = new Guid("f5efd735-ff95-4256-ad17-dde585aeb4e2");
        private readonly CharacterSubclassDefinition Subclass;

        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return DatabaseHelper.FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
        }
        internal override CharacterSubclassDefinition GetSubclass()
        {
            return Subclass;
        }

        internal RoyalKnight()
        {
            GuiPresentationBuilder royalKnightPresentation = new GuiPresentationBuilder("Subclass/&FighterRoyalKnightDescription", "Subclass/&FighterRoyalKnightTitle")
                .SetSpriteReference(DatabaseHelper.FightingStyleDefinitions.Protection.GuiPresentation.SpriteReference);

            Subclass = new CharacterSubclassDefinitionBuilder("FighterRoyalKnight", GuidHelper.Create(SubclassNamespace, "FighterRoyalKnight").ToString())
                .SetGuiPresentation(royalKnightPresentation.Build())
                .AddFeatureAtLevel(RallyingCryPowerBuilder.RallyingCryPower, 3)
                .AddFeatureAtLevel(RoyalEnvoyFeatureBuilder.RoyalEnvoyFeatureSet, 7)
                .AddFeatureAtLevel(InspiringSurgePowerBuilder.InspiringSurgePower, 10)
                .AddToDB();
        }

        internal class RoyalEnvoyAbilityCheckAffinityBuilder : BaseDefinitionBuilder<FeatureDefinitionAbilityCheckAffinity>
        {
            private const string RoyalEnvoyAbilityCheckName = "RoyalEnvoyAbilityCheckAffinity";
            private const string RoyalEnvoyAbilityCheckGuid = "b16f8b68-0dab-49e5-b1a2-6fdfd8836849";

            protected RoyalEnvoyAbilityCheckAffinityBuilder(string name, string guid) : base(DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityChampionRemarkableAthlete, name, guid)
            {
                Definition.AffinityGroups.Clear();
                Definition.AffinityGroups.Add(new FeatureDefinitionAbilityCheckAffinity.AbilityCheckAffinityGroup()
                {
                    abilityScoreName = "Charisma",
                    affinity = RuleDefinitions.CharacterAbilityCheckAffinity.HalfProficiencyWhenNotProficient
                });
            }

            public static FeatureDefinitionAbilityCheckAffinity CreateAndAddToDB(string name, string guid)
                => new RoyalEnvoyAbilityCheckAffinityBuilder(name, guid).AddToDB();

            public static FeatureDefinitionAbilityCheckAffinity RoyalEnvoyAbilityCheckAffinity
                => CreateAndAddToDB(RoyalEnvoyAbilityCheckName, RoyalEnvoyAbilityCheckGuid);
        }

        public class RoyalEnvoyFeatureBuilder : BaseDefinitionBuilder<FeatureDefinitionFeatureSet>
        {
            private const string RoyalEnvoyFeatureName = "RoyalEnvoyFeature";
            private const string RoyalEnvoyFeatureGuid = "c8299685-d806-4e20-aff0-ca3dd4000e05";

            protected RoyalEnvoyFeatureBuilder(string name, string guid) : base(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetChampionRemarkableAthlete, name, guid)
            {
                Definition.GuiPresentation.Title = "Feature/&RoyalEnvoyFeatureTitle";
                Definition.GuiPresentation.Description = "Feature/&RoyalEnvoyFeatureDescription";
                Definition.FeatureSet.Clear();
                Definition.FeatureSet.Add(RoyalEnvoyAbilityCheckAffinityBuilder.RoyalEnvoyAbilityCheckAffinity);
                Definition.FeatureSet.Add(DatabaseHelper.FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfSolasta);
            }

            public static FeatureDefinitionFeatureSet CreateAndAddToDB(string name, string guid)
                 => new RoyalEnvoyFeatureBuilder(name, guid).AddToDB();

            public static FeatureDefinitionFeatureSet RoyalEnvoyFeatureSet
                => CreateAndAddToDB(RoyalEnvoyFeatureName, RoyalEnvoyFeatureGuid);
        }

        public class RallyingCryPowerBuilder : BaseDefinitionBuilder<FeatureDefinitionPower>
        {
            private const string RallyingCryPowerName = "RallyingCryPower";
            private const string RallyingCryPowerGuid = "cabe94a7-7e51-4231-ae6d-e8e6e3954611";

            protected RallyingCryPowerBuilder(string name, string guid) : base(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLifePreserveLife, name, guid)
            {
                Definition.SetOverriddenPower(DatabaseHelper.FeatureDefinitionPowers.PowerFighterSecondWind);
                Definition.SetActivationTime(RuleDefinitions.ActivationTime.BonusAction);
                Definition.SetRechargeRate(RuleDefinitions.RechargeRate.None);
                Definition.SetAbilityScore("Charisma");
                Definition.SetUsesAbilityScoreName("Charisma");

                SetupGUI();

                EffectDescription effectDescription = new EffectDescription();
                effectDescription.Copy(Definition.EffectDescription);
                effectDescription.EffectForms[0].HealingForm.HealingCap = RuleDefinitions.HealingCap.MaximumHitPoints;
                effectDescription.EffectForms[0].HealingForm.DiceNumber = 4;
                Definition.SetEffectDescription(effectDescription);
            }

            private void SetupGUI()
            {
                Definition.GuiPresentation.Title = "Feature/&RallyingCryPowerTitle";
                Definition.GuiPresentation.Description = "Feature/&RallyingCryPowerDescription";
                Definition.SetShortTitleOverride("Feature/&RallyingCryPowerTitleShort");
                Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.SpellDefinitions.HealingWord.GuiPresentation.SpriteReference);
            }

            public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
                => new RallyingCryPowerBuilder(name, guid).AddToDB();

            public static FeatureDefinitionPower RallyingCryPower
                => CreateAndAddToDB(RallyingCryPowerName, RallyingCryPowerGuid);
        }

        internal class InspiringSurgePowerBuilder : BaseDefinitionBuilder<FeatureDefinitionPower>
        {
            private const string InspiringSurgePowerName = "InspiringSurgePower";
            private const string InspiringSurgePowerNameGuid = "c2930ad2-dd02-4ff3-bad8-46d93e328fbd";

            protected InspiringSurgePowerBuilder(string name, string guid) : base(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLifePreserveLife, name, guid)
            {
                Definition.SetActivationTime(RuleDefinitions.ActivationTime.BonusAction);
                Definition.SetRechargeRate(RuleDefinitions.RechargeRate.LongRest);

                SetupGUI();

                EffectDescription effectDescription = new EffectDescription();

                effectDescription.Copy(Definition.EffectDescription);
                effectDescription.SetTargetType(RuleDefinitions.TargetType.Individuals);
                effectDescription.SetTargetParameter(1);
                effectDescription.SetTargetParameter2(2);
                effectDescription.SetTargetSide(RuleDefinitions.Side.Ally);
                effectDescription.SetCanBePlacedOnCharacter(true);
                effectDescription.SetTargetFilteringMethod(RuleDefinitions.TargetFilteringMethod.CharacterOnly);
                effectDescription.DurationType = RuleDefinitions.DurationType.Round;
                effectDescription.SetRequiresVisibilityForPosition(true);
                effectDescription.SetRangeType(RuleDefinitions.RangeType.Distance);
                effectDescription.SetRangeParameter(20);

                effectDescription.EffectForms.Clear();

                foreach (EffectForm effectForm in DatabaseHelper.FeatureDefinitionPowers.PowerFighterActionSurge.EffectDescription.EffectForms)
                {
                    effectDescription.EffectForms.Add(effectForm);
                }

                Definition.SetEffectDescription(effectDescription);
            }

            private void SetupGUI()
            {
                Definition.GuiPresentation.Title = "Feature/&InspiringSurgePowerTitle";
                Definition.GuiPresentation.Description = "Feature/&InspiringSurgePowerDescription";
                Definition.SetShortTitleOverride("Feature/&InspiringSurgePowerTitleShort");
                Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.SpellDefinitions.Heroism.GuiPresentation.SpriteReference);
            }

            public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
                => new InspiringSurgePowerBuilder(name, guid).AddToDB();

            public static FeatureDefinitionPower InspiringSurgePower
                => CreateAndAddToDB(InspiringSurgePowerName, InspiringSurgePowerNameGuid);
        }
    }
}
