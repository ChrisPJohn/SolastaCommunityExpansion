﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.Patches;
using TA;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.GadgetBlueprints;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;
using Object = UnityEngine.Object;

namespace SolastaUnfinishedBusiness.Models;

internal static class GameUiContext
{
    private static readonly int[][][] FormationGridSetTemplates =
    {
        new[] // default
        {
            new[] { 1, 1, 0, 0, 0 }, //
            new[] { 1, 1, 0, 0, 0 }, //
            new[] { 1, 1, 0, 0, 0 }, //
            new[] { 1, 1, 0, 0, 0 }, //
            new[] { 0, 0, 0, 0, 0 } //
        },
        new[] // triangle
        {
            new[] { 0, 0, 1, 0, 0 }, //
            new[] { 0, 1, 0, 1, 0 }, //
            new[] { 1, 0, 1, 0, 1 }, //
            new[] { 0, 1, 0, 1, 0 }, //
            new[] { 0, 0, 0, 0, 0 } //
        },
        new[]
        {
            new[] { 0, 0, 1, 0, 0 }, //
            new[] { 0, 0, 0, 0, 0 }, //
            new[] { 0, 1, 1, 1, 0 }, //
            new[] { 0, 1, 0, 1, 0 }, //
            new[] { 1, 0, 0, 0, 1 } //
        },
        new[]
        {
            new[] { 1, 0, 1, 0, 0 }, //
            new[] { 0, 1, 0, 0, 0 }, //
            new[] { 0, 1, 0, 0, 0 }, //
            new[] { 1, 0, 1, 0, 0 }, //
            new[] { 1, 0, 1, 0, 0 } //
        },
        new[] // spaced
        {
            new[] { 1, 0, 0, 1, 0 }, //
            new[] { 0, 0, 0, 0, 0 }, //
            new[] { 1, 0, 0, 1, 0 }, //
            new[] { 1, 0, 0, 1, 0 }, //
            new[] { 1, 0, 0, 1, 0 } //
        }
    };

    internal const int GridSize = 5;

    // Toggle HUD components
    private const InputCommands.Id CtrlShiftH = (InputCommands.Id)44440004;

    // Debug Overlay
    private const InputCommands.Id CtrlShiftD = (InputCommands.Id)44440005;

    // Export Character
    private const InputCommands.Id CtrlShiftE = (InputCommands.Id)44440006;

    // Spawn Encounter
    private const InputCommands.Id CtrlShiftS = (InputCommands.Id)44440007;

    // Teleport Party
    private const InputCommands.Id CtrlShiftT = (InputCommands.Id)44440008;

    // Rejoin Party
    private const InputCommands.Id CtrlShiftR = (InputCommands.Id)44440009;

    // Formation
    private const InputCommands.Id CtrlShift1 = (InputCommands.Id)44440010;
    private const InputCommands.Id CtrlShift2 = (InputCommands.Id)44440011;
    private const InputCommands.Id CtrlShift3 = (InputCommands.Id)44440012;
    private const InputCommands.Id CtrlShift4 = (InputCommands.Id)44440013;
    private const InputCommands.Id CtrlShift5 = (InputCommands.Id)44440014;

    private static readonly List<RectTransform> SpellLineTables = new();
    private static ItemPresentation EmpressGarbOriginalItemPresentation { get; set; }

    // Converts continuous ratio into series of stepped values
    internal static float GetSteppedHealthRatio(float ratio)
    {
        return ratio switch
        {
            // Green
            >= 1f => 1f,
            // Green
            >= 0.5f => 0.75f,
            // Orange
            >= 0.25f => 0.5f,
            // Red
            > 0f => 0.25f,
            _ => ratio
        };
    }

    internal static void RefreshMetamagicOffering(MetaMagicSubPanel __instance)
    {
        __instance.relevantMetamagicOptions.Clear();

        foreach (var allElement in DatabaseRepository.GetDatabase<MetamagicOptionDefinition>().GetAllElements())
        {
            if (!allElement.GuiPresentation.Hidden)
            {
                __instance.relevantMetamagicOptions.Add(allElement);
            }
        }

        __instance.relevantMetamagicOptions.Sort(MetamagicContext.CompareMetamagic);

        Gui.ReleaseChildrenToPool(__instance.Table);

        while (__instance.Table.childCount < __instance.relevantMetamagicOptions.Count)
        {
            Gui.GetPrefabFromPool(__instance.ItemPrefab, __instance.Table);
        }
    }

    internal static void SpellSelectionPanelMultilineUnbind()
    {
        foreach (var spellTable in SpellLineTables
                     .Where(spellTable =>
                         spellTable != null && spellTable.gameObject.activeSelf && spellTable.childCount > 0))
        {
            Gui.ReleaseChildrenToPool(spellTable);
            spellTable.SetParent(null);
            Object.Destroy(spellTable.gameObject);
        }

        SpellLineTables.Clear();
    }

    internal static void SpellSelectionPanelMultilineBind(
        SpellSelectionPanel __instance,
        GuiCharacter caster,
        SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged,
        ActionDefinitions.ActionType actionType,
        bool cantripOnly)
    {
        var spellRepertoireLines = __instance.spellRepertoireLines;
        var spellRepertoireSecondaryLine = __instance.spellRepertoireSecondaryLine;
        var spellRepertoireLinesTable = __instance.spellRepertoireLinesTable;
        var slotAdvancementPanel = __instance.SlotAdvancementPanel;

        foreach (var spellRepertoireLine in spellRepertoireLines)
        {
            spellRepertoireLine.Unbind();
        }

        spellRepertoireLines.Clear();
        Gui.ReleaseChildrenToPool(spellRepertoireLinesTable);
        spellRepertoireSecondaryLine.Unbind();
        spellRepertoireSecondaryLine.gameObject.SetActive(false);

        if (spellRepertoireLinesTable.parent.GetComponent<VerticalLayoutGroup>() == null)
        {
            GameObject spellLineHolder = new();

            var vertGroup = spellLineHolder.AddComponent<VerticalLayoutGroup>();

            vertGroup.spacing = 10;
            spellLineHolder.AddComponent<ContentSizeFitter>();
            spellLineHolder.transform.SetParent(spellRepertoireLinesTable.parent, true);
            spellLineHolder.transform.SetAsFirstSibling();
            spellLineHolder.transform.localScale = Vector3.one;
            spellRepertoireLinesTable.SetParent(spellLineHolder.transform);
        }

        var spellRepertoires = __instance.Caster.RulesetCharacter.SpellRepertoires
            .Where(r => r.SpellCastingFeature.SpellListDefinition != SpellsContext.EmptySpellList)
            .ToList();

        var needNewLine = true;
        var lineIndex = 0;
        var indexOfLine = 0;
        var spellLevelsOnLine = 0;
        var curTable = spellRepertoireLinesTable;

        foreach (var rulesetSpellRepertoire in spellRepertoires)
        {
            var startLevel = 0;
            var maxLevel = rulesetSpellRepertoire.MaxSpellLevelOfSpellCastingLevel;

            SharedSpellsContext.FactorMysticArcanum(caster.RulesetCharacterHero, rulesetSpellRepertoire,
                ref maxLevel);

            for (var level = startLevel; level <= maxLevel; level++)
            {
                if (!IsLevelActive(rulesetSpellRepertoire, level, actionType))
                {
                    continue;
                }

                spellLevelsOnLine++;

                if (spellLevelsOnLine < 4) // Main.Settings.MaxSpellLevelsPerLine)
                {
                    continue;
                }

                curTable = AddActiveSpellsToLine(
                    __instance,
                    spellCastEngaged,
                    actionType,
                    cantripOnly,
                    spellRepertoireLines,
                    curTable,
                    slotAdvancementPanel,
                    spellRepertoires,
                    needNewLine,
                    lineIndex,
                    indexOfLine,
                    rulesetSpellRepertoire,
                    startLevel,
                    level);

                startLevel = level + 1;
                lineIndex++;
                spellLevelsOnLine = 0;
                needNewLine = true;
                indexOfLine = 0;
            }

            if (spellLevelsOnLine == 0)
            {
                continue;
            }

            curTable = AddActiveSpellsToLine(
                __instance,
                spellCastEngaged,
                actionType,
                cantripOnly,
                spellRepertoireLines,
                curTable,
                slotAdvancementPanel,
                spellRepertoires,
                needNewLine,
                lineIndex,
                indexOfLine,
                rulesetSpellRepertoire,
                startLevel,
                maxLevel);

            needNewLine = false;
            indexOfLine++;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(curTable);
        __instance.RectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            spellRepertoireLinesTable.rect.width);
    }

    private static RectTransform AddActiveSpellsToLine(
        SpellSelectionPanel __instance,
        SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged,
        ActionDefinitions.ActionType actionType,
        bool cantripOnly,
        ICollection<SpellRepertoireLine> spellRepertoireLines,
        RectTransform spellRepertoireLinesTable,
        SlotAdvancementPanel slotAdvancementPanel,
        IReadOnlyCollection<RulesetSpellRepertoire> spellRepertoires,
        bool needNewLine,
        int lineIndex,
        int indexOfLine,
        RulesetSpellRepertoire rulesetSpellRepertoire,
        int startLevel,
        int level)
    {
        if (needNewLine)
        {
            var previousTable = spellRepertoireLinesTable;

            LayoutRebuilder.ForceRebuildLayoutImmediate(previousTable);

            if (lineIndex > 0)
            {
                // instantiate new table
                spellRepertoireLinesTable =
                    Object.Instantiate(spellRepertoireLinesTable, previousTable.parent.transform);
                // clear it of children
                spellRepertoireLinesTable.DetachChildren();
                //spellRepertoireLinesTable.SetParent(previousTable.parent.transform, true);
                spellRepertoireLinesTable.localScale = previousTable.localScale;
                spellRepertoireLinesTable.transform.SetAsFirstSibling();
                SpellLineTables.Add(spellRepertoireLinesTable);
            }
        }

        var curLine = SetUpNewLine(indexOfLine, spellRepertoireLinesTable, spellRepertoireLines, __instance);

        curLine.Bind(
            __instance.Caster,
            rulesetSpellRepertoire,
            spellRepertoires.Count > 1,
            spellCastEngaged,
            slotAdvancementPanel,
            actionType,
            cantripOnly,
            startLevel,
            level,
            false);

        return spellRepertoireLinesTable;
    }

    private static SpellRepertoireLine SetUpNewLine(
        int index,
        Transform spellRepertoireLinesTable,
        ICollection<SpellRepertoireLine> spellRepertoireLines,
        SpellSelectionPanel __instance)
    {
        GameObject newLine;

        if (spellRepertoireLinesTable.childCount <= index)
        {
            newLine = Gui.GetPrefabFromPool(__instance.spellRepertoireLinePrefab,
                spellRepertoireLinesTable);
        }
        else
        {
            newLine = spellRepertoireLinesTable.GetChild(index).gameObject;
        }

        newLine.SetActive(true);

        var component = newLine.GetComponent<SpellRepertoireLine>();

        spellRepertoireLines.Add(component);

        return component;
    }

    private static bool IsLevelActive(
        RulesetSpellRepertoire spellRepertoire, int level,
        ActionDefinitions.ActionType actionType)
    {
        var spellActivationTime = actionType switch
        {
            ActionDefinitions.ActionType.Bonus => RuleDefinitions.ActivationTime.BonusAction,
            ActionDefinitions.ActionType.Main => RuleDefinitions.ActivationTime.Action,
            ActionDefinitions.ActionType.Reaction => RuleDefinitions.ActivationTime.Reaction,
            ActionDefinitions.ActionType.NoCost => RuleDefinitions.ActivationTime.NoCost,
            _ => RuleDefinitions.ActivationTime.Action
        };

        if (level == 0)
        {
            // changed to support game v1.3.44 and allow ancestry cantrips to display off battle
            return actionType == ActionDefinitions.ActionType.None ||
                   spellRepertoire.KnownCantrips.Any(cantrip => cantrip.ActivationTime == spellActivationTime) ||
                   (spellRepertoire.ExtraSpellsByTag.TryGetValue("BonusCantrips", out var bonusCantrips) &&
                    bonusCantrips.Any(cantrip => cantrip.ActivationTime == spellActivationTime));
        }

        switch (spellRepertoire.SpellCastingFeature.SpellReadyness)
        {
            case RuleDefinitions.SpellReadyness.Prepared when spellRepertoire.PreparedSpells
                .Any(spellDefinition =>
                    spellDefinition.SpellLevel == level
                    && spellDefinition.ActivationTime == spellActivationTime):
            case RuleDefinitions.SpellReadyness.AllKnown
                when spellRepertoire.KnownSpells.Any(spellDefinition => spellDefinition.SpellLevel == level):

                return true;

            default:
                return false;
        }
    }

    internal static void SetTeleporterGadgetActiveAnimation(WorldGadget worldGadget, bool visibility = false)
    {
        if (worldGadget.UserGadget == null)
        {
            return;
        }

        if (worldGadget.UserGadget.GadgetBlueprint == TeleporterIndividual)
        {
            var visualEffect = worldGadget.transform.FindChildRecursive("Vfx_Teleporter_Individual_Idle_01");

            // NOTE: don't use visualEffect?. which bypasses Unity object lifetime check
            if (visualEffect)
            {
                visualEffect.gameObject.SetActive(visibility);
            }
        }
        else if (worldGadget.UserGadget.GadgetBlueprint == TeleporterParty)
        {
            var visualEffect = worldGadget.transform.FindChildRecursive("Vfx_Teleporter_Party_Idle_01");

            // NOTE: don't use visualEffect?. which bypasses Unity object lifetime check
            if (visualEffect)
            {
                visualEffect.gameObject.SetActive(visibility);
            }
        }
    }

    private static bool IsGadgetExit(GadgetBlueprint gadgetBlueprint, bool onlyWithGizmos = false)
    {
        const int ExitsWithGizmos = 2;

        GadgetBlueprint[] gadgetExits =
        {
            VirtualExit, VirtualExitMultiple, Exit, ExitMultiple, TeleporterIndividual, TeleporterParty
        };

        return Array.IndexOf(gadgetExits, gadgetBlueprint) >= (onlyWithGizmos ? ExitsWithGizmos : 0);
    }

    internal static void HideExitsAndTeleportersGizmosIfNotDiscovered(
        GameGadget __instance,
        int conditionIndex,
        bool state)
    {
        if (conditionIndex < 0 || conditionIndex >= __instance.conditionNames.Count)
        {
            return;
        }

        if (!__instance.CheckIsEnabled() || !__instance.IsTeleport())
        {
            return;
        }

        var service = ServiceRepository.GetService<IGameLocationService>();

        if (service == null)
        {
            return;
        }

        var worldGadget = service.WorldLocation.WorldSectors
            .SelectMany(ws => ws.WorldGadgets)
            .FirstOrDefault(wg => wg.GameGadget == __instance);

        if (worldGadget == null)
        {
            return;
        }

        SetTeleporterGadgetActiveAnimation(worldGadget, state);
    }

    internal static void ComputeIsRevealedExtended(GameGadget __instance, ref bool __result)
    {
        var userGadget = Gui.GameLocation.UserLocation.UserRooms
            .SelectMany(a => a.UserGadgets)
            .FirstOrDefault(b => b.UniqueName == __instance.UniqueNameId);

        if (userGadget == null || !IsGadgetExit(userGadget.GadgetBlueprint))
        {
            return;
        }

        // reverts the revealed state and recalculates it
        __instance.revealed = false;
        __result = false;

        var x = (int)__instance.FeedbackPosition.x;
        var y = (int)__instance.FeedbackPosition.z;

        var feedbackPosition = new int3(x, 0, y);
        var referenceBoundingBox = new BoxInt(feedbackPosition, feedbackPosition);

        var gridAccessor = GridAccessor.Default;

        foreach (var position in referenceBoundingBox.EnumerateAllPositionsWithin())
        {
            if (!gridAccessor.Visited(position))
            {
                continue;
            }

            var gameLocationService = ServiceRepository.GetService<IGameLocationService>();
            var worldGadgets = gameLocationService.WorldLocation.WorldSectors.SelectMany(ws => ws.WorldGadgets);
            var worldGadget = worldGadgets.FirstOrDefault(wg => wg.GameGadget == __instance);

            var isInvisible = __instance.IsInvisible();
            var isEnabled = __instance.CheckIsEnabled();

            if (worldGadget != null)
            {
                SetTeleporterGadgetActiveAnimation(worldGadget, isEnabled && !isInvisible);
            }

            __instance.revealed = true;
            __result = true;

            break;
        }
    }

    internal static void SetHighlightVisibilityExtended(WorldGadget __instance, ref bool visible)
    {
        if (IsGadgetExit(__instance.UserGadget.GadgetBlueprint, true))
        {
            return;
        }

        var activator = DatabaseHelper.GadgetDefinitions.Activator;
        var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();
        var gameLocationVisibilityService = ServiceRepository.GetService<IGameLocationVisibilityService>();
        var feedbackPosition = __instance.GameGadget.FeedbackPosition;

        // activators aren't detected in their original position so we handle them in a different way
        if (!__instance.GadgetDefinition == activator)
        {
            var position = new int3((int)feedbackPosition.x, (int)feedbackPosition.y, (int)feedbackPosition.z);

            foreach (var gameLocationCharacter in gameLocationCharacterService.PartyCharacters)
            {
                visible = gameLocationVisibilityService.IsCellPerceivedByCharacter(position, gameLocationCharacter);

                if (visible)
                {
                    return;
                }
            }

            return;
        }

        // scan activators surrounding cells
        for (var x = -1; x <= 1; x++)
        {
            for (var z = -1; z <= 1; z++)
            {
                // jump original position
                if (x == 0 && z == 0)
                {
                    continue;
                }

                var position = new int3((int)feedbackPosition.x + x, (int)feedbackPosition.y,
                    (int)feedbackPosition.z + z);

                foreach (var gameLocationCharacter in gameLocationCharacterService.PartyCharacters)
                {
                    visible = gameLocationVisibilityService.IsCellPerceivedByCharacter(position,
                        gameLocationCharacter);

                    if (visible)
                    {
                        return;
                    }
                }
            }
        }
    }

    private static void LoadRemoveBugVisualModels()
    {
        if (!Main.Settings.RemoveBugVisualModels)
        {
            return;
        }

        // Spiderlings, fire spider, kindred spirit spider, BadlandsSpider(normal, conjured and wildshape versions)
        const string ASSET_REFERENCE_SPIDER_1 = "362fc51df586d254ab182ef854396f82";
        //CrimsonSpiderling, PhaseSpider, SpectralSpider, CrimsonSpider, deep spider(normal, conjured and wildshape versions)
        const string ASSET_REFERENCE_SPIDER_2 = "40b5fe532a9a0814097acdb16c74e967";
        // spider queen
        const string ASSET_REFERENCE_SPIDER_3 = "8fc96b2a8c5fcc243b124d31c63df5d9";
        //Giant_Beetle, Small_Beetle, Redeemer_Zealot, Redeemer_Pilgrim
        const string ASSET_REFERENCE_BEETLE = "04dfcec8c8afb8642a80c1116de218d4";
        //Young_Remorhaz, Remorhaz
        const string ASSET_REFERENCE_REMORHAZ = "ded896e0c4ef46144904375ecadb1bb1";

        var brownBear = DatabaseHelper.MonsterDefinitions.BrownBear;
        var bearPrefab = new AssetReference("cc36634f504fa7049a4499a91749d7d5");

        var wolf = DatabaseHelper.MonsterDefinitions.Wolf;
        var wolfPrefab = new AssetReference("6e02c9bcfb5122042a533e7732182b1d");

        var ape = DatabaseHelper.MonsterDefinitions.Ape_MonsterDefinition;
        var apePrefab = new AssetReference("8f4589a9a294b444785fab045256a713");

        var dbMonsterDefinition = DatabaseRepository.GetDatabase<MonsterDefinition>();

        // check every monster for targeted prefab guid references
        foreach (var monster in dbMonsterDefinition)
        {
            // get monster asset reference for prefab guid comparison
            var value = monster.MonsterPresentation.malePrefabReference;

            switch (value.AssetGUID)
            {
                // swap bears for spiders
                case ASSET_REFERENCE_SPIDER_1:
                case ASSET_REFERENCE_SPIDER_2:
                case ASSET_REFERENCE_SPIDER_3:
                    monster.MonsterPresentation.malePrefabReference = bearPrefab;
                    monster.MonsterPresentation.femalePrefabReference = bearPrefab;
                    monster.GuiPresentation.spriteReference = brownBear.GuiPresentation.SpriteReference;
                    monster.bestiarySpriteReference = brownBear.BestiarySpriteReference;
                    monster.MonsterPresentation.monsterPresentationDefinitions = brownBear.MonsterPresentation
                        .MonsterPresentationDefinitions;
                    break;
                // swap apes for remorhaz
                case ASSET_REFERENCE_REMORHAZ:
                    monster.MonsterPresentation.malePrefabReference = apePrefab;
                    monster.MonsterPresentation.femalePrefabReference = apePrefab;
                    monster.GuiPresentation.spriteReference = ape.GuiPresentation.SpriteReference;
                    monster.bestiarySpriteReference = ape.BestiarySpriteReference;
                    monster.MonsterPresentation.monsterPresentationDefinitions = ape.MonsterPresentation
                        .MonsterPresentationDefinitions;
                    break;
                // swap wolves for beetles
                case ASSET_REFERENCE_BEETLE:
                    monster.MonsterPresentation.malePrefabReference = wolfPrefab;
                    monster.MonsterPresentation.femalePrefabReference = wolfPrefab;
                    monster.GuiPresentation.spriteReference = wolf.GuiPresentation.SpriteReference;
                    monster.bestiarySpriteReference = wolf.BestiarySpriteReference;
                    monster.MonsterPresentation.monsterPresentationDefinitions = wolf.MonsterPresentation
                        .MonsterPresentationDefinitions;

                    // changing beetle scale to suit replacement model
                    monster.MonsterPresentation.maleModelScale = 0.655f;
                    monster.MonsterPresentation.femaleModelScale = 0.655f;
                    break;
            }
        }
    }

    internal static void SwitchCrownOfTheMagister()
    {
        var crowns = new[]
        {
            CrownOfTheMagister, CrownOfTheMagister01, CrownOfTheMagister02, CrownOfTheMagister03,
            CrownOfTheMagister04, CrownOfTheMagister05, CrownOfTheMagister06, CrownOfTheMagister07,
            CrownOfTheMagister08, CrownOfTheMagister09, CrownOfTheMagister10, CrownOfTheMagister11,
            CrownOfTheMagister12
        };

        foreach (var itemPresentation in crowns.Select(x => x.ItemPresentation))
        {
            var maleBodyPartBehaviours = itemPresentation.GetBodyPartBehaviours(RuleDefinitions.CreatureSex.Male);

            maleBodyPartBehaviours[0] = Main.Settings.EnableInvisibleCrownOfTheMagister
                ? GraphicsCharacterDefinitions.BodyPartBehaviour.Shape
                : GraphicsCharacterDefinitions.BodyPartBehaviour.Armor;
        }
    }

    internal static void SwitchEmpressGarb()
    {
        EmpressGarbOriginalItemPresentation ??= Enchanted_ChainShirt_Empress_war_garb.ItemPresentation;

        switch (Main.Settings.EmpressGarbAppearanceIndex)
        {
            case 0: //"Normal":
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = EmpressGarbOriginalItemPresentation;
                break;

            case 1: // Barbarian Clothes
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = BarbarianClothes.ItemPresentation;
                break;

            case 2: // Druid Leather
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = LeatherDruid.ItemPresentation;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.useArmorAddressableName = true;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.armorAddressableName = LeatherDruid.Name;
                break;

            case 3: // Elven Chain
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = ElvenChain.ItemPresentation;
                break;

            case 4: // Sorcerer's Armor
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = SorcererArmor.ItemPresentation;
                break;

            case 5: // Studded Leather
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = StuddedLeather.ItemPresentation;
                break;

            case 6: // Sylvan Armor
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = GreenmageArmor.ItemPresentation;
                break;

            case 7: // Wizard Clothes
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = WizardClothes_Alternate.ItemPresentation;
                break;

            case 8: // Scavenger Ranger
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = ClothesScavenger_A.ItemPresentation;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.useArmorAddressableName = true;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.armorAddressableName = ClothesScavenger_A.Name;
                break;

            case 9: // Scavenger Rogue
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = ClothesScavenger_B.ItemPresentation;
                break;

            case 10: // Bard Armor
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = Bard_Armor.ItemPresentation;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.useArmorAddressableName = true;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.armorAddressableName = Bard_Armor.Name;
                break;

            case 11: // Warlock Armor
                Enchanted_ChainShirt_Empress_war_garb.itemPresentation = Warlock_Armor.ItemPresentation;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.useArmorAddressableName = true;
                Enchanted_ChainShirt_Empress_war_garb.ItemPresentation.armorAddressableName = Warlock_Armor.Name;
                break;
        }
    }

    internal static FeatureDefinitionActionAffinity ActionAffinityFeatCrusherToggle { get; private set; }

    private static void LoadFeatCrusherToggle()
    {
        _ = ActionDefinitionBuilder
            .Create(DatabaseHelper.ActionDefinitions.MetamagicToggle, "FeatCrusherToggle")
            .SetOrUpdateGuiPresentation(Category.Action)
            .RequiresAuthorization()
            .SetActionId(ExtraActionId.FeatCrusherToggle)
            .AddToDB();

        ActionAffinityFeatCrusherToggle = FeatureDefinitionActionAffinityBuilder
            .Create(DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinitySorcererMetamagicToggle,
                "ActionAffinityFeatCrusherToggle")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.FeatCrusherToggle)
            .AddToDB();
    }

    internal static FeatureDefinitionActionAffinity ActionAffinityMonkKiPointsToggle { get; private set; }

    private static void LoadMonkKiPointsToggle()
    {
        _ = ActionDefinitionBuilder
            .Create(DatabaseHelper.ActionDefinitions.MetamagicToggle, "MonkKiPointsToggle")
            .SetOrUpdateGuiPresentation(Category.Action)
            .RequiresAuthorization()
            .SetActionId(ExtraActionId.MonkKiPointsToggle)
            .AddToDB();

        ActionAffinityMonkKiPointsToggle = FeatureDefinitionActionAffinityBuilder
            .Create(DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinitySorcererMetamagicToggle,
                "ActionAffinityMonkKiPointsToggle")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.MonkKiPointsToggle)
            .AddToDB();
    }

    internal static FeatureDefinitionActionAffinity ActionAffinityPaladinSmiteToggle { get; private set; }

    private static void LoadPaladinSmiteToggle()
    {
        _ = ActionDefinitionBuilder
            .Create(DatabaseHelper.ActionDefinitions.MetamagicToggle, "PaladinSmiteToggle")
            .SetOrUpdateGuiPresentation(Category.Action)
            .RequiresAuthorization()
            .SetActionId(ExtraActionId.PaladinSmiteToggle)
            .AddToDB();

        ActionAffinityPaladinSmiteToggle = FeatureDefinitionActionAffinityBuilder
            .Create(DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinitySorcererMetamagicToggle,
                "ActionAffinityPaladinSmiteToggle")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.PaladinSmiteToggle)
            .AddToDB();
    }

#if false
    private static FeatureDefinitionActionAffinity ActionAffinityMonsterSwapAttackToggle { get; set; }

    private static void LoadMonsterSwapAttackToggle()
    {
        _ = ActionDefinitionBuilder
            .Create(DatabaseHelper.ActionDefinitions.MetamagicToggle, "MonsterSwapAttackToggle")
            .SetOrUpdateGuiPresentation(Category.Action)
            .RequiresAuthorization()
            .SetActionId(ExtraActionId.MonsterSwapAttackToggle)
            .AddToDB();

        ActionAffinityMonsterSwapAttackToggle = FeatureDefinitionActionAffinityBuilder
            .Create(DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinitySorcererMetamagicToggle,
                "ActionAffinityMonsterSwapAttackToggle")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.MonsterSwapAttackToggle)
            .AddToDB();
    }

    internal static void AddMonsterSwapAttackToggle(RulesetCharacterMonster monster)
    {
        var monsterDefinition = monster.MonsterDefinition;

        if (Main.Settings.AddMonsterSwapAttackToggle &&
            monsterDefinition.AttackIterations.Count > 1 &&
            monsterDefinition.AttackIterations[0] != monsterDefinition.AttackIterations[1])
        {
            monster.ActiveFeatures.Add(ActionAffinityMonsterSwapAttackToggle);
        }
    }
#endif

    internal static void ResetFormationGrid(int selectedSet)
    {
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                Main.Settings.FormationGridSets[selectedSet][y][x] = FormationGridSetTemplates[selectedSet][y][x];
            }
        }
    }

    internal static void ResetAllFormationGrids()
    {
        for (var i = 0; i < FormationGridSetTemplates.Length; i++)
        {
            ResetFormationGrid(i);
        }

        Main.Settings.FormationGridSelectedSet = 0;
    }

    private static void LoadFormationGrid()
    {
        if (Main.Settings.FormationGridSelectedSet < 0)
        {
            ResetAllFormationGrids();
        }
        else
        {
            FillDefinitionFromFormationGrid();
        }
    }

#if false
    private static void FillFormationGridFromDefinition(int selectedSet)
    {
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                Main.Settings.FormationGridSets[selectedSet][y][x] = 0;
            }
        }

        foreach (var position in DatabaseHelper.FormationDefinitions.Column2.FormationPositions)
        {
            Main.Settings.FormationGridSets[selectedSet][-position.z][position.x + 2] = 1;
        }
    }
#endif

    internal static void FillDefinitionFromFormationGrid()
    {
        var position = 0;
        var selectedSet = Main.Settings.FormationGridSelectedSet;

        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                if (Main.Settings.FormationGridSets[selectedSet][y][x] == 1)
                {
                    DatabaseHelper.FormationDefinitions.Column2.FormationPositions[position++] = new int3(x - 2, 0, -y);
                }
            }
        }

        if (UnityModManagerUIPatcher.ModManagerUI.IsOpen)
        {
            return;
        }

        Gui.GuiService.ShowAlert(
            Gui.Format("ModUi/&FormationSelected", (Main.Settings.FormationGridSelectedSet + 1).ToString()),
            Gui.ColorAlert);
    }

    internal static void Load()
    {
        InventoryManagementContext.Load();
        SwitchCrownOfTheMagister();
        SwitchEmpressGarb();
        LoadRemoveBugVisualModels();
        LoadFeatCrusherToggle();
        LoadMonkKiPointsToggle();
        LoadPaladinSmiteToggle();
        //LoadMonsterSwapAttackToggle();
        LoadFormationGrid();

        var inputService = ServiceRepository.GetService<IInputService>();

        // Dungeon Maker
        inputService.RegisterCommand(InputCommands.Id.EditorRotate, (int)KeyCode.R, (int)KeyCode.LeftShift);

        // HUD
        inputService.RegisterCommand(CtrlShiftH, (int)KeyCode.H, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Debug Overlay
        inputService.RegisterCommand(CtrlShiftD, (int)KeyCode.D, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Export Character
        inputService.RegisterCommand(CtrlShiftE, (int)KeyCode.E, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Spawn Encounter
        inputService.RegisterCommand(CtrlShiftS, (int)KeyCode.S, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Teleport
        inputService.RegisterCommand(CtrlShiftT, (int)KeyCode.T, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Rejoin
        inputService.RegisterCommand(CtrlShiftR, (int)KeyCode.R, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);

        // Formation
        inputService.RegisterCommand(CtrlShift1, (int)KeyCode.Alpha1, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);
        inputService.RegisterCommand(CtrlShift2, (int)KeyCode.Alpha2, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);
        inputService.RegisterCommand(CtrlShift3, (int)KeyCode.Alpha3, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);
        inputService.RegisterCommand(CtrlShift4, (int)KeyCode.Alpha4, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);
        inputService.RegisterCommand(CtrlShift5, (int)KeyCode.Alpha5, (int)KeyCode.LeftShift,
            (int)KeyCode.LeftControl);
    }

    internal static void HandleInput(GameLocationBaseScreen gameLocationBaseScreen, InputCommands.Id command)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (command)
        {
            case CtrlShiftE when Main.Settings.EnableCharacterExport:
                CharacterExportContext.ExportInspectedCharacter();
                break;
            case CtrlShiftH when Main.Settings.EnableHotkeyToggleHud:
                GameHud.ShowAll(gameLocationBaseScreen, GetInitiativeOrPartyPanel(), GetTimeAndNavigationPanel());
                break;
            case CtrlShiftD when Main.Settings.EnableHotkeyDebugOverlay:
                ServiceRepository.GetService<IDebugOverlayService>()?.ToggleActivation();
                break;
            case CtrlShiftT when Main.Settings.EnableTeleportParty:
                Teleporter.ConfirmTeleportParty(Teleporter.GetEncounterPosition);
                break;
            case CtrlShiftR when Main.Settings.EnableRejoinParty:
                Teleporter.ConfirmTeleportParty(Teleporter.GetLeaderPosition);
                break;
            case CtrlShiftS when EncountersSpawnContext.EncounterCharacters.Count > 0:
                EncountersSpawnContext.ConfirmStageEncounter();
                break;
            case CtrlShift1 when Main.Settings.EnableHotkeySwapFormationSets && !Global.IsMultiplayer:
                Main.Settings.FormationGridSelectedSet = 0;
                FillDefinitionFromFormationGrid();
                break;
            case CtrlShift2 when Main.Settings.EnableHotkeySwapFormationSets && !Global.IsMultiplayer:
                Main.Settings.FormationGridSelectedSet = 1;
                FillDefinitionFromFormationGrid();
                break;
            case CtrlShift3 when Main.Settings.EnableHotkeySwapFormationSets && !Global.IsMultiplayer:
                Main.Settings.FormationGridSelectedSet = 2;
                FillDefinitionFromFormationGrid();
                break;
            case CtrlShift4 when Main.Settings.EnableHotkeySwapFormationSets && !Global.IsMultiplayer:
                Main.Settings.FormationGridSelectedSet = 3;
                FillDefinitionFromFormationGrid();
                break;
            case CtrlShift5 when Main.Settings.EnableHotkeySwapFormationSets && !Global.IsMultiplayer:
                Main.Settings.FormationGridSelectedSet = 4;
                FillDefinitionFromFormationGrid();
                break;
        }

        [CanBeNull]
        GuiPanel GetInitiativeOrPartyPanel()
        {
            return gameLocationBaseScreen switch
            {
                GameLocationScreenExploration gameLocationScreenExploration => gameLocationScreenExploration
                    .partyControlPanel,
                GameLocationScreenBattle gameLocationScreenBattle => gameLocationScreenBattle.initiativeTable,
                _ => null
            };
        }

        [CanBeNull]
        TimeAndNavigationPanel GetTimeAndNavigationPanel()
        {
            return gameLocationBaseScreen switch
            {
                GameLocationScreenExploration gameLocationScreenExploration => gameLocationScreenExploration
                    .timeAndNavigationPanel,
                GameLocationScreenBattle gameLocationScreenBattle => gameLocationScreenBattle.timeAndNavigationPanel,
                _ => null
            };
        }
    }

    internal static class GameHud
    {
        internal static void ShowAll([NotNull] GameLocationBaseScreen gameLocationBaseScreen,
            GuiPanel initiativeOrPartyPanel,
            TimeAndNavigationPanel timeAndNavigationPanel)
        {
            var guiConsoleScreen = Gui.GuiService.GetScreen<GuiConsoleScreen>();
            var anyVisible = guiConsoleScreen.Visible || gameLocationBaseScreen.CharacterControlPanel.Visible ||
                             initiativeOrPartyPanel.Visible || timeAndNavigationPanel.Visible;

            ShowCharacterControlPanel(gameLocationBaseScreen, anyVisible);
            TogglePanelVisibility(guiConsoleScreen, anyVisible);
            TogglePanelVisibility(initiativeOrPartyPanel);
            TogglePanelVisibility(timeAndNavigationPanel, anyVisible);
        }

        private static void ShowCharacterControlPanel([NotNull] GameLocationBaseScreen gameLocationBaseScreen,
            bool forceHide = false)
        {
            var characterControlPanel = gameLocationBaseScreen.CharacterControlPanel;

            if (characterControlPanel.Visible || forceHide)
            {
                characterControlPanel.Hide();
                characterControlPanel.Unbind();
            }
            else
            {
                var gameLocationSelectionService = ServiceRepository.GetService<IGameLocationSelectionService>();

                if (gameLocationSelectionService.SelectedCharacters.Count <= 0)
                {
                    return;
                }

                characterControlPanel.Bind(gameLocationSelectionService.SelectedCharacters[0],
                    gameLocationBaseScreen.ActionTooltipDock);
                characterControlPanel.Show();
            }
        }

        private static void TogglePanelVisibility(GuiPanel guiPanel, bool forceHide = false)
        {
            if (guiPanel == null)
            {
                return;
            }

            if (guiPanel.Visible || forceHide)
            {
                guiPanel.Hide();
            }
            else
            {
                guiPanel.Show();
            }
        }

        internal static void RefreshCharacterControlPanel()
        {
            if (Gui.CurrentLocationScreen != null && Gui.CurrentLocationScreen is GameLocationBaseScreen location)
            {
                location.CharacterControlPanel.RefreshNow();
            }
        }
    }

    private static class Teleporter
    {
        internal static void ConfirmTeleportParty(Func<int3> getPosition)
        {
            var position = getPosition();

            Gui.GuiService.ShowMessage(
                MessageModal.Severity.Attention2,
                "Message/&TeleportPartyTitle",
                Gui.Format("Message/&TeleportPartyDescription", position.x.ToString(), position.x.ToString()),
                "Message/&MessageYesTitle", "Message/&MessageNoTitle",
                () => TeleportParty(position),
                null);
        }

        internal static int3 GetEncounterPosition()
        {
            var gameLocationService = ServiceRepository.GetService<IGameLocationService>();
            var x = (int)gameLocationService.GameLocation.LastCameraPosition.x;
            var z = (int)gameLocationService.GameLocation.LastCameraPosition.z;

            return new int3(x, 0, z);
        }

        internal static int3 GetLeaderPosition()
        {
            var gameLocationCharacterService =
                ServiceRepository.GetService<IGameLocationCharacterService>() as GameLocationCharacterManager;

            if (gameLocationCharacterService == null)
            {
                return int3.invalid;
            }

            var position = gameLocationCharacterService.PartyCharacters[0].LocationPosition;
            var currentCharacter = Global.CurrentCharacter ??
                                   gameLocationCharacterService.PartyCharacters[0].RulesetCharacter;
            var locationCharacter = gameLocationCharacterService.AllValidEntities
                .FirstOrDefault(x => x.RulesetCharacter == currentCharacter);

            return locationCharacter?.LocationPosition ?? position;
        }

        private static void TeleportParty(int3 position)
        {
            var gameLocationActionService = ServiceRepository.GetService<IGameLocationActionService>();
            var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();
            var gameLocationPositioningService = ServiceRepository.GetService<IGameLocationPositioningService>();
            var formationPositions = new List<int3>();
            var partyAndGuests = new List<GameLocationCharacter>();
            var positions = new List<int3>();

            for (var iy = 0; iy < 4; iy++)
            {
                for (var ix = 0; ix < 2; ix++)
                {
                    formationPositions.Add(new int3(ix, 0, iy));
                }
            }

            partyAndGuests.AddRange(gameLocationCharacterService.PartyCharacters);
            partyAndGuests.AddRange(gameLocationCharacterService.GuestCharacters);

            gameLocationPositioningService.ComputeFormationPlacementPositions(
                partyAndGuests,
                position,
                LocationDefinitions.Orientation.North,
                formationPositions,
                CellHelpers.PlacementMode.Station,
                positions,
                new List<RulesetActor.SizeParameters>(),
                25);

            for (var index = 0; index < positions.Count; index++)
            {
                partyAndGuests[index].LocationPosition = positions[index];

                // rotates the characters in position to force the game to redrawn them
                gameLocationActionService.MoveCharacter(
                    partyAndGuests[index],
                    positions[(index + 1) % positions.Count],
                    LocationDefinitions.Orientation.North, 0,
                    ActionDefinitions.MoveStance.Walk);
            }
        }
    }
}
