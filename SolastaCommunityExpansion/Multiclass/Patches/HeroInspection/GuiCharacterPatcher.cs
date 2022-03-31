﻿using HarmonyLib;
using SolastaCommunityExpansion;
using SolastaMulticlass.Models;

namespace SolastaMulticlass.Patches.HeroInspection
{
    internal static class GuiCharacterPatcher
    {
        [HarmonyPatch(typeof(GuiCharacter), "MainClassDefinition", MethodType.Getter)]
        internal static class GuiCharacterMainClassDefinitionGetter
        {
            internal static void Postfix(ref CharacterClassDefinition __result)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    return;
                }

                // NOTE: don't use SelectedClass??. which bypasses Unity object lifetime check
                if (InspectionPanelContext.SelectedClass)
                {
                    __result = InspectionPanelContext.SelectedClass;
                }
            }
        }
    }
}
