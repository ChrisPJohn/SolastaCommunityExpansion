﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.InitialChoices
{
    internal static class GuiFeatDefinitionPatcher
    {
        [HarmonyPatch(typeof(GuiFeatDefinition), "IsFeatMacthingPrerequisites")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class GuiFeatDefinition_IsFeatMacthingPrerequisites
        {
            //internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            //{
            //    var code = new List<CodeInstruction>(instructions);
            //    code.Find(x => x.opcode == OpCodes.Ldc_I4_1).opcode = OpCodes.Ldc_I4_0;

            //    return code;
            //}

            internal static void Postfix(FeatDefinition feat, RulesetCharacterHero hero, ref bool __result)
            {
                if (Main.Settings.EnableFirstLevelCasterFeats && !__result && feat.MustCastSpellsPrerequisite && hero.SpellRepertoires.Count == 0)
                {
                    GetLastAssignedClassAndLevel(hero, out CharacterClassDefinition lastClassDefinition, out int _);

                    foreach (FeatureUnlockByLevel featureUnlock in lastClassDefinition.FeatureUnlocks)
                    {
                        if (featureUnlock.FeatureDefinition is FeatureDefinitionCastSpell)
                        {
                            __result = true;
                            return;
                        }
                    }

                    if (hero.ClassesAndSubclasses.TryGetValue(lastClassDefinition, out CharacterSubclassDefinition subclassDefinition))
                    {
                        foreach (FeatureUnlockByLevel featureUnlock in subclassDefinition.FeatureUnlocks)
                        {
                            if (featureUnlock.FeatureDefinition is FeatureDefinitionCastSpell)
                            {
                                __result = true;
                                return;
                            }
                        }
                    }
                }
            }

            private static void GetLastAssignedClassAndLevel(RulesetCharacterHero hero, out CharacterClassDefinition lastClassDefinition, out int level)
            {
                lastClassDefinition = null;
                level = 0;

                if (hero.ClassesHistory.Count > 0)
                {
                    lastClassDefinition = hero.ClassesHistory[hero.ClassesHistory.Count - 1];
                    level = hero.ClassesAndLevels[lastClassDefinition];
                }
            }
        }
    }
}
