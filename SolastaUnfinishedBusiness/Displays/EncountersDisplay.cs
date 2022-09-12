﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

public static class EncountersDisplay
{
    private static bool _showStats;

    private static bool _showAttributes;

    private static readonly Dictionary<MonsterDefinition, bool> CurrentFeaturesMonster = new();

    private static readonly Dictionary<MonsterDefinition, bool> CurrentAttacksMonster = new();

    private static readonly Dictionary<RulesetCharacterHero, bool> CurrentItemsHeroes = new();

    [NotNull]
    private static string SplitCamelCase([NotNull] string str)
    {
        return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
    }

    private static void DisplayHeroStats([NotNull] RulesetCharacterHero hero, string actionText, Action action)
    {
        bool flip;
        var inventory = hero.CharacterInventory.EnumerateAllSlots(false, true);

        using (UI.HorizontalScope())
        {
            UI.ActionButton(actionText.Bold().Red(), action, UI.Width(30));
            UI.Label($"{hero.Name} {hero.SurName}".Orange().Bold(), UI.Width(240));
            UI.Label($"{hero.RaceDefinition.FormatTitle()} {hero.ClassesHistory[0].FormatTitle()}".White(),
                UI.Width(120));

            var attributesLabel = _showAttributes ? "" : "Attributes";

            UI.DisclosureToggle(attributesLabel, ref _showAttributes, attributesLabel.Length * 12);

            if (_showAttributes)
            {
                UI.Label($"Str: {hero.GetAttribute(AttributeDefinitions.Strength).CurrentValue:0#}".White(),
                    UI.Width(48));
                UI.Label($"Dex: {hero.GetAttribute(AttributeDefinitions.Dexterity).CurrentValue:0#}".White(),
                    UI.Width(48));
                UI.Label($"Con: {hero.GetAttribute(AttributeDefinitions.Constitution).CurrentValue:0#}".Khaki(),
                    UI.Width(48));
                UI.Label($"Int: {hero.GetAttribute(AttributeDefinitions.Intelligence).CurrentValue:0#}".Khaki(),
                    UI.Width(48));
                UI.Label($"Wis: {hero.GetAttribute(AttributeDefinitions.Wisdom).CurrentValue:0#}".White(),
                    UI.Width(48));
                UI.Label($"Cha: {hero.GetAttribute(AttributeDefinitions.Charisma).CurrentValue:0#}".Khaki(),
                    UI.Width(48));
            }

            var statsLabel = _showStats ? "" : "Stats";

            UI.DisclosureToggle(statsLabel, ref _showStats, statsLabel.Length * 12);

            if (_showStats)
            {
                UI.Label($"AC: {hero.GetAttribute(AttributeDefinitions.ArmorClass).CurrentValue:0#}".White(),
                    UI.Width(48));
                UI.Label($"HD: {hero.MaxHitDiceCount():0#}{hero.MainHitDie}".Khaki(), UI.Width(72));
                UI.Label($"XP: {hero.GetAttribute(AttributeDefinitions.Experience).CurrentValue}".White(),
                    UI.Width(72));
                UI.Label($"LV: {hero.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue:0#}".White(),
                    UI.Width(48));
            }

            CurrentItemsHeroes.TryGetValue(hero, out flip);

            if (UI.DisclosureToggle("Inventory", ref flip, 132))
            {
                CurrentItemsHeroes.AddOrReplace(hero, flip);
            }
        }

        CurrentItemsHeroes.TryGetValue(hero, out flip);

        if (!flip)
        {
            return;
        }

        using (UI.VerticalScope())
        {
            using (UI.HorizontalScope())
            {
                UI.Space(30);
                UI.Label("Inventory".Bold().Cyan());
            }

            foreach (var slot in inventory)
            {
                if (slot.EquipedItem == null)
                {
                    continue;
                }

                using (UI.HorizontalScope())
                {
                    UI.Space(60);
                    UI.Label(slot.EquipedItem.ItemDefinition.FormatTitle(), UI.Width(192));
                }
            }
        }
    }

    private static void DisplayMonsterStats([NotNull] MonsterDefinition monsterDefinition, string actionText,
        Action action)
    {
        bool flip;

        using (UI.HorizontalScope())
        {
            UI.ActionButton(actionText.Bold().Red(), action, UI.Width(30));
            UI.Label($"{monsterDefinition.FormatTitle()}".Orange().Bold(), UI.Width(240));
            UI.Label($"{SplitCamelCase(monsterDefinition.Alignment)}".White(), UI.Width(120));

            var attributesLabel = _showAttributes ? "" : "Attributes";

            UI.DisclosureToggle(attributesLabel, ref _showAttributes, attributesLabel.Length * 12);

            if (_showAttributes)
            {
                UI.Label($"Str: {monsterDefinition.AbilityScores[0]:0#}".White(), UI.Width(48));
                UI.Label($"Dex: {monsterDefinition.AbilityScores[1]:0#}".Khaki(), UI.Width(48));
                UI.Label($"Con: {monsterDefinition.AbilityScores[2]:0#}".White(), UI.Width(48));
                UI.Label($"Int: {monsterDefinition.AbilityScores[3]:0#}".Khaki(), UI.Width(48));
                UI.Label($"Wis: {monsterDefinition.AbilityScores[4]:0#}".White(), UI.Width(48));
                UI.Label($"Cha: {monsterDefinition.AbilityScores[5]:0#}".Khaki(), UI.Width(48));
            }

            var statsLabel = _showStats ? "" : "Stats";

            UI.DisclosureToggle(statsLabel, ref _showStats, statsLabel.Length * 12);

            if (_showStats)
            {
                UI.Label($"AC: {monsterDefinition.ArmorClass}".White(), UI.Width(48));
                UI.Label($"HD: {monsterDefinition.HitDice:0#}{monsterDefinition.HitDiceType}".Khaki(),
                    UI.Width(72));
                UI.Label($"CR: {monsterDefinition.ChallengeRating}".Khaki(), UI.Width(72));
            }

            CurrentAttacksMonster.TryGetValue(monsterDefinition, out flip);

            if (UI.DisclosureToggle($"Attacks ({monsterDefinition.AttackIterations.Count:0#})", ref flip, 132))
            {
                CurrentAttacksMonster.AddOrReplace(monsterDefinition, flip);
            }

            CurrentFeaturesMonster.TryGetValue(monsterDefinition, out flip);

            if (UI.DisclosureToggle($"Features ({monsterDefinition.Features.Count:0#})", ref flip, 144))
            {
                CurrentFeaturesMonster.AddOrReplace(monsterDefinition, flip);
            }
        }

        CurrentFeaturesMonster.TryGetValue(monsterDefinition, out flip);
        if (flip)
        {
            using (UI.VerticalScope())
            {
                using (UI.HorizontalScope())
                {
                    UI.Space(30);
                    UI.Label("Features".Bold().Cyan());
                }

                foreach (var feature in monsterDefinition.Features)
                {
                    using (UI.HorizontalScope())
                    {
                        var title = feature.FormatTitle();

                        if (title == "None")
                        {
                            title = SplitCamelCase(feature.Name);
                        }

                        UI.Space(60);
                        UI.Label(title, UI.Width(192));
                    }
                }
            }
        }

        CurrentAttacksMonster.TryGetValue(monsterDefinition, out flip);

        if (!flip)
        {
            return;
        }

        {
            using (UI.VerticalScope())
            {
                using (UI.HorizontalScope())
                {
                    UI.Space(30);
                    UI.Label("Attacks".Bold().Cyan());
                }

                foreach (var attackIteration in monsterDefinition.AttackIterations)
                {
                    using (UI.HorizontalScope())
                    {
                        var title = attackIteration.MonsterAttackDefinition.FormatTitle();

                        if (title == "None")
                        {
                            title = SplitCamelCase(attackIteration.MonsterAttackDefinition.name);
                        }

                        UI.Space(60);
                        UI.Label(title, UI.Width(192));
                        UI.Label($"action type: {attackIteration.MonsterAttackDefinition.ActionType}".Green(),
                            UI.Width(120));
                        UI.Label($"reach: {attackIteration.MonsterAttackDefinition.ReachRange}".Green(),
                            UI.Width(108));
                        UI.Label($"hit bonus: {attackIteration.MonsterAttackDefinition.ToHitBonus}".Green(),
                            UI.Width(108));
                        UI.Label(
                            attackIteration.MonsterAttackDefinition.MaxUses < 0
                                ? "max uses: inf".Green()
                                : $"max uses: {attackIteration.MonsterAttackDefinition.MaxUses}".Green(),
                            UI.Width(108));

                        if (attackIteration.MonsterAttackDefinition.Magical)
                        {
                            UI.Label(TagsDefinitions.Magical.Green(), UI.Width(108));
                        }
                    }
                }
            }
        }
    }

    internal static void DisplayEncountersGeneral()
    {
        UI.Label("");
        UI.Label("Controllers:".Khaki());
        UI.Label("");

        UI.Label(". Note the encounters feature won't work in a Multiplayer session. Yet...");
        UI.Label("");

        var toggle = Main.Settings.EnableEnemiesControlledByPlayer;
        if (UI.Toggle("Enable enemies controlled by players", ref toggle))
        {
            Main.Settings.EnableEnemiesControlledByPlayer = toggle;
        }

        toggle = Main.Settings.EnableHeroesControlledByComputer;
        if (UI.Toggle("Enable heroes controlled by computer", ref toggle))
        {
            Main.Settings.EnableHeroesControlledByComputer = toggle;

            if (toggle)
            {
                PlayerControllerContext.RefreshGuiState();
            }
        }

        if (Main.Settings.EnableHeroesControlledByComputer)
        {
            UI.Label("");

            if (Global.IsOffGame)
            {
                UI.Label("Load a game to modify heroes AI...".Bold().Red(), UI.AutoWidth());
            }
            else if (Global.IsMultiplayer)
            {
                UI.Label("You can only change controllers in a local session...".Bold().Red(), UI.AutoWidth());
            }
            else
            {
                var controllers = PlayerControllerContext.Controllers;
                var playerCharacters = PlayerControllerContext.PlayerCharacters;
                var playerCharactersChoices = PlayerControllerContext.PlayerCharactersChoices;

                for (var i = 0; i < playerCharacters.Count; i++)
                {
                    // Prevent captured closure
                    var index = i;

                    UI.HStack(playerCharacters[index].Name, 1, () =>
                    {
                        if (UI.SelectionGrid(ref playerCharactersChoices[index], controllers, controllers.Length, 2,
                                UI.Width(300)))
                        {
                            PlayerControllerContext.PlayerCharactersChoices = playerCharactersChoices;
                        }
                    });
                }
            }
        }

        UI.Label("");
        UI.Label("Encounters:".Khaki());
        UI.Label("");

        UI.Label(". encounters only work in custom campaigns or locations");
        UI.Label(". use the Bestiary tab to add monsters to the challenge", UI.AutoWidth());
        UI.Label(". use the Characters Pool tab to add heroes as enemies to the challenge", UI.AutoWidth());
        UI.Label(". click " + "minus".Italic() + " to remove participants from the group");
        UI.Label(
            ". pan the camera to the desired encounter location and press <color=#1E81B0>CTRL-SHIFT-(S)</color>pawn to place the enemies",
            UI.AutoWidth());
        UI.Label("");

        if (EncountersSpawnContext.EncounterCharacters.Count == 0)
        {
            UI.Label("Encounter table is empty...".Red().Bold());
        }
        else
        {
            for (var index = 0; index < EncountersSpawnContext.EncounterCharacters.Count; index++)
            {
                // Prevent captured closure
                var index2 = index;

                switch (EncountersSpawnContext.EncounterCharacters[index2])
                {
                    case RulesetCharacterMonster
                        rulesetCharacterMonster:
                        DisplayMonsterStats(rulesetCharacterMonster.MonsterDefinition, "-",
                            () => EncountersSpawnContext.RemoveFromEncounter(index2));
                        break;
                    case RulesetCharacterHero
                        rulesetCharacterHero:
                        DisplayHeroStats(rulesetCharacterHero, "-",
                            () => EncountersSpawnContext.RemoveFromEncounter(index2));
                        break;
                }
            }
        }

        UI.Label("");
    }

    internal static void DisplayBestiary()
    {
        UI.Label("");
        UI.Label(
            $". Click + to add up to {EncountersSpawnContext.MaxEncounterCharacters} characters to the encounter list");
        UI.Label("");

        foreach (var monsterDefinition in EncountersSpawnContext.GetMonsters())
        {
            DisplayMonsterStats(monsterDefinition, "+",
                () => EncountersSpawnContext.AddToEncounter(monsterDefinition));
        }
    }

    internal static void DisplayNpcs()
    {
        using (UI.VerticalScope(UI.AutoWidth(), UI.AutoHeight()))
        {
            UI.Label("");
            UI.Label(
                $". Click + to add up to {EncountersSpawnContext.MaxEncounterCharacters} characters to the encounter list");
            UI.Label("");

            foreach (var hero in EncountersSpawnContext.GetHeroes())
            {
                DisplayHeroStats(hero, "+", () => EncountersSpawnContext.AddToEncounter(hero));
            }
        }
    }
}
