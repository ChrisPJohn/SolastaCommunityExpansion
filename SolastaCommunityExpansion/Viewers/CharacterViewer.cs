﻿using System.Linq;
using ModKit;
using UnityEngine;
using UnityModManagerNet;
using static SolastaCommunityExpansion.Viewers.Displays.CharacterDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.RacesClassesAndSubclassesDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.FeatsAndFightingStylesDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.SpellsDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.Shared;

namespace SolastaCommunityExpansion.Viewers
{
    public class CharacterViewer : IMenuSelectablePage
    {
        public string Name => "Character";

        public int Priority => 10;

        private static int selectedPane;

        private static readonly NamedAction[] actions =
        {
            new NamedAction("General", DisplayCharacter),
            new NamedAction("Races, Classes & Subclasses", DisplayClassesAndSubclasses),
            new NamedAction("Feats & Fighting Styles", DisplayFeatsAndFightingStyles),
            new NamedAction("Spells", DisplaySpells)
        };

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            UI.Label(WelcomeMessage);
            UI.Div();

            if (Main.Enabled)
            {
                var titles = actions.Select((a, i) => i == selectedPane ? a.name.orange().bold() : a.name).ToArray();

                UI.SelectionGrid(ref selectedPane, titles, titles.Length, UI.ExpandWidth(true));
                GUILayout.BeginVertical("box");
                actions[selectedPane].action();
                GUILayout.EndVertical();
            }
        }
    }
}
