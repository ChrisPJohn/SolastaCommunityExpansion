﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using static SolastaUnfinishedBusiness.Models.ItemCraftingContext;

namespace SolastaUnfinishedBusiness.ItemCrafting;

internal static class GreatSwordData
{
    private static ItemCollection _items;

    [NotNull]
    internal static ItemCollection Items =>
        _items ??= new ItemCollection
        {
            BaseGuid = new Guid("16757d1b-518f-4669-af43-1ddf5d23c223"),
            BaseWeapons =
                new List<ItemDefinition> { DatabaseHelper.ItemDefinitions.Greatsword },
            PossiblePrimedItemsToReplace = new List<ItemDefinition> { DatabaseHelper.ItemDefinitions.Battleaxe },
            MagicToCopy = new List<ItemCollection.MagicItemDataHolder>
            {
                new("Punisher", DatabaseHelper.ItemDefinitions.Enchanted_Battleaxe_Punisher,
                    DatabaseHelper.RecipeDefinitions.Recipe_Enchantment_BattleaxePunisher)
            }
        };
}
