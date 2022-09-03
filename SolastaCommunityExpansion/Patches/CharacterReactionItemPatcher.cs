﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.CustomUI;
using SolastaCommunityExpansion.Utils;
using UnityEngine;

namespace SolastaCommunityExpansion.Patches;

internal static class CharacterReactionItemPatcher
{
    [HarmonyPatch(typeof(CharacterReactionItem), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        [NotNull]
        internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: replaces calls to the Bind of `CharacterReactionSubitem` with custom method
            
            var codes = instructions.ToList();
            var customBindMethod =
                new Action<CharacterReactionSubitem, RulesetSpellRepertoire, int, string, bool,
                    CharacterReactionSubitem.SubitemSelectedHandler, ReactionRequest>(CustomBind).Method;

            var bind = typeof(CharacterReactionSubitem).GetMethod("Bind",
                BindingFlags.Public | BindingFlags.Instance);

            var bindIndex = codes.FindIndex(x => x.Calls(bind));

            if (bindIndex > 0)
            {
                codes[bindIndex] = new CodeInstruction(OpCodes.Call, customBindMethod);
                codes.Insert(bindIndex, new CodeInstruction(OpCodes.Ldarg_1));
            }

            //PATCH: removes Trace.Assert() that checks if character has any spell repertoires
            //this assert was added in 1.4.5 and triggers if non-spell caster does AoO
            //happens because we replaced default AoO reaction with warcaster one, so they would merge properly when several are triggered at once
            Transpile.RemoveBoolAsserts(codes);

            return codes.AsEnumerable();
        }

        internal static void Postfix([NotNull] CharacterReactionItem __instance)
        {
            var request = __instance.ReactionRequest;
            var size = request is ReactionRequestWarcaster or ReactionRequestSpendBundlePower
                ? 400
                : 290;

            __instance.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        }

        //patch implementation
        //calls custom warcaster and power bundle binds when appropriate
        private static void CustomBind(
            [NotNull] CharacterReactionSubitem instance,
            RulesetSpellRepertoire spellRepertoire,
            int slotLevel,
            string text,
            bool interactable,
            CharacterReactionSubitem.SubitemSelectedHandler subitemSelected,
            ReactionRequest reactionRequest)
        {
            switch (reactionRequest)
            {
                case ReactionRequestWarcaster warcasterRequest:
                    instance.BindWarcaster(warcasterRequest, slotLevel, interactable, subitemSelected);
                    break;
                case ReactionRequestSpendBundlePower bundlePowerRequest:
                    instance.BindPowerBundle(bundlePowerRequest, slotLevel, interactable, subitemSelected);
                    break;
                default:
                    instance.Bind(spellRepertoire, slotLevel, text, interactable, subitemSelected);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterReactionItem), "GetSelectedSubItem")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GetSelectedSubItem_Patch
    {
        internal static bool Prefix([NotNull] CharacterReactionItem __instance, out int __result)
        {
            //TODO: check if still relevant - while this method wasn't touched, maybe sub-items are now disposed properly?
            //PATCH: Bugfix: Replace `GetSelectedSubItem` to fix reaction selection crashes.
            // Default one selects last item that is Selected, regardless if it is active or not, leading to wrong spell slots for smites being selected
            // This implementation returns first item that is both Selected and active
            
            __result = 0;

            var itemsTable = __instance.subItemsTable;

            for (var index = 0; index < itemsTable.childCount; ++index)
            {
                var item = itemsTable.GetChild(index).GetComponent<CharacterReactionSubitem>();

                if (!item.gameObject.activeSelf || !item.Selected)
                {
                    continue;
                }

                __result = index;
                break;
            }

            return false;
        }
    }
}