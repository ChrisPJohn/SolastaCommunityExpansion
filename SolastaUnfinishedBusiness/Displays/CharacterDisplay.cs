﻿using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

internal static class CharacterDisplay
{
    internal static void DisplayCharacter()
    {
        UI.Label();

        using (UI.HorizontalScope())
        {
            UI.ActionButton("Discord".Bold().Khaki(), BootContext.OpenDiscord, UI.Width((float)200));
            UI.ActionButton("Wiki".Bold().Khaki(), BootContext.OpenWiki, UI.Width((float)200));
            UI.ActionButton("Change History".Bold().Khaki(), BootContext.OpenChangeLog, UI.Width((float)200));
        }

        using (UI.HorizontalScope())
        {
            UI.ActionButton("<b>Donate:</b> GitHub Sponsors".Khaki(), BootContext.OpenDonateGithubSponsors,
                UI.Width((float)200));
            UI.ActionButton("<b>Donate:</b> Patreon".Khaki(), BootContext.OpenDonatePatreon, UI.Width((float)200));
            UI.ActionButton("<b>Donate:</b> PayPal".Khaki(), BootContext.OpenDonatePayPal, UI.Width((float)200));
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&InitialChoices"));
        UI.Label();

        var toggle = Main.Settings.AddHelpActionToAllRaces;
        if (UI.Toggle(Gui.Localize("ModUi/&AddHelpActionToAllRaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddHelpActionToAllRaces = toggle;
            CharacterContext.SwitchHelpPower();
        }

        toggle = Main.Settings.DisableSenseDarkVisionFromAllRaces;
        if (UI.Toggle(Gui.Localize("ModUi/&DisableSenseDarkVisionFromAllRaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DisableSenseDarkVisionFromAllRaces = toggle;
        }

        toggle = Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces;
        if (UI.Toggle(Gui.Localize("ModUi/&DisableSenseSuperiorDarkVisionFromAllRaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces = toggle;
        }

        toggle = Main.Settings.EnableFlexibleBackgrounds;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableFlexibleBackgrounds"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableFlexibleBackgrounds = toggle;
            FlexibleBackgroundsContext.SwitchFlexibleBackgrounds();
        }

        toggle = Main.Settings.EnableFlexibleRaces;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableFlexibleRaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableFlexibleRaces = toggle;
            FlexibleRacesContext.SwitchFlexibleRaces();
        }

        toggle = Main.Settings.EnableAlternateHuman;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableAlternateHuman"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableAlternateHuman = toggle;
            CharacterContext.SwitchFirstLevelTotalFeats();
        }

        UI.Label();


        toggle = Main.Settings.DisableClassPrerequisitesOnModFeats;
        if (UI.Toggle(Gui.Localize("ModUi/&DisableClassPrerequisitesOnModFeats"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DisableClassPrerequisitesOnModFeats = toggle;
        }


        toggle = Main.Settings.DisableRacePrerequisitesOnModFeats;
        if (UI.Toggle(Gui.Localize("ModUi/&DisableRacePrerequisitesOnModFeats"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DisableRacePrerequisitesOnModFeats = toggle;
        }

        UI.Label();

        toggle = Main.Settings.AddHumanoidFavoredEnemyToRanger;
        if (UI.Toggle(Gui.Localize("ModUi/&AddHumanoidFavoredEnemyToRanger"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddHumanoidFavoredEnemyToRanger = toggle;
            CharacterContext.SwitchRangerHumanoidFavoredEnemy();
        }

        if (Main.Settings.EnableBetaContent)
        {
            toggle = Main.Settings.EnumerateOriginSubFeatures;
            if (UI.Toggle(Gui.Localize("ModUi/&EnumerateOriginSubFeatures"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnumerateOriginSubFeatures = toggle;
            }
        }

        UI.Label();

        toggle = Main.Settings.EnableEpicPointsAndArray;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableEpicPointsAndArray"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableEpicPointsAndArray = toggle;
        }

        toggle = Main.Settings.ImproveLevelUpFeaturesSelection;
        if (UI.Toggle(Gui.Localize("ModUi/&ImproveLevelUpFeaturesSelection"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ImproveLevelUpFeaturesSelection = toggle;
            CharacterContext.SwitchRangerHumanoidFavoredEnemy();
        }

        UI.Label();
        UI.Label();

        var intValue = Main.Settings.TotalFeatsGrantedFirstLevel;
        if (UI.Slider(Gui.Localize("ModUi/&TotalFeatsGrantedFirstLevel"), ref intValue,
                CharacterContext.MinInitialFeats, CharacterContext.MaxInitialFeats, 0, "",
                UI.AutoWidth()))
        {
            Main.Settings.TotalFeatsGrantedFirstLevel = intValue;
            CharacterContext.SwitchFirstLevelTotalFeats();
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Progression"));
        UI.Label();

        toggle = Main.Settings.EnablesAsiAndFeat;
        if (UI.Toggle(Gui.Localize("ModUi/&EnablesAsiAndFeat"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnablesAsiAndFeat = toggle;
            CharacterContext.SwitchAsiAndFeat();
        }

        toggle = Main.Settings.EnableFeatsAtEvenLevels;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableFeatsAtEvenLevels"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableFeatsAtEvenLevels = toggle;
            CharacterContext.SwitchEvenLevelFeats();
        }

        toggle = Main.Settings.EnableFighterArmamentAdroitness;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableFighterArmamentAdroitness"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableFighterArmamentAdroitness = toggle;
            CharacterContext.SwitchFighterArmamentAdroitness();
        }

        toggle = Main.Settings.EnableLevel20;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableLevel20"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableLevel20 = toggle;
        }

        toggle = Main.Settings.EnableMulticlass;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableMulticlass"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableMulticlass = toggle;
            Main.Settings.MaxAllowedClasses = MulticlassContext.DefaultClasses;
            Main.Settings.EnableMinInOutAttributes = true;
            Main.Settings.EnableRelearnSpells = false;
            Main.Settings.DisplayAllKnownSpellsDuringLevelUp = true;
            Main.Settings.DisplayPactSlotsOnSpellSelectionPanel = true;
        }

        if (Main.Settings.EnableMulticlass)
        {
            UI.Label();

            intValue = Main.Settings.MaxAllowedClasses;
            if (UI.Slider(Gui.Localize("ModUi/&MaxAllowedClasses"), ref intValue,
                    2, MulticlassContext.MaxClasses, MulticlassContext.DefaultClasses, "", UI.AutoWidth()))
            {
                Main.Settings.MaxAllowedClasses = intValue;
            }

            UI.Label();

            toggle = Main.Settings.EnableMinInOutAttributes;
            if (UI.Toggle(Gui.Localize("ModUi/&EnableMinInOutAttributes"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnableMinInOutAttributes = toggle;
            }

            UI.Label();

            toggle = Main.Settings.EnableRelearnSpells;
            if (UI.Toggle(Gui.Localize("ModUi/&EnableRelearnSpells"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnableRelearnSpells = toggle;
            }

            toggle = Main.Settings.DisplayAllKnownSpellsDuringLevelUp;
            if (UI.Toggle(Gui.Localize("ModUi/&DisplayAllKnownSpellsDuringLevelUp"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.DisplayAllKnownSpellsDuringLevelUp = toggle;
            }

            toggle = Main.Settings.DisplayPactSlotsOnSpellSelectionPanel;
            if (UI.Toggle(Gui.Localize("ModUi/&DisplayPactSlotsOnSpellSelectionPanel"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.DisplayPactSlotsOnSpellSelectionPanel = toggle;
            }

            UI.Label();
            UI.Label(Gui.Localize("ModUi/&MulticlassKeyHelp"));
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Visuals"));
        UI.Label();

        toggle = Main.Settings.AllowBeardlessDwarves;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowBeardlessDwarves"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowBeardlessDwarves = toggle;
        }

        toggle = Main.Settings.AllowHornsOnAllRaces;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowHornsOnAllRaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowHornsOnAllRaces = toggle;
        }

        toggle = Main.Settings.OfferAdditionalLoreFriendlyNames;
        if (UI.Toggle(Gui.Localize("ModUi/&OfferAdditionalLoreFriendlyNames"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.OfferAdditionalLoreFriendlyNames = toggle;
        }

        toggle = Main.Settings.UnlockAllNpcFaces;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockAllNpcFaces"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockAllNpcFaces = toggle;
        }

        UI.Label();

        toggle = Main.Settings.AllowUnmarkedSorcerers;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowUnmarkedSorcerers"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowUnmarkedSorcerers = toggle;
        }

        toggle = Main.Settings.UnlockMarkAndTattoosForAllCharacters;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockMarkAndTattoosForAllCharacters"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockMarkAndTattoosForAllCharacters = toggle;
        }

        toggle = Main.Settings.UnlockGlowingColorsForAllMarksAndTattoos;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockGlowingColorsForAllMarksAndTattoos"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockGlowingColorsForAllMarksAndTattoos = toggle;
        }

        UI.Label();

        toggle = Main.Settings.UnlockGlowingEyeColors;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockGlowingEyeColors"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockGlowingEyeColors = toggle;
        }

        toggle = Main.Settings.AddNewBrightEyeColors;
        if (UI.Toggle(Gui.Localize("ModUi/&AddNewBrightEyeColors"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddNewBrightEyeColors = toggle;
        }

        toggle = Main.Settings.UnlockEyeStyles;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockEyeStyles"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockEyeStyles = toggle;
        }

        toggle = Main.Settings.UnlockSkinColors;
        if (UI.Toggle(Gui.Localize("ModUi/&UnlockSkinColors"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UnlockSkinColors = toggle;
        }

        UI.Label();
    }
}
