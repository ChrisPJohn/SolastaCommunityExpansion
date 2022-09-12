﻿using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

internal static class GameUiDisplay
{
    internal static void DisplayGameUi()
    {
        int intValue;

        #region Battle

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&Battle"));
        UI.Label("");

        var toggle = Main.Settings.DontFollowCharacterInBattle;
        if (UI.Toggle(Gui.Localize("ModUi/&DontFollowCharacterInBattle"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DontFollowCharacterInBattle = toggle;
        }

        if (Main.Settings.DontFollowCharacterInBattle)
        {
            intValue = Main.Settings.DontFollowMargin;
            if (UI.Slider(Gui.Localize("ModUi/&DontFollowMargin"), ref intValue, 0, 20,
                    1, "%", UI.AutoWidth()))
            {
                Main.Settings.DontFollowMargin = intValue;
            }

            UI.Label("");
        }

        toggle = Main.Settings.AutoPauseOnVictory;
        if (UI.Toggle(Gui.Localize("ModUi/&AutoPauseOnVictory"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AutoPauseOnVictory = toggle;
        }

        UI.Label("");

        var floatValue = Main.Settings.FasterTimeModifier;
        if (UI.Slider(Gui.Localize("ModUi/&FasterTimeModifier"), ref floatValue, 1.5f, 10f, 1.5f,
                1, "X", UI.AutoWidth()))
        {
            Main.Settings.FasterTimeModifier = floatValue;
        }

        #endregion

        #region Campaign

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&CampaignsAndLocations"));
        UI.Label("");

        toggle = Main.Settings.FollowCharactersOnTeleport;
        if (UI.Toggle(Gui.Localize("ModUi/&FollowCharactersOnTeleport"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.FollowCharactersOnTeleport = toggle;
        }

        UI.Label("");

        toggle = Main.Settings.EnableAdditionalBackstoryDisplay;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableAdditionalBackstoryDisplay"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableAdditionalBackstoryDisplay = toggle;
        }

        toggle = Main.Settings.EnableLogDialoguesToConsole;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableLogDialoguesToConsole"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableLogDialoguesToConsole = toggle;
        }

        UI.Label("");

        toggle = Main.Settings.EnableStatsOnHeroTooltip;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableStatsOnHeroTooltip"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableStatsOnHeroTooltip = toggle;
        }

        toggle = Main.Settings.EnableAdditionalIconsOnLevelMap;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableAdditionalIconsOnLevelMap"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableAdditionalIconsOnLevelMap = toggle;

            if (toggle)
            {
                Main.Settings.MarkInvisibleTeleportersOnLevelMap = false;
            }
        }

        if (Main.Settings.EnableAdditionalIconsOnLevelMap)
        {
            toggle = Main.Settings.MarkInvisibleTeleportersOnLevelMap;
            if (UI.Toggle(Gui.Localize("ModUi/&MarkInvisibleTeleportersOnLevelMap"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.MarkInvisibleTeleportersOnLevelMap = toggle;
            }
        }

        toggle = Main.Settings.HideExitsAndTeleportersGizmosIfNotDiscovered;
        if (UI.Toggle(Gui.Localize("ModUi/&HideExitAndTeleporterGizmosIfNotDiscovered"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.HideExitsAndTeleportersGizmosIfNotDiscovered = toggle;
        }

        #endregion

        #region Item

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&InventoryAndItems"));
        UI.Label("");

        toggle = Main.Settings.EnableInventoryFilteringAndSorting;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInventoryFilteringAndSorting"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInventoryFilteringAndSorting = toggle;
            Main.Settings.EnableInventoryTaintNonProficientItemsRed = toggle;
            InventoryManagementContext.RefreshControlsVisibility();
        }

        if (Main.Settings.EnableInventoryFilteringAndSorting)
        {
            toggle = Main.Settings.EnableInventoryTaintNonProficientItemsRed;
            if (UI.Toggle(Gui.Localize("ModUi/&EnableInventoryTaintNonProficientItemsRed"), ref toggle,
                    UI.AutoWidth()))
            {
                Main.Settings.EnableInventoryTaintNonProficientItemsRed = toggle;
            }
        }

        toggle = Main.Settings.EnableInvisibleCrownOfTheMagister;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInvisibleCrownOfTheMagister"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInvisibleCrownOfTheMagister = toggle;
            ItemOptionsContext.SwitchCrownOfTheMagister();
        }

        UI.Label("");

        using (UI.HorizontalScope())
        {
            UI.Label(Gui.Localize("ModUi/&EmpressGarbAppearance"), UI.Width(325));

            intValue = Main.Settings.EmpressGarbAppearanceIndex;
            if (UI.SelectionGrid(ref intValue, ItemOptionsContext.EmpressGarbAppearances,
                    ItemOptionsContext.EmpressGarbAppearances.Length, 2, UI.Width(440)))
            {
                Main.Settings.EmpressGarbAppearanceIndex = intValue;
                ItemOptionsContext.SwitchEmpressGarb();
            }
        }

        #endregion

        #region Monster

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&Monsters"));
        UI.Label("");

        toggle = Main.Settings.HideMonsterHitPoints;
        if (UI.Toggle(Gui.Localize("ModUi/&HideMonsterHitPoints"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.HideMonsterHitPoints = toggle;
        }

        toggle = Main.Settings.RemoveBugVisualModels;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveBugVisualModels"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveBugVisualModels = toggle;
        }

        #endregion

        #region Spell

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&Spells"));
        UI.Label("");

        intValue = Main.Settings.MaxSpellLevelsPerLine;
        if (UI.Slider(Gui.Localize("ModUi/&MaxSpellLevelsPerLine"), ref intValue, 3, 7, 5, "", UI.AutoWidth()))
        {
            Main.Settings.MaxSpellLevelsPerLine = intValue;
        }

        #endregion

        UI.Label("");
    }
}
