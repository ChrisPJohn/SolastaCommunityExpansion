﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Builders.Features
{
    public sealed class FeatureDefinitionFeatureSetBuilder : DefinitionBuilder<FeatureDefinitionFeatureSet>
    {
        /*        private FeatureDefinitionFeatureSetBuilder(string name, string guid)
                    : base(name, guid)
                {
                }

                private FeatureDefinitionFeatureSetBuilder(string name, Guid namespaceGuid, Category category = Category.None)
                    : base(name, namespaceGuid, category)
                {
                }

                private FeatureDefinitionFeatureSetBuilder(FeatureDefinitionFeatureSet original, string name, string guid)
                    : base(original, name, guid)
                {
                }*/

        private FeatureDefinitionFeatureSetBuilder(FeatureDefinitionFeatureSet original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }

        public static FeatureDefinitionFeatureSetBuilder Create(FeatureDefinitionFeatureSet original, string name, Guid namespaceGuid)
        {
            return new FeatureDefinitionFeatureSetBuilder(original, name, namespaceGuid);
        }

        public FeatureDefinitionFeatureSetBuilder ClearFeatures()
        {
            Definition.FeatureSet.Clear();
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder AddFeature(FeatureDefinition featureDefinition)
        {
            Definition.FeatureSet.Add(featureDefinition);
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder SetFeatures(params FeatureDefinition[] featureDefinitions)
        {
            return SetFeatures(featureDefinitions.AsEnumerable());
        }

        public FeatureDefinitionFeatureSetBuilder SetFeatures(IEnumerable<FeatureDefinition> featureDefinitions)
        {
            Definition.FeatureSet.SetRange(featureDefinitions);
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder SetMode(FeatureDefinitionFeatureSet.FeatureSetMode mode)
        {
            Definition.SetMode(mode);
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder SetUniqueChoices(bool uniqueChoice)
        {
            Definition.SetUniqueChoices(uniqueChoice);
            return this;
        }
    }
}
