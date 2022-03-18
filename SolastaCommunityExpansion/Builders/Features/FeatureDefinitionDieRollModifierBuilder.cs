﻿using System;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionDieRollModifierBuilder
        : FeatureDefinitionAffinityBuilder<FeatureDefinitionDieRollModifier, FeatureDefinitionDieRollModifierBuilder>
    {
        #region Constructors
        protected FeatureDefinitionDieRollModifierBuilder(FeatureDefinitionDieRollModifier original) : base(original)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(FeatureDefinitionDieRollModifier original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(FeatureDefinitionDieRollModifier original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatureDefinitionDieRollModifierBuilder(FeatureDefinitionDieRollModifier original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion

        public FeatureDefinitionDieRollModifierBuilder SetModifiers(
            RuleDefinitions.RollContext context, int rerollCount, int minRerollValue, string consoleLocalizationKey)
        {
            Definition.SetValidityContext(context);
            Definition.SetRerollLocalizationKey(consoleLocalizationKey);
            Definition.SetRerollCount(rerollCount);
            Definition.SetMinRerollValue(minRerollValue);
            Definition.SetMinRollValue(minRerollValue);
            return this;
        }
    }
}
