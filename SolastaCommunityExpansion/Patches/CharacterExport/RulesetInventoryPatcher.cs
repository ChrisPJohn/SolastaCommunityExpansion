﻿using HarmonyLib;

namespace SolastaCommunityExpansion.Patches
{
    // When inspecting a character out of game, there is no RulesetEntityService registered.
    // We need to temporarily add the dummy RulesetEntityService and then remove it.
    // We don't want to register it or remove it when the real one has been registered.
    [HarmonyPatch(typeof(RulesetInventory), "SerializeElements")]
    internal static class RulesetInventory_SerializeElements
    {
        static readonly object Locker = new object();

        internal static void Prefix()
        {
            if (Main.Settings.EnableCharacterExport)
            {
                lock (Locker)
                {
                    var registeredService = ServiceRepository.GetService<IRulesetEntityService>();

                    if (registeredService == null)
                    {
                        Main.Log("Adding DummyRulesetEntityService");
                        ServiceRepository.AddService(Models.CharacterExportContext.DummyRulesetEntityService.Instance);
                    }
                }
            }
        }

        internal static void Postfix()
        {
            if (Main.Settings.EnableCharacterExport)
            {
                lock (Locker)
                {
                    if (ServiceRepository.GetService<IRulesetEntityService>() is Models.CharacterExportContext.DummyRulesetEntityService)
                    {
                        Main.Log("Removing DummyRulesetEntityService");
                        ServiceRepository.RemoveService<IRulesetEntityService>();
                    }
                }
            }
        }
    }
}
