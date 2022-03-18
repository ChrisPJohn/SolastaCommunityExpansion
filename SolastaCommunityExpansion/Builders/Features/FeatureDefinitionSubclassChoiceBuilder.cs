﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Builders.Features
{
    public sealed class FeatureDefinitionSubclassChoiceBuilder
        : FeatureDefinitionBuilder<FeatureDefinitionSubclassChoice, FeatureDefinitionSubclassChoiceBuilder>
    {
        #region Constructors
        private FeatureDefinitionSubclassChoiceBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        private FeatureDefinitionSubclassChoiceBuilder(string name, Guid namespaceGuid)
            : base(name, namespaceGuid)
        {
        }

        private FeatureDefinitionSubclassChoiceBuilder(FeatureDefinitionSubclassChoice original, string name, string guid)
            : base(original, name, guid)
        {
        }

        private FeatureDefinitionSubclassChoiceBuilder(FeatureDefinitionSubclassChoice original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }
        #endregion

        public FeatureDefinitionSubclassChoiceBuilder SetFilterByDeity(bool requireDeity)
        {
            Definition.SetFilterByDeity(requireDeity);
            return this;
        }

        public FeatureDefinitionSubclassChoiceBuilder SetSubclassSuffix(string subclassSuffix)
        {
            Definition.SetSubclassSuffix(subclassSuffix);
            return this;
        }

        public FeatureDefinitionSubclassChoiceBuilder SetSubclasses(params CharacterSubclassDefinition[] subclasses)
        {
            return SetSubclasses(subclasses.AsEnumerable());
        }

        public FeatureDefinitionSubclassChoiceBuilder SetSubclasses(IEnumerable<CharacterSubclassDefinition> subclasses)
        {
            Definition.Subclasses.SetRange(subclasses.Select(sc => sc.Name));
            return this;
        }
    }
}
