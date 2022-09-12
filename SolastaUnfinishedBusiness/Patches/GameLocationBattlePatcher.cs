﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaUnfinishedBusiness.Patches;

//PATCH: EnableEnemiesControlledByPlayer
[HarmonyPatch(typeof(GameLocationBattle), "GetMyContenders")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class GetMyContenders_Patch
{
    internal static void Postfix(GameLocationBattle __instance, ref List<GameLocationCharacter> __result)
    {
        if (!Main.Settings.EnableEnemiesControlledByPlayer || __instance == null)
        {
            return;
        }

        var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();

        if (!gameLocationCharacterService.PartyCharacters.Contains(__instance.ActiveContender)
            && !gameLocationCharacterService.GuestCharacters.Contains(__instance.ActiveContender))
        {
            __result = __instance.EnemyContenders;
        }
    }
}

//PATCH: EnableEnemiesControlledByPlayer
[HarmonyPatch(typeof(GameLocationBattle), "GetOpposingContenders")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class GetOpposingContenders_Patch
{
    internal static void Postfix(GameLocationBattle __instance, ref List<GameLocationCharacter> __result)
    {
        if (!Main.Settings.EnableEnemiesControlledByPlayer || __instance == null)
        {
            return;
        }

        var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();

        if (!gameLocationCharacterService.PartyCharacters.Contains(__instance.ActiveContender)
            && !gameLocationCharacterService.GuestCharacters.Contains(__instance.ActiveContender))
        {
            __result = __instance.PlayerContenders;
        }
    }
}
