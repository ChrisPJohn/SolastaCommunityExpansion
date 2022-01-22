﻿using SolastaModApi.Extensions;
using System.Collections.Generic;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionCastSpells;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;

namespace SolastaCommunityExpansion.Level20.Classes
{
    internal static class DruidBuilder
    {
        internal static void Load()
        {
            // add missing progression
            Druid.FeatureUnlocks.AddRange(new List<FeatureUnlockByLevel> {
                // TODO 18: BEAST SPELLS
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 19),
                // TODO 20: ARCHDRUID
            });

            CastSpellDruid.SetSpellCastingLevel(9);

            CastSpellDruid.SlotsPerLevels.Clear();
            CastSpellDruid.SlotsPerLevels.AddRange(SpellsHelper.FullCastingSlots);

            CastSpellDruid.ReplacedSpells.Clear();
            CastSpellDruid.ReplacedSpells.AddRange(SpellsHelper.EmptyReplacedSpells);
        }
    }
}