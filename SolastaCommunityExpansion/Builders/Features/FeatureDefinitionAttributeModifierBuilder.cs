﻿using System;
using SolastaModApi.Extensions;
using static FeatureDefinitionAttributeModifier;

namespace SolastaCommunityExpansion.Builders.Features
{
    public sealed class FeatureDefinitionAttributeModifierBuilder : DefinitionBuilder<FeatureDefinitionAttributeModifier>
    {
        private FeatureDefinitionAttributeModifierBuilder(FeatureDefinitionAttributeModifier original, string name, string guid)
            : base(original, name, guid)
        {
        }

        private FeatureDefinitionAttributeModifierBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        private FeatureDefinitionAttributeModifierBuilder(string name, Guid namespaceGuid)
            : base(name, namespaceGuid)
        {
        }

        private FeatureDefinitionAttributeModifierBuilder(FeatureDefinitionAttributeModifier original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }

        public static FeatureDefinitionAttributeModifierBuilder Create(string name, Guid namespaceGuid)
        {
            return new FeatureDefinitionAttributeModifierBuilder(name, namespaceGuid);
        }

        public static FeatureDefinitionAttributeModifierBuilder Create(string name, string guid)
        {
            return new FeatureDefinitionAttributeModifierBuilder(name, guid);
        }

        public static FeatureDefinitionAttributeModifierBuilder Create(FeatureDefinitionAttributeModifier original, string name, Guid namespaceGuid)
        {
            return new FeatureDefinitionAttributeModifierBuilder(original, name, namespaceGuid);
        }

        public static FeatureDefinitionAttributeModifierBuilder Create(FeatureDefinitionAttributeModifier original, string name, string guid)
        {
            return new FeatureDefinitionAttributeModifierBuilder(original, name, guid);
        }

        public FeatureDefinitionAttributeModifierBuilder SetModifier(AttributeModifierOperation modifierType, string attribute, int amount)
        {
            Definition.SetModifierType2(modifierType);
            Definition.SetModifiedAttribute(attribute);
            Definition.SetModifierValue(amount);
            return this;
        }

        public FeatureDefinitionAttributeModifierBuilder SetModifierAbilityScore(string abilityScore)
        {
            Definition.SetModifierAbilityScore(abilityScore);
            return this;
        }
    }
}
