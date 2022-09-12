﻿using SolastaUnfinishedBusiness.Api.ModKit;

namespace SolastaUnfinishedBusiness.Displays;

internal static class KeyboardAndMouseDisplay
{
    private static bool SelectAll { get; set; } =
        Main.Settings.EnableHotkeyToggleIndividualHud &&
        Main.Settings.EnableHotkeyToggleHud &&
        Main.Settings.EnableCharacterExport &&
        Main.Settings.EnableHotkeyDebugOverlay &&
        Main.Settings.EnableHotkeyZoomCamera &&
        Main.Settings.EnableTeleportParty &&
        Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView &&
        Main.Settings.InvertAltBehaviorOnTooltips;

    private static void UpdateSettings(bool flag)
    {
        Main.Settings.EnableHotkeyToggleIndividualHud = flag;
        Main.Settings.EnableHotkeyToggleHud = flag;
        Main.Settings.EnableCharacterExport = flag;
        Main.Settings.EnableHotkeyDebugOverlay = flag;
        Main.Settings.EnableHotkeyZoomCamera = flag;
        Main.Settings.EnableTeleportParty = flag;
        Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView = flag;
        Main.Settings.InvertAltBehaviorOnTooltips = flag;
    }

    internal static void DisplayKeyboardAndMouse()
    {
        #region Hotkeys

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&General"));
        UI.Label("");

        var toggle = SelectAll;
        if (UI.Toggle(Gui.Localize("ModUi/&SelectAll"), ref toggle, UI.AutoWidth()))
        {
            SelectAll = toggle;
            UpdateSettings(SelectAll);
        }

        UI.Label("");

        // NO NEED TO TRANSLATE THIS
        toggle = Main.Settings.EnableHotkeyToggleIndividualHud;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyToggleIndividualHud"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyToggleIndividualHud = toggle;
            SelectAll = false;
        }

        toggle = Main.Settings.EnableHotkeyToggleHud;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyToggleHud"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyToggleHud = toggle;
            SelectAll = false;
        }

        UI.Label("");

        toggle = Main.Settings.EnableCharacterExport;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCharacterExport"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableCharacterExport = toggle;
            SelectAll = false;
        }

        UI.Label("");

        toggle = Main.Settings.EnableHotkeyDebugOverlay;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyDebugOverlay"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyDebugOverlay = toggle;
            SelectAll = false;
        }

        toggle = Main.Settings.EnableHotkeyZoomCamera;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyZoomCamera"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyZoomCamera = toggle;
            SelectAll = false;
        }

        toggle = Main.Settings.EnableTeleportParty;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableTeleportParty"), ref toggle))
        {
            Main.Settings.EnableTeleportParty = toggle;
            SelectAll = false;
        }

        UI.Label("");

        toggle = Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView;
        if (UI.Toggle(Gui.Localize("ModUi/&AltOnlyHighlightItemsInPartyFieldOfView"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AltOnlyHighlightItemsInPartyFieldOfView = toggle;
            SelectAll = false;
        }

        toggle = Main.Settings.InvertAltBehaviorOnTooltips;
        if (UI.Toggle(Gui.Localize("ModUi/&InvertAltBehaviorOnTooltips"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.InvertAltBehaviorOnTooltips = toggle;
            SelectAll = false;
        }

        #endregion

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&MulticlassKeyHelp"));
        UI.Label("");
    }
}
