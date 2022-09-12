﻿using System;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.Builders.Features;

public class FeatureDefinitionOnMagicalAttackDamageEffectBuilder : FeatureDefinitionBuilder<
    FeatureDefinitionOnMagicalAttackDamageEffect, FeatureDefinitionOnMagicalAttackDamageEffectBuilder>
{
    public FeatureDefinitionOnMagicalAttackDamageEffectBuilder SetOnMagicalAttackDamageDelegates(
        OnMagicalAttackDamageDelegate before, OnMagicalAttackDamageDelegate after)
    {
        Definition.SetOnMagicalAttackDamageDelegates(before, after);
        return this;
    }

    #region Constructors

    protected FeatureDefinitionOnMagicalAttackDamageEffectBuilder(string name, Guid namespaceGuid) : base(name,
        namespaceGuid)
    {
    }

    protected FeatureDefinitionOnMagicalAttackDamageEffectBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionOnMagicalAttackDamageEffectBuilder(
        FeatureDefinitionOnMagicalAttackDamageEffect original, string name, Guid namespaceGuid) : base(original,
        name, namespaceGuid)
    {
    }

    protected FeatureDefinitionOnMagicalAttackDamageEffectBuilder(
        FeatureDefinitionOnMagicalAttackDamageEffect original, string name, string definitionGuid) : base(original,
        name, definitionGuid)
    {
    }

    #endregion
}
