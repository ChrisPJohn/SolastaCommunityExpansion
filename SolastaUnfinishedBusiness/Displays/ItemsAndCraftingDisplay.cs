﻿using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Displays;

internal static class ItemsAndCraftingDisplay
{
    private const int MaxColumns = 1;

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsFilters =
    {
        (Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true),
        (Gui.Localize("Equipment/&ItemTypeAmmunitionTitle"), a => a.IsAmmunition),
        (Gui.Localize("MerchantCategory/&ArmorTitle"), a => a.IsArmor),
        (Gui.Localize("MerchantCategory/&DocumentTitle"), a => a.IsDocument),
        (Gui.Localize("Equipment/&ItemTypeSpellFocusTitle"), a => a.IsFocusItem),
        (Gui.Localize("Screen/&TravelFoodTitle"), a => a.IsFood),
        (Gui.Localize("Equipment/&ItemTypeLightSourceTitle"), a => a.IsLightSourceItem),
        (Gui.Localize("Equipment/&SpellbookTitle"), a => a.IsSpellbook),
        (Gui.Localize("Equipment/&ItemTypeStarterPackTitle"), a => a.IsStarterPack),
        (Gui.Localize("Screen/&ProficiencyToggleToolTitle"), a => a.IsTool),
        (Gui.Localize("Merchant/&DungeonMakerMagicalDevicesTitle"), a => a.IsUsableDevice),
        (Gui.Localize("MerchantCategory/&WeaponTitle"), a => a.IsWeapon),
        (Gui.Localize("Tooltip/&TagFactionRelicTitle"), a => a.IsFactionRelic)
    };

    private static readonly string[] ItemsFiltersLabels = ItemsFilters.Select(x => x.Item1).ToArray();

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsItemTagsFilters =
        TagsDefinitions.AllItemTags
            .Select<string, (string, Func<ItemDefinition, bool>)>(x =>
                (Gui.Localize($"Tooltip/&Tag{x}Title"), a => a.ItemTags.Contains(x)))
            .AddItem((Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true))
            .OrderBy(x => x.Item1)
            .ToArray();

    private static readonly string[] ItemsItemTagsFiltersLabels = ItemsItemTagsFilters.Select(x => x.Item1).ToArray();

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsWeaponTagsFilters =
        TagsDefinitions.AllWeaponTags
            .Select<string, (string, Func<ItemDefinition, bool>)>(x =>
                (Gui.Localize($"Tooltip/&Tag{x}Title"),
                    a => a.IsWeapon && a.WeaponDescription.WeaponTags.Contains(x)))
            .AddItem((Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true))
            .AddItem((Gui.Localize("Tooltip/&TagRangeTitle"),
                a => a.IsWeapon && a.WeaponDescription.WeaponTags.Contains("Range")))
            .OrderBy(x => x.Item1)
            .ToArray();

    private static readonly string[] ItemsWeaponTagsFiltersLabels =
        ItemsWeaponTagsFilters.Select(x => x.Item1).ToArray();

    private static Vector2 ItemPosition { get; set; } = Vector2.zero;

    private static int CurrentItemsFilterIndex { get; set; }

    private static int CurrentItemsItemTagsFilterIndex { get; set; }

    private static int CurrentItemsWeaponTagsFilterIndex { get; set; }

    private static bool DisplayFactionRelationsToggle { get; set; }

    private static bool DisplayItemsToggle { get; set; }

    internal static void DisplayItemsAndCrafting()
    {
        DisplayGeneral();
        DisplayCrafting();
        DisplayFactionRelations();
        DisplayItems();
        DisplayMerchants();
        UI.Label();
    }

    private static void DisplayGeneral()
    {
        UI.Label();
        UI.Label();

        var toggle = Main.Settings.AddCustomIconsToOfficialItems;
        if (UI.Toggle(Gui.Localize(Gui.Localize("ModUi/&AddCustomIconsToOfficialItems")), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddCustomIconsToOfficialItems = toggle;
        }

        toggle = Main.Settings.AddNewWeaponsAndRecipesToShops;
        if (UI.Toggle(Gui.Localize(Gui.Localize("ModUi/&AddNewWeaponsAndRecipesToShops")), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddNewWeaponsAndRecipesToShops = toggle;
            Main.Settings.AddNewWeaponsAndRecipesToEditor = toggle;
        }

        if (Main.Settings.AddNewWeaponsAndRecipesToShops)
        {
            toggle = Main.Settings.AddNewWeaponsAndRecipesToEditor;
            if (UI.Toggle(Gui.Localize(Gui.Localize("ModUi/&EnableAdditionalItemsInDungeonMaker")), ref toggle,
                    UI.AutoWidth()))
            {
                Main.Settings.AddNewWeaponsAndRecipesToEditor = toggle;
            }
        }

        toggle = Main.Settings.AddPickPocketableLoot;
        if (UI.Toggle(Gui.Localize("ModUi/&AddPickPocketableLoot"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddPickPocketableLoot = toggle;
            if (toggle)
            {
                PickPocketContext.Load();
            }
        }

        UI.Label();

        toggle = Main.Settings.AllowAnyClassToUseArcaneShieldstaff;
        if (UI.Toggle(Gui.Localize("ModUi/&ArcaneShieldstaffOptions"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowAnyClassToUseArcaneShieldstaff = toggle;
            ItemCraftingMerchantContext.SwitchAttuneArcaneShieldstaff();
        }

        toggle = Main.Settings.RemoveAttunementRequirements;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveAttunementRequirements"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveAttunementRequirements = toggle;
        }

        toggle = Main.Settings.RemoveIdentificationRequirements;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveIdentificationRequirements"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveIdentificationRequirements = toggle;
        }

        toggle = Main.Settings.IgnoreHandXbowFreeHandRequirements;
        if (UI.Toggle(Gui.Localize("ModUi/&IgnoreHandXbowFreeHandRequirements"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.IgnoreHandXbowFreeHandRequirements = toggle;
        }

        UI.Label();

        toggle = Main.Settings.ShowCraftingRecipeInDetailedTooltips;
        if (UI.Toggle(Gui.Localize("ModUi/&ShowCraftingRecipeInDetailedTooltips"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ShowCraftingRecipeInDetailedTooltips = toggle;
        }

        UI.Label();

        var intValue = Main.Settings.RecipeCost;
        if (UI.Slider(Gui.Localize("ModUi/&RecipeCost"), ref intValue, 1, 500, 200, "G", UI.AutoWidth()))
        {
            Main.Settings.RecipeCost = intValue;
            CraftingContext.UpdateRecipeCost();
        }

        UI.Label();

        intValue = Main.Settings.TotalCraftingTimeModifier;
        if (UI.Slider(Gui.Localize("ModUi/&TotalCraftingTimeModifier"), ref intValue, 0, 100, 0, "%", UI.AutoWidth()))
        {
            Main.Settings.TotalCraftingTimeModifier = intValue;
        }

        UI.Label();

        intValue = Main.Settings.SetBeltOfDwarvenKindBeardChances;
        if (UI.Slider(Gui.Localize("ModUi/&SetBeltOfDwarvenKindBeardChances"), ref intValue,
                0, 100, 50, "%", UI.Width((float)500)))
        {
            Main.Settings.SetBeltOfDwarvenKindBeardChances = intValue;
            ItemCraftingMerchantContext.SwitchSetBeltOfDwarvenKindBeardChances();
        }

        UI.Label();

        using (UI.HorizontalScope())
        {
            UI.Label(Gui.Localize("ModUi/&EmpressGarbAppearance"), UI.Width((float)325));

            intValue = Main.Settings.EmpressGarbAppearanceIndex;
            // ReSharper disable once InvertIf
            if (UI.SelectionGrid(ref intValue, ItemCraftingMerchantContext.EmpressGarbAppearances,
                    ItemCraftingMerchantContext.EmpressGarbAppearances.Length, 2, UI.Width((float)440)))
            {
                Main.Settings.EmpressGarbAppearanceIndex = intValue;
                GameUiContext.SwitchEmpressGarb();
            }
        }
    }

    private static void DisplayCrafting()
    {
        UI.Label();

        var toggle = Main.Settings.DisplayCraftingToggle;
        if (UI.DisclosureToggle(Gui.Localize("ModUi/&Crafting"), ref toggle, 200))
        {
            Main.Settings.DisplayCraftingToggle = toggle;
        }

        if (!Main.Settings.DisplayCraftingToggle)
        {
            return;
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&CraftingHelp"));
        UI.Label();

        using (UI.HorizontalScope(UI.AutoWidth()))
        {
            UI.Space((float)204);

            toggle = CraftingContext.RecipeBooks.Keys.Count == Main.Settings.CraftingInStore.Count;
            if (UI.Toggle(Gui.Localize("ModUi/&AddAllToStore"), ref toggle, UI.Width((float)125)))
            {
                Main.Settings.CraftingInStore.Clear();

                if (toggle)
                {
                    Main.Settings.CraftingInStore.AddRange(CraftingContext.RecipeBooks.Keys);
                }
            }

            toggle = CraftingContext.RecipeBooks.Keys.Count == Main.Settings.CraftingRecipesInDm.Count;
            if (UI.Toggle(Gui.Localize("ModUi/&AllRecipesInDm"), ref toggle, UI.Width((float)125)))
            {
                Main.Settings.CraftingRecipesInDm.Clear();

                if (toggle)
                {
                    Main.Settings.CraftingRecipesInDm.AddRange(CraftingContext.RecipeBooks.Keys);
                }
            }

            toggle = CraftingContext.RecipeBooks.Keys.Count == Main.Settings.CraftingItemsInDm.Count;
            if (UI.Toggle(Gui.Localize("ModUi/&AllItemInDm"), ref toggle, UI.Width((float)125)))
            {
                Main.Settings.CraftingItemsInDm.Clear();

                if (toggle)
                {
                    Main.Settings.CraftingItemsInDm.AddRange(CraftingContext.RecipeBooks.Keys);
                }
            }
        }

        UI.Label();

        var keys = CraftingContext.RecipeBooks.Keys;
        var current = 0;
        var count = keys.Count;

        while (current < count)
        {
            var cols = 0;

            using (UI.HorizontalScope())
            {
                while (current < count && cols < MaxColumns)
                {
                    AddUIForWeaponKey(keys.ElementAt(current));

                    cols++;
                    current++;
                }
            }
        }
    }

    private static void DisplayFactionRelations()
    {
        var toggle = DisplayFactionRelationsToggle;

        UI.Label();

        if (UI.DisclosureToggle(Gui.Localize("ModUi/&FactionRelations"), ref toggle))
        {
            DisplayFactionRelationsToggle = toggle;
        }

        if (!DisplayFactionRelationsToggle)
        {
            return;
        }

        UI.Label();

        var flip = true;
        var gameCampaign = Gui.GameCampaign;
        var gameFactionService = ServiceRepository.GetService<IGameFactionService>();

        // NOTE: don't use gameCampaign?. which bypasses Unity object lifetime check
        if (gameFactionService != null && gameCampaign != null &&
            gameCampaign.CampaignDefinitionName != "UserCampaign")
        {
            foreach (var faction in gameFactionService.RegisteredFactions)
            {
                if (faction.BuiltIn)
                {
                    // These are things like monster factions, generally set to a specific relation and can't be changed.
                    continue;
                }

                if (faction.GuiPresentation.Hidden)
                {
                    // These are things like Silent Whispers and Church Of Einar that are not fully implemented factions.
                    continue;
                }

                var title = faction.FormatTitle();

                title = flip ? title.Khaki() : title.White();

                var intValue = gameFactionService.FactionRelations[faction.Name];

                if (UI.Slider("                              " + title, ref intValue, faction.MinRelationCap,
                        faction.MaxRelationCap, 0, "", UI.AutoWidth()))
                {
                    SetFactionRelation(faction.Name, intValue);
                }

                flip = !flip;
            }
        }
        else
        {
            UI.Label(Gui.Localize("ModUi/&FactionHelp"));
        }
    }

    private static void DisplayItems()
    {
        var toggle = DisplayItemsToggle;

        UI.Label();

        if (UI.DisclosureToggle(Gui.Localize("ModUi/&Items"), ref toggle))
        {
            DisplayItemsToggle = toggle;
        }

        if (!DisplayItemsToggle)
        {
            return;
        }

        UI.Label();

        var characterInspectionScreen = Gui.GuiService.GetScreen<CharacterInspectionScreen>();

        if (!characterInspectionScreen.Visible || characterInspectionScreen.externalContainer == null)
        {
            UI.Label(Gui.Localize("ModUi/&ItemsHelp1"));

            return;
        }

        using (UI.HorizontalScope())
        {
            UI.Space(40f);
            UI.Label("Category".Bold(), UI.Width((float)100));

            if (CurrentItemsFilterIndex == 11 /* Weapons */)
            {
                UI.Space(40f);
                UI.Label("Weapon Tag".Bold(), UI.Width((float)100));
            }

            UI.Space(40f);
            UI.Label("Item Tag".Bold(), UI.Width((float)100));

            UI.Space(40f);
            UI.Label(Gui.Localize("ModUi/&ItemsHelp2"));
        }

        using (UI.HorizontalScope(UI.Width((float)800), UI.Height(400)))
        {
            var intValue = CurrentItemsFilterIndex;
            if (UI.SelectionGrid(
                    ref intValue,
                    ItemsFiltersLabels,
                    ItemsFiltersLabels.Length,
                    1, UI.Width((float)140)))
            {
                CurrentItemsFilterIndex = intValue;

                if (CurrentItemsFilterIndex != 11 /* Weapons */)
                {
                    CurrentItemsWeaponTagsFilterIndex = 0;
                }
            }

            if (CurrentItemsFilterIndex == 11 /* Weapons */)
            {
                intValue = CurrentItemsWeaponTagsFilterIndex;
                if (UI.SelectionGrid(
                        ref intValue,
                        ItemsWeaponTagsFiltersLabels,
                        ItemsWeaponTagsFiltersLabels.Length,
                        1, UI.Width((float)140)))
                {
                    CurrentItemsWeaponTagsFilterIndex = intValue;
                }
            }

            intValue = CurrentItemsItemTagsFilterIndex;
            if (UI.SelectionGrid(
                    ref intValue,
                    ItemsItemTagsFiltersLabels,
                    ItemsItemTagsFiltersLabels.Length,
                    1, UI.Width((float)140)))
            {
                CurrentItemsItemTagsFilterIndex = intValue;
            }

            DisplayItemsBox();
        }
    }

    private static void DisplayMerchants()
    {
        UI.Label();

        var toggle = Main.Settings.DisplayMerchantsToggle;
        if (UI.DisclosureToggle(Gui.Localize("ModUi/&Merchants"), ref toggle, 200))
        {
            Main.Settings.DisplayMerchantsToggle = toggle;
        }

        if (!Main.Settings.DisplayMerchantsToggle)
        {
            return;
        }

        UI.Label();

        toggle = Main.Settings.ScaleMerchantPricesCorrectly;
        if (UI.Toggle(Gui.Localize("ModUi/&ScaleMerchantPricesCorrectly"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ScaleMerchantPricesCorrectly = toggle;
        }

        toggle = Main.Settings.StockGorimStoreWithAllNonMagicalClothing;
        if (UI.Toggle(Gui.Localize("ModUi/&StockGorimStoreWithAllNonMagicalClothing"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.StockGorimStoreWithAllNonMagicalClothing = toggle;
        }

        toggle = Main.Settings.StockGorimStoreWithAllNonMagicalInstruments;
        if (UI.Toggle(Gui.Localize("ModUi/&StockGorimStoreWithAllNonMagicalInstruments"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.StockGorimStoreWithAllNonMagicalInstruments = toggle;
        }

        toggle = Main.Settings.StockHugoStoreWithAdditionalFoci;
        if (UI.Toggle(Gui.Localize("ModUi/&StockHugoStoreWithAdditionalFoci"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.StockHugoStoreWithAdditionalFoci = toggle;
            Main.Settings.EnableAdditionalFociInDungeonMaker = toggle;
            ItemCraftingMerchantContext.SwitchFociItems();
        }

        if (Main.Settings.StockHugoStoreWithAdditionalFoci)
        {
            toggle = Main.Settings.EnableAdditionalFociInDungeonMaker;
            if (UI.Toggle(Gui.Localize("ModUi/&EnableAdditionalItemsInDungeonMaker"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.EnableAdditionalFociInDungeonMaker = toggle;
                ItemCraftingMerchantContext.SwitchFociItemsDungeonMaker();
            }
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&RestockHelp"));
        UI.Label();

        toggle = Main.Settings.RestockAntiquarians;
        if (UI.Toggle(Gui.Localize("ModUi/&RestockAntiquarians"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RestockAntiquarians = toggle;
            ItemCraftingMerchantContext.SwitchRestockAntiquarian();
        }

        toggle = Main.Settings.RestockArcaneum;
        if (UI.Toggle(Gui.Localize("ModUi/&RestockArcaneum"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RestockArcaneum = toggle;
            ItemCraftingMerchantContext.SwitchRestockArcaneum();
        }

        toggle = Main.Settings.RestockCircleOfDanantar;
        if (UI.Toggle(Gui.Localize("ModUi/&RestockCircleOfDanantar"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RestockCircleOfDanantar = toggle;
            ItemCraftingMerchantContext.SwitchRestockCircleOfDanantar();
        }

        toggle = Main.Settings.RestockTowerOfKnowledge;
        // ReSharper disable once InvertIf
        if (UI.Toggle(Gui.Localize("ModUi/&RestockTowerOfKnowledge"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RestockTowerOfKnowledge = toggle;
            ItemCraftingMerchantContext.SwitchRestockTowerOfKnowledge();
        }
    }

    private static void SetFactionRelation(string name, int value)
    {
        var service = ServiceRepository.GetService<IGameFactionService>();

        service?.ExecuteFactionOperation(name, FactionDefinition.FactionOperation.Increase,
            value - service.FactionRelations[name], "",
            null /* this string and monster doesn't matter if we're using "SetValue" */);
    }

    private static void AddUIForWeaponKey([NotNull] string key)
    {
        using (UI.HorizontalScope(UI.AutoWidth()))
        {
            UI.ActionButton(CraftingContext.RecipeTitles[key], () => CraftingContext.LearnRecipes(key),
                UI.Width((float)180));
            UI.Space((float)20);

            var toggle = Main.Settings.CraftingInStore.Contains(key);
            if (UI.Toggle(Gui.Localize("ModUi/&AddToStore"), ref toggle, UI.Width((float)125)))
            {
                if (toggle)
                {
                    Main.Settings.CraftingInStore.Add(key);
                }
                else
                {
                    Main.Settings.CraftingInStore.Remove(key);
                }

                CraftingContext.AddToStore(key);
            }

            toggle = Main.Settings.CraftingRecipesInDm.Contains(key);
            if (UI.Toggle(Gui.Localize("ModUi/&RecipesInDm"), ref toggle, UI.Width((float)125)))
            {
                if (toggle)
                {
                    Main.Settings.CraftingRecipesInDm.Add(key);
                }
                else
                {
                    Main.Settings.CraftingRecipesInDm.Remove(key);
                }

                CraftingContext.UpdateCraftingRecipesInDmState(key);
            }

            if (!CraftingContext.BaseGameItemsCategories.Contains(key))
            {
                toggle = Main.Settings.CraftingItemsInDm.Contains(key);

                if (!UI.Toggle(Gui.Localize("ModUi/&ItemInDm"), ref toggle, UI.Width((float)125)))
                {
                    return;
                }

                if (toggle)
                {
                    Main.Settings.CraftingItemsInDm.Add(key);
                }
                else
                {
                    Main.Settings.CraftingItemsInDm.Remove(key);
                }

                CraftingContext.UpdateCraftingItemsInDmState(key);
            }
            else
            {
                UI.Space(128f);
            }
        }
    }

    private static void DisplayItemsBox()
    {
        var characterInspectionScreen = Gui.GuiService.GetScreen<CharacterInspectionScreen>();
        var rulesetItemFactoryService = ServiceRepository.GetService<IRulesetItemFactoryService>();
        var characterName = characterInspectionScreen.InspectedCharacter.Name;

        var items = DatabaseRepository.GetDatabase<ItemDefinition>()
            .Where(x => !x.guiPresentation.Hidden)
            .Where(x => ItemsFilters[CurrentItemsFilterIndex].Item2(x))
            .Where(x => ItemsItemTagsFilters[CurrentItemsItemTagsFilterIndex].Item2(x))
            .Where(x => ItemsWeaponTagsFilters[CurrentItemsWeaponTagsFilterIndex].Item2(x))
            .OrderBy(x => x.FormatTitle());

        using var scrollView =
            new GUILayout.ScrollViewScope(ItemPosition, UI.AutoWidth(), UI.AutoHeight());

        ItemPosition = scrollView.scrollPosition;

        foreach (var item in items)
        {
            using (UI.HorizontalScope())
            {
                UI.ActionButton("+".Bold().Red(), () =>
                    {
                        var rulesetItem = rulesetItemFactoryService.CreateStandardItem(item, true, characterName);

                        characterInspectionScreen.externalContainer.AddSubItem(rulesetItem);
                    },
                    UI.Width((float)30));

                var label = item.GuiPresentation.Title.StartsWith("Equipment/&CraftingManual")
                    ? Gui.Format(item.GuiPresentation.Title,
                        item.DocumentDescription.RecipeDefinition.CraftedItem.FormatTitle())
                    : item.FormatTitle();

                UI.Label(label, UI.AutoWidth());
            }
        }
    }
}
