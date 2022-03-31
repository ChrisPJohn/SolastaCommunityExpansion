﻿using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaCommunityExpansion;

namespace SolastaMulticlass.Patches.Cantrips
{
    internal static class CharacterActionCastSpellPatcher
    {
        // enforces cantrips to be cast at character level
        internal static readonly Dictionary<ulong, int> CasterLevel = new();

        [HarmonyPatch(typeof(CharacterActionCastSpell), "GetAdvancementData")]
        internal static class CharacterActionCastSpellGetAdvancementData
        {
            public static int SpellCastingLevel(RulesetSpellRepertoire rulesetSpellRepertoire, CharacterActionCastSpell characterActionCastSpell)
            {
                if (characterActionCastSpell.ActingCharacter.RulesetCharacter is RulesetCharacterHero hero 
                    && characterActionCastSpell.ActiveSpell.SpellDefinition.SpellLevel == 0)
                {
                    return hero.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
                }
                
                return rulesetSpellRepertoire.SpellCastingLevel;
            }

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!Main.Settings.EnableMulticlass)
                {
                    foreach (var instruction in instructions)
                    {
                        yield return instruction;
                    }

                    yield break;
                }

                var spellCastingLevelMethod = typeof(RulesetSpellRepertoire).GetMethod("get_SpellCastingLevel");
                var mySpellCastingLevelMethod = typeof(CharacterActionCastSpellGetAdvancementData).GetMethod("SpellCastingLevel");

                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(spellCastingLevelMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        yield return new CodeInstruction(OpCodes.Call, mySpellCastingLevelMethod);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }
    }
}
