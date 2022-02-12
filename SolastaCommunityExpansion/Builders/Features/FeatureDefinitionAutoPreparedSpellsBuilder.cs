﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;
using static FeatureDefinitionAutoPreparedSpells;

namespace SolastaCommunityExpansion.Builders.Features
{
    public sealed class FeatureDefinitionAutoPreparedSpellsBuilder : DefinitionBuilder<FeatureDefinitionAutoPreparedSpells>
    {
        /*
        private FeatureDefinitionAutoPreparedSpellsBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        private FeatureDefinitionAutoPreparedSpellsBuilder(FeatureDefinitionAutoPreparedSpells original, string name, string guid)
            : base(original, name, guid)
        {
        }

        private FeatureDefinitionAutoPreparedSpellsBuilder(FeatureDefinitionAutoPreparedSpells original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }
        */

        private FeatureDefinitionAutoPreparedSpellsBuilder(string name, Guid namespaceGuid)
            : base(name, namespaceGuid)
        {
        }

        // Add other standard Create methods and constructors as required.

        public static FeatureDefinitionAutoPreparedSpellsBuilder Create(string name, Guid namespaceGuid)
        {
            return new FeatureDefinitionAutoPreparedSpellsBuilder(name, namespaceGuid);
        }

        public FeatureDefinitionAutoPreparedSpellsBuilder SetPreparedSpellGroups(params AutoPreparedSpellsGroup[] autospelllists)
        {
            return SetPreparedSpellGroups(autospelllists.AsEnumerable());
        }

        public FeatureDefinitionAutoPreparedSpellsBuilder SetPreparedSpellGroups(IEnumerable<AutoPreparedSpellsGroup> autospelllists)
        {
            Definition.AutoPreparedSpellsGroups.SetRange(autospelllists);
            return this;
        }

        public FeatureDefinitionAutoPreparedSpellsBuilder SetCharacterClass(CharacterClassDefinition castingClass)
        {
            Definition.SetSpellcastingClass(castingClass);
            return this;
        }

        public FeatureDefinitionAutoPreparedSpellsBuilder SetAffinityRace(CharacterRaceDefinition castingRace)
        {
            Definition.SetAffinityRace(castingRace);
            return this;
        }

        /**
         * This tag is used to create a tooltip:
         * this.autoPreparedTitle.Text = string.Format("Screen/&{0}SpellTitle", autoPreparedTag);
	     * this.autoPreparedTooltip.Content = string.Format("Screen/&{0}SpellDescription", autoPreparedTag);
         */
        public FeatureDefinitionAutoPreparedSpellsBuilder SetAutoTag(string tag)
        {
            Definition.SetAutopreparedTag(tag);
            return this;
        }

        public FeatureDefinitionAutoPreparedSpellsBuilder SetSpellcastingClass(CharacterClassDefinition characterClass)
        {
            Definition.SetSpellcastingClass(characterClass);
            return this;
        }
    }

    public static class AutoPreparedSpellsGroupBuilder
    {
        public static AutoPreparedSpellsGroup Build(int classLevel, params SpellDefinition[] spellnames)
        {
            return Build(classLevel, spellnames.AsEnumerable());
        }

        public static AutoPreparedSpellsGroup Build(int classLevel, IEnumerable<SpellDefinition> spellnames)
        {
            return new AutoPreparedSpellsGroup
            {
                ClassLevel = classLevel,
                SpellsList = spellnames.ToList()
            };
        }
    }
}
