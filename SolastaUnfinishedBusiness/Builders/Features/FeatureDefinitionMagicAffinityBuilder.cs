﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SolastaUnfinishedBusiness.Builders.Features;

public class FeatureDefinitionMagicAffinityBuilder : FeatureDefinitionAffinityBuilder<FeatureDefinitionMagicAffinity
    , FeatureDefinitionMagicAffinityBuilder>
{
    public FeatureDefinitionMagicAffinityBuilder SetConcentrationModifiers(
        RuleDefinitions.ConcentrationAffinity concentrationAffinity,
        int threshold = -1)
    {
        Definition.concentrationAffinity = concentrationAffinity;

        if (threshold > 0)
        {
            Definition.overConcentrationThreshold = threshold;
        }

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetHandsFullCastingModifiers(
        bool weapon,
        bool weaponOrShield,
        bool weaponAsFocus)
    {
        Definition.somaticWithWeaponOrShield = weaponOrShield;
        Definition.somaticWithWeapon = weapon;
        Definition.canUseProficientWeaponAsFocus = weaponAsFocus;

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetCastingModifiers(
        int attackModifier,
        RuleDefinitions.SpellParamsModifierType attackModifierType,
        int dcModifier,
        RuleDefinitions.SpellParamsModifierType dcModifierType,
        bool noProximityPenalty,
        bool cantripRetribution,
        bool halfDamageCantrips)
    {
        Definition.spellAttackModifierType = attackModifierType;
        Definition.spellAttackModifier = attackModifier;
        Definition.rangeSpellNoProximityPenalty = noProximityPenalty;
        Definition.saveDCModifierType = dcModifierType;
        Definition.saveDCModifier = dcModifier;
        Definition.cantripRetribution = cantripRetribution;
        Definition.forceHalfDamageOnCantrips = halfDamageCantrips;

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetWarList(
        int levelBonus,
        params SpellDefinition[] spells)
    {
        return SetWarList(levelBonus, spells.AsEnumerable());
    }

    public FeatureDefinitionMagicAffinityBuilder SetWarList(
        int levelBonus,
        IEnumerable<SpellDefinition> spells)
    {
        Definition.usesWarList = true;
        Definition.warListSlotBonus = levelBonus;
        Definition.WarListSpells.AddRange(spells.Select(s => s.Name));
        Definition.WarListSpells.Sort();

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetSpellLearnAndPrepModifiers(
        float scribeDurationMultiplier,
        float scribeCostMultiplier,
        int additionalScribedSpells,
        RuleDefinitions.AdvantageType scribeAdvantage,
        RuleDefinitions.PreparedSpellsModifier preparedModifier)
    {
        Definition.scribeCostMultiplier = scribeCostMultiplier;
        Definition.scribeDurationMultiplier = scribeDurationMultiplier;
        Definition.additionalScribedSpells = additionalScribedSpells;
        Definition.scribeAdvantageType = scribeAdvantage;
        Definition.preparedSpellModifier = preparedModifier;

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetRitualCasting(RuleDefinitions.RitualCasting ritualCasting)
    {
        Definition.ritualCasting = ritualCasting;

        return this;
    }

    public FeatureDefinitionMagicAffinityBuilder SetExtendedSpellList(SpellListDefinition spellListDefinition)
    {
        Definition.extendedSpellList = spellListDefinition;

        return this;
    }

    #region Constructors

    protected FeatureDefinitionMagicAffinityBuilder(string name, string guid)
        : base(name, guid)
    {
    }

    protected FeatureDefinitionMagicAffinityBuilder(string name, Guid namespaceGuid)
        : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionMagicAffinityBuilder(FeatureDefinitionMagicAffinity original, string name,
        string guid)
        : base(original, name, guid)
    {
    }

    protected FeatureDefinitionMagicAffinityBuilder(FeatureDefinitionMagicAffinity original, string name,
        Guid namespaceGuid)
        : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
