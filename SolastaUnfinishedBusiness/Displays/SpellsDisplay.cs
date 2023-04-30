﻿using System.Linq;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

internal static class SpellsDisplay
{
    private const int ShowAll = -1;

    private static int SpellLevelFilter { get; set; } = ShowAll;

    internal static void DisplaySpells()
    {
        UI.Label();
        UI.Label();

        var toggle = Main.Settings.AllowAssigningOfficialSpells;

        if (UI.Toggle(Gui.Localize("ModUi/&AllowAssigningOfficialSpells"), ref toggle,
                UI.Width(ModUi.PixelsPerColumn)))
        {
            Main.Settings.AllowAssigningOfficialSpells = toggle;
            SpellsContext.SwitchAllowAssigningOfficialSpells();
        }

        UI.Label();

        using (UI.HorizontalScope())
        {
            UI.ActionButton("UB Spells docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("UnfinishedBusinessSpells.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Solasta Spells docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaSpells.md"), UI.Width((float)200));
        }

        UI.Label();

        var intValue = SpellLevelFilter;
        if (UI.Slider(Gui.Localize("ModUi/&SpellLevelFilter"), ref intValue, ShowAll, 9, ShowAll))
        {
            SpellLevelFilter = intValue;
        }

        UI.Label();

        using (UI.HorizontalScope())
        {
            toggle = SpellsContext.IsAllSetSelected();
            if (UI.Toggle(Gui.Localize("ModUi/&SelectAll"), ref toggle, UI.Width(ModUi.PixelsPerColumn)))
            {
                SpellsContext.SelectAllSet(toggle);
            }

            toggle = SpellsContext.IsSuggestedSetSelected();
            if (UI.Toggle(Gui.Localize("ModUi/&SelectSuggested"), ref toggle, UI.Width(ModUi.PixelsPerColumn)))
            {
                SpellsContext.SelectSuggestedSet(toggle);
            }

            toggle = Main.Settings.DisplaySpellListsToggle.All(x => x.Value);
            if (UI.Toggle(Gui.Localize("ModUi/&ExpandAll"), ref toggle, UI.Width(ModUi.PixelsPerColumn)))
            {
                var keys = Main.Settings.DisplaySpellListsToggle.Keys.ToHashSet();

                foreach (var key in keys)
                {
                    Main.Settings.DisplaySpellListsToggle[key] = toggle;
                }
            }
        }

        UI.Div();

        foreach (var kvp in SpellsContext.SpellLists)
        {
            var spellListDefinition = kvp.Value;
            var spellListContext = SpellsContext.SpellListContextTab[spellListDefinition];
            var name = spellListDefinition.name;
            var displayToggle = Main.Settings.DisplaySpellListsToggle[name];
            var sliderPos = Main.Settings.SpellListSliderPosition[name];
            var spellEnabled = Main.Settings.SpellListSpellEnabled[name];
            var allowedSpells = spellListContext.AllSpells
                .Where(x => x.ContentPack == CeContentPackContext.CeContentPack ||
                            Main.Settings.AllowAssigningOfficialSpells)
                .Where(x => SpellLevelFilter == ShowAll || x.SpellLevel == SpellLevelFilter)
                .ToHashSet();

            void AdditionalRendering()
            {
                toggle = spellListContext.IsAllSetSelected;
                if (UI.Toggle(Gui.Localize("ModUi/&SelectAll"), ref toggle, UI.Width(ModUi.PixelsPerColumn)))
                {
                    spellListContext.SelectAllSetInternal(toggle);
                }

                toggle = spellListContext.IsSuggestedSetSelected;
                if (UI.Toggle(Gui.Localize("ModUi/&SelectSuggested"), ref toggle, UI.Width(ModUi.PixelsPerColumn)))
                {
                    spellListContext.SelectSuggestedSetInternal(toggle);
                }
            }

            ModUi.DisplayDefinitions(
                kvp.Key.Khaki(),
                spellListContext.Switch,
                allowedSpells,
                spellEnabled,
                ref displayToggle,
                ref sliderPos,
                additionalRendering: AdditionalRendering);

            Main.Settings.DisplaySpellListsToggle[name] = displayToggle;
            Main.Settings.SpellListSliderPosition[name] = sliderPos;
        }

        UI.Label();
    }
}
