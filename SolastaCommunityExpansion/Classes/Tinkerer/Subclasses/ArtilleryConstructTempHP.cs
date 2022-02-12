﻿using HarmonyLib;
using SolastaCommunityExpansion.Builders;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Classes.Tinkerer.Subclasses
{
    //*****************************************************************************************************************************************
    //***********************************		TempHPShieldBuilder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class TempHPShieldBuilder : DefinitionBuilder<FeatureDefinitionPower>
    {
        private const string TempHPShieldName = "TempHPShield";
        private const string TempHPShieldGuid = "9ca27524-0b49-479e-b11d-085e00e77b8f";

        protected TempHPShieldBuilder(string name, string guid) : base(ThunderShieldBuilder.ThunderShield, name, guid)
        {
            Definition.GuiPresentation.Title = "Feature/&TempHPShieldTitle";
            Definition.SetShortTitleOverride("Feature/&TempHPShieldTitle");
            Definition.GuiPresentation.Description = "Feat/&TempHPShieldDescription";
            Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.SpellDefinitions.Aid.GuiPresentation.SpriteReference);

            Definition.SetActivationTime(RuleDefinitions.ActivationTime.Action);
            Definition.SetRechargeRate(RuleDefinitions.RechargeRate.AtWill);

            TemporaryHitPointsForm tempHPShield = new TemporaryHitPointsForm
            {
                DieType = RuleDefinitions.DieType.D8,
                DiceNumber = 1,
                // to stand in for ability bonus
                BonusHitPoints = 4
            };

            EffectForm effect = new EffectForm
            {
                FormType = EffectForm.EffectFormType.TemporaryHitPoints
            };
            effect.SetTemporaryHitPointsForm(tempHPShield);
            effect.SetCreatedByCharacter(true);

            effect.AddBonusMode = RuleDefinitions.AddBonusMode.AbilityBonus;
            effect.SetLevelMultiplier(1);
            effect.SetLevelType(RuleDefinitions.LevelSourceType.EffectLevel);
            effect.SetApplyLevel(EffectForm.LevelApplianceType.No);

            Definition.EffectDescription.EffectAdvancement.Clear();
            Definition.EffectDescription.EffectForms.Clear();
            Definition.EffectDescription.EffectForms.Add(effect);
            Definition.EffectDescription.SetTargetType(RuleDefinitions.TargetType.Sphere);
            Definition.EffectDescription.SetTargetParameter(2);
            Definition.EffectDescription.SetTargetSide(RuleDefinitions.Side.Ally);
            Definition.EffectDescription.SetRangeParameter(2);
            Definition.EffectDescription.HasSavingThrow = false;
            Definition.EffectDescription.SavingThrowAbility = DatabaseHelper.SmartAttributeDefinitions.Dexterity.Name;

            Definition.EffectDescription.SetCreatedByCharacter(true);
            Definition.EffectDescription.SetCanBePlacedOnCharacter(true);
            Definition.EffectDescription.SetRangeType(RuleDefinitions.RangeType.Self);
            Definition.EffectDescription.SetEffectParticleParameters(DatabaseHelper.SpellDefinitions.Shield.EffectDescription.EffectParticleParameters);
        }

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
        {
            return new TempHPShieldBuilder(name, guid).AddToDB();
        }

        public static readonly FeatureDefinitionPower TempHPShield = CreateAndAddToDB(TempHPShieldName, TempHPShieldGuid);
    }
    //*****************************************************************************************************************************************
    //***********************************		TempHPShieldConstructBuilder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class TempHPShieldConstructBuilder : DefinitionBuilder<MonsterDefinition>
    {
        private const string TempHPShieldConstructName = "TempHPShieldConstruct";
        private const string TempHPShieldConstructGuid = "65223373-24a2-4596-b778-75e6f197b73f";

        protected TempHPShieldConstructBuilder(string name, string guid) : base(DatabaseHelper.MonsterDefinitions.Magic_Mouth, name, guid)
        {
            // cant use set, need to copy individual parts of presentation
            //Definition.SetMonsterPresentation(DatabaseHelper.MonsterDefinitions.CubeOfLight.MonsterPresentation);

            Definition.GuiPresentation.Title = "Feat/&TempHPShieldConstructTitle";
            Definition.GuiPresentation.Description = "Feat/&TempHPShieldConstructDescription";
            Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.MonsterDefinitions.Magic_Mouth.GuiPresentation.SpriteReference);

            Definition.MonsterPresentation.SetHasMonsterPortraitBackground(true);
            Definition.MonsterPresentation.SetCanGeneratePortrait(true);
            Definition.MonsterPresentation.SetCustomShaderReference(DatabaseHelper.MonsterDefinitions.KindredSpiritBear.MonsterPresentation.CustomShaderReference);
            Definition.MonsterPresentation.SetOverrideCharacterShaderColors(true);
            Definition.MonsterPresentation.SetFirstCharacterShaderColor(DatabaseHelper.MonsterDefinitions.KindredSpiritBear.MonsterPresentation.FirstCharacterShaderColor);
            Definition.MonsterPresentation.SetSecondCharacterShaderColor(DatabaseHelper.MonsterDefinitions.KindredSpiritBear.MonsterPresentation.SecondCharacterShaderColor);

            Definition.SetArmorClass(18);
            Definition.SetNoExperienceGain(true);
            Definition.SetHitDice(3);
            Definition.SetHitDiceType(RuleDefinitions.DieType.D8);

            Definition.AbilityScores.Empty();
            Definition.AbilityScores.AddToArray(10);    // STR
            Definition.AbilityScores.AddToArray(10);    // DEX
            Definition.AbilityScores.AddToArray(10);    // CON
            Definition.AbilityScores.AddToArray(10);     // INT
            Definition.AbilityScores.AddToArray(10);    // WIS
            Definition.AbilityScores.AddToArray(10);     // CHA

            Definition.SetFullyControlledWhenAllied(true);
            Definition.SetDungeonMakerPresence(MonsterDefinition.DungeonMaker.None);
            Definition.SetStandardHitPoints(15);
            Definition.SetDefaultFaction("Party");
            Definition.SetCharacterFamily(TinkererConstructFamilyBuilder.TinkererConstructFamily.Name);
            //

            Definition.Features.Clear();
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionSenses.SenseNormalVision);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionMoveModes.MoveModeMove2);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionMovementAffinitys.MovementAffinityJump);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityCharmImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityExhaustionImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityFrightenedImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityBlindnessImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityDiseaseImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityGrappledImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityParalyzedmmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityPetrifiedImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionConditionAffinitys.ConditionAffinityUnconsciousImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityPoisonImmunity);
            Definition.Features.Add(DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityPsychicImmunity);

            Definition.Features.Add(TempHPShieldBuilder.TempHPShield);

            Definition.CreatureTags.Add("ScalingTinkererArtilleryConstruct");
        }

        public static MonsterDefinition CreateAndAddToDB(string name, string guid)
        {
            return new TempHPShieldConstructBuilder(name, guid).AddToDB();
        }

        public static readonly MonsterDefinition TempHPShieldConstruct = CreateAndAddToDB(TempHPShieldConstructName, TempHPShieldConstructGuid);
    }

    //*****************************************************************************************************************************************
    //***********************************		TempHPShieldConstruct_9Builder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class TempHPShieldConstruct9Builder : DefinitionBuilder<MonsterDefinition>
    {
        private const string TempHPShieldConstruct_9Name = "TempHPShieldConstruct_9";
        private const string TempHPShieldConstruct_9Guid = "75f8541a-65c3-4226-9c42-a80dd76b04cd";

        protected TempHPShieldConstruct9Builder(string name, string guid) : base(TempHPShieldConstructBuilder.TempHPShieldConstruct, name, guid)
        {
            Definition.GuiPresentation.Title = "Feat/&TempHPShieldConstructTitle_3";

            Definition.Features.Add(SelfDestructBuilder.SelfDestruct);
        }

        public static MonsterDefinition CreateAndAddToDB(string name, string guid)
        {
            return new TempHPShieldConstruct9Builder(name, guid).AddToDB();
        }

        public static readonly MonsterDefinition TempHPShieldConstruct9 = CreateAndAddToDB(TempHPShieldConstruct_9Name, TempHPShieldConstruct_9Guid);
    }

    //*****************************************************************************************************************************************
    //***********************************		TempHPShieldConstruct_15Builder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class TempHPShieldConstruct15Builder : DefinitionBuilder<MonsterDefinition>
    {
        private const string TempHPShieldConstruct_15Name = "TempHPShieldConstruct_15";
        private const string TempHPShieldConstruct_15Guid = "243d5f04-2106-4c20-a3f2-38484ecc345c";

        protected TempHPShieldConstruct15Builder(string name, string guid) : base(TempHPShieldConstruct9Builder.TempHPShieldConstruct9, name, guid)
        {
            Definition.GuiPresentation.Title = "Feat/&TempHPShieldConstructTitle_5";

            Definition.Features.Add(HalfCoverShieldBuilder.HalfCoverShield);
        }

        public static MonsterDefinition CreateAndAddToDB(string name, string guid)
        {
            return new TempHPShieldConstruct15Builder(name, guid).AddToDB();
        }

        public static readonly MonsterDefinition TempHPShieldConstruct15 = CreateAndAddToDB(TempHPShieldConstruct_15Name, TempHPShieldConstruct_15Guid);
    }

    //*****************************************************************************************************************************************
    //***********************************		SummonTempHPShieldSpellConstructBuilder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class SummonTempHPShieldSpellConstructBuilder : DefinitionBuilder<SpellDefinition>
    {
        private const string SummonTempHPShieldConstructName = "SummonTempHPShieldConstruct";
        private const string SummonTempHPShieldConstructGuid = "db9e7e8e-b749-4b46-9ba3-60a7bf221b0b";

        protected SummonTempHPShieldSpellConstructBuilder(string name, string guid) : base(DatabaseHelper.SpellDefinitions.DancingLights, name, guid)
        {
            Definition.GuiPresentation.Title = "Feature/&TempHPShieldModePowerTitle";
            Definition.GuiPresentation.Description = "Feature/&TempHPShieldModePowerDescription";
            Definition.GuiPresentation.SetSpriteReference(DatabaseHelper.SpellDefinitions.Aid.GuiPresentation.SpriteReference);

            Definition.SetSpellLevel(1);
            Definition.SetRequiresConcentration(false);
            Definition.SetUniqueInstance(true);
            Definition.SetCastingTime(RuleDefinitions.ActivationTime.Action);

            Definition.SetEffectDescription(ArtilleryConstructlevel03FeatureSetBuilder.TempHPShield_03modepower.EffectDescription);
        }

        public static SpellDefinition CreateAndAddToDB(string name, string guid)
        {
            return new SummonTempHPShieldSpellConstructBuilder(name, guid).AddToDB();
        }

        public static readonly SpellDefinition SummonTempHPShieldConstruct = CreateAndAddToDB(SummonTempHPShieldConstructName, SummonTempHPShieldConstructGuid);
    }

    //*****************************************************************************************************************************************
    //***********************************		SummonTempHPShieldSpellConstruct_9Builder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class SummonTempHPShieldSpellConstruct9Builder : DefinitionBuilder<SpellDefinition>
    {
        private const string SummonTempHPShieldConstruct_9Name = "SummonTempHPShieldConstruct_9";
        private const string SummonTempHPShieldConstruct_9Guid = "f1e88575-40ca-4f4e-9447-616058e213a4";

        protected SummonTempHPShieldSpellConstruct9Builder(string name, string guid) : base(SummonTempHPShieldSpellConstructBuilder.SummonTempHPShieldConstruct, name, guid)
        {
            Definition.GuiPresentation.Title = "Feature/&TempHPShield_09ModePowerTitle";
            Definition.GuiPresentation.Description = "Feature/&TempHPShield_09ModePowerDescription";

            Definition.EffectDescription.EffectForms[0].SummonForm.SetMonsterDefinitionName(TempHPShieldConstruct9Builder.TempHPShieldConstruct9.Name);
        }

        public static SpellDefinition CreateAndAddToDB(string name, string guid)
        {
            return new SummonTempHPShieldSpellConstruct9Builder(name, guid).AddToDB();
        }

        public static readonly SpellDefinition SummonTempHPShieldConstruct_9 = CreateAndAddToDB(SummonTempHPShieldConstruct_9Name, SummonTempHPShieldConstruct_9Guid);
    }

    //*****************************************************************************************************************************************
    //***********************************		SummonTempHPShieldSpellConstruct_15Builder		*******************************************************************
    //*****************************************************************************************************************************************

    internal class SummonTempHPShieldSpellConstruct15Builder : DefinitionBuilder<SpellDefinition>
    {
        private const string SummonTempHPShieldConstruct_15Name = "SummonTempHPShieldConstruct_15";
        private const string SummonTempHPShieldConstruct_15Guid = "84ddce96-ec58-4141-933d-371080d611d2";

        protected SummonTempHPShieldSpellConstruct15Builder(string name, string guid) : base(SummonTempHPShieldSpellConstructBuilder.SummonTempHPShieldConstruct, name, guid)
        {
            Definition.GuiPresentation.Title = "Feature/&TempHPShield_15ModePowerTitle";
            Definition.GuiPresentation.Description = "Feature/&TempHPShield_15ModePowerDescription";
            Definition.SetUniqueInstance(false);
            Definition.EffectDescription.EffectForms[0].SummonForm.SetMonsterDefinitionName(TempHPShieldConstruct15Builder.TempHPShieldConstruct15.Name);
        }

        public static SpellDefinition CreateAndAddToDB(string name, string guid)
        {
            return new SummonTempHPShieldSpellConstruct15Builder(name, guid).AddToDB();
        }

        public static readonly SpellDefinition SummonTempHPShieldConstruct15 = CreateAndAddToDB(SummonTempHPShieldConstruct_15Name, SummonTempHPShieldConstruct_15Guid);
    }
}
