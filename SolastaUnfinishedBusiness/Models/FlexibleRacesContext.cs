﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;

namespace SolastaUnfinishedBusiness.Models;

internal static class FlexibleRacesContext
{
    private static readonly FeatureUnlockByLevel AttributeChoiceThree = new(
        FeatureDefinitionPointPoolBuilder
            .Create("PointPoolAbilityScore3", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.FlexibleRaces)
            .SetPool(HeroDefinitions.PointsPoolType.AbilityScore, 3)
            .AddToDB(),
        1);

    private static readonly FeatureUnlockByLevel AttributeChoiceFour = new(
        FeatureDefinitionPointPoolBuilder
            .Create("PointPoolAbilityScore4", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.FlexibleRaces)
            .SetPool(HeroDefinitions.PointsPoolType.AbilityScore, 4)
            .AddToDB(),
        1);

    private static readonly Dictionary<string, FeatureUnlockByLevel> AddedFeatures = new()
    {
        { "Dragonborn", AttributeChoiceThree },
        { "Dwarf", AttributeChoiceThree },
        { "Elf", AttributeChoiceThree },
        { "Halfling", AttributeChoiceThree },
        { "HalfElf", AttributeChoiceFour },
        { "HalfOrc", AttributeChoiceThree },
        // unofficial races
        { "RaceBolgrif", AttributeChoiceThree },
        { "RaceHalfElfVariant", AttributeChoiceFour }
    };

    private static readonly Dictionary<string, List<string>> RemovedFeatures = new()
    {
        { "Dragonborn", new List<string> { "FeatureSetDragonbornAbilityScoreIncrease" } },
        { "Dwarf", new List<string> { "AttributeModifierDwarfAbilityScoreIncrease" } },
        { "Elf", new List<string> { "AttributeModifierElfAbilityScoreIncrease" } },
        { "Halfling", new List<string> { "AttributeModifierHalflingAbilityScoreIncrease" } },
        { "HalfElf", new List<string> { "FeatureSetHalfElfAbilityScoreIncrease" } },
        { "DwarfHill", new List<string> { "AttributeModifierDwarfHillAbilityScoreIncrease" } },
        { "DwarfSnow", new List<string> { "AttributeModifierDwarfSnowAbilityScoreIncrease" } },
        { "ElfHigh", new List<string> { "AttributeModifierElfHighAbilityScoreIncrease" } },
        { "ElfSylvan", new List<string> { "AttributeModifierElfSylvanAbilityScoreIncrease" } },
        { "HalflingIsland", new List<string> { "AttributeModifierHalflingIslandAbilityScoreIncrease" } },
        { "HalflingMarsh", new List<string> { "AttributeModifierHalflingMarshAbilityScoreIncrease" } },
        { "HalfOrc", new List<string> { "FeatureSetHalfOrcAbilityScoreIncrease" } },
        // unofficial races
        {
            "RaceBolgrif",
            new List<string>
            {
                "AttributeModifierBolgrifStrengthAbilityScoreIncrease",
                "AttributeModifierBolgrifWisdomAbilityScoreIncrease"
            }
        },
        { "RaceDarkelf", new List<string> { "AttributeModifierDarkelfCharismaAbilityScoreIncrease" } },
        { "RaceHalfElfVariant", new List<string> { "FeatureSetHalfElfAbilityScoreIncrease" } },
        { "RaceGrayDwarf", new List<string> { "AttributeModifierGrayDwarfStrengthAbilityScoreIncrease" } }
    };

    private static void RemoveMatchingFeature([NotNull] List<FeatureUnlockByLevel> unlocks, BaseDefinition toRemove)
    {
        unlocks.RemoveAll(u => u.FeatureDefinition.GUID == toRemove.GUID);
    }

    internal static void LateLoad()
    {
        Switch();
    }

    internal static void Switch()
    {
        var enabled = Main.Settings.EnableFlexibleRaces;
        var dbCharacterRaceDefinition = DatabaseRepository.GetDatabase<CharacterRaceDefinition>();
        var dbFeatureDefinition = DatabaseRepository.GetDatabase<FeatureDefinition>();

        foreach (var keyValuePair in AddedFeatures)
        {
            var characterRaceDefinition = dbCharacterRaceDefinition.GetElement(keyValuePair.Key, true);

            if (characterRaceDefinition == null)
            {
                continue;
            }

            var exists = characterRaceDefinition.FeatureUnlocks.Exists(x =>
                x.FeatureDefinition == keyValuePair.Value.FeatureDefinition);

            switch (exists)
            {
                case false when enabled:
                    characterRaceDefinition.FeatureUnlocks.Add(keyValuePair.Value);
                    break;
                case true when !enabled:
                    characterRaceDefinition.FeatureUnlocks.Remove(keyValuePair.Value);
                    break;
            }
        }

        foreach (var keyValuePair in RemovedFeatures)
        {
            var characterRaceDefinition = dbCharacterRaceDefinition.GetElement(keyValuePair.Key, true);

            if (characterRaceDefinition == null)
            {
                continue;
            }

            foreach (var featureDefinitionName in keyValuePair.Value)
            {
                var featureDefinition = dbFeatureDefinition.GetElement(featureDefinitionName, true);

                if (featureDefinition == null)
                {
                    continue;
                }

                var exists =
                    characterRaceDefinition.FeatureUnlocks.Exists(x => x.FeatureDefinition == featureDefinition);

                switch (exists)
                {
                    case true when enabled:
                        RemoveMatchingFeature(characterRaceDefinition.FeatureUnlocks, featureDefinition);
                        break;
                    case false when !enabled:
                        characterRaceDefinition.FeatureUnlocks.Add(new FeatureUnlockByLevel(featureDefinition, 1));
                        break;
                }
            }
        }
    }
}
