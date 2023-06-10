﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CursorLocationBattleActionExecutingPatcher
{
    [HarmonyPatch(typeof(CursorLocationBattleActionExecuting),
        nameof(CursorLocationBattleActionExecuting.ActionExecuted))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ActionExecuted_Patch
    {
        [NotNull]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: fixes target selection panel remaining open after casting some targeted spells through invocations
            //was caused by CastInvocation action (and custom ones) being still marked as available by CustomActionIdContext and this cursor re-opened previous cursor
            //fixed by marking all invocation actions as unavailable for this method, so it won't try to reopen

            var method = typeof(GameLocationCharacter).GetMethod(nameof(GameLocationCharacter.GetActionStatus));
            var custom = new Func<
                GameLocationCharacter, // instance,
                ActionDefinitions.Id, // actionId,
                ActionDefinitions.ActionScope, // scope,
                ActionDefinitions.ActionStatus, // actionTypeStatus,
                RulesetAttackMode, // optionalAttackMode,
                bool, // ignoreMovePoints,
                bool, // allowUsingDelegatedPowersAsPowers
                ActionDefinitions.ActionStatus // result
            >(GetActionStatus).Method;

            return instructions.ReplaceCall(method, 1,
                "CursorLocationBattleActionExecuting.ActionExecute",
                new CodeInstruction(OpCodes.Call, custom));
        }

        private static ActionDefinitions.ActionStatus GetActionStatus(
            GameLocationCharacter instance,
            ActionDefinitions.Id actionId,
            ActionDefinitions.ActionScope scope,
            ActionDefinitions.ActionStatus actionTypeStatus,
            RulesetAttackMode optionalAttackMode,
            bool ignoreMovePoints,
            bool allowUsingDelegatedPowersAsPowers)
        {
            if (CustomActionIdContext.IsInvocationActionId(actionId))
            {
                return ActionDefinitions.ActionStatus.Unavailable;
            }

            return instance.GetActionStatus(actionId, scope, actionTypeStatus, optionalAttackMode, ignoreMovePoints,
                allowUsingDelegatedPowersAsPowers);
        }
    }
}
