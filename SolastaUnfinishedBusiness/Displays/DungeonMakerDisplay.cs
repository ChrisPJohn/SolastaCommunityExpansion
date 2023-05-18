﻿using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

internal static class DungeonMakerDisplay
{
    internal static void DisplayDungeonMaker()
    {
        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Basic"));
        UI.Label();

        UI.Label(Gui.Localize("ModUi/&DungeonMakerBasicHelp"));
        UI.Label();

        var toggle = Main.Settings.EnableSortingDungeonMakerAssets;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableSortingDungeonMakerAssets"), ref toggle))
        {
            Main.Settings.EnableSortingDungeonMakerAssets = toggle;
        }

        toggle = Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowGadgetsAndPropsToBePlacedAnywhere"), ref toggle))
        {
            Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere = toggle;
        }

        toggle = Main.Settings.UnleashEnemyAsNpc;
        if (UI.Toggle(Gui.Localize("ModUi/&UnleashEnemyAsNpc"), ref toggle))
        {
            Main.Settings.UnleashEnemyAsNpc = toggle;
        }

        UI.Label();

        using (UI.HorizontalScope())
        {
            UI.ActionButton("Aberrations docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersAberration.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Beasts docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersBeasts.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Celestial docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersCelestial.md"), UI.Width((float)200));
        }

        using (UI.HorizontalScope())
        {
            UI.ActionButton("Constructs docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersConstruct.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Dragons docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersDragon.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Elementals docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersElemental.md"), UI.Width((float)200));
        }

        using (UI.HorizontalScope())
        {
            UI.ActionButton("Fey docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersFey.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Fiend docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersFiend.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Giants docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersGiant.md"), UI.Width((float)200));
        }

        using (UI.HorizontalScope())
        {
            UI.ActionButton("Humanoids docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersHumanoid.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Monstrosities docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersMonstrosity.md"), UI.Width((float)200));
            20.Space();
            UI.ActionButton("Undead docs".Bold().Khaki(),
                () => BootContext.OpenDocumentation("SolastaMonstersUndead.md"), UI.Width((float)200));
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Advanced"));
        UI.Label();

        UI.Label(Gui.Localize("ModUi/&AdvancedHelp"));
        UI.Label();

        toggle = Main.Settings.UnleashNpcAsEnemy;
        if (UI.Toggle(Gui.Localize("ModUi/&UnleashNpcAsEnemy"), ref toggle))
        {
            Main.Settings.UnleashNpcAsEnemy = toggle;
        }

        toggle = Main.Settings.EnableDungeonMakerModdedContent;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableDungeonMakerModdedContent"), ref toggle))
        {
            Main.Settings.EnableDungeonMakerModdedContent = toggle;
        }

        UI.Label();
        UI.Label();
    }
}
