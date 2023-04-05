﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;

namespace SolastaUnfinishedBusiness.Api.Helpers;

internal static class EffectHelpers
{
    /**DC and magic attack bonus will be calculated based on the stats of the user, not from device itself*/
    public const int BasedOnUser = -1;

    /**DC and magic attack bonus will be calculated based on the stats of character who summoned item, not from device itself*/
    public const int BasedOnItemSummoner = -2;

    // use this to start a custom visual effect during combat
    internal static void StartVisualEffect(
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        IMagicEffect magicEffect,
        EffectType effectType = EffectType.Impact)
    {
        var prefab = effectType switch
        {
            EffectType.Caster => magicEffect.EffectDescription.EffectParticleParameters.CasterParticle,
            EffectType.Effect => magicEffect.EffectDescription.EffectParticleParameters.EffectParticle,
            EffectType.Impact => magicEffect.EffectDescription.EffectParticleParameters.ImpactParticle,
            _ => throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null)
        };

        var sentParameters = new ParticleSentParameters(attacker, defender, magicEffect.Name);

        WorldLocationPoolManager
            .GetElement(prefab, true)
            .GetComponent<ParticleSetup>()
            .Setup(sentParameters);
    }

    internal static int CalculateSaveDc(RulesetCharacter character, EffectDescription effectDescription,
        string className, int def = 10)
    {
        switch (effectDescription.DifficultyClassComputation)
        {
            case RuleDefinitions.EffectDifficultyClassComputation.SpellCastingFeature:
            {
                var rulesetSpellRepertoire = character.GetClassSpellRepertoire(className);

                if (rulesetSpellRepertoire != null)
                {
                    return rulesetSpellRepertoire.SaveDC;
                }

                break;
            }
            case RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency:
                var attributeValue = character.TryGetAttributeValue(effectDescription.SavingThrowDifficultyAbility);
                var proficiencyBonus = character.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);

                return RuleDefinitions.ComputeAbilityScoreBasedDC(attributeValue, proficiencyBonus);

            case RuleDefinitions.EffectDifficultyClassComputation.FixedValue:
                return effectDescription.FixedSavingThrowDifficultyClass;
            //TODO: implement missing computation methods (like Ki and Breath Weapon)
            case RuleDefinitions.EffectDifficultyClassComputation.Ki:
                break;
            case RuleDefinitions.EffectDifficultyClassComputation.BreathWeapon:
                break;
            case RuleDefinitions.EffectDifficultyClassComputation.CustomAbilityModifierAndProficiency:
                break;
            default:
                throw new ArgumentException("effectDescription.DifficultyClassComputation");
        }

        return def;
    }

    internal static RulesetCharacter GetSummoner(RulesetCharacter summon)
    {
        return summon.TryGetConditionOfCategoryAndType(AttributeDefinitions.TagConjure,
            RuleDefinitions.ConditionConjuredCreature, out var activeCondition)
            ? GetCharacterByGuid(activeCondition.SourceGuid)
            : null;
    }

    internal static RulesetCharacter GetCharacterByGuid(ulong guid)
    {
        if (guid == 0)
        {
            return null;
        }

        if (!RulesetEntity.TryGetEntity<RulesetEntity>(guid, out var entity))
        {
            return null;
        }

        return entity as RulesetCharacter;
    }

    internal static RulesetEffect GetEffectByGuid(ulong guid)
    {
        if (guid == 0)
        {
            return null;
        }

        return !RulesetEntity.TryGetEntity<RulesetEffect>(guid, out var entity) ? null : entity;
    }

    internal static RulesetItem GetItemByGuid(ulong guid)
    {
        if (guid == 0)
        {
            return null;
        }

        return !RulesetEntity.TryGetEntity<RulesetItem>(guid, out var item) ? null : item;
    }

    internal static List<RulesetCharacter> GetSummonedCreatures(RulesetEffect effect)
    {
        var summons = new List<RulesetCharacter>();

        if (effect == null)
        {
            return summons;
        }

        foreach (var conditionGuid in effect.trackedConditionGuids)
        {
            if (!RulesetEntity.TryGetEntity<RulesetCondition>(conditionGuid, out var condition)
                || condition.Name != RuleDefinitions.ConditionConjuredCreature)
            {
                continue;
            }

            if (RulesetEntity.TryGetEntity<RulesetCharacter>(condition.TargetGuid, out var creature)
                && creature != null)
            {
                summons.TryAdd(creature);
            }
        }

        return summons;
    }

    internal static RulesetCharacter GetCharacterByEffectGuid(ulong guid)
    {
        return GetEffectByGuid(guid) switch
        {
            RulesetEffectSpell spell => spell.Caster,
            RulesetEffectPower power => power.User,
            _ => null
        };
    }

    internal static List<RulesetEffect> GetAllEffectsBySourceGuid(ulong guid)
    {
        return ServiceRepository.GetService<IRulesetEntityService>().RulesetEntities.Values
            .OfType<RulesetEffect>()
            .Where(e => e.SourceGuid == guid)
            .ToList();
    }

    internal static List<RulesetCondition> GetAllConditionsBySourceGuid(ulong guid)
    {
        return ServiceRepository.GetService<IRulesetEntityService>().RulesetEntities.Values
            .OfType<RulesetCondition>()
            .Where(e => e.SourceGuid == guid)
            .ToList();
    }

    internal static void DoTerminate(this RulesetEffect effect, RulesetCharacter source = null)
    {
        source ??= GetCharacterByGuid(effect.SourceGuid);

        if (source != null)
        {
            switch (effect)
            {
                case RulesetEffectPower power:
                    source.TerminatePower(power);
                    return;
                case RulesetEffectSpell spell:
                    source.TerminateSpell(spell);
                    return;
            }
        }

        effect.Terminate(true);
    }

    internal static (RulesetCharacter, BaseDefinition) GetCharacterAndSourceDefinitionByEffectGuid(ulong guid)
    {
        if (guid == 0)
        {
            return (null, null);
        }

        if (!RulesetEntity.TryGetEntity<RulesetEffect>(guid, out var effect))
        {
            return (null, null);
        }

        return effect switch
        {
            RulesetEffectSpell spell => (spell.Caster, spell.SourceDefinition),
            RulesetEffectPower power => (power.User, power.PowerDefinition),
            _ => (null, null)
        };
    }

    internal static void SetGuid(this RulesetEffect effect, ulong guid)
    {
        switch (effect)
        {
            case RulesetEffectPower power:
                power.userId = guid;
                break;
            case RulesetEffectSpell spell:
                spell.casterId = guid;
                break;
        }
    }

    internal enum EffectType
    {
        Caster = 0,
        Effect = 1,
        Impact = 2
    }
}
