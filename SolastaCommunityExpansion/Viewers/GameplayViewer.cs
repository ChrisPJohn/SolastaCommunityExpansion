﻿using System.Linq;
using ModKit;
using UnityEngine;
using UnityModManagerNet;
using static SolastaCommunityExpansion.Viewers.Displays.CampaignsAndLocationsDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.ItemsAndCraftingDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.RulesDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.ToolsDisplay;
using static SolastaCommunityExpansion.Viewers.Displays.Shared;

namespace SolastaCommunityExpansion.Viewers
{
    public class GameplayViewer : IMenuSelectablePage
    {
        public string Name => "Gameplay";

        public int Priority => 20;

        private static int selectedPane;

        private static readonly NamedAction[] actions =
        {
            new NamedAction("Rules", DisplayRules),
            new NamedAction("Campaigns & Locations", DisplayCampaignsAndLocations),
            new NamedAction("Items, Crafting & Merchants", DisplayItemsAndCrafting),
            new NamedAction("Tools", DisplayTools),
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
