﻿using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Displays;

internal static class GameUiDisplay
{
    private static bool _selectedForSwap;
    private static int _selectedX, _selectedY;
    private static readonly string[] SetNames = { "1", "2", "3", "4", "5" };

    private static void DisplayFormationGrid()
    {
        var selectedSet = Main.Settings.FormationGridSelectedSet;

        using (UI.HorizontalScope())
        {
            UI.ActionButton(Gui.Localize("ModUi/&FormationResetAllSets"), () =>
                {
                    _selectedForSwap = false;
                    GameUiContext.ResetAllFormationGrids();
                },
                UI.Width((float)110));

            if (UI.SelectionGrid(ref selectedSet, SetNames, SetNames.Length, SetNames.Length, UI.Width((float)165)))
            {
                _selectedForSwap = false;
                Main.Settings.FormationGridSelectedSet = selectedSet;
                GameUiContext.FillDefinitionFromFormationGrid();
            }

            UI.Label(Gui.Localize("ModUi/&FormationHelp1"));
        }

        UI.Label();

        for (var y = 0; y < GameUiContext.GridSize; y++)
        {
            using (UI.HorizontalScope())
            {
                // first line
                if (y == 0)
                {
                    UI.ActionButton(Gui.Localize("ModUi/&FormationResetThisSet"), () =>
                        {
                            _selectedForSwap = false;
                            GameUiContext.ResetFormationGrid(Main.Settings.FormationGridSelectedSet);
                        },
                        UI.Width((float)110));
                }
                else
                {
                    UI.Label("", UI.Width((float)110));
                }

                for (var x = 0; x < GameUiContext.GridSize; x++)
                {
                    var saveColor = GUI.color;
                    string label;

                    if (Main.Settings.FormationGridSets[selectedSet][y][x] == 1)
                    {
                        // yep 256 not 255 for a light contrast
                        GUI.color = new Color(0x1E / 256f, 0x81 / 256f, 0xB0 / 256f);
                        label = "@";
                    }
                    else
                    {
                        label = "..";
                    }

                    if (_selectedForSwap && _selectedX == x && _selectedY == y)
                    {
                        label = $"<b><color=red>{label}</color></b>";
                    }

                    UI.ActionButton(label, () =>
                    {
                        // ReSharper disable once InlineTemporaryVariable
                        // ReSharper disable once AccessToModifiedClosure
                        var localX = x;
                        // ReSharper disable once InlineTemporaryVariable
                        // ReSharper disable once AccessToModifiedClosure
                        var localY = y;

                        if (_selectedForSwap)
                        {
                            (Main.Settings.FormationGridSets[selectedSet][localY][localX],
                                Main.Settings.FormationGridSets[selectedSet][_selectedY][_selectedX]) = (
                                Main.Settings.FormationGridSets[selectedSet][_selectedY][_selectedX],
                                Main.Settings.FormationGridSets[selectedSet][localY][localX]);

                            GameUiContext.FillDefinitionFromFormationGrid();

                            _selectedForSwap = false;
                        }
                        else
                        {
                            _selectedX = localX;
                            _selectedY = localY;
                            _selectedForSwap = true;
                        }
                    }, UI.Width((float)30));

                    GUI.color = saveColor;
                }

                // first line
                if (y <= 1)
                {
                    UI.Label(Gui.Localize("ModUi/&FormationHelp" + (y + 2)));
                }
            }
        }
    }

    internal static void DisplayGameUi()
    {
        #region Campaign

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&CampaignsAndLocations"));
        UI.Label();

        var toggle = Main.Settings.DontFollowCharacterInBattle;
        if (UI.Toggle(Gui.Localize("ModUi/&DontFollowCharacterInBattle"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DontFollowCharacterInBattle = toggle;
        }

        if (Main.Settings.DontFollowCharacterInBattle)
        {
            var intValue = Main.Settings.DontFollowMargin;
            if (UI.Slider(Gui.Localize("ModUi/&DontFollowMargin"), ref intValue, 0, 20,
                    1, "%", UI.AutoWidth()))
            {
                Main.Settings.DontFollowMargin = intValue;
            }
        }

        UI.Label();

        toggle = Main.Settings.EnableStatsOnHeroTooltip;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableStatsOnHeroTooltip"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableStatsOnHeroTooltip = toggle;
        }

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

        UI.Label();

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

        UI.Label();

        toggle = Main.Settings.AllowMoreRealStateOnRestPanel;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowMoreRealStateOnRestPanel"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowMoreRealStateOnRestPanel = toggle;
        }

        UI.Label();

        toggle = Main.Settings.AddMonkKiPointsToggle;
        if (UI.Toggle(Gui.Localize("ModUi/&AddMonkKiPointsToggle"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddMonkKiPointsToggle = toggle;
        }

        toggle = Main.Settings.AddPaladinSmiteToggle;
        if (UI.Toggle(Gui.Localize("ModUi/&AddPaladinSmiteToggle"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddPaladinSmiteToggle = toggle;
        }

        toggle = Main.Settings.ShowChannelDivinityOnPortrait;
        if (UI.Toggle(Gui.Localize("ModUi/&ShowChannelDivinityOnPortrait"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ShowChannelDivinityOnPortrait = toggle;
        }

        #endregion

        #region Formation

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Formation"));
        UI.Label();

        if (Global.IsMultiplayer)
        {
            UI.Label(Gui.Localize("ModUi/&FormationError"));
        }
        else
        {
            DisplayFormationGrid();
        }

        #endregion

        #region Input

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Input"));
        UI.Label();

        toggle = Main.Settings.InvertAltBehaviorOnTooltips;
        if (UI.Toggle(Gui.Localize("ModUi/&InvertAltBehaviorOnTooltips"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.InvertAltBehaviorOnTooltips = toggle;
        }

        toggle = Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView;
        if (UI.Toggle(Gui.Localize("ModUi/&AltOnlyHighlightItemsInPartyFieldOfView"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView = toggle;
        }

        UI.Label();

        toggle = Main.Settings.EnableHotkeySwapFormationSets;
        if (UI.Toggle(Gui.Localize("ModUi/&FormationHotkey"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeySwapFormationSets = toggle;
        }

        toggle = Main.Settings.EnableHotkeyToggleHud;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyToggleHud"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyToggleHud = toggle;
        }

        toggle = Main.Settings.EnableCharacterExport;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCharacterExport"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableCharacterExport = toggle;
        }

        toggle = Main.Settings.EnableTeleportParty;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableTeleportParty"), ref toggle))
        {
            Main.Settings.EnableTeleportParty = toggle;
        }

        toggle = Main.Settings.EnableRejoinParty;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableRejoinParty"), ref toggle))
        {
            Main.Settings.EnableRejoinParty = toggle;
        }

        toggle = Main.Settings.EnableCancelEditOnRightMouseClick;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCancelEditOnRightMouseClick"), ref toggle))
        {
            Main.Settings.EnableCancelEditOnRightMouseClick = toggle;
        }

        #endregion

        #region Item

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&InventoryAndItems"));
        UI.Label();

        toggle = Main.Settings.DisableAutoEquip;
        if (UI.Toggle(Gui.Localize("ModUi/&DisableAutoEquip"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DisableAutoEquip = toggle;
        }

        toggle = Main.Settings.EnableInventoryFilteringAndSorting;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInventoryFilteringAndSorting"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInventoryFilteringAndSorting = toggle;
            InventoryManagementContext.RefreshControlsVisibility();
        }

        toggle = Main.Settings.EnableInventoryTaintNonProficientItemsRed;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInventoryTaintNonProficientItemsRed"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInventoryTaintNonProficientItemsRed = toggle;
        }

        toggle = Main.Settings.EnableInventoryTintKnownRecipesRed;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInventoryTintKnownRecipesRed"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInventoryTintKnownRecipesRed = toggle;
        }

        toggle = Main.Settings.EnableInvisibleCrownOfTheMagister;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableInvisibleCrownOfTheMagister"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableInvisibleCrownOfTheMagister = toggle;
            GameUiContext.SwitchCrownOfTheMagister();
        }

        toggle = Main.Settings.ShowCraftedItemOnRecipeIcon;
        if (UI.Toggle(Gui.Localize("ModUi/&ShowCraftedItemOnRecipeIcon"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ShowCraftedItemOnRecipeIcon = toggle;
        }

        if (Main.Settings.ShowCraftedItemOnRecipeIcon)
        {
            toggle = Main.Settings.SwapCraftedItemAndRecipeIcons;
            if (UI.Toggle(Gui.Localize("ModUi/&SwapCraftedItemAndRecipeIcons"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.SwapCraftedItemAndRecipeIcons = toggle;
            }
        }

        #endregion

        #region Monster

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Monsters"));
        UI.Label();

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

        if (Main.Settings.EnableBetaContent)
        {
            toggle = Main.Settings.ShowButtonWithControlledMonsterInfo;
            if (UI.Toggle(Gui.Localize("ModUi/&ShowButtonWithControlledMonsterInfo"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.ShowButtonWithControlledMonsterInfo = toggle;
                if (!toggle)
                {
                    CustomCharacterStatsPanel.MaybeInstance?.Unbind();
                }
            }
        }

        #endregion

        UI.Label();
    }
}
