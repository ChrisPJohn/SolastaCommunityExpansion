﻿using System;

namespace SolastaUnfinishedBusiness.Builders;

public abstract class
    MonsterAttackDefinitionBuilder<TDefinition, TBuilder> : DefinitionBuilder<TDefinition, TBuilder>
    where TDefinition : MonsterAttackDefinition
    where TBuilder : MonsterAttackDefinitionBuilder<TDefinition, TBuilder>

{
    public TBuilder SetActionType(ActionDefinitions.ActionType value)
    {
        Definition.actionType = value;
        return This();
    }

    public TBuilder SetToHitBonus(int bonus)
    {
        Definition.ToHitBonus = bonus;
        return This();
    }

    public TBuilder SetProximity(RuleDefinitions.AttackProximity proximity)
    {
        Definition.proximity = proximity;
        return This();
    }

    public TBuilder SetEffectDescription(EffectDescription effect)
    {
        Definition.EffectDescription = effect;
        return This();
    }

    #region Constructors

    protected MonsterAttackDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected MonsterAttackDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
    {
    }

    protected MonsterAttackDefinitionBuilder(TDefinition original, string name, Guid namespaceGuid) : base(original,
        name, namespaceGuid)
    {
    }

    protected MonsterAttackDefinitionBuilder(TDefinition original, string name, string definitionGuid) : base(
        original, name, definitionGuid)
    {
    }

    #endregion
}

public class MonsterAttackDefinitionBuilder : MonsterAttackDefinitionBuilder<MonsterAttackDefinition,
    MonsterAttackDefinitionBuilder>
{
    #region Constructors

    public MonsterAttackDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    public MonsterAttackDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
    {
    }

    public MonsterAttackDefinitionBuilder(MonsterAttackDefinition original, string name, Guid namespaceGuid) : base(
        original, name, namespaceGuid)
    {
    }

    public MonsterAttackDefinitionBuilder(MonsterAttackDefinition original, string name, string definitionGuid) :
        base(original, name, definitionGuid)
    {
    }

    #endregion
}
