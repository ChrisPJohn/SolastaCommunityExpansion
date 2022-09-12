﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

internal static class GameCampaignPartyPatcher
{
    //PATCH: Correctly updates the level cap under Level 20 scenarios
    [HarmonyPatch(typeof(GameCampaignParty), "UpdateLevelCaps")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UpdateLevelCaps_Patch
    {
        internal static bool Prefix([NotNull] GameCampaignParty __instance, int levelCap)
        {
            var max = Main.Settings.EnableLevel20 ? Level20Context.ModMaxLevel : Level20Context.GameMaxLevel;

            levelCap = Main.Settings.OverrideMinMaxLevel ? Level20Context.ModMaxLevel : levelCap;

            foreach (var character in __instance.CharactersList)
            {
                var characterLevel = character.RulesetCharacter.GetAttribute(AttributeDefinitions.CharacterLevel);
                var experience = character.RulesetCharacter.GetAttribute(AttributeDefinitions.Experience);

                characterLevel.MaxValue = levelCap > 0 ? Mathf.Min(levelCap, max) : max;
                characterLevel.Refresh();

                experience.MaxValue = HeroDefinitions.MaxHeroExperience(characterLevel.MaxValue);
                experience.Refresh();
            }

            return false;
        }
    }
}
