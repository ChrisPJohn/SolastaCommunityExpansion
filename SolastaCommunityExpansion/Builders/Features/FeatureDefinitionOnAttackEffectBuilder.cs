﻿using System;
using SolastaCommunityExpansion.CustomDefinitions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionOnAttackEffectBuilder : FeatureDefinitionBuilder<FeatureDefinitionOnAttackEffect, FeatureDefinitionOnAttackEffectBuilder>
    {
        #region Constructors
        protected FeatureDefinitionOnAttackEffectBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatureDefinitionOnAttackEffectBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatureDefinitionOnAttackEffectBuilder(FeatureDefinitionOnAttackEffect original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatureDefinitionOnAttackEffectBuilder(FeatureDefinitionOnAttackEffect original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion

        public FeatureDefinitionOnAttackEffectBuilder SetOnAttackDelegate(OnAttackDelegate del)
        {
            Definition.SetOnAttackDelegate(del);
            return this;
        }
    }
}
