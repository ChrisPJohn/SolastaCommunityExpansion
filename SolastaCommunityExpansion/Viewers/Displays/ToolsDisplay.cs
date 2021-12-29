﻿using ModKit;
using SolastaCommunityExpansion.Models;

namespace SolastaCommunityExpansion.Viewers.Displays
{
    internal static class ToolsDisplay
    {
        private static bool enableDebugCamera;
        private static bool enableDebugOverlay;

        internal static void DisplayTools()
        {
            bool toggle;
            int intValue;

            UI.Label("");
            UI.Label("Campaigns and Locations:".yellow());
            UI.Label("");

            toggle = Main.Settings.EnableSaveByLocation;
            if (UI.Toggle("Enable save by campaigns / locations", ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnableSaveByLocation = toggle;
            }

            toggle = Main.Settings.EnableTelemaCampaign;
            if (UI.Toggle("Enable the Telema Kickstarter demo location", ref toggle))
            {
                Main.Settings.EnableTelemaCampaign = toggle;
            }

            toggle = Main.Settings.EnableTeleportParty;
            if (UI.Toggle("Enable the hotkey " + "ctrl-shift-(T)eleport".cyan() + " in game locations" + "\nYou might break quests or maps if you teleport to an undiscovered place".italic().yellow(), ref toggle))
            {
                Main.Settings.EnableTeleportParty = toggle;
            }

            UI.Label("");

            toggle = Main.Settings.OverrideMinMaxLevel;
            if (UI.Toggle("Override required min / max level", ref toggle))
            {
                Main.Settings.OverrideMinMaxLevel = toggle;
            }

            UI.Label("");

            intValue = Main.Settings.OverridePartySize;
            if (UI.Slider("Override party size ".white() + "[only in custom dungeons]".italic().yellow(), ref intValue, DungeonMakerContext.MIN_PARTY_SIZE, DungeonMakerContext.MAX_PARTY_SIZE, DungeonMakerContext.GAME_PARTY_SIZE, "", UI.AutoWidth()))
            {
                Main.Settings.OverridePartySize = intValue;
            }

            UI.Label("");

            intValue = Main.Settings.maxBackupFilesPerLocationCampaign;
            if (UI.Slider("Max. backup files per location or campaign".white(), ref intValue, 0, 20, 10))
            {
                Main.Settings.maxBackupFilesPerLocationCampaign = intValue;
            }

            UI.Label("");
            UI.Label(". Backup files are saved under " + "GAME_FOLDER/Mods/SolastaCommunityExpansion/DungeonMakerBackups".italic().yellow());
            UI.Label("");
            UI.Label("Debug:".yellow());
            UI.Label("");

            toggle = Main.Settings.EnableCheatMenu;
            if (UI.Toggle("Enable the cheats menu", ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnableCheatMenu = toggle;
            }

            if (UI.Toggle("Enable the debug camera", ref enableDebugCamera, UI.AutoWidth()))
            {
                IViewService viewService = ServiceRepository.GetService<IViewService>();
                ICameraService cameraService = ServiceRepository.GetService<ICameraService>();

                if (viewService == null || cameraService == null)
                {
                    enableDebugCamera = false;
                }
                else
                {
                    cameraService.DebugCameraEnabled = enableDebugCamera;
                }
            }

            if (UI.Toggle("Enable the debug overlay", ref enableDebugOverlay, UI.AutoWidth()))
            {
                ServiceRepository.GetService<IDebugOverlayService>().ToggleActivation();
            }

            UI.Label("");
            UI.Label("Experience:".yellow());
            UI.Label("");

            toggle = Main.Settings.NoExperienceOnLevelUp;
            if (UI.Toggle("No experience is required to level up", ref toggle, UI.AutoWidth()))
            {
                Main.Settings.NoExperienceOnLevelUp = toggle;
            }

            UI.Label("");

            intValue = Main.Settings.MultiplyTheExperienceGainedBy;
            if (UI.Slider("Multiply the experience gained by ".white() + "[%]".red(), ref intValue, 0, 200, 100, "", UI.Width(100)))
            {
                Main.Settings.MultiplyTheExperienceGainedBy = intValue;
            }

            UI.Label("");
            UI.Label("Faction Relations:".yellow());
            UI.Label("");

            bool flip = true;
            var gameCampaign = Gui.GameCampaign;
            var gameFactionService = ServiceRepository.GetService<IGameFactionService>();

            if (gameFactionService != null && gameCampaign?.CampaignDefinitionName != "UserCampaign")
            {
                foreach (FactionDefinition faction in gameFactionService.RegisteredFactions)
                {
                    if (faction.BuiltIn)
                    {
                        // These are things like monster factions, generally set to a specific relation and can't be changed.
                        continue;
                    }

                    if (faction.GuiPresentation.Hidden)
                    {
                        // These are things like Silent Whispers and Church Of Einar that are not fully implemented factions.
                        continue;
                    }

                    string title = faction.FormatTitle();

                    if (flip)
                    {
                        title = title.yellow();
                    }
                    else
                    {
                        title = title.white();
                    }

                    intValue = gameFactionService.FactionRelations[faction.Name];

                    if (UI.Slider("                              " + title, ref intValue, faction.MinRelationCap, faction.MaxRelationCap, 0, "", UI.AutoWidth()))
                    {
                        SetFactionRelationsContext.SetFactionRelation(faction.Name, intValue);
                    }

                    flip = !flip;
                }
            }
            else
            {
                UI.Label("Load an official campaign game to modify faction relations...".red());
            }

            UI.Label("");
        }
    }
}
