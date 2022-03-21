﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Builders
{
    public abstract class FeatDefinitionBuilder<TDefinition, TBuilder> : DefinitionBuilder<TDefinition, TBuilder>
        where TDefinition : FeatDefinition
        where TBuilder : FeatDefinitionBuilder<TDefinition, TBuilder>
    {
        #region Constructors
        protected FeatDefinitionBuilder(TDefinition original) : base(original)
        {
        }

        protected FeatDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatDefinitionBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected FeatDefinitionBuilder(TDefinition original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected FeatDefinitionBuilder(TDefinition original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatDefinitionBuilder(TDefinition original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion

        public TBuilder SetFeatures(params FeatureDefinition[] features)
        {
            return SetFeatures(features.AsEnumerable());
        }

        public TBuilder SetFeatures(IEnumerable<FeatureDefinition> features)
        {
            Definition.Features.SetRange(features);
            return This();
        }

        public TBuilder AddFeatures(params FeatureDefinition[] features)
        {
            return AddFeatures(features.AsEnumerable());
        }

        public TBuilder AddFeatures(IEnumerable<FeatureDefinition> features)
        {
            Definition.Features.AddRange(features);
            return This();
        }

        public TBuilder SetAbilityScorePrerequisite(string abilityScore, int value)
        {
            Definition.SetMinimalAbilityScorePrerequisite(true);
            Definition.SetMinimalAbilityScoreName(abilityScore);
            Definition.SetMinimalAbilityScoreValue(value);
            return This();
        }

        public TBuilder SetMustCastSpellsPrerequisite()
        {
            Definition.SetMustCastSpellsPrerequisite(true);
            return This();
        }

        public TBuilder SetClassPrerequisite(params string[] classes)
        {
            return SetClassPrerequisite(classes.AsEnumerable());
        }

        public TBuilder SetClassPrerequisite(IEnumerable<string> classes)
        {
            Definition.CompatibleClassesPrerequisite.SetRange(classes);
            return This();
        }

        public TBuilder SetRacePrerequisite(params string[] races)
        {
            return SetRacePrerequisite(races.AsEnumerable());
        }

        public TBuilder SetRacePrerequisite(IEnumerable<string> races)
        {
            Definition.CompatibleRacesPrerequisite.SetRange(races);
            return This();
        }

        public TBuilder SetFeatPrerequisite(params string[] feats)
        {
            return SetFeatPrerequisite(feats.AsEnumerable());
        }

        public TBuilder SetFeatPrerequisite(IEnumerable<string> feats)
        {
            Definition.KnownFeatsPrerequisite.SetRange(feats);
            return This();
        }

        public TBuilder SetArmorProficiencyPrerequisite(ArmorCategoryDefinition category)
        {
            Definition.SetArmorProficiencyPrerequisite(true);
            Definition.SetArmorProficiencyCategory(category.Name);
            return This();
        }
    }

    public class FeatDefinitionBuilder : FeatDefinitionBuilder<FeatDefinition, FeatDefinitionBuilder>
    {
        #region Constructors
        protected FeatDefinitionBuilder(FeatDefinition original) : base(original)
        {
        }

        protected FeatDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatDefinitionBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected FeatDefinitionBuilder(FeatDefinition original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected FeatDefinitionBuilder(FeatDefinition original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatDefinitionBuilder(FeatDefinition original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion
    }
}
