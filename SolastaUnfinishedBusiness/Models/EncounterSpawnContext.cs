﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TA;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.DecisionPackageDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FactionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FormationDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static class EncountersSpawnContext
{
    internal const int MaxEncounterCharacters = 16;

    private static readonly List<RulesetCharacterHero> Heroes = new();

    private static readonly List<MonsterDefinition> Monsters = new();

    internal static readonly List<RulesetCharacter> EncounterCharacters = new();

    private static ulong EncounterId { get; set; } = 10000;

    internal static void AddToEncounter(RulesetCharacterHero hero)
    {
        if (EncounterCharacters.Count < MaxEncounterCharacters)
        {
            EncounterCharacters.Add(hero);
        }
    }

    internal static void AddToEncounter(MonsterDefinition monsterDefinition)
    {
        if (EncounterCharacters.Count < MaxEncounterCharacters)
        {
            EncounterCharacters.Add(new RulesetCharacterMonster(monsterDefinition, 0,
                new RuleDefinitions.SpawnOverrides(), GadgetDefinitions.CreatureSex.Male));
        }
    }

    internal static void RemoveFromEncounter(int index)
    {
        if (index < EncounterCharacters.Count)
        {
            EncounterCharacters.RemoveAt(index);
        }
    }

    [NotNull]
    internal static List<MonsterDefinition> GetMonsters()
    {
        if (Monsters.Count != 0)
        {
            return Monsters;
        }

        var monsterDefinitionDatabase = DatabaseRepository.GetDatabase<MonsterDefinition>();

        if (monsterDefinitionDatabase == null)
        {
            return Monsters;
        }

        Monsters.AddRange(monsterDefinitionDatabase.Where(x =>
            x.DungeonMakerPresence == MonsterDefinition.DungeonMaker.Monster));
        Monsters.Sort((a, b) => Math.Abs(a.ChallengeRating - b.ChallengeRating) < 0.001f
            ? String.Compare(a.FormatTitle(), b.FormatTitle(), StringComparison.CurrentCultureIgnoreCase)
            : a.ChallengeRating.CompareTo(b.ChallengeRating));

        return Monsters;
    }

    [NotNull]
    internal static List<RulesetCharacterHero> GetHeroes()
    {
        if (Heroes.Count != 0)
        {
            return Heroes;
        }

        var characterPoolService = ServiceRepository.GetService<ICharacterPoolService>();

        if (characterPoolService == null)
        {
            return Heroes;
        }

        foreach (var filename in characterPoolService.Pool.Keys.Select(name =>
                     characterPoolService.BuildCharacterFilename(name.Substring(0, name.Length - 4))))
        {
            characterPoolService.LoadCharacter(filename, out var hero, out _);
            Heroes.Add(hero);
        }

        Heroes.Sort((a, b) =>
        {
            var compareName = String.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);

            if (compareName == 0)
            {
                compareName = String.Compare(a.SurName, b.SurName, StringComparison.CurrentCultureIgnoreCase);
            }

            return compareName;
        });

        return Heroes;
    }

    internal static void ConfirmStageEncounter()
    {
        if (Global.IsMultiplayer)
        {
            Gui.GuiService.ShowMessage(
                MessageModal.Severity.Informative1,
                "Message/&SpawnCustomEncounterTitle",
                "Message/&SpawnCustomEncounterErrorDescription",
                "Message/&MessageOkTitle", string.Empty,
                null,
                null);
        }
        else if (Global.IsUserLocation)
        {
            var position = GetEncounterPosition();

            Gui.GuiService.ShowMessage(
                MessageModal.Severity.Attention2,
                "Message/&SpawnCustomEncounterTitle",
                Gui.Format("Message/&SpawnCustomEncounterDescription", position.x.ToString(),
                    position.x.ToString()),
                "Message/&MessageYesTitle", "Message/&MessageNoTitle",
                () => StageEncounter(position),
                null);
        }
    }

    private static int3 GetEncounterPosition()
    {
        var gameLocationService = ServiceRepository.GetService<IGameLocationService>();

        var x = (int)gameLocationService.GameLocation.LastCameraPosition.x;
        var z = (int)gameLocationService.GameLocation.LastCameraPosition.z;

        return new int3(x, 0, z);
    }

    private static void StageEncounter(int3 position)
    {
        var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();
        var gameLocationPositioningService = ServiceRepository.GetService<IGameLocationPositioningService>();
        var positions = new List<int3>();
        var formationPositions = new List<int3>();
        var sizeList = new List<RulesetActor.SizeParameters>();
        var characters = new List<GameLocationCharacter>();

        for (var iy = 0; iy < 4; iy++)
        {
            for (var ix = 0; ix < 4; ix++)
            {
                formationPositions.Add(new int3(ix, 0, iy));
            }
        }

        foreach (var gameLocationCharacter in EncounterCharacters.Select(character =>
                     gameLocationCharacterService.CreateCharacter(
                         PlayerControllerManager.DmControllerId, character, RuleDefinitions.Side.Enemy,
                         new GameLocationBehaviourPackage
                         {
                             BattleStartBehavior =
                                 GameLocationBehaviourPackage.BattleStartBehaviorType.DoNotRaiseAlarm,
                             DecisionPackageDefinition = IdleGuard_Default,
                             EncounterId = EncounterId++,
                             FormationDefinition = EncounterCharacters.Count > 1 ? Squad4 : SingleCreature
                         })))
        {
            gameLocationCharacter.CollectExistingLightSources(true);
            gameLocationCharacter.RefreshActionPerformances();
            gameLocationCharacter.RulesetCharacter.SetBaseFaction(HostileMonsters);
            characters.Add(gameLocationCharacter);
        }

        gameLocationPositioningService.ComputeFormationPlacementPositions(
            characters, position, LocationDefinitions.Orientation.North, formationPositions,
            CellHelpers.PlacementMode.Station, positions, sizeList, 25);

        for (var index = 0; index < positions.Count; index++)
        {
            gameLocationPositioningService.PlaceCharacter(characters[index], positions[index],
                LocationDefinitions.Orientation.North);
            gameLocationCharacterService.RevealCharacter(characters[index]);
        }

        Heroes.Clear();
        Monsters.Clear();
        EncounterCharacters.Clear();
    }
}
