﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaCommunityExpansion.Models;
using SolastaModApi.Infrastructure;
using UnityEngine.UI;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.PowersBundle
{
    [HarmonyPatch(typeof(AfterRestActionItem), "OnExecuteCb")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class AfterRestActionItem_OnExecuteCb
    {
        internal static bool Prefix(
            AfterRestActionItem __instance,
            bool ___executing)
        {
            if (!Main.Settings.EnablePowersBundlePatch)
            {
                return true;
            }

            if (___executing)
            {
                return true;
            }

            var activity = __instance.RestActivityDefinition;

            if (activity.Functor == PowerBundleContext.UseCustomRestPowerFunctorName && activity.StringParameter != null)
            {
                var masterPower = PowerBundleContext.GetPower(activity.StringParameter);

                if (masterPower)
                {
                    var masterSpell = PowerBundleContext.GetSpell(masterPower);
                    var repertoire = new RulesetSpellRepertoire();
                    var subspellSelectionModalScreen = Gui.GuiService.GetScreen<SubspellSelectionModal>();
                    var handler = new SpellsByLevelBox.SpellCastEngagedHandler(
                        (spellRepertoire, spell, slotLevel) => PowerEngagedHandler(__instance, spell));

                    repertoire.KnownSpells.AddRange(masterSpell.SubspellsList);

                    subspellSelectionModalScreen.Bind(masterSpell, __instance.Hero, repertoire, handler, 0, __instance.RectTransform);
                    subspellSelectionModalScreen.Show();

                    return false;
                }
            }

            return true;
        }

        private static void PowerEngagedHandler(AfterRestActionItem item, SpellDefinition spell)
        {
            item.GetField<Button>("button").interactable = false;

            var power = PowerBundleContext.GetPower(spell).Name;

            ServiceRepository.GetService<IGameRestingService>().ExecuteAsync(ExecuteAsync(item, power), power);
        }

        private static IEnumerator ExecuteAsync(AfterRestActionItem item, string powerName)
        {
            item.SetField("executing", true);

            var parameters = new FunctorParametersDescription { RestingHero = item.Hero, StringParameter = powerName };
            var gameRestingService = ServiceRepository.GetService<IGameRestingService>();

            yield return ServiceRepository.GetService<IFunctorService>()
                .ExecuteFunctorAsync(item.RestActivityDefinition.Functor, parameters, gameRestingService);

            yield return null;

            var gameLocationActionService = ServiceRepository.GetService<IGameLocationActionService>();
            var gameLocationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();

            if (gameLocationActionService != null && gameLocationCharacterService != null)
            {
                bool needsToWait;

                do
                {
                    needsToWait = false;
                    foreach (var partyCharacter in gameLocationCharacterService.PartyCharacters)
                    {
                        if (gameLocationActionService.IsCharacterActing(partyCharacter))
                        {
                            needsToWait = true;
                            break;
                        }
                    }

                    if (needsToWait)
                    {
                        yield return null;
                    }
                } while (needsToWait);
            }

            item.AfterRestActionTaken?.Invoke();
            item.SetField("executing", false);

            var button = item.GetField<Button>("button");

            if (button != null)
            {
                button.interactable = true;
            }
        }
    }
}
