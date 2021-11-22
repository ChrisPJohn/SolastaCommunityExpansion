﻿using SolastaModApi.Extensions;
using System.Collections.Generic;
using static SolastaCommunityExpansion.Level20.Features.RangerVanishActionBuilder;
using static SolastaCommunityExpansion.Level20.Features.RangerFeralSensesBuilder;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionCastSpells;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;
//using static SolastaCommunityExpansion.Models.Features.FeatureSetRangerFoeSlayerBuilder;

namespace SolastaCommunityExpansion.Level20.Classes
{
    internal static class RangerBuilder
    {
        internal static void Load()
        {
            Ranger.FeatureUnlocks.AddRange(new List<FeatureUnlockByLevel> {
                new FeatureUnlockByLevel(AdditionalDamageRangerFavoredEnemyChoice, 14),
                new FeatureUnlockByLevel(RangerVanishAction, 14),
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 16),
                new FeatureUnlockByLevel(RangerFeralSenses, 18),
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 19),
                //new FeatureUnlockByLevel(FeatureSetRangerFoeSlayer, 20)
            });

            CastSpellRanger.SetSpellCastingLevel(5);

            CastSpellRanger.SlotsPerLevels.Clear();
            CastSpellRanger.SlotsPerLevels.AddRange(SpellsHelper.HalfCastingSlots);

            CastSpellRanger.ReplacedSpells.Clear();
            CastSpellRanger.ReplacedSpells.AddRange(SpellsHelper.HalfCasterReplacedSpells);
        }
    }
}
