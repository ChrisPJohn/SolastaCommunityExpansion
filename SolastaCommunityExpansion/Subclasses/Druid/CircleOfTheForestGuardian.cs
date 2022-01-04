﻿using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.CustomFeatureDefinitions;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModApi.BuilderHelpers;
using System;
using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.SpellDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaCommunityExpansion.Subclasses.Druid
{
    internal class CircleOfTheForestGuardian : AbstractSubclass
    {
        private CharacterSubclassDefinition Subclass;
        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return DatabaseHelper.FeatureDefinitionSubclassChoices.SubclassChoiceDruidCircle;
        }
        internal override CharacterSubclassDefinition GetSubclass()
        {
            if (Subclass == null)
            {
                Subclass = BuildAndAddSubclass();
            }
            return Subclass;
        }

        private const string DruidForestGuardianDruidSubclassName = "DruidForestGuardianDruidSubclass";
        private const string DruidForestGuardianDruidSubclassGuid = "45a7595b-5d5f-4351-b7f1-cb78c9d0a136";

        public static readonly Guid DFG_BASE_GUID = new Guid(DruidForestGuardianDruidSubclassGuid);
        public static readonly FeatureDefinitionAutoPreparedSpells druid_forestGuardian_magic = createDruidForestGuardianMagic();
        public static readonly FeatureDefinitionAttributeModifier extra_attack = createExtraAttack();
        public static readonly FeatureDefinitionAttributeModifier sylvan_resistance = createSylvanDurability();
        public static readonly FeatureDefinitionMagicAffinity sylvan_war_magic = createSylvanWarMagic();
        public static readonly Dictionary<int, FeatureDefinitionPowerSharedPool> bark_ward_dict = CreateBarkWard();

        public static CharacterSubclassDefinition BuildAndAddSubclass()
        {
            var subclassGuiPresentation = new GuiPresentationBuilder(
                    "Subclass/&DruidForestGuardianDruidSubclassDescription",
                    "Subclass/&DruidForestGuardianSubclassTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.MartialMountaineer.GuiPresentation.SpriteReference)
                    .Build();

            var definition = new CharacterSubclassDefinitionBuilder(DruidForestGuardianDruidSubclassName, DruidForestGuardianDruidSubclassGuid)
                    .SetGuiPresentation(subclassGuiPresentation)
                    .AddFeatureAtLevel(druid_forestGuardian_magic, 2)
                    .AddFeatureAtLevel(sylvan_resistance, 2)
                    .AddFeatureAtLevel(sylvan_war_magic, 2)
                    .AddFeatureAtLevel(bark_ward_dict[2], 2)
                    .AddFeatureAtLevel(extra_attack, 6)
                    .AddFeatureAtLevel(bark_ward_dict[10], 10)
                    .AddFeatureAtLevel(bark_ward_dict[14], 14)
                    .AddToDB();

            return definition;
        }
        // Create Auto-prepared Spell list
        private static FeatureDefinitionAutoPreparedSpells createDruidForestGuardianMagic()
        {
            GuiPresentationBuilder forestGuardianMagicGui = new GuiPresentationBuilder(
                "Feature/&DruidForestGuardianMagicDescription",
                "Feature/&DruidForestGuardianMagicTitle");

            FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup ForestGuardianSpells1 = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                ClassLevel = 2,
                SpellsList = new List<SpellDefinition>() { Shield, FogCloud, }
            };
            FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup ForestGuardianSpells2 = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                ClassLevel = 3,
                SpellsList = new List<SpellDefinition>() { Blur, FlameBlade, }
            };
            FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup ForestGuardianSpells3 = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                ClassLevel = 5,
                SpellsList = new List<SpellDefinition>() { ProtectionFromEnergy, DispelMagic, }
            };
            FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup ForestGuardianSpells4 = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                ClassLevel = 7,
                SpellsList = new List<SpellDefinition>() { FireShield, DeathWard, }
            };
            FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup ForestGuardianSpells5 = new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup()
            {
                ClassLevel = 9,
                SpellsList = new List<SpellDefinition>() { HoldMonster, GreaterRestoration, }
            };

            var ForestGuardianSpells = new FeatureDefinitionAutoPreparedSpellsBuilder("ForestGuardianAutoPreparedSpells",
                GuidHelper.Create(DFG_BASE_GUID, "ForestGuardianAutoPreparedSpells").ToString(),
                new List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup>() {
                    ForestGuardianSpells1, ForestGuardianSpells2, ForestGuardianSpells3, ForestGuardianSpells4, ForestGuardianSpells5 },
                forestGuardianMagicGui.Build()).AddToDB();
            ForestGuardianSpells.SetSpellcastingClass(DatabaseHelper.CharacterClassDefinitions.Druid);

            return ForestGuardianSpells;
        }

        // Create Sylvan War Magic
        private static FeatureDefinitionMagicAffinity createSylvanWarMagic()
        {
            GuiPresentationBuilder sylvanWarMagicGui = new GuiPresentationBuilder(
                "Feature/&DruidForestGuardianSylvanWarMagicDescription",
                "Feature/&DruidForestGuardianSylvanWarMagicTitle");

            return new FeatureDefinitionMagicAffinityBuilder(DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityBattleMagic,
                "DruidForestGuardianSylvanWarMagic",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianSylvanWarMagic").ToString(), sylvanWarMagicGui.Build()).AddToDB();
        }

        // Create Sylvan Durability
        private static FeatureDefinitionAttributeModifier createSylvanDurability()
        {
            GuiPresentationBuilder sylvanDurabilityGui = new GuiPresentationBuilder(
               "Feature/&DruidForestGuardianSylvanDurabilityDescription",
               "Feature/&DruidForestGuardianSylvanDurabilityTitle");

            return new FeatureDefinitionAttributeModifierBuilder(
                "AttributeModifierDruidForestGuardianSylvanDurability",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianSylvanDurability").ToString(),
                FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                AttributeDefinitions.HitPointBonusPerLevel,
                1,
                sylvanDurabilityGui.Build()).AddToDB();
        }

        // Create Bark Ward Wild Shape Power (and the two higher variants, improved and superior)
        private static Dictionary<int, FeatureDefinitionPowerSharedPool> CreateBarkWard()
        {
            GuiPresentationBuilder barkWardGui = new GuiPresentationBuilder(
               "Feature/&DruidForestGuardianBarkWardDescription",
               "Feature/&DruidForestGuardianBarkWardTitle")
                .SetSpriteReference(PowerDruidWildShape.GuiPresentation.SpriteReference);

            GuiPresentationBuilder improvedBarkWardGui = new GuiPresentationBuilder(
               "Feature/&DruidForestGuardianImprovedBarkWardDescription",
               "Feature/&DruidForestGuardianImprovedBarkWardTitle")
                .SetSpriteReference(PowerDruidWildShape.GuiPresentation.SpriteReference);

            GuiPresentationBuilder superiorBarkWardGui = new GuiPresentationBuilder(
               "Feature/&DruidForestGuardianSuperiorBarkWardDescription",
               "Feature/&DruidForestGuardianSuperiorBarkWardTitle")
                .SetSpriteReference(PowerDruidWildShape.GuiPresentation.SpriteReference);

            EffectFormBuilder tempHPEffect = new EffectFormBuilder();
            tempHPEffect.SetTempHPForm(4, RuleDefinitions.DieType.D1, 0);
            tempHPEffect.SetLevelAdvancement(EffectForm.LevelApplianceType.Multiply, RuleDefinitions.LevelSourceType.ClassLevel, 1);
            tempHPEffect.CreatedByCharacter();

            EffectFormBuilder barkWardBuff = new EffectFormBuilder();
            barkWardBuff.SetConditionForm(ConditionBarkWardBuilder.GetOrAdd(), ConditionForm.ConditionOperation.Add, true, true, new List<ConditionDefinition>());

            EffectFormBuilder improvedBarkWardBuff = new EffectFormBuilder();
            improvedBarkWardBuff.SetConditionForm(ConditionImprovedBarkWardBuilder.GetOrAdd(), ConditionForm.ConditionOperation.Add, true, true, new List<ConditionDefinition>());

            EffectFormBuilder superiorBarkWardBuff = new EffectFormBuilder();
            superiorBarkWardBuff.SetConditionForm(ConditionSuperiorBarkWardBuilder.GetOrAdd(), ConditionForm.ConditionOperation.Add, true, true, new List<ConditionDefinition>());

            EffectDescriptionBuilder barkWardEffectDescription = new EffectDescriptionBuilder();
            barkWardEffectDescription.SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 1, ActionDefinitions.ItemSelectionType.None);
            barkWardEffectDescription.SetCreatedByCharacter();
            barkWardEffectDescription.SetDurationData(RuleDefinitions.DurationType.Minute, 10, RuleDefinitions.TurnOccurenceType.EndOfTurn);
            barkWardEffectDescription.AddEffectForm(tempHPEffect.Build());
            barkWardEffectDescription.AddEffectForm(barkWardBuff.Build());
            barkWardEffectDescription.SetEffectAdvancement(RuleDefinitions.EffectIncrementMethod.None, 1,
                0, 0, 0, 0, 0, 0, 0, 0, RuleDefinitions.AdvancementDuration.None);

            EffectDescriptionBuilder improvedBarkWardEffectDescription = new EffectDescriptionBuilder();
            improvedBarkWardEffectDescription.SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 1, ActionDefinitions.ItemSelectionType.None);
            improvedBarkWardEffectDescription.SetCreatedByCharacter();
            improvedBarkWardEffectDescription.SetDurationData(RuleDefinitions.DurationType.Minute, 10, RuleDefinitions.TurnOccurenceType.EndOfTurn);
            improvedBarkWardEffectDescription.AddEffectForm(tempHPEffect.Build());
            improvedBarkWardEffectDescription.AddEffectForm(improvedBarkWardBuff.Build());
            improvedBarkWardEffectDescription.SetEffectAdvancement(RuleDefinitions.EffectIncrementMethod.None, 1,
                0, 0, 0, 0, 0, 0, 0, 0, RuleDefinitions.AdvancementDuration.None);

            EffectDescriptionBuilder superiorBarkWardEffectDescription = new EffectDescriptionBuilder();
            superiorBarkWardEffectDescription.SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 1, ActionDefinitions.ItemSelectionType.None);
            superiorBarkWardEffectDescription.SetCreatedByCharacter();
            superiorBarkWardEffectDescription.SetDurationData(RuleDefinitions.DurationType.Minute, 10, RuleDefinitions.TurnOccurenceType.EndOfTurn);
            superiorBarkWardEffectDescription.AddEffectForm(tempHPEffect.Build());
            superiorBarkWardEffectDescription.AddEffectForm(superiorBarkWardBuff.Build());
            superiorBarkWardEffectDescription.SetEffectAdvancement(RuleDefinitions.EffectIncrementMethod.None, 1,
                0, 0, 0, 0, 0, 0, 0, 0, RuleDefinitions.AdvancementDuration.None);

            var barkWard = new FeatureDefinitionPowerSharedPoolBuilder(
                "DruidForestGuardianBarkWard",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianBarkWard").ToString(),
                PowerDruidWildShape,
                RuleDefinitions.RechargeRate.ShortRest,
                RuleDefinitions.ActivationTime.BonusAction,
                1,
                false,
                false,
                AttributeDefinitions.Wisdom,
                barkWardEffectDescription.Build(),
                barkWardGui.Build(),
                true).AddToDB();

            var improvedBarkWard = new FeatureDefinitionPowerSharedPoolBuilder(
                "DruidForestGuardianImprovedBarkWard",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianImprovedBarkWard").ToString(),
                PowerDruidWildShape,
                RuleDefinitions.RechargeRate.ShortRest,
                RuleDefinitions.ActivationTime.BonusAction,
                1,
                false,
                false,
                AttributeDefinitions.Wisdom,
                improvedBarkWardEffectDescription.Build(),
                improvedBarkWardGui.Build(),
                true).AddToDB();
            improvedBarkWard.SetOverriddenPower(barkWard);

            var superiorBarkWard = new FeatureDefinitionPowerSharedPoolBuilder(
                "DruidForestGuardianSuperiorBarkWard",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianSuperiorBarkWard").ToString(),
                PowerDruidWildShape,
                RuleDefinitions.RechargeRate.ShortRest,
                RuleDefinitions.ActivationTime.BonusAction,
                1,
                false,
                false,
                AttributeDefinitions.Wisdom,
                superiorBarkWardEffectDescription.Build(),
                superiorBarkWardGui.Build(),
                true).AddToDB();
            superiorBarkWard.SetOverriddenPower(improvedBarkWard);


            return new Dictionary<int, FeatureDefinitionPowerSharedPool>{
                {2, barkWard},
                {10, improvedBarkWard},
                {14, superiorBarkWard} };
        }

        // Create Extra Attack
        private static FeatureDefinitionAttributeModifier createExtraAttack()
        {
            GuiPresentationBuilder extraAttackGui = new GuiPresentationBuilder(
               "Feature/&DruidForestGuardianExtraAttackDescription",
               "Feature/&DruidForestGuardianExtraAttackTitle");

            return new FeatureDefinitionAttributeModifierBuilder(
                "AttributeModifierDruidForestGuardianExtraAttack",
                GuidHelper.Create(DFG_BASE_GUID, "DruidForestGuardianExtraAttack").ToString(),
                FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                AttributeDefinitions.AttacksNumber,
                1,
                extraAttackGui.Build()).AddToDB();
        }

        // A builder to help us build a custom damage affinity for our Bark Ward conditions
        public class FeatureDefinitionDamageAffinityBuilder : BaseDefinitionBuilder<FeatureDefinitionDamageAffinity>
        {
            public FeatureDefinitionDamageAffinityBuilder(string name, string guid, bool retaliateWhenHit, int retaliationRange,
                FeatureDefinitionPower retaliationPower, RuleDefinitions.DamageAffinityType damageAffinityType, string damageType,
                GuiPresentation guiPresentation) : base(DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityFireShieldWarm, name, guid)
            {
                Definition.SetDamageAffinityType(damageAffinityType);
                Definition.SetDamageType(damageType);
                Definition.SetRetaliateWhenHit(retaliateWhenHit);
                Definition.SetRetaliateRangeCells(retaliationRange);
                Definition.SetRetaliatePower(retaliationPower);
                Definition.SetGuiPresentation(guiPresentation);
                Definition.SetAncestryDefinesDamageType(false);
            }
        }
    }

    // Creates a dedicated builder for the the three Bark Ward conditions
    internal class ConditionBarkWardBuilder : BaseDefinitionBuilder<ConditionDefinition>
    {
        protected ConditionBarkWardBuilder(string name, string guid) : base(DatabaseHelper.ConditionDefinitions.ConditionBarkskin, name, guid)
        {
            Definition.GuiPresentation.Title = "Condition/&ConditionBarkWardTitle";
            Definition.GuiPresentation.Description = "Condition/&ConditionBarkWardDescription";

            Definition.Features.Clear();
            Definition.SetAllowMultipleInstances(false);
            Definition.SetDurationParameter(10);
            Definition.SetDurationType(RuleDefinitions.DurationType.Minute);
            Definition.SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn);
        }

        public static ConditionDefinition CreateAndAddToDB()
            => new ConditionBarkWardBuilder("BarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "BarkWard").ToString()).AddToDB();

        public static ConditionDefinition GetOrAdd()
        {
            var db = DatabaseRepository.GetDatabase<ConditionDefinition>();
            return db.TryGetElement("BarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "BarkWard").ToString()) ?? CreateAndAddToDB();
        }
    }

    internal class ConditionImprovedBarkWardBuilder : BaseDefinitionBuilder<ConditionDefinition>
    {
        private static FeatureDefinitionPower createImprovedBarkWardRetaliate()
        {
            GuiPresentationBuilder improvedBarkWardRetaliateGui = new GuiPresentationBuilder(
                "Feature/&NoContentTitle",
                "Feature/&NoContentTitle");

            EffectFormBuilder damageEffect = new EffectFormBuilder();
            damageEffect.SetDamageForm(false, RuleDefinitions.DieType.D8,
                "DamagePiercing",
                0, RuleDefinitions.DieType.D8,
                2, RuleDefinitions.HealFromInflictedDamage.Never,
                new List<RuleDefinitions.TrendInfo>());
            damageEffect.CreatedByCondition();

            EffectDescriptionBuilder improvedBarkWardRetaliationEffect = new EffectDescriptionBuilder();
            improvedBarkWardRetaliationEffect.AddEffectForm(damageEffect.Build());



            return new FeatureDefinitionPowerBuilder("improvedBarkWardRetaliate",
                GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "improvedBarkWardRetaliate").ToString(),
                0,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Wisdom,
                RuleDefinitions.ActivationTime.NoCost,
                0,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Wisdom,
                improvedBarkWardRetaliationEffect.Build(),
                improvedBarkWardRetaliateGui.Build(),
                true
                ).AddToDB();
        }

        private static FeatureDefinitionDamageAffinity createImprovedBarkWardDamage()
        {
            GuiPresentationBuilder improvedBarkWardDamageGui = new GuiPresentationBuilder(
                "Feature/&NoContentTitle",
                "Feature/&NoContentTitle");

            return new CircleOfTheForestGuardian.FeatureDefinitionDamageAffinityBuilder("ImprovedBarkWardRetaliationDamage",
                GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "ImprovedBarkWardRetaliationDamage").ToString(),
                true,
                1,
                createImprovedBarkWardRetaliate(),
                RuleDefinitions.DamageAffinityType.None,
                RuleDefinitions.DamageTypePoison,
                improvedBarkWardDamageGui.Build()).AddToDB();
        }

        protected ConditionImprovedBarkWardBuilder(string name, string guid) : base(DatabaseHelper.ConditionDefinitions.ConditionBarkskin, name, guid)
        {
            Definition.GuiPresentation.Title = "Condition/&ConditionImprovedBarkWardTitle";
            Definition.GuiPresentation.Description = "Condition/&ConditionImprovedBarkWardDescription";

            Definition.Features.Clear();
            Definition.Features.Add(createImprovedBarkWardDamage());
            Definition.SetAllowMultipleInstances(false);
            Definition.SetDurationParameter(10);
            Definition.SetDurationType(RuleDefinitions.DurationType.Minute);
            Definition.SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn);

        }

        public static ConditionDefinition CreateAndAddToDB()
            => new ConditionImprovedBarkWardBuilder("ImprovedBarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "ImprovedBarkWard").ToString()).AddToDB();

        public static ConditionDefinition GetOrAdd()
        {
            var db = DatabaseRepository.GetDatabase<ConditionDefinition>();
            return db.TryGetElement("ImprovedBarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "ImprovedBarkWard").ToString()) ?? CreateAndAddToDB();
        }
    }

    internal class ConditionSuperiorBarkWardBuilder : BaseDefinitionBuilder<ConditionDefinition>
    {
        private static FeatureDefinitionPower createSuperiorBarkWardRetaliate()
        {
            GuiPresentationBuilder superiorBarkWardRetaliateGui = new GuiPresentationBuilder(
                "Feature/&NoContentTitle",
                "Feature/&NoContentTitle");

            EffectFormBuilder damageEffect = new EffectFormBuilder();
            damageEffect.SetDamageForm(false, RuleDefinitions.DieType.D8,
                "DamagePiercing",
                0, RuleDefinitions.DieType.D8,
                3, RuleDefinitions.HealFromInflictedDamage.Never,
                new List<RuleDefinitions.TrendInfo>());
            damageEffect.CreatedByCondition();

            EffectDescriptionBuilder superiorBarkWardRetaliationEffect = new EffectDescriptionBuilder();
            superiorBarkWardRetaliationEffect.AddEffectForm(damageEffect.Build());

            return new FeatureDefinitionPowerBuilder("superiorBarkWardRetaliate",
                GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "superiorBarkWardRetaliate").ToString(),
                0,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Wisdom,
                RuleDefinitions.ActivationTime.NoCost,
                0,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Wisdom,
                superiorBarkWardRetaliationEffect.Build(),
                superiorBarkWardRetaliateGui.Build(),
                true
                ).AddToDB();
        }

        private static FeatureDefinitionDamageAffinity createSuperiorBarkWardDamage()
        {
            GuiPresentationBuilder superiorBarkWardDamageGui = new GuiPresentationBuilder(
                "Feature/&NoContentTitle",
                "Feature/&NoContentTitle");

            return new CircleOfTheForestGuardian.FeatureDefinitionDamageAffinityBuilder("SuperiorBarkWardRetaliationDamage",
                GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "SuperiorBarkWardRetaliationDamage").ToString(),
                true,
                1,
                createSuperiorBarkWardRetaliate(),
                RuleDefinitions.DamageAffinityType.Immunity,
                RuleDefinitions.DamageTypePoison,
               superiorBarkWardDamageGui.Build()).AddToDB();
        }

        protected ConditionSuperiorBarkWardBuilder(string name, string guid) : base(DatabaseHelper.ConditionDefinitions.ConditionBarkskin, name, guid)
        {
            Definition.GuiPresentation.Title = "Condition/&ConditionSuperiorBarkWardTitle";
            Definition.GuiPresentation.Description = "Condition/&ConditionSuperiorBarkWardDescription";

            Definition.Features.Clear();
            Definition.Features.Add(createSuperiorBarkWardDamage());
            Definition.SetAllowMultipleInstances(false);
            Definition.SetDurationParameter(10);
            Definition.SetDurationType(RuleDefinitions.DurationType.Minute);
            Definition.SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn);

        }

        public static ConditionDefinition CreateAndAddToDB()
            => new ConditionSuperiorBarkWardBuilder("SuperiorBarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "SuperiorBarkWard").ToString()).AddToDB();

        public static ConditionDefinition GetOrAdd()
        {
            var db = DatabaseRepository.GetDatabase<ConditionDefinition>();
            return db.TryGetElement("SuperiorBarkWard", GuidHelper.Create(CircleOfTheForestGuardian.DFG_BASE_GUID, "SuperiorBarkWard").ToString()) ?? CreateAndAddToDB();
        }
    }
}
