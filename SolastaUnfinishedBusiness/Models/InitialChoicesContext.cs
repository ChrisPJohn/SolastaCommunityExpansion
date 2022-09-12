﻿using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPointPools;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;

namespace SolastaUnfinishedBusiness.Models;

internal static class InitialChoicesContext
{
    internal const int MinInitialFeats = 0;
    internal const int MaxInitialFeats = 10;

    internal const int GameMaxAttribute = 15;
    internal const int GameBuyPoints = 27;

    internal const int ModMaxAttribute = 17;
    internal const int ModBuyPoints = 35;

    private static int PreviousTotalFeatsGrantedFistLevel { get; set; } = -1;
    private static bool PreviousAlternateHuman { get; set; }

    internal static void Load()
    {
        // +1 here as need to count the Alternate Human Feat
        for (var i = 2; i <= MaxInitialFeats + 1; i++)
        {
            _ = FeatureDefinitionPointPoolBuilder
                .Create($"PointPool{i}BonusFeats", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation($"PointPoolSelect{i}Feats", Category.Race)
                .SetPool(HeroDefinitions.PointsPoolType.Feat, i)
                .AddToDB();
        }

        LoadEpicArray();
        LoadVision();
    }

    internal static void LateLoad()
    {
        SwitchAsiAndFeat();
        SwitchEvenLevelFeats();
        SwitchFirstLevelTotalFeats();
    }

    private static void LoadEpicArray()
    {
        AttributeDefinitions.PredeterminedRollScores = Main.Settings.EnableEpicPointsAndArray
            ? new[] { 17, 15, 13, 12, 10, 8 }
            : new[] { 15, 14, 13, 12, 10, 8 };
    }

    private static void LoadVision()
    {
        if (Main.Settings.DisableSenseDarkVisionFromAllRaces)
        {
            foreach (var featureUnlocks in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                         .Select(crd => crd.FeatureUnlocks))
            {
                featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision");
                // Half-orcs have a different darkvision.
                featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision12");
            }
        }

        if (Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces)
        {
            foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAll(x =>
                    x.FeatureDefinition.name == "SenseSuperiorDarkvision");
            }
        }

        if (Main.Settings.IncreaseSenseNormalVision > SrdAndHouseRulesContext.DefaultVisionRange)
        {
            SenseNormalVision.senseRange = Main.Settings.IncreaseSenseNormalVision;
        }
    }

    internal static void SwitchAsiAndFeat()
    {
        FeatureSetAbilityScoreChoice.mode = Main.Settings.EnablesAsiAndFeat
            ? FeatureDefinitionFeatureSet.FeatureSetMode.Union
            : FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion;
    }

    internal static void SwitchEvenLevelFeats()
    {
        var levels = new[] { 2, 6, 10, 14 };
        var dbCharacterClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
        var dbFeatureDefinitionPointPool = DatabaseRepository.GetDatabase<FeatureDefinitionPointPool>();
        var pointPool2BonusFeats = dbFeatureDefinitionPointPool.GetElement("PointPool2BonusFeats");

        foreach (var characterClassDefinition in dbCharacterClassDefinition)
        {
            foreach (var level in levels)
            {
                var featureUnlockPointPool1 = new FeatureUnlockByLevel(PointPoolBonusFeat, level);
                var featureUnlockPointPool2 = new FeatureUnlockByLevel(pointPool2BonusFeats, level);

                if (Main.Settings.EnableFeatsAtEvenLevels)
                {
                    if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool1))
                    {
                        characterClassDefinition.FeatureUnlocks.Remove(featureUnlockPointPool1);
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool2);
                    }
                    else
                    {
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool1);
                    }
                }
                else
                {
                    if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool2))
                    {
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool1);
                    }
                    else if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool1))
                    {
                        characterClassDefinition.FeatureUnlocks.Remove(featureUnlockPointPool1);
                    }
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

    private static void BuildFeatureUnlocks(int initialFeats, bool alternateHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelNonHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelHuman)
    {
        var featureDefinitionPointPoolDb = DatabaseRepository.GetDatabase<FeatureDefinitionPointPool>();
        string name;

        featureUnlockByLevelNonHuman = null;
        featureUnlockByLevelHuman = null;

        switch (initialFeats)
        {
            case 0:
            {
                if (alternateHuman)
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);
                }

                break;
            }
            case 1:
            {
                featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);

                name = "PointPool2BonusFeats";
                if (alternateHuman && featureDefinitionPointPoolDb.TryGetElement(name, out var pointPool2BonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPool2BonusFeats, 1);
                }

                break;
            }
            case > 1:
            {
                name = $"PointPool{initialFeats}BonusFeats";
                if (featureDefinitionPointPoolDb.TryGetElement(name, out var featureDefinitionPointPool))
                {
                    featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(featureDefinitionPointPool, 1);
                }

                name = $"PointPool{initialFeats + 1}BonusFeats";
                if (alternateHuman && featureDefinitionPointPoolDb.TryGetElement(name, out var pointPoolXBonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPoolXBonusFeats, 1);
                }

                break;
            }
        }
    }

    private static void LoadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = DatabaseHelper.CharacterRaceDefinitions.Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman, out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                     .GetAllElements())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    human.FeatureUnlocks.Add(featureUnlockByLevelHuman);
                }

                var pointPoolAbilityScoreImprovement =
                    new FeatureUnlockByLevel(PointPoolAbilityScoreImprovement, 1);
                human.FeatureUnlocks.Add(pointPoolAbilityScoreImprovement);

                var pointPoolHumanSkillPool = new FeatureUnlockByLevel(PointPoolHumanSkillPool, 1);
                human.FeatureUnlocks.Add(pointPoolHumanSkillPool);

                Remove(human,
                    DatabaseHelper.FeatureDefinitionAttributeModifiers
                        .AttributeModifierHumanAbilityScoreIncrease);
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

    private static void UnloadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = DatabaseHelper.CharacterRaceDefinitions.Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman, out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                     .GetAllElements())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    Remove(human, featureUnlockByLevelHuman);
                }

                Remove(human, PointPoolAbilityScoreImprovement);
                Remove(human, PointPoolHumanSkillPool);

                var humanAttributeIncrease = new FeatureUnlockByLevel(
                    DatabaseHelper.FeatureDefinitionAttributeModifiers
                        .AttributeModifierHumanAbilityScoreIncrease, 1);
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

    private static void Remove([NotNull] CharacterRaceDefinition characterRaceDefinition, BaseDefinition toRemove)
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

    private static void Remove([NotNull] CharacterRaceDefinition characterRaceDefinition,
        [NotNull] FeatureUnlockByLevel featureUnlockByLevel)
    {
        Remove(characterRaceDefinition, featureUnlockByLevel.FeatureDefinition);
    }

    private static bool IsSubRace(CharacterRaceDefinition raceDefinition)
    {
        return DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
            .Any(crd => crd.SubRaces.Contains(raceDefinition));
    }
}
