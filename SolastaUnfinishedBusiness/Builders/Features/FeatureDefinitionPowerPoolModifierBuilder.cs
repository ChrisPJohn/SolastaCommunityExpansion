﻿using System;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.CustomDefinitions;

namespace SolastaUnfinishedBusiness.Builders.Features;

/**
     * Note this is based on FeatureDefinitionPower so that you can take advantage of power usage calculations
     * like proficiency or ability score usage. However in order to do that the game needs to add a power to
     * the hero and only one power for a given name+guid is added. Which means if you want to add a +1 modifier
     * at 4 different character levels you need to create 4 different FeatureDefinitionPowerPoolModifier.
     */
public class FeatureDefinitionPowerPoolModifierBuilder : FeatureDefinitionPowerBuilder<
    FeatureDefinitionPowerPoolModifier, FeatureDefinitionPowerPoolModifierBuilder>
{
    protected override void Initialise()
    {
        base.Initialise();

        if (!IsNew)
        {
            return;
        }

        // This is just an activation time that should not be shown in the UI.
        Definition.activationTime = RuleDefinitions.ActivationTime.Permanent;

        // Math for usage gets weird if this isn't 1.
        Definition.costPerUse = 1;
    }

    internal override void Validate()
    {
        base.Validate();

        Preconditions.IsNotNull(Definition.PoolPower, $"{GetType().Name}[{Definition.Name}].PoolPower is null.");
        Preconditions.AreEqual(Definition.CostPerUse, 1,
            $"{GetType().Name}[{Definition.Name}].CostPerUse must be set to 1.");
    }

    public FeatureDefinitionPowerPoolModifierBuilder Configure(
        int powerPoolModifier, RuleDefinitions.UsesDetermination usesDetermination,
        string usesAbilityScoreName, FeatureDefinitionPower poolPower)
    {
        Preconditions.IsNotNull(poolPower, $"{GetType().Name}[{Definition.Name}] poolPower is null.");

        Definition.fixedUsesPerRecharge = powerPoolModifier;
        Definition.usesDetermination = usesDetermination;
        Definition.usesAbilityScoreName = usesAbilityScoreName;
        Definition.overriddenPower = Definition;

        Definition.PoolPower = poolPower;

        return This();
    }

    #region Constructors

    protected FeatureDefinitionPowerPoolModifierBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionPowerPoolModifierBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionPowerPoolModifierBuilder(FeatureDefinitionPowerPoolModifier original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionPowerPoolModifierBuilder(FeatureDefinitionPowerPoolModifier original, string name,
        string definitionGuid) : base(original, name, definitionGuid)
    {
    }

    #endregion
}
