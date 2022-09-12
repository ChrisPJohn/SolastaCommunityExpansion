﻿using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Subclasses.Rogue;
using SolastaUnfinishedBusiness.Subclasses.Wizard;
using static SolastaUnfinishedBusiness.Displays.Shared;

namespace SolastaUnfinishedBusiness.Displays;

internal static class RacesClassesAndSubclassesDisplay
{
    private static void DisplayGeneral()
    {
        UI.Label("");

        var toggle = Main.Settings.DisplayGeneralRaceClassSubClassToggle;
        if (UI.DisclosureToggle(Gui.Localize("ModUi/&General"), ref toggle, 200))
        {
            Main.Settings.DisplayGeneralRaceClassSubClassToggle = toggle;
        }

        if (!Main.Settings.DisplayGeneralRaceClassSubClassToggle)
        {
            return;
        }

        UI.Label("");

        toggle = Main.Settings.EnableShortRestRechargeOfArcaneWeaponOnWizardArcaneFighter;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableShortRestRechargeOfArcaneWeaponOnWizardArcaneFighter"),
                ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableShortRestRechargeOfArcaneWeaponOnWizardArcaneFighter = toggle;
            WizardArcaneFighter.UpdateEnchantWeapon();
        }

        toggle = Main.Settings.EnableUnlimitedArcaneRecoveryOnWizardSpellMaster;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableUnlimitedArcaneRecoveryOnWizardSpellMaster"),
                ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableUnlimitedArcaneRecoveryOnWizardSpellMaster = toggle;
            WizardSpellMaster.UpdateBonusRecovery();
        }

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&OverrideRogueConArtistImprovedManipulationSpellDc"));
        var intValue = Main.Settings.OverrideRogueConArtistImprovedManipulationSpellDc;
        if (UI.Slider("", ref intValue, 0, 5, 3, "", UI.AutoWidth()))
        {
            Main.Settings.OverrideRogueConArtistImprovedManipulationSpellDc = intValue;
            RoguishConArtist.UpdateSpellDcBoost();
        }

        UI.Label("");
        UI.Label(Gui.Localize("ModUi/&OverrideWizardMasterManipulatorArcaneManipulationSpellDc"));
        intValue = Main.Settings.OverrideWizardMasterManipulatorArcaneManipulationSpellDc;
        if (UI.Slider("", ref intValue, 0, 5, 2, "", UI.AutoWidth()))
        {
            Main.Settings.OverrideWizardMasterManipulatorArcaneManipulationSpellDc = intValue;
            WizardMasterManipulator.UpdateSpellDcBoost();
        }

        UI.Label("");

        toggle = Main.Settings.ReduceDarkElfLightPenalty;
        if (UI.Toggle(Gui.Localize("ModUi/&ReduceDarkelfLightPenalty"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ReduceDarkElfLightPenalty = toggle;
        }

        toggle = Main.Settings.ReduceGrayDwarfLightPenalty;
        if (UI.Toggle(Gui.Localize("ModUi/&ReduceGrayDwarfLightPenalty"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ReduceGrayDwarfLightPenalty = toggle;
        }

        toggle = Main.Settings.HalfHighElfUseCharisma;
        if (UI.Toggle(Gui.Localize("ModUi/&HalfHighElfUseCharisma"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.HalfHighElfUseCharisma = toggle;
        }
    }

    internal static void DisplayClassesAndSubclasses()
    {
        DisplayGeneral();

        var displayToggle = Main.Settings.DisplayRacesToggle;
        var sliderPos = Main.Settings.RaceSliderPosition;
        DisplayDefinitions(
            Gui.Localize("ModUi/&Races"),
            RacesContext.Switch,
            RacesContext.Races,
            Main.Settings.RaceEnabled,
            ref displayToggle,
            ref sliderPos);
        Main.Settings.DisplayRacesToggle = displayToggle;
        Main.Settings.RaceSliderPosition = sliderPos;

        displayToggle = Main.Settings.DisplayClassesToggle;
        sliderPos = Main.Settings.ClassSliderPosition;
        DisplayDefinitions(
            Gui.Localize("ModUi/&Classes"),
            ClassesContext.Switch,
            ClassesContext.Classes,
            Main.Settings.ClassEnabled,
            ref displayToggle,
            ref sliderPos);
        Main.Settings.DisplayClassesToggle = displayToggle;
        Main.Settings.ClassSliderPosition = sliderPos;

        displayToggle = Main.Settings.DisplaySubclassesToggle;
        sliderPos = Main.Settings.SubclassSliderPosition;
        DisplayDefinitions(
            Gui.Localize("ModUi/&Subclasses"),
            SubclassesContext.Switch,
            SubclassesContext.Subclasses,
            Main.Settings.SubclassEnabled,
            ref displayToggle,
            ref sliderPos);
        Main.Settings.DisplaySubclassesToggle = displayToggle;
        Main.Settings.SubclassSliderPosition = sliderPos;

        UI.Label("");
    }
}
