﻿using System;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.Builders.Features;

public class FeatureDefinitionOnAttackHitEffectBuilder : FeatureDefinitionBuilder<FeatureDefinitionOnAttackHitEffect
    , FeatureDefinitionOnAttackHitEffectBuilder>
{
    public FeatureDefinitionOnAttackHitEffectBuilder SetOnAttackHitDelegates(OnAttackHitDelegate before,
        OnAttackHitDelegate after)
    {
        Definition.SetOnAttackHitDelegates(before, after);
        return this;
    }

    #region Constructors

    protected FeatureDefinitionOnAttackHitEffectBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionOnAttackHitEffectBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionOnAttackHitEffectBuilder(FeatureDefinitionOnAttackHitEffect original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionOnAttackHitEffectBuilder(FeatureDefinitionOnAttackHitEffect original, string name,
        string definitionGuid) : base(original, name, definitionGuid)
    {
    }

    #endregion
}
