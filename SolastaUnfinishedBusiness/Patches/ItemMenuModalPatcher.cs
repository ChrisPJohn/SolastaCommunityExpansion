﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

internal static class ItemMenuModalPatcher
{
    //PATCH: allows mark deity to work with MC heroes (Multiclass)
    [HarmonyPatch(typeof(ItemMenuModal), "SetupFromItem")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SetupFromItem_Patch
    {
        public static bool RequiresDeity(ItemMenuModal itemMenuModal)
        {
            return itemMenuModal.GuiCharacter.RulesetCharacterHero.ClassesHistory.Exists(x => x.RequiresDeity);
        }

        public static int MaxSpellLevelOfSpellCastingLevel(RulesetSpellRepertoire repertoire)
        {
            return SharedSpellsContext.GetClassSpellLevel(repertoire);
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var requiresDeityMethod = typeof(CharacterClassDefinition).GetMethod("get_RequiresDeity");
            var myRequiresDeityMethod = typeof(SetupFromItem_Patch).GetMethod("RequiresDeity");
            var maxSpellLevelOfSpellCastingLevelMethod =
                typeof(RulesetSpellRepertoire).GetMethod("get_MaxSpellLevelOfSpellCastingLevel");
            var myMaxSpellLevelOfSpellCastingLevelMethod =
                typeof(SetupFromItem_Patch).GetMethod("MaxSpellLevelOfSpellCastingLevel");

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(requiresDeityMethod))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, myRequiresDeityMethod);
                }
                else if (instruction.Calls(maxSpellLevelOfSpellCastingLevelMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, myMaxSpellLevelOfSpellCastingLevelMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
