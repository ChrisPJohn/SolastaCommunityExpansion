﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Utils;

namespace SolastaUnfinishedBusiness.Patches;

internal static class MapGadgetItemPatcher
{
    //PATCH: EnableAdditionalIconsOnLevelMap
    [HarmonyPatch(typeof(MapGadgetItem), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        internal static bool Prefix(
            MapGadgetItem __instance,
            GameGadget gameGadget,
            MapGadgetItem.ItemType itemType)
        {
            if (!Main.Settings.EnableAdditionalIconsOnLevelMap || Gui.GameLocation.UserLocation == null)
            {
                return true;
            }

            // Handle standard item types in game code
            if (itemType >= 0)
            {
                return true;
            }

            switch ((int)itemType)
            {
                case -1:
                    __instance.backgroundImage.sprite = __instance.backgroundSprites[2];
                    __instance.iconImage.sprite = CustomIcons.GetOrCreateSprite("Fire", Resources.Fire, 24);
                    __instance.guiTooltip.Content = "Camp";
                    break;
                case -2:
                    __instance.backgroundImage.sprite = __instance.backgroundSprites[2];
                    __instance.iconImage.sprite =
                        CustomIcons.GetOrCreateSprite("Entrance", Resources.Entry, 24);
                    __instance.guiTooltip.Content = "Exit";
                    break;
                case -3:
                    __instance.backgroundImage.sprite = __instance.backgroundSprites[2];
                    __instance.iconImage.sprite =
                        CustomIcons.GetOrCreateSprite("Teleport", Resources.Teleport, 24);
                    __instance.guiTooltip.Content = "Teleporter";
                    break;
                default:
                    return true;
            }

            __instance.gameGadget = gameGadget;
            __instance.gameObject.SetActive(true);
            return false;
        }
    }
}
