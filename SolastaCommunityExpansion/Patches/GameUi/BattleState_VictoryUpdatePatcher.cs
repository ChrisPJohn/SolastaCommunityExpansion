﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.GameUi
{
    [HarmonyPatch(typeof(BattleState_Victory), "Update")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class BattleState_VictoryUpdatePatcher
    {
        public static void Postfix()
        {
            if (!Main.Settings.AutoPauseOnVictory) return;

            var battleService = ServiceRepository.GetService<IGameLocationBattleService>();

            if (battleService == null) return;
            if (battleService.Battle != null) return;

            INarrativeDirectionService narrativeService = ServiceRepository.GetService<INarrativeDirectionService>();

            if (narrativeService != null && narrativeService.CurrentSequence != null)
            {
                // Don't pause in the middle of a narrative sequence it hangs the game.
                // For example during the tutorial shoving the rock to destroy the bridge transitions
                // directly into a narrative sequence. I believe there are several other battle ->
                // narrative transitions in the game (like the crown vision paladin fight).
                return;
            }

            var campaign = ServiceRepository.GetService<IGameService>()?.Game?.GameCampaign;

            if (campaign == null) return;

            campaign.GameTime.Pause();
        }
    }
}
