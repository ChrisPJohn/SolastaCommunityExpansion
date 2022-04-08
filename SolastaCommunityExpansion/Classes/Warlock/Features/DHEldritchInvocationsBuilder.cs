﻿using System;
using System.Collections.Generic;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaCommunityExpansion.Builders;
using UnityEngine;
using SolastaCommunityExpansion.Builders.Features;

namespace SolastaCommunityExpansion.Classes.Warlock.Features
{
    public static class EldritchInvocationsBuilder
    {
        public static Dictionary<string, SpellDefinition> DictionaryofEIPseudoCantrips { get; private set; } = new();
        public static Dictionary<string, SpellDefinition> DictionaryofEIPseudoSpells { get; private set; } = new();

        public static Dictionary<string, FeatureDefinitionPower> DictionaryofEIPowers { get; private set; } = new();

        public static SpellDefinition EldritchBlast { get; set; }
        public static List<string> ListofEBImprovements { get; private set; } = new();
        public static Dictionary<string, FeatureDefinitionBonusCantrips> DictionaryofEBInvocations { get; private set; } = new();
        public static List<string> ListofEIAttributeModifers { get; private set; } = new();
        public static Dictionary<string, FeatureDefinitionFeatureSet> DictionaryofEIAttributeModifers { get; private set; } = new();
        public static FeatureDefinitionFeatureSet AgonizingBlastFeatureSet { get; set; }
        public static FeatureDefinitionFeatureSet HinderingBlastFeatureSet { get; set; }

        public static void Build()
        {
            SpellsToCantripsForEldritchInvocations();
            EldritchBlastAndEBInvocations();
            EIAttributeModifers();
        }

        private static void SpellsToCantripsForEldritchInvocations()
        {
            //at will
            DictionaryofEIPseudoCantrips.Add("ArmorofShadows", DatabaseHelper.SpellDefinitions.MageArmor);// self
            DictionaryofEIPseudoCantrips.Add("EldritchSight", DatabaseHelper.SpellDefinitions.DetectMagic);
            DictionaryofEIPseudoCantrips.Add("FiendishVigor", DatabaseHelper.SpellDefinitions.FalseLife);// self
            DictionaryofEIPseudoCantrips.Add("AscendantStep", DatabaseHelper.SpellDefinitions.Levitate);// self
            DictionaryofEIPseudoCantrips.Add("OtherworldlyLeap", DatabaseHelper.SpellDefinitions.Jump);// self
            DictionaryofEIPseudoCantrips.Add("ChainsofCarceri", DatabaseHelper.SpellDefinitions.HoldMonster);
            DictionaryofEIPseudoCantrips.Add("ShroudofShadow", DatabaseHelper.SpellDefinitions.Invisibility);

            // 1/day
            DictionaryofEIPseudoSpells.Add("ThiefofFiveFates", DatabaseHelper.SpellDefinitions.Bane);
            DictionaryofEIPseudoSpells.Add("MiretheMind", DatabaseHelper.SpellDefinitions.Slow);
            //cant do it this way as it uses subspells  DictionaryofEIPseudoSpells.Add("SignofIllOmen", DatabaseHelper.SpellDefinitions.BestowCurse);
            DictionaryofEIPseudoSpells.Add("DreadfulWord", DatabaseHelper.SpellDefinitions.Confusion);
            DictionaryofEIPseudoSpells.Add("Trickster'sEscape", DatabaseHelper.SpellDefinitions.FreedomOfMovement);// self



            // EI that arent valid for game right now
            //{"BeastSpeech",         DatabaseHelper.SpellDefinitions.},
            //{"BookofAncientSecrets",DatabaseHelper.SpellDefinitions.},
            //{"MaskofManyFaces",     DatabaseHelper.SpellDefinitions.},
            //{"MistyVisions",        DatabaseHelper.SpellDefinitions.},
            //{"FarScribe",           DatabaseHelper.SpellDefinitions.},
            //{"GiftoftheDepths",     DatabaseHelper.SpellDefinitions.},
            //{"UndyingServitude",    DatabaseHelper.SpellDefinitions.},
            //{"BewitchingWhispers",  DatabaseHelper.SpellDefinitions.},
            //{"SculptorofFlesh",     DatabaseHelper.SpellDefinitions.},
            //{"WhispersoftheGrave",  DatabaseHelper.SpellDefinitions.},
            //{"MasterofMyriadForms", DatabaseHelper.SpellDefinitions.},
            // {"VisionsofDistantRealms",DatabaseHelper.SpellDefinitions.},
            // cant convert spellsbundle to power
            //  DictionaryofEIPseudoSpells.Add("MinionsofChaos", DatabaseHelper.SpellDefinitions.ConjureElemental);


            // at will EI
            foreach (KeyValuePair<string, SpellDefinition> entry in DictionaryofEIPseudoCantrips)
            {



                string textPseudoCantrips = "DHEldritchInvocation" + entry.Value.name;




                var guiPresentationEIPseudoCantrips = new GuiPresentationBuilder(
                    "Feature/&" + entry.Key + "Title",         //                    entry.Value.GuiPresentation.title) // textPseudoCantrips + "Title")
                    "Cast " + entry.Value.name + " at will")  //entry.Value.GuiPresentation.Description,// textPseudoCantrips + "Description",
                     .SetSpriteReference(entry.Value.GuiPresentation.SpriteReference)
                     .Build();

                var EIPowerBuilder = FeatureDefinitionPowerBuilder
                    .Create(textPseudoCantrips)
                    .SetGuiPresentation(guiPresentationEIPseudoCantrips)
                    .Configure(
                     1,
                     RuleDefinitions.UsesDetermination.Fixed,
                     AttributeDefinitions.Charisma,
                     entry.Value.ActivationTime,
                     1,
                     RuleDefinitions.RechargeRate.AtWill,
                     false,
                     false,
                     AttributeDefinitions.Charisma,
                     entry.Value.EffectDescription,
                     true);
                FeatureDefinitionPower EIPower = EIPowerBuilder.AddToDB();


                if (entry.Key == "ArmorofShadows" || entry.Key == "FiendishVigor" || entry.Key == "AscendantStep" || entry.Key == "OtherworldlyLeap")
                {
                    EIPower.EffectDescription.TargetType = RuleDefinitions.TargetType.Self;
                }

                DictionaryofEIPowers.Add(entry.Key, EIPower);

            }



            // 1/day EI
            foreach (KeyValuePair<string, SpellDefinition> entry in DictionaryofEIPseudoSpells)
            {
                string textPseudoSpells = "DHEldritchInvocation" + entry.Value.name;

                var guiPresentationEIPseudoSpells = new GuiPresentationBuilder(
                    "Feature/&" + entry.Key + "Title",                   // entry.Value.GuiPresentation.title)        //textPseudoSpells + "Title")
                    "Cast " + entry.Value.name + " once per day")                   // entry.Value.GuiPresentation.Description,  //textPseudoSpells + "Description",
                    .SetSpriteReference(entry.Value.GuiPresentation.SpriteReference)
                    .Build();
                var EIPowerBuilder = FeatureDefinitionPowerBuilder
                    .Create(textPseudoSpells)
                    .SetGuiPresentation(guiPresentationEIPseudoSpells)
                    .Configure(
                     1,
                     RuleDefinitions.UsesDetermination.Fixed,
                     AttributeDefinitions.Charisma,
                     entry.Value.ActivationTime,
                     1,
                     RuleDefinitions.RechargeRate.LongRest,
                     false,
                     false,
                     AttributeDefinitions.Charisma,
                     entry.Value.EffectDescription,
                     true);
                FeatureDefinitionPower EIPower = EIPowerBuilder.AddToDB();

                if (entry.Key == "Trickster'sEscape")
                {
                    EIPower.EffectDescription.TargetType = RuleDefinitions.TargetType.Self;
                }

                DictionaryofEIPowers.Add(entry.Key, EIPower);
            }

        }


        private static void EldritchBlastAndEBInvocations()
        {

            GuiPresentationBuilder EldritchBlastGui = new GuiPresentationBuilder(
            "Spell/&EldritchBlastTitle",
            "Spell/&EldritchBlastDescription");
            EldritchBlastGui.SetSpriteReference(DatabaseHelper.SpellDefinitions.MagicMissile.GuiPresentation.SpriteReference);

            EffectDescription EldritchBlastEffect = new EffectDescriptionBuilder()
            .AddEffectForm(
                new EffectFormBuilder().SetDamageForm(
                    false,
                    RuleDefinitions.DieType.D10,
                    RuleDefinitions.DamageTypeForce,
                    0,
                    RuleDefinitions.DieType.D10,
                    1,
                    RuleDefinitions.HealFromInflictedDamage.Never,
                    new List<RuleDefinitions.TrendInfo>())
                .Build())
            .SetTargetingData(
                    RuleDefinitions.Side.Enemy,
                    RuleDefinitions.RangeType.RangeHit,
                    24,
                    RuleDefinitions.TargetType.Individuals,
                    1,
                    1,
                    ActionDefinitions.ItemSelectionType.None)
            .SetEffectAdvancement(
                RuleDefinitions.EffectIncrementMethod.CasterLevelTable,
                5,
                1,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                RuleDefinitions.AdvancementDuration.None
                )
            .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.MagicMissile.EffectDescription.EffectParticleParameters)
            .Build();



            SpellDefinitionBuilder EldritchBlastBuilder = SpellDefinitionBuilder
                .Create("DHEldritchBlast")
                .SetGuiPresentation(EldritchBlastGui.Build())
                .SetSchoolOfMagic(DatabaseHelper.SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSpellLevel(0)
                .SetCastingTime(RuleDefinitions.ActivationTime.Action)
                .SetVerboseComponent(true)
                .SetSomaticComponent(true)
                .SetEffectDescription(EldritchBlastEffect)
                .SetAiParameters(new SpellAIParameters());
            EldritchBlast = EldritchBlastBuilder.AddToDB();


            //  AdditionalDamageSorcererDraconicElementalAffinity and AncestrySorcererDraconicGold - works for single class but would be a bug/buff for multiclass
            // ListofEBImprovements.Add("AgonizingBlast");    // AttackModifierOathOfDevotionSacredWeapon//PowerTraditionShockArcanistArcaneFury
            // ListofEBImprovements.Add("EldritchSpear" );  // not really useful for game
            //    ListofEBImprovements.Add("HinderingBlast");// ConditionHindered
            ListofEBImprovements.Add("RepellingBlast");// pushFromOrigin
            ListofEBImprovements.Add("GraspingHand");//rope grapple but part of EB



            // at will EI
            foreach (string entry in ListofEBImprovements)
            {



                string textEBImprovements = "DHEldritchBlast" + entry;

                EffectDescription effect = new EffectDescription();
                effect.Copy(EldritchBlastEffect);

                SpellDefinitionBuilder EIcantripBuilder = SpellDefinitionBuilder
                    .Create(textEBImprovements)
                .SetGuiPresentation(new GuiPresentationBuilder
                    (
                    "Spell/&" + entry + "Title",
                    "Spell/&" + entry + "Description"
                    )
                    .SetSpriteReference(DatabaseHelper.SpellDefinitions.MagicMissile.GuiPresentation.SpriteReference).Build()
                    )
                .SetSchoolOfMagic(DatabaseHelper.SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSpellLevel(0)
                .SetCastingTime(RuleDefinitions.ActivationTime.Action)
                .SetVerboseComponent(true)
                .SetSomaticComponent(true)
                .SetEffectDescription(effect)
                .SetAiParameters(new SpellAIParameters());
                SpellDefinition EIcantrip = EIcantripBuilder.AddToDB();


                GuiPresentation guiPresentationEBImprovements = new GuiPresentationBuilder(
                    "Feature/&" + entry + "MagicAffinityTitle",
                    "Feature/&" + entry + "MagicAffinityDescription")
                    .Build();

                FeatureDefinitionBonusCantrips BonusCantrip = FeatureDefinitionBonusCantripsBuilder
                    .Create(DatabaseHelper.FeatureDefinitionBonusCantripss.BonusCantripsDomainOblivion, textEBImprovements + "BonusCantrip")
                    .SetGuiPresentation(guiPresentationEBImprovements)
                        .ClearBonusCantrips()
                        .AddBonusCantrip(EIcantrip)
                        .AddToDB();


                DictionaryofEBInvocations.Add(entry, BonusCantrip);
            }

            DictionaryofEBInvocations["RepellingBlast"].BonusCantrips[0].EffectDescription.EffectForms
                .Add(new EffectFormBuilder()
                .SetMotionForm(
                    MotionForm.MotionType.PushFromOrigin,
                    2)
                .Build());

            DictionaryofEBInvocations["GraspingHand"].BonusCantrips[0].EffectDescription.EffectForms
                .Add(new EffectFormBuilder()
                .SetMotionForm(
                    MotionForm.MotionType.DragToOrigin,
                    2)
                .Build());




            // Agonizing blast could have Cha added via SpellDamageMatchesSourceAncestry for force damage
            // not a problem for warlocks but could buff magic missile if a warlock wizard multiclass is a thing
            // Bigger problem is that its limited to first roll only...
            // could do the same for Hindering/lethargic blast (via condition operations)

            // TODO: FeatureDefinitionAncestryBuilder
            var EldritchBlastAncestry = ScriptableObject.CreateInstance<FeatureDefinitionAncestry>();
            EldritchBlastAncestry.SetDamageType(RuleDefinitions.DamageTypeForce);
            EldritchBlastAncestry.name = "AgonizingBlastForceAncestry";
            EldritchBlastAncestry.SetGuid(DefinitionBuilder.CENamespaceGuid.ToString());
            EldritchBlastAncestry.GuiPresentation = new GuiPresentationBuilder(
                "Feature/&NoContentTitle", "Feature/&NoContentTitle").Build();

            FeatureDefinitionAdditionalDamage AdditionalDamageAgonizingBlast = FeatureDefinitionAdditionalDamageBuilder
                .Create("AdditionalDamageAgonizingBlast")
                .Configure(
                    "Agonizing_Blast",
                    RuleDefinitions.FeatureLimitedUsage.None, RuleDefinitions.AdditionalDamageValueDetermination.SpellcastingBonus,
                    RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamageMatchesSourceAncestry, RuleDefinitions.AdditionalDamageRequiredProperty.None,
                    false /* attack only */, RuleDefinitions.DieType.D4, 1 /* dice number */, RuleDefinitions.AdditionalDamageType.AncestryDamageType, RuleDefinitions.DamageTypeForce,
                    RuleDefinitions.AdditionalDamageAdvancement.None, new List<DiceByRank>())
                .SetGuiPresentation(Category.Feature)
                .AddToDB();

            AgonizingBlastFeatureSet = FeatureDefinitionFeatureSetBuilder
                .Create(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetGreenmageWardenOfTheForest, "AgonizingBlastFeatureSet")
                .SetGuiPresentation(new GuiPresentationBuilder(
                    "Feature/&AgonizingBlastFeatureSetTitle",
                    "Feature/&AgonizingBlastFeatureSetDescription")
                .Build())
               .ClearFeatureSet()
               .AddFeatureSet(EldritchBlastAncestry)
               .AddFeatureSet(AdditionalDamageAgonizingBlast)
               .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
               .SetUniqueChoices(false)
               .AddToDB();

            var hinderingConditionOperation = new ConditionOperationDescription();
            hinderingConditionOperation.SetOperation(ConditionOperationDescription.ConditionOperation.Add);
            hinderingConditionOperation.SetConditionDefinition(DatabaseHelper.ConditionDefinitions.ConditionHindered_By_Frost);
            hinderingConditionOperation.SetConditionName(DatabaseHelper.ConditionDefinitions.ConditionHindered.name);

            FeatureDefinitionAdditionalDamage AdditionalDamageHinderingBlast = FeatureDefinitionAdditionalDamageBuilder
                .Create("AdditionalDamageHinderingBlast")
                .Configure(
                    "Hindering Blast",
                    RuleDefinitions.FeatureLimitedUsage.OncePerTurn, RuleDefinitions.AdditionalDamageValueDetermination.Die,
                    RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamageMatchesSourceAncestry, RuleDefinitions.AdditionalDamageRequiredProperty.None,
                    false /* attack only */, RuleDefinitions.DieType.D1, 0 /* dice number */, RuleDefinitions.AdditionalDamageType.AncestryDamageType, RuleDefinitions.DamageTypeForce,
                    RuleDefinitions.AdditionalDamageAdvancement.None, new List<DiceByRank>())
                .SetGuiPresentation(Category.Feature)
                .SetConditionOperations(new List<ConditionOperationDescription> { hinderingConditionOperation })
                .AddToDB();

            HinderingBlastFeatureSet = FeatureDefinitionFeatureSetBuilder
                .Create(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetGreenmageWardenOfTheForest, "HinderingBlastFeatureSet")
               .SetGuiPresentation(new GuiPresentationBuilder(
                   "Feature/&HinderingBlastFeatureSetTitle",
                   "Feature/&HinderingBlastFeatureSetDescription"
                   ).Build())
               .ClearFeatureSet()
               .AddFeatureSet(EldritchBlastAncestry)
               .AddFeatureSet(AdditionalDamageHinderingBlast)
               .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
               .SetUniqueChoices(false)
               .AddToDB();

        }
        private static void EIAttributeModifers()
        {
            ListofEIAttributeModifers.AddRange(new List<String>
                {
                    "AspectoftheMoon",               // DatabaseHelper.FeatureDefinitionCampAffinitys.CampAffinityElfTrance,DatabaseHelper.FeatureDefinitionCampAffinitys.CampAffinityDomainOblivionPeacefulRest);
                    "BeguilingInfluence",            // DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyFeatManipulatorSkillOrExpertise);
                    "EldritchMind",                  // DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityFeatFlawlessConcentration);
                    "EyesoftheRuneKeeper",           // DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAllLanguages);
                    "GiftoftheEver-LivingOnes",      // DatabaseHelper.FeatureDefinitionHealingModifiers.HealingModifierBeaconOfHope);
                    "ImprovedPactWeapon",            // AttackModifierMagicWeapon + MagicAffinitySpellBladeIntoTheFray
                    "EldritchSmite",                 // DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamagePaladinDivineSmite);
                    "ThirstingBlade",                // DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierFighterExtraAttack);
                    "GiftoftheProtectors",           // DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityHalfOrcRelentlessEndurance);
                    "BondoftheTalisman",             // DatabaseHelper.FeatureDefinitionPowers.PowerSorakShadowEscape);
                    "WitchSight",                    // DatabaseHelper.FeatureDefinitionSenses.SenseSeeInvisible12;
                    "OneWithShadows"
                    //   "Lifedrinker",                   // similar to AdditionalDamageDomainOblivionStrikeOblivion +damageValueDetermination = RuleDefinitions.AdditionalDamageValueDetermination.SpellcastingBonus;
                    //"Devil'sSight",                // DatabaseHelper.FeatureDefinitionSenses.SenseDarkvision24 or maybe similar to ConditionAffinityVeilImmunity needs to cover multiple darkness conditions ConditionDarkness ConditionVeil
                    //"GazeofTwoMinds",              //
                    //"InvestmentoftheChainMaster",  // multiple featurs through summoning affinity, could just reuse SummoningAffinityKindredSpiritBond for a similar but not direct copy of non srd EI
                    //"RebukeoftheTalisman",         //
                    //"VoiceoftheChainMaster",       //
                    //"CloakofFlies",                //
                    //"MaddeningHex",                //
                    //"TombofLevistus",              //
                    //"GhostlyGaze",                 //
                    //"ProtectionoftheTalisman",     //
                    //"RelentlessHex",               //
                });

            // EI AttributeModifers
            foreach (string entry in ListofEIAttributeModifers)
            {

                string textEIAttributeModifers = "DHEIAttributeModifers" + entry;



                GuiPresentation guiFeatureSetEldritchInvocations = new GuiPresentationBuilder(
                 "Feature/&" + entry + "Title",
                "Feature/&" + entry + "Description")
                .Build();
                var FeatureSetEldritchInvocationsBuilder = FeatureDefinitionFeatureSetBuilder
                    .Create(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetGreenmageWardenOfTheForest, textEIAttributeModifers)
                    .SetGuiPresentation(guiFeatureSetEldritchInvocations);

                FeatureDefinitionFeatureSet FeatureSetEldritchInvocations = FeatureSetEldritchInvocationsBuilder
                    .ClearFeatureSet()
                    .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                    .SetUniqueChoices(false)
                    .AddToDB();


                DictionaryofEIAttributeModifers.Add(entry, FeatureSetEldritchInvocations);
            }


            DictionaryofEIAttributeModifers["AspectoftheMoon"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionCampAffinitys.CampAffinityElfTrance);
            DictionaryofEIAttributeModifers["AspectoftheMoon"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionCampAffinitys.CampAffinityDomainOblivionPeacefulRest);

            DictionaryofEIAttributeModifers["BeguilingInfluence"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionProficiencys.ProficiencyFeatManipulatorSkillOrExpertise);
            DictionaryofEIAttributeModifers["EldritchMind"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinityFeatFlawlessConcentration);
            DictionaryofEIAttributeModifers["EyesoftheRuneKeeper"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAllLanguages);
            DictionaryofEIAttributeModifers["GiftoftheEver-LivingOnes"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionHealingModifiers.HealingModifierBeaconOfHope);

            DictionaryofEIAttributeModifers["ImprovedPactWeapon"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionAttackModifiers.AttackModifierMagicWeapon);
            DictionaryofEIAttributeModifers["ImprovedPactWeapon"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionMagicAffinitys.MagicAffinitySpellBladeIntoTheFray);

            DictionaryofEIAttributeModifers["EldritchSmite"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamagePaladinDivineSmite);
            DictionaryofEIAttributeModifers["ThirstingBlade"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierFighterExtraAttack);
            DictionaryofEIAttributeModifers["GiftoftheProtectors"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityHalfOrcRelentlessEndurance);
            DictionaryofEIAttributeModifers["BondoftheTalisman"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionPowers.PowerSorakShadowEscape);
            //  DictionaryofEIAttributeModifers["Lifedrinker"].FeatureSet.Add();
            DictionaryofEIAttributeModifers["WitchSight"].FeatureSet.Add(DatabaseHelper.FeatureDefinitionSenses.SenseSeeInvisible12);


            var Unlit = new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Unlit,
                condition = DatabaseHelper.ConditionDefinitions.ConditionInvisible
            };
            var Dim = new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Dim,
                condition = DatabaseHelper.ConditionDefinitions.ConditionInvisible
            };
            var Darkness = new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Darkness,
                condition = DatabaseHelper.ConditionDefinitions.ConditionInvisible
            };

            FeatureDefinitionLightAffinity OneWithShadowsLightAffinity = FeatureDefinitionLightAffinityBuilder
                .Create("OneWithShadowsLightAffinity", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation("OneWithShadowsLightAffinity", Category.Feature)
                .AddLightingEffectAndCondition(Unlit)
                .AddLightingEffectAndCondition(Dim)
                .AddLightingEffectAndCondition(Darkness)
                .AddToDB();

            DictionaryofEIAttributeModifers["OneWithShadows"].FeatureSet.Add(OneWithShadowsLightAffinity);


        }
    }
}







