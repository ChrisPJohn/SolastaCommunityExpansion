﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static SolastaCommunityExpansion.Models.Level20Context;

namespace SolastaCommunityExpansion.Patches
{
    // allows custom dungeons to be set for parties up to level 20
    internal static class UserLocationSettingsModalPatcher
    {
        [HarmonyPatch(typeof(UserLocationSettingsModal), "OnMinLevelEndEdit")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        public static class UserLocationSettingsModal_OnMinLevelEndEdit_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = new List<CodeInstruction>(instructions);

                if (Main.Settings.EnableLevel20)
                {
                    var opcodes = code.FindAll(x => x.opcode.Name == "ldc.i4.s" && Convert.ToInt32(x.operand) == GAME_MAX_LEVEL);

                    foreach (var opcode in opcodes)
                    {
                        opcode.operand = MOD_MAX_LEVEL;
                    }
                }

                return code;
            }
        }

        [HarmonyPatch(typeof(UserLocationSettingsModal), "OnMaxLevelEndEdit")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        public static class UserLocationSettingsModal_OnMaxLevelEndEdit_Patch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = new List<CodeInstruction>(instructions);

                if (Main.Settings.EnableLevel20)
                {
                    var opcodes = code.FindAll(x => x.opcode.Name == "ldc.i4.s" && Convert.ToInt32(x.operand) == GAME_MAX_LEVEL);

                    foreach (var opcode in opcodes)
                    {
                        opcode.operand = MOD_MAX_LEVEL;
                    }
                }

                return code;
            }
        }
    }
}
