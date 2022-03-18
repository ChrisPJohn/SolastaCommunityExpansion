﻿using System;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionAttackModifierBuilder
        : FeatureDefinitionAffinityBuilder<FeatureDefinitionAttackModifier, FeatureDefinitionAttackModifierBuilder>
    {
        #region Constructors
        protected FeatureDefinitionAttackModifierBuilder(FeatureDefinitionAttackModifier original) : base(original)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(FeatureDefinitionAttackModifier original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(FeatureDefinitionAttackModifier original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatureDefinitionAttackModifierBuilder(FeatureDefinitionAttackModifier original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion

        public FeatureDefinitionAttackModifierBuilder SetAbilityScoreReplacement(RuleDefinitions.AbilityScoreReplacement replacement)
        {
            Definition.SetAbilityScoreReplacement(replacement);

            return This();
        }

        public FeatureDefinitionAttackModifierBuilder SetAdditionalAttackTag(string tag)
        {
            Definition.SetAdditionalAttackTag(tag);

            return This();
        }
    }
}
