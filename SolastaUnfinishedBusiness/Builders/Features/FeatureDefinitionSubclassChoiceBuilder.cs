﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.Infrastructure;

namespace SolastaUnfinishedBusiness.Builders.Features;

public class FeatureDefinitionSubclassChoiceBuilder
    : FeatureDefinitionBuilder<FeatureDefinitionSubclassChoice, FeatureDefinitionSubclassChoiceBuilder>
{
    public FeatureDefinitionSubclassChoiceBuilder SetFilterByDeity(bool requireDeity)
    {
        Definition.filterByDeity = requireDeity;
        return this;
    }

    public FeatureDefinitionSubclassChoiceBuilder SetSubclassSuffix(string subclassSuffix)
    {
        Definition.subclassSuffix = subclassSuffix;
        return this;
    }

    public FeatureDefinitionSubclassChoiceBuilder SetSubclasses(params CharacterSubclassDefinition[] subclasses)
    {
        return SetSubclasses(subclasses.AsEnumerable());
    }

    public FeatureDefinitionSubclassChoiceBuilder SetSubclasses(IEnumerable<CharacterSubclassDefinition> subclasses)
    {
        Definition.Subclasses.SetRange(subclasses.Select(sc => sc.Name));
        Definition.Subclasses.Sort();
        return this;
    }

    #region Constructors

    protected FeatureDefinitionSubclassChoiceBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionSubclassChoiceBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionSubclassChoiceBuilder(FeatureDefinitionSubclassChoice original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionSubclassChoiceBuilder(FeatureDefinitionSubclassChoice original, string name,
        string definitionGuid) : base(original, name, definitionGuid)
    {
    }

    #endregion
}
