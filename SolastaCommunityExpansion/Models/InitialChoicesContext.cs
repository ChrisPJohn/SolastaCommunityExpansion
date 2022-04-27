﻿using System;
using System.Linq;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaModApi;
using SolastaModApi.Extensions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPointPools;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionSenses;

namespace SolastaCommunityExpansion.Models
{
    internal static class InitialChoicesContext
    {
        internal const int MIN_INITIAL_FEATS = 0;
        internal const int MAX_INITIAL_FEATS = 10;

        internal static int PreviousTotalFeatsGrantedFistLevel { get; set; } = -1;
        internal static bool PreviousAlternateHuman { get; set; }

        internal const int GAME_MAX_ATTRIBUTE = 15;
        internal const int GAME_BUY_POINTS = 27;

        internal const int MOD_MAX_ATTRIBUTE = 17;
        internal const int MOD_BUY_POINTS = 35;

        internal static void Load()
        {
            // keep this outside loop for backward compatibility
            _ = FeatureDefinitionPointPoolBuilder
                .Create("PointPool2BonusFeats", "dbec86c7-468f-4569-917b-2d96d21f9ddf")
                .SetGuiPresentation("PointPoolSelect2Feats", Category.Race)
                .SetPool(HeroDefinitions.PointsPoolType.Feat, 2)
                .AddToDB();

            // 11 here as need to count the Alternate Human Feat
            for (var i = 3; i <= 11; i++)
            {
                _ = FeatureDefinitionPointPoolBuilder.Create($"PointPool{i}BonusFeats", DefinitionBuilder.CENamespaceGuid)
                    .SetGuiPresentation($"PointPoolSelect{i}Feats", Category.Race)
                    .SetPool(HeroDefinitions.PointsPoolType.Feat, i)
                    .AddToDB();
            }

            LoadVision();
        }

        internal static void LateLoad()
        {
            SwitchAsiAndFeat();
            SwitchEpicArray();
            SwitchEvenLevelFeats();
            SwitchFirstLevelTotalFeats();
        }

        internal static void LoadVision()
        {
            if (Main.Settings.DisableSenseDarkVisionFromAllRaces)
            {
                foreach (var featureUnlocks in DatabaseRepository.GetDatabase<CharacterRaceDefinition>().Select(crd => crd.FeatureUnlocks))
                {
                    featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision");
                    // Half-orcs have a different darkvisition.
                    featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision12");
                }
            }

            if (Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces)
            {
                foreach (CharacterRaceDefinition characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
                {
                    characterRaceDefinition.FeatureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseSuperiorDarkvision");
                }
            }

            if (Main.Settings.IncreaseSenseNormalVision)
            {
                SenseNormalVision.SetSenseRange(120);
            }
        }

        internal static void SwitchAsiAndFeat()
        {
            if (Main.Settings.EnablesAsiAndFeat)
            {
                FeatureSetAbilityScoreChoice.SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union);
            }
            else
            {
                FeatureSetAbilityScoreChoice.SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion);
            }
        }

        internal static void SwitchEpicArray()
        {
            if (Main.Settings.EnableEpicArray)
            {
                AttributeDefinitions.PredeterminedRollScores = new int[] { 17, 15, 13, 12, 10, 8 };
            }
            else
            {
                AttributeDefinitions.PredeterminedRollScores = new int[] { 15, 14, 13, 12, 10, 8 };
            }
        }

        internal static void SwitchEvenLevelFeats()
        {
            var levels = new int[] { 2, 6, 10, 14 };
            var dbCharacterClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>();

            foreach (var characterClassDefinition in dbCharacterClassDefinition)
            {
                if (Main.Settings.EnableFeatsAtEvenLevels)
                {
                    foreach (var level in levels)
                    {
                        characterClassDefinition.FeatureUnlocks.TryAdd(
                            new FeatureUnlockByLevel(PointPoolBonusFeat, level));
                    }
                }
                else
                {
                    foreach (var level in levels)
                    {
                        characterClassDefinition.FeatureUnlocks.Remove(
                            new FeatureUnlockByLevel(PointPoolBonusFeat, level));
                    }
                }
            }
        }

        internal static void SwitchFirstLevelTotalFeats()
        {
            if (PreviousTotalFeatsGrantedFistLevel > -1)
            {
                UnloadRacesLevel1Feats(PreviousTotalFeatsGrantedFistLevel, PreviousAlternateHuman);
            }
            PreviousTotalFeatsGrantedFistLevel = Main.Settings.TotalFeatsGrantedFistLevel;
            PreviousAlternateHuman = Main.Settings.EnableAlternateHuman;
            LoadRacesLevel1Feats(Main.Settings.TotalFeatsGrantedFistLevel, Main.Settings.EnableAlternateHuman);
        }

        internal static void BuildFeatureUnlocks(int initialFeats, bool alternateHuman, out FeatureUnlockByLevel featureUnlockByLevelNonHuman, out FeatureUnlockByLevel featureUnlockByLevelHuman)
        {
            var featureDefinitionPointPoolDb = DatabaseRepository.GetDatabase<FeatureDefinitionPointPool>();
            string name;

            featureUnlockByLevelNonHuman = null;
            featureUnlockByLevelHuman = null;

            if (initialFeats == 0)
            {
                if (alternateHuman)
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionPointPools.PointPoolBonusFeat, 1);
                }
            }
            else if (initialFeats == 1)
            {
                featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionPointPools.PointPoolBonusFeat, 1);

                name = "PointPool2BonusFeats";
                if (alternateHuman && featureDefinitionPointPoolDb.TryGetElement(name, out FeatureDefinitionPointPool pointPool2BonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPool2BonusFeats, 1);
                }
            }
            else if (initialFeats > 1)
            {
                name = $"PointPool{initialFeats}BonusFeats";
                if (featureDefinitionPointPoolDb.TryGetElement(name, out FeatureDefinitionPointPool featureDefinitionPointPool))
                {
                    featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(featureDefinitionPointPool, 1);
                }

                name = $"PointPool{initialFeats + 1}BonusFeats";
                if (alternateHuman && featureDefinitionPointPoolDb.TryGetElement(name, out FeatureDefinitionPointPool pointPoolXBonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPoolXBonusFeats, 1);
                }
            }
        }

        internal static void LoadRacesLevel1Feats(int initialFeats, bool alternateHuman)
        {
            var human = DatabaseHelper.CharacterRaceDefinitions.Human;

            BuildFeatureUnlocks(initialFeats, alternateHuman, out FeatureUnlockByLevel featureUnlockByLevelNonHuman, out FeatureUnlockByLevel featureUnlockByLevelHuman);

            foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>().GetAllElements())
            {
                if (!IsSubRace(characterRaceDefinition))
                {
                    if (alternateHuman && characterRaceDefinition == human)
                    {
                        if (featureUnlockByLevelHuman != null)
                        {
                            human.FeatureUnlocks.Add(featureUnlockByLevelHuman);
                        }

                        FeatureUnlockByLevel pointPoolAbilityScoreImprovement = new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionPointPools.PointPoolAbilityScoreImprovement, 1);
                        human.FeatureUnlocks.Add(pointPoolAbilityScoreImprovement);

                        FeatureUnlockByLevel pointPoolHumanSkillPool = new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionPointPools.PointPoolHumanSkillPool, 1);
                        human.FeatureUnlocks.Add(pointPoolHumanSkillPool);

                        Remove(human, DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierHumanAbilityScoreIncrease);
                    }
                    else
                    {
                        if (featureUnlockByLevelNonHuman != null)
                        {
                            characterRaceDefinition.FeatureUnlocks.Add(featureUnlockByLevelNonHuman);
                        }
                    }
                }
            }
        }

        internal static void UnloadRacesLevel1Feats(int initialFeats, bool alternateHuman)
        {
            var human = DatabaseHelper.CharacterRaceDefinitions.Human;

            BuildFeatureUnlocks(initialFeats, alternateHuman, out FeatureUnlockByLevel featureUnlockByLevelNonHuman, out FeatureUnlockByLevel featureUnlockByLevelHuman);

            foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>().GetAllElements())
            {
                if (!IsSubRace(characterRaceDefinition))
                {
                    if (alternateHuman && characterRaceDefinition == human)
                    {
                        if (featureUnlockByLevelHuman != null)
                        {
                            Remove(human, featureUnlockByLevelHuman);
                        }
                        Remove(human, DatabaseHelper.FeatureDefinitionPointPools.PointPoolAbilityScoreImprovement);
                        Remove(human, DatabaseHelper.FeatureDefinitionPointPools.PointPoolHumanSkillPool);

                        FeatureUnlockByLevel humanAttributeIncrease = new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierHumanAbilityScoreIncrease, 1);
                        human.FeatureUnlocks.Add(humanAttributeIncrease);
                    }
                    else
                    {
                        if (featureUnlockByLevelNonHuman != null)
                        {
                            Remove(characterRaceDefinition, featureUnlockByLevelNonHuman);
                        }
                    }
                }
            }
        }

        private static void Remove(CharacterRaceDefinition characterRaceDefinition, FeatureDefinition toRemove)
        {
            var ndx = -1;

            for (var i = 0; i < characterRaceDefinition.FeatureUnlocks.Count; i++)
            {
                if (characterRaceDefinition.FeatureUnlocks[i].Level == 1 &&
                    characterRaceDefinition.FeatureUnlocks[i].FeatureDefinition == toRemove)
                {
                    ndx = i;
                }
            }

            if (ndx >= 0)
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAt(ndx);
            }
        }

        private static void Remove(CharacterRaceDefinition characterRaceDefinition, FeatureUnlockByLevel featureUnlockByLevel)
        {
            Remove(characterRaceDefinition, featureUnlockByLevel.FeatureDefinition);
        }

        private static bool IsSubRace(CharacterRaceDefinition raceDefinition)
        {
            return DatabaseRepository.GetDatabase<CharacterRaceDefinition>().Any(crd => crd.SubRaces.Contains(raceDefinition));
        }
    }
}
