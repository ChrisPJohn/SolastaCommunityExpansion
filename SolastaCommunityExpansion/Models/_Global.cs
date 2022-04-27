﻿using System.Collections.Generic;
using System.Linq;

namespace SolastaCommunityExpansion.Models
{
    // keep public for sidecars
    public static class Global
    {
        // holds the active player character when in battle
        public static GameLocationCharacter ActivePlayerCharacter { get; set; }

        // holds the current action from any character on the map
        public static CharacterAction CurrentAction { get; set; }

        // holds a collection of conditions that should display on char panel even if set to silent
        public static HashSet<ConditionDefinition> CharacterLabelEnabledConditions { get; } = new();

        // true if in a multiplayer game
        public static bool IsMultiplayer => ServiceRepository.GetService<INetworkingService>().IsMultiplayerGame;

        // true if not in game
        public static bool IsOffGame => Gui.Game == null;

        // level up hero
        public static RulesetCharacterHero ActiveLevelUpHero => ServiceRepository.GetService<ICharacterBuildingService>()?.CurrentLocalHeroCharacter;

        public static bool ActiveLevelUpHeroHasCantrip(SpellDefinition spellDefinition)
        {
            var hero = ActiveLevelUpHero;

            if (hero == null)
            {
                return true;
            }

            return hero.SpellRepertoires.Any(x => x.KnownCantrips.Contains(spellDefinition));
        }
    }
}
