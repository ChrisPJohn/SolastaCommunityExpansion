﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches
{
    // enables epic points
    [HarmonyPatch(typeof(CharacterStageAbilityScoresPanel), "Reset")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GameManager_Reset
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (Main.Settings.EnableEpicPoints)
                {
                    if (instruction.opcode == OpCodes.Ldc_I4_S && instruction.operand.ToString() == Settings.GAME_BUY_POINTS.ToString())
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, Settings.MOD_BUY_POINTS);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    // enables epic points
    [HarmonyPatch(typeof(CharacterStageAbilityScoresPanel), "Refresh")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GameManager_Refresh
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (Main.Settings.EnableEpicPoints)
                {
                    if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand.ToString() == Settings.GAME_BUY_POINTS.ToString())
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 1f * Settings.MOD_BUY_POINTS);
                    }
                    else if (instruction.opcode == OpCodes.Ldc_I4_S && instruction.operand.ToString() == Settings.GAME_BUY_POINTS.ToString())
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, Settings.MOD_BUY_POINTS);
                    }
                    else if (instruction.opcode == OpCodes.Ldc_I4_S && instruction.operand.ToString() == Settings.GAME_MAX_ATTRIBUTE.ToString())
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, Settings.MOD_MAX_ATTRIBUTE);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
