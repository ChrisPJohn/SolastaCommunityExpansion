﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.Extensions;

namespace SolastaUnfinishedBusiness.CustomDefinitions;

public sealed class FeatureDefinitionIgnoreDynamicVisionImpairment : FeatureDefinition
{
    public float maxRange;
    public List<FeatureDefinition> requiredFeatures = new();
    public List<FeatureDefinition> forbiddenFeatures = new();

    public bool CanIgnoreDynamicVisionImpairment(RulesetCharacter character, float range)
    {
        return range <= maxRange
               && character.HasAllFeatures(requiredFeatures)
               && !character.HasAnyFeature(forbiddenFeatures);
    }
}
