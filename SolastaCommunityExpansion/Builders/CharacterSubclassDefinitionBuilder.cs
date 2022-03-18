﻿using System;
using System.Linq;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders
{
    public class CharacterSubclassDefinitionBuilder : DefinitionBuilder<CharacterSubclassDefinition, CharacterSubclassDefinitionBuilder>
    {
        #region Constructors
        protected CharacterSubclassDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected CharacterSubclassDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected CharacterSubclassDefinitionBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected CharacterSubclassDefinitionBuilder(CharacterSubclassDefinition original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected CharacterSubclassDefinitionBuilder(CharacterSubclassDefinition original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected CharacterSubclassDefinitionBuilder(CharacterSubclassDefinition original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }

        protected CharacterSubclassDefinitionBuilder(CharacterSubclassDefinition original) : base(original)
        {
        }
        #endregion

        public CharacterSubclassDefinitionBuilder AddPersonality(PersonalityFlagDefinition personalityType, int weight)
        {
            Definition.PersonalityFlagOccurences.Add(
              new PersonalityFlagOccurence(DatabaseHelper.CharacterSubclassDefinitions.MartialChampion.PersonalityFlagOccurences[0])
                .SetWeight(weight)
                .SetPersonalityFlag(personalityType.Name));
            return this;
        }

        public CharacterSubclassDefinitionBuilder AddFeatureAtLevel(FeatureDefinition feature, int level)
        {
            Definition.AddFeatureUnlocks(new FeatureUnlockByLevel(feature, level));
            return this;
        }

        public CharacterSubclassDefinitionBuilder AddFeaturesAtLevel(int level, params FeatureDefinition[] features)
        {
            Definition.AddFeatureUnlocks(features.Select(f => new FeatureUnlockByLevel(f, level)));
            return this;
        }
    }
}
