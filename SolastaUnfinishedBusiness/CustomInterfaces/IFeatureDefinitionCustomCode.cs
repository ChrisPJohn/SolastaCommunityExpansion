﻿using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IFeatureDefinitionCustomCode
{
    // Use this to add the feature to the character
    [UsedImplicitly]
    public void ApplyFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag);

    // Use this to remove the feature from the character. In particular this is used to allow level down functionality
    [UsedImplicitly]
    public void RemoveFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag);
}
