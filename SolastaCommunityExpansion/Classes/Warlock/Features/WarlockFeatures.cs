﻿using System.Collections.Generic;
using System.Linq;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaModApi.Extensions;
using static SolastaCommunityExpansion.Builders.DefinitionBuilder;
using static SolastaCommunityExpansion.Classes.Warlock.Features.EldritchInvocationsBuilder;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaCommunityExpansion.Classes.Warlock.Features
{
    internal static class WarlockFeatures
    {
        internal static readonly FeatureDefinitionFeatureSet WarlockMysticArcanumSetLevel11 = CreateMysticArcanumSet(11, 6);
        internal static readonly FeatureDefinitionFeatureSet WarlockMysticArcanumSetLevel13 = CreateMysticArcanumSet(13, 7, 6);
        internal static readonly FeatureDefinitionFeatureSet WarlockMysticArcanumSetLevel15 = CreateMysticArcanumSet(15, 8, 7, 6);
        internal static readonly FeatureDefinitionFeatureSet WarlockMysticArcanumSetLevel17 = CreateMysticArcanumSet(17, 9, 8, 7, 6);

        private static FeatureDefinitionPower warlockEldritchMasterPower;
        internal static FeatureDefinitionPower WarlockEldritchMasterPower => warlockEldritchMasterPower ??= FeatureDefinitionPowerBuilder
            .Create(PowerWizardArcaneRecovery, "ClassWarlockEldritchMaster", CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetActivationTime(RuleDefinitions.ActivationTime.Minute1)
            .AddToDB();

        #region WarlockEldritchInvocationSetLevel2
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel2;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel2 => warlockEldritchInvocationSetLevel2 ??= FeatureDefinitionFeatureSetBuilder
            .Create(TerrainTypeAffinityRangerNaturalExplorerChoice, "ClassWarlockEldritchInvocationSetLevel2", CENamespaceGuid)
            .SetGuiPresentation("Feature/&ClassWarlockEldritchInvocationSetLevelTitle", "Feature/&ClassWarlockEldritchInvocationSetLevelDescription")
            /*
            EI that might need a bit more work
            Investment of the Chain Master - multiple features through summoning affinity
            Book of ancient secrets - similar to MagicAffinityWizardRitualCasting or ritual casting feat
            */
            .SetFeatureSet(
                AgonizingBlastFeatureSet,
                HinderingBlastFeatureSet,
                DictionaryofEBInvocations["RepellingBlast"],
                DictionaryofEBInvocations["GraspingHand"],
                DictionaryofEIPowers["ArmorofShadows"],
                DictionaryofEIPowers["EldritchSight"],
                DictionaryofEIPowers["FiendishVigor"],
                DictionaryofEIPowers["ThiefofFiveFates"],
                DictionaryofEIAttributeModifers["AspectoftheMoon"],
                DictionaryofEIAttributeModifers["BeguilingInfluence"],
                DictionaryofEIAttributeModifers["EldritchMind"],
                DictionaryofEIAttributeModifers["EyesoftheRuneKeeper"],
                DictionaryofEIAttributeModifers["GiftoftheEver-LivingOnes"]
            )
            .SetUniqueChoices(true)
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel5
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel5;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel5 => warlockEldritchInvocationSetLevel5 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel2, "ClassWarlockEldritchInvocationSetLevel5", CENamespaceGuid)
            /*
            EI that might need a bit more work
            Sign of Ill Omen - create a feature set that adds converted versions of the subspells
            Tomb of Levistus - add tempHP and then apply incapcitated or stunned status
            Undying Servitude - summon a a skeleton or zombie
            */
            .AddFeatureSet(
                DictionaryofEIPowers["MiretheMind"],
                DictionaryofEIAttributeModifers["EldritchSmite"],
                DictionaryofEIAttributeModifers["ThirstingBlade"],
                DictionaryofEIAttributeModifers["ImprovedPactWeapon"]
            )
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel7
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel7;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel7 => warlockEldritchInvocationSetLevel7 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel5, "ClassWarlockEldritchInvocationSetLevel7", CENamespaceGuid)
            .AddFeatureSet(
                DictionaryofEIAttributeModifers["OneWithShadows"],
                DictionaryofEIPowers["DreadfulWord"],
                DictionaryofEIPowers["Trickster'sEscape"]
            )
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel9
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel9;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel9 => warlockEldritchInvocationSetLevel9 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel7, "ClassWarlockEldritchInvocationSetLevel9", CENamespaceGuid)
            .AddFeatureSet(
                DictionaryofEIPowers["AscendantStep"],
                DictionaryofEIPowers["OtherworldlyLeap"],
                DictionaryofEIAttributeModifers["GiftoftheProtectors"]
            )
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel12
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel12;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel12 => warlockEldritchInvocationSetLevel12 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel9, "ClassWarlockEldritchInvocationSetLevel12", CENamespaceGuid)
            .AddFeatureSet(
                DictionaryofEIAttributeModifers["BondoftheTalisman"]
            )
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel15
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel15;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel15 => warlockEldritchInvocationSetLevel15 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel12, "ClassWarlockEldritchInvocationSetLevel15", CENamespaceGuid)
            .AddFeatureSet(
                /*
                *EI that  more work
                Master of Myriad Forms - would need to create the alter self spell then convert it
                */
                DictionaryofEIPowers["ChainsofCarceri"],
                DictionaryofEIPowers["ShroudofShadow"],
                DictionaryofEIAttributeModifers["WitchSight"]
            )
            .AddToDB();
        #endregion

        #region WarlockEldritchInvocationSetLevel18
        private static FeatureDefinitionFeatureSet warlockEldritchInvocationSetLevel18;
        public static FeatureDefinitionFeatureSet WarlockEldritchInvocationSetLevel18 => warlockEldritchInvocationSetLevel18 ??= FeatureDefinitionFeatureSetBuilder
            .Create(WarlockEldritchInvocationSetLevel15, "ClassWarlockEldritchInvocationSetLevel18", CENamespaceGuid)
            .AddToDB();
        #endregion

        #region SupportCode
        private static FeatureDefinitionPower CreateMysticArcanumPower(string baseName, SpellDefinition spell)
        {
            return FeatureDefinitionPowerBuilder
                .Create(baseName + spell.name, DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(spell.GuiPresentation)
                .Configure(
                    1,
                    RuleDefinitions.UsesDetermination.Fixed,
                    AttributeDefinitions.Charisma,
                    spell.ActivationTime,
                    1,
                    RuleDefinitions.RechargeRate.LongRest,
                    false,
                    false,
                    AttributeDefinitions.Charisma,
                    spell.EffectDescription,
                    true)
                .AddToDB();
        }

        private static IEnumerable<SpellDefinition> GetSpells(params int[] levels)
        {
            return levels.SelectMany(level => WarlockSpells.WarlockSpellList.SpellsByLevel[level].Spells);
        }

        private static FeatureDefinitionFeatureSet CreateMysticArcanumSet(int setLevel, params int[] spellLevels)
        {
            return FeatureDefinitionFeatureSetBuilder
                .Create(TerrainTypeAffinityRangerNaturalExplorerChoice, $"ClassWarlockMysticArcanumSetLevel{setLevel}", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation("ClassWarlockMysticArcanumSet", Category.Feature)
                .SetFeatureSet(GetSpells(spellLevels).Select(spell => CreateMysticArcanumPower($"DH_MysticArcanum{setLevel}_", spell)))
                .SetUniqueChoices(true)
                .AddToDB();
        }
        #endregion
    }
}
