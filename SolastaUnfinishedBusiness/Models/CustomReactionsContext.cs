﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Feats;
using UnityEngine;
using static ActionDefinitions;
using Object = UnityEngine.Object;

namespace SolastaUnfinishedBusiness.Models;

public static class CustomReactionsContext
{
    private static bool _forcePreferredCantrip; //used by actual feature
    private static bool _forcePreferredCantripUI; //used for local UI state

    public static void Load()
    {
        MakeReactDefinition(ReactionRequestWarcaster.Name);
        MakeReactDefinition(ReactionRequestSpendBundlePower.Name);
        MakeReactDefinition(ReactionRequestReactionAttack.Name(EwFeats.SentinelFeat));
    }

    private static void MakeReactDefinition(string name)
    {
        ReactionDefinitionBuilder
            .Create(DatabaseHelper.ReactionDefinitions.OpportunityAttack, name,
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Reaction)
            .AddToDB();
    }

    //leaving this in case we need react with spell functionality
    private static IEnumerator ReactWithSpell(SpellDefinition spell, GameLocationCharacter caster,
        GameLocationCharacter target)
    {
        var actionManager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

        if (actionManager == null)
        {
            yield break;
        }

        var ruleCaster = caster.RulesetCharacter;
        var spellSlot = ruleCaster.GetLowestSlotLevelAndRepertoireToCastSpell(spell, out var spellBook);

        if (spellBook == null)
        {
            yield break;
        }

        var ruleset = ServiceRepository.GetService<IRulesetImplementationService>();
        var reactionParams = new CharacterActionParams(caster, Id.CastReaction)
        {
            IntParameter = 0,
            RulesetEffect =
                ruleset.InstantiateEffectSpell(ruleCaster, spellBook, spell, spellSlot, false),
            IsReactionEffect = true
        };

        reactionParams.TargetCharacters.Add(target);
        reactionParams.ActionModifiers.Add(new ActionModifier());

        var reactions = actionManager.PendingReactionRequestGroups.Count;
        var reaction = new ReactionRequestCastDamageSpell(reactionParams, target, spellSlot == 0);

        actionManager.AddInterruptRequest(reaction);

        yield return WaitForReactions(actionManager, reactions);
    }

    private static IEnumerator WaitForReactions([CanBeNull] IGameLocationActionService actionService,
        int previousReactionCount)
    {
        while (actionService?.PendingReactionRequestGroups != null &&
               previousReactionCount < actionService.PendingReactionRequestGroups.Count)
        {
            yield return null;
        }
    }

    internal static void SaveReadyActionPreferredCantrip([CanBeNull] CharacterActionParams actionParams,
        ReadyActionType readyActionType)
    {
        if (actionParams != null && readyActionType == ReadyActionType.Cantrip)
        {
            actionParams.BoolParameter4 = _forcePreferredCantripUI;
        }
    }

    internal static void ReadReadyActionPreferredCantrip(CharacterActionParams actionParams)
    {
        if (actionParams is { ReadyActionType: ReadyActionType.Cantrip })
        {
            _forcePreferredCantrip = actionParams.BoolParameter4;
        }
    }

    public static void SetupForcePreferredToggle(RectTransform parent)
    {
        PersonalityFlagToggle toggle;
        if (parent.childCount < 3)
        {
            var prefab = Resources.Load<GameObject>("Gui/Prefabs/CharacterEdition/PersonalityFlagToggle");
            var asset = Object.Instantiate(prefab, parent, false);
            asset.name = "ForcePreferredToggle";

            var transform = asset.GetComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.anchoredPosition = new Vector2(0f, 1);
            transform.localPosition = new Vector3(0, -30);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);

            var title = parent.GetChild(0);
            title.localPosition = new Vector3(-100, 55);

            var group = parent.GetChild(1).GetComponent<RectTransform>();
            group.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            group.localPosition = new Vector3(-100, 5);

            toggle = asset.GetComponent<PersonalityFlagToggle>();

            var guiLabel = toggle.titleLabel;
            guiLabel.Text = "UI/&ForcePreferredCantripTitle";

            var tooltip = toggle.tooltip;
            tooltip.Content = "UI/&ForcePreferredCantripDescription";

            toggle.PersonalityFlagDefinition = DatabaseHelper.PersonalityFlagDefinitions.Authority;
            toggle.PersonalityFlagSelected = (_, _, state) =>
            {
                _forcePreferredCantripUI = state;
                tooltip.Content = "UI/&ForcePreferredCantripDescription";
            };
        }
        else
        {
            toggle = parent.FindChildRecursive("ForcePreferredToggle").GetComponent<PersonalityFlagToggle>();
        }

        toggle.Refresh(_forcePreferredCantripUI, true);
        toggle.tooltip.Content = "UI/&ForcePreferredCantripDescription";
    }

    public static void ForcePreferredCantripUsage(List<CodeInstruction> codes)
    {
        var customBindMethod =
            new Func<List<SpellDefinition>, SpellDefinition, bool>(CheckAndModifyCantrips).Method;

        var containsIndex = -1;
        //TODO: is there a better way to detect proper placement?
        for (var i = 0; i < codes.Count; i++)
        {
            if (i < 1)
            {
                continue;
            }

            var code = codes[i];

            if (code.opcode != OpCodes.Callvirt || !code.operand.ToString().Contains("Contains"))
            {
                continue;
            }

            var prev = codes[i - 1];

            if (prev.opcode != OpCodes.Callvirt || !prev.operand.ToString().Contains("PreferredReadyCantrip"))
            {
                continue;
            }

            containsIndex = i;
            break;
        }

        if (containsIndex > 0)
        {
            codes[containsIndex] = new CodeInstruction(OpCodes.Call, customBindMethod);
        }
    }

    private static bool CheckAndModifyCantrips(List<SpellDefinition> readied,
        SpellDefinition preferred)
    {
        if (!_forcePreferredCantrip)
        {
            return readied.Contains(preferred);
        }

        readied.RemoveAll(c => c != preferred);
        return !readied.Empty();
    }
}
