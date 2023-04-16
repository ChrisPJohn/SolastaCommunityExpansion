﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using TA.AI;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class AiLocationManagerPatcher
{
    [HarmonyPatch(typeof(AiLocationManager), nameof(AiLocationManager.BuildActivitiesMaps))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class BuildActivitiesMaps_Patch
    {
        [UsedImplicitly]
        public static void Postfix(AiLocationManager __instance)
        {
            foreach (var type in
                     Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.IsSubclassOf(typeof(ActivityBase))))
            {
                __instance.activitiesMap.Add(type.ToString().Split('.').Last(), type);

                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    var parameters = method.GetParameters();
                    if (method.ReturnType == typeof(ContextType))
                    {
                        __instance.activityContextsMap.Add(
                            type.ToString().Split('.').Last(),
                            (AiLocationDefinitions.GetContextTypeHandler)Delegate.CreateDelegate(
                                typeof(AiLocationDefinitions.GetContextTypeHandler), method));
                    }
                    else if (method.ReturnType == typeof(void) &&
                             parameters.Length == 2 &&
                             parameters[0].ParameterType.GetElementType() == typeof(ActionDefinitions.Id) &&
                             parameters[1].ParameterType.GetElementType() == typeof(ActionDefinitions.Id))
                    {
                        __instance.activityActionIdsMap.Add(
                            type.ToString().Split('.').Last(),
                            (AiLocationDefinitions.GetActionIdHandler)Delegate.CreateDelegate(
                                typeof(AiLocationDefinitions.GetActionIdHandler), method));
                    }
                    else if (method.ReturnType == typeof(bool) &&
                             parameters.Length == 2 && parameters[0].ParameterType.GetElementType() ==
                             typeof(GameLocationCharacter))
                    {
                        __instance.activityShouldBeSkippedMap.Add(
                            type.ToString().Split('.').Last(),
                            (AiLocationDefinitions.ShouldBeSkippedHandler)Delegate.CreateDelegate(
                                typeof(AiLocationDefinitions.ShouldBeSkippedHandler), method));
                    }
                    else if (method.ReturnType == typeof(bool) && parameters.Length == 0)
                    {
                        __instance.activityUsesMovementContextsMap.Add(
                            type.ToString().Split('.').Last(),
                            (AiLocationDefinitions.UsesMovementContextsHandler)Delegate.CreateDelegate(
                                typeof(AiLocationDefinitions.UsesMovementContextsHandler), method));
                    }
                }
            }
        }
    }
}
