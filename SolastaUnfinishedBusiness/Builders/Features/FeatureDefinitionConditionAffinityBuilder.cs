﻿using System;

namespace SolastaUnfinishedBusiness.Builders.Features;

public abstract class
    FeatureDefinitionConditionAffinityBuilder<TDefinition, TBuilder> : FeatureDefinitionAffinityBuilder<TDefinition,
        TBuilder>
    where TDefinition : FeatureDefinitionConditionAffinity
    where TBuilder : FeatureDefinitionConditionAffinityBuilder<TDefinition, TBuilder>
{
    public TBuilder SetConditionAffinityType(RuleDefinitions.ConditionAffinityType value)
    {
        Definition.conditionAffinityType = value;
        return This();
    }

    public TBuilder SetConditionType(ConditionDefinition value)
    {
        Definition.conditionType = value.Name;
        return This();
    }

    #region Constructors

    protected FeatureDefinitionConditionAffinityBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(TDefinition original, string name, Guid namespaceGuid) :
        base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(TDefinition original, string name, string definitionGuid) :
        base(original, name, definitionGuid)
    {
    }

    #endregion
}

public class FeatureDefinitionConditionAffinityBuilder
    : FeatureDefinitionConditionAffinityBuilder<FeatureDefinitionConditionAffinity,
        FeatureDefinitionConditionAffinityBuilder>
{
    #region Constructors

    protected FeatureDefinitionConditionAffinityBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(FeatureDefinitionConditionAffinity original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionConditionAffinityBuilder(FeatureDefinitionConditionAffinity original, string name,
        string definitionGuid) : base(original, name, definitionGuid)
    {
    }

    #endregion
}
