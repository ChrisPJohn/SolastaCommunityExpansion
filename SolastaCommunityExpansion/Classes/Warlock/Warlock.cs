﻿using System.Collections.Generic;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.Classes.Warlock.Features;
using SolastaCommunityExpansion.Classes.Warlock.Subclasses;
using SolastaModApi;
using static EquipmentDefinitions;
using static SolastaModApi.DatabaseHelper;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.ToolTypeDefinitions;
using static SolastaCommunityExpansion.Builders.EquipmentOptionsBuilder;
using static SolastaCommunityExpansion.Classes.Warlock.Features.WarlockFeatures;

namespace SolastaCommunityExpansion.Classes.Warlock
{
    public static class Warlock
    {
        public static CharacterClassDefinition ClassWarlock { get; private set; }

        private static FeatureDefinitionProficiency FeatureDefinitionProficiencyArmor { get; set; }

        private static FeatureDefinitionProficiency FeatureDefinitionProficiencyWeapon { get; set; }

        private static FeatureDefinitionProficiency FeatureDefinitionProficiencyTool { get; set; }

        private static FeatureDefinitionProficiency FeatureDefinitionProficiencySavingThrow { get; set; }

        private static FeatureDefinitionPointPool FeatureDefinitionSkillPoints { get; set; }

        private static FeatureDefinitionCastSpell FeatureDefinitionClassWarlockCastSpell { get; set; }

        private static void BuildEquipment(CharacterClassDefinitionBuilder classWarlockBuilder)
        {
            classWarlockBuilder
                .AddEquipmentRow(
                    Column(
                        Option(ItemDefinitions.LightCrossbow, OptionWeapon, 1),
                        Option(ItemDefinitions.Bolt, OptionAmmoPack, 1)
                    ),
                    Column(Option(ItemDefinitions.LightCrossbow, OptionWeaponSimpleChoice, 1)))
                .AddEquipmentRow(
                    Column(Option(ItemDefinitions.ScholarPack, OptionStarterPack, 1)),
                    Column(Option(ItemDefinitions.DungeoneerPack, OptionStarterPack, 1)))
                .AddEquipmentRow(
                    Column(
                        Option(ItemDefinitions.Leather, OptionArmor, 1),
                        Option(ItemDefinitions.ComponentPouch, OptionFocus, 1),
                        Option(ItemDefinitions.Dagger, OptionWeapon, 2),
                        Option(ItemDefinitions.Dagger, OptionWeaponSimpleChoice, 1)
                    ));
        }

        private static void BuildProficiencies()
        {
            static FeatureDefinitionProficiency BuildProficiency(string name, RuleDefinitions.ProficiencyType type, params string[] proficiencies)
            {
                return FeatureDefinitionProficiencyBuilder
                    .Create(name, DefinitionBuilder.CENamespaceGuid)
                    .SetGuiPresentation(Category.Feature)
                    .SetProficiencies(type, proficiencies)
                    .AddToDB();
            }

            FeatureDefinitionProficiencyArmor =
                BuildProficiency("ClassWarlockArmorProficiency", RuleDefinitions.ProficiencyType.Armor, LightArmorCategory);

            FeatureDefinitionProficiencyWeapon =
                BuildProficiency("ClassWarlockWeaponProficiency", RuleDefinitions.ProficiencyType.Weapon, SimpleWeaponCategory);

            FeatureDefinitionProficiencyTool =
                BuildProficiency("ClassWarlockToolsProficiency", RuleDefinitions.ProficiencyType.Tool, EnchantingToolType.Name, HerbalismKitType.Name);

            FeatureDefinitionProficiencySavingThrow =
                BuildProficiency("ClassWarlockSavingThrowProficiency", RuleDefinitions.ProficiencyType.SavingThrow, AttributeDefinitions.Charisma, AttributeDefinitions.Wisdom);

            FeatureDefinitionSkillPoints = FeatureDefinitionPointPoolBuilder
                .Create("ClassWarlockSkillProficiency", DefinitionBuilder.CENamespaceGuid)
                .SetPool(HeroDefinitions.PointsPoolType.Skill, 2)
                .OnlyUniqueChoices()
                .RestrictChoices(
                    SkillDefinitions.Arcana,
                    SkillDefinitions.Deception,
                    SkillDefinitions.History,
                    SkillDefinitions.Intimidation,
                    SkillDefinitions.Investigation,
                    SkillDefinitions.Nature,
                    SkillDefinitions.Religion)
                .SetGuiPresentation(Category.Feature)
                .AddToDB();
        }

        private static void BuildSpells()
        {
            var classWarlockCastSpell = FeatureDefinitionCastSpellBuilder
                .Create(FeatureDefinitionCastSpells.CastSpellSorcerer, "ClassWarlockCastSpell", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation("ClassWarlockSpellcasting", Category.Feature)
                .SetKnownCantrips(2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4)
                .SetKnownSpells(2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15)
                .SetReplacedSpells(0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0)
                .SetSlotsPerLevel(WarlockSpells.WarlockCastingSlots)
                .SetSlotsRecharge(RuleDefinitions.RechargeRate.ShortRest)
                .SetSpellCastingAbility(AttributeDefinitions.Charisma)
                .SetSpellCastingLevel(9)
                .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Class)
                .SetSpellList(WarlockSpells.WarlockSpellList)
                .SetSpellKnowledge(RuleDefinitions.SpellKnowledge.Selection)
                .SetSpellReadyness(RuleDefinitions.SpellReadyness.AllKnown);

            FeatureDefinitionClassWarlockCastSpell = classWarlockCastSpell.AddToDB();
        }

        private static void BuildProgression(CharacterClassDefinitionBuilder classWarlockBuilder)
        {
            //
            // building but not adding to avoid issues with gamers who already took this class
            //
            _ = DHWarlockSubclassAncientForestPatron.Build();

            FeatureDefinitionSubclassChoice subclassChoices = FeatureDefinitionSubclassChoiceBuilder
                .Create("ClassWarlockSubclassChoice", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation("ClassWarlockPatron", Category.Subclass)
                .SetSubclassSuffix("Patron")
                .SetFilterByDeity(false)
                .SetSubclasses(
                    AHWarlockSubclassSoulBladePact.Subclass,
                    // needs more work and verification,  find a better solution for the rest panel
                    //DHWarlockSubclassAncientForestPatron.Build(),
                    DHWarlockSubclassElementalPatron.Build(),
                    DHWarlockSubclassMoonLitPatron.Build(),
                    DHWarlockSubclassRiftWalkerPatron.Build(),
                    // needs more work and verification, autoprepared spells cant just be reused because they specific cleric class, battle domain divine Fortitude (wrath in code) also didnt work
                    //DHWarlockSubclassUrPriestPatron.Build(),
                    DHWarlockSubclassToadKingPatron.Build()
                    )
                .AddToDB();

            classWarlockBuilder
                .AddFeaturesAtLevel(1,
                    FeatureDefinitionProficiencySavingThrow,
                    FeatureDefinitionProficiencyArmor,
                    FeatureDefinitionProficiencyWeapon,
                    FeatureDefinitionProficiencyTool,
                    FeatureDefinitionSkillPoints,
                    FeatureDefinitionClassWarlockCastSpell,
                    subclassChoices)
                .AddFeaturesAtLevel(2, WarlockEldritchInvocationSetLevel2)
                .AddFeaturesAtLevel(2, WarlockEldritchInvocationSetLevel2)
                .AddFeaturesAtLevel(3, WarlockClassPactBoonSetBuilder.WarlockClassPactBoonSet)
                .AddFeaturesAtLevel(4, FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice)
                .AddFeaturesAtLevel(5, WarlockEldritchInvocationSetLevel5)
                //level 6 - subclass feature
                .AddFeatureAtLevel(7, WarlockEldritchInvocationSetLevel7)
                .AddFeatureAtLevel(8, FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice)
                .AddFeatureAtLevel(9, WarlockEldritchInvocationSetLevel9)
                //level 10 - subclass feature
                .AddFeatureAtLevel(11, WarlockMysticArcanumSetLevel11)
                .AddFeatureAtLevel(12, FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice)
                .AddFeatureAtLevel(12, WarlockEldritchInvocationSetLevel12)
                .AddFeatureAtLevel(13, WarlockMysticArcanumSetLevel13)
                //level 14 - subclass feature
                .AddFeatureAtLevel(15, WarlockMysticArcanumSetLevel15)
                .AddFeatureAtLevel(15, WarlockEldritchInvocationSetLevel15)
                .AddFeatureAtLevel(16, FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice)
                .AddFeatureAtLevel(17, WarlockMysticArcanumSetLevel17)
                .AddFeatureAtLevel(18, WarlockEldritchInvocationSetLevel18)
                .AddFeatureAtLevel(19, FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice)
                .AddFeatureAtLevel(20, WarlockEldritchMasterPower);
        }

        internal static CharacterClassDefinition BuildWarlockClass()
        {
            var warlockSpriteReference = Utils.CustomIcons.CreateAssetReferenceSprite("Warlock", Properties.Resources.Warlock, 1024, 576);

            var classWarlockBuilder = CharacterClassDefinitionBuilder
                .Create("ClassWarlock", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(Category.Class, warlockSpriteReference, 1)
                .AddFeatPreferences(FeatDefinitions.PowerfulCantrip, FeatDefinitions.FlawlessConcentration, FeatDefinitions.Robust)
                .AddPersonality(PersonalityFlagDefinitions.Violence, 3)
                .AddPersonality(PersonalityFlagDefinitions.Self_Preservation, 3)
                .AddPersonality(PersonalityFlagDefinitions.Normal, 3)
                .AddPersonality(PersonalityFlagDefinitions.GpSpellcaster, 5)
                .AddPersonality(PersonalityFlagDefinitions.GpExplorer, 1)
                .AddSkillPreferences(
                    DatabaseHelper.SkillDefinitions.Deception,
                    DatabaseHelper.SkillDefinitions.Intimidation,
                    DatabaseHelper.SkillDefinitions.Arcana,
                    DatabaseHelper.SkillDefinitions.History,
                    DatabaseHelper.SkillDefinitions.Investigation,
                    DatabaseHelper.SkillDefinitions.Religion,
                    DatabaseHelper.SkillDefinitions.Nature,
                    DatabaseHelper.SkillDefinitions.Persuasion)
                .AddToolPreferences(EnchantingToolType, HerbalismKitType)
                .SetAbilityScorePriorities(
                    AttributeDefinitions.Charisma,
                    AttributeDefinitions.Constitution,
                    AttributeDefinitions.Dexterity,
                    AttributeDefinitions.Strength,
                    AttributeDefinitions.Wisdom,
                    AttributeDefinitions.Intelligence)
                .SetAnimationId(AnimationDefinitions.ClassAnimationId.Wizard)
                .SetBattleAI(DecisionPackageDefinitions.DefaultSupportCasterWithBackupAttacksDecisions)
                .SetHitDice(RuleDefinitions.DieType.D8)
                .SetIngredientGatheringOdds(Sorcerer.IngredientGatheringOdds)
                .SetPictogram(Wizard.ClassPictogramReference);

            EldritchInvocationsBuilder.Build();

            BuildEquipment(classWarlockBuilder);
            BuildProficiencies();
            BuildSpells();
            BuildProgression(classWarlockBuilder);

            ClassWarlock = classWarlockBuilder.AddToDB();

            var itemlist = new List<ItemDefinition>
            {
                ItemDefinitions.WandOfLightningBolts,
                ItemDefinitions.StaffOfFire,
                ItemDefinitions.ArcaneShieldstaff,
                ItemDefinitions.WizardClothes_Alternate
            };

            foreach (ItemDefinition item in itemlist)
            {
                item.RequiredAttunementClasses.Add(ClassWarlock);
            }

            return ClassWarlock;
        }
    }
}
