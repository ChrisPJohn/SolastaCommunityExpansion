﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.CustomBehaviors;

namespace SolastaUnfinishedBusiness.Patches;

internal static class CharacterActionAttackPatcher
{
    [HarmonyPatch(typeof(CharacterActionAttack), "ExecuteImpl")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ExecuteImpl_Patch
    {
        internal static IEnumerator Postfix([NotNull] IEnumerator values, [NotNull] CharacterActionAttack __instance)
        {
            //PATCH: adds support for `IReactToAttackFinished` by calling `HandleReactToAttackFinished` on features

            var actingCharacter = __instance.ActingCharacter;
            var character = actingCharacter.RulesetCharacter;
            var found = false;
            GameLocationCharacter defender = null;
            var outcome = RuleDefinitions.RollOutcome.Neutral;
            CharacterActionParams actionParams = null;
            RulesetAttackMode mode = null;
            ActionModifier modifier = null;

            var features = character?.GetSubFeaturesByType<IReactToAttackFinished>();

            // ReSharper disable InconsistentNaming
            void AttackImpactStartHandler(
                    GameLocationCharacter _,
                    GameLocationCharacter _defender,
                    RuleDefinitions.RollOutcome _outcome,
                    CharacterActionParams _params,
                    RulesetAttackMode _mode,
                    ActionModifier _modifier)
                // ReSharper enable InconsistentNaming
            {
                found = true;
                defender = _defender;
                outcome = _outcome;
                actionParams = _params;
                mode = _mode;
                modifier = _modifier;
            }

            if (features != null && !features.Empty())
            {
                actingCharacter.AttackImpactStart += AttackImpactStartHandler;
            }

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            actingCharacter.AttackImpactStart -= AttackImpactStartHandler;

            if (!found || features == null || features.Empty())
            {
                yield break;
            }

            foreach (var feature in features)
            {
                yield return feature.HandleReactToAttackFinished(
                    actingCharacter, defender, outcome, actionParams, mode, modifier);
            }
        }
    }
}
