﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Models;

public static class GlobalUniqueEffects
{
    public enum Group { Familiar, Tinkerer }

    private static readonly Dictionary<Group, (List<FeatureDefinitionPower>, List<SpellDefinition>)>
        Groups = new();

    private static (List<FeatureDefinitionPower>, List<SpellDefinition>) GetGroup(Group group)
    {
        if (Groups.ContainsKey(group))
        {
            return Groups[group];
        }

        var newGroup = new ValueTuple<List<FeatureDefinitionPower>, List<SpellDefinition>>
        {
            Item1 = new List<FeatureDefinitionPower>(), Item2 = new List<SpellDefinition>()
        };

        Groups.Add(group, newGroup);

        return newGroup;
    }

    /**Returns copies*/
    private static (HashSet<FeatureDefinitionPower>, HashSet<SpellDefinition>) GetSameGroupItems(
        FeatureDefinitionPower power)
    {
        var powers = new HashSet<FeatureDefinitionPower>();
        var spells = new HashSet<SpellDefinition>();

        foreach (var group in Groups.Where(e => e.Value.Item1.Contains(power)))
        {
            foreach (var p in group.Value.Item1)
            {
                powers.Add(p);
            }

            foreach (var s in group.Value.Item2)
            {
                spells.Add(s);
            }
        }

        return (powers, spells);
    }

    private static (HashSet<FeatureDefinitionPower>, HashSet<SpellDefinition>) GetSameGroupItems(
        SpellDefinition spell)
    {
        var powers = new HashSet<FeatureDefinitionPower>();
        var spells = new HashSet<SpellDefinition>();
        foreach (var group in Groups.Where(e => e.Value.Item2.Contains(spell)))
        {
            foreach (var p in group.Value.Item1)
            {
                powers.Add(p);
            }

            foreach (var s in group.Value.Item2)
            {
                spells.Add(s);
            }
        }

        return (powers, spells);
    }

    public static void AddToGroup(Group group, [NotNull] params FeatureDefinitionPower[] powers)
    {
        GetGroup(group).Item1.AddRange(powers);
    }

    public static void AddToGroup(Group group, [NotNull] params SpellDefinition[] spells)
    {
        GetGroup(group).Item2.AddRange(spells);
    }

    /**
     * Used in the patch to terminate all matching powers and spells of same group
     */
    internal static void TerminateMatchingUniquePower(RulesetCharacter character, FeatureDefinitionPower power)
    {
        var (powers, spells) = GetSameGroupItems(power);

        powers.Add(power);
        TerminatePowers(character, power, powers);
        TerminateSpells(character, null, spells);
    }

    /**
     * Used in the patch to terminate all matching powers and spells of same group
     */
    internal static void TerminateMatchingUniqueSpell(RulesetCharacter character, SpellDefinition spell)
    {
        var (powers, spells) = GetSameGroupItems(spell);

        spells.Add(spell);
        TerminatePowers(character, null, powers);
        TerminateSpells(character, spell, spells);
    }

    private static void TerminatePowers(RulesetCharacter character, FeatureDefinitionPower exclude,
        IEnumerable<FeatureDefinitionPower> powers)
    {
        var allSubPowers = new HashSet<FeatureDefinitionPower>();

        foreach (var power in powers)
        {
            allSubPowers.Add(power);

            var bundles = PowersBundleContext.GetMasterPowersBySubPower(power);

            foreach (var subPower in bundles.Select(PowersBundleContext.GetBundle).Where(bundle => bundle.TerminateAll)
                         .SelectMany(bundle => bundle.SubPowers))
            {
                allSubPowers.Add(subPower);
            }
        }

        if (exclude != null)
        {
            allSubPowers.Remove(exclude);
        }

        var toTerminate = character.PowersUsedByMe.Where(u => allSubPowers.Contains(u.PowerDefinition)).ToList();
        foreach (var power in toTerminate)
        {
            character.TerminatePower(power);
        }
    }

    private static void TerminateSpells(RulesetCharacter character, SpellDefinition exclude,
        IEnumerable<SpellDefinition> spells)
    {
        var allSubSpells = new HashSet<SpellDefinition>();

        foreach (var spell in spells)
        {
            allSubSpells.Add(spell);
            foreach (var allElement in DatabaseRepository.GetDatabase<SpellDefinition>().GetAllElements())
            {
                if (!spell.IsSubSpellOf(allElement))
                {
                    continue;
                }

                foreach (var subSpell in allElement.SubspellsList)
                {
                    allSubSpells.Add(subSpell);
                }
            }
        }

        if (exclude != null)
        {
            allSubSpells.Remove(exclude);
        }

        var toTerminate = character.SpellsCastByMe.Where(c => allSubSpells.Contains(c.SpellDefinition)).ToList();
        foreach (var spell in toTerminate)
        {
            character.TerminateSpell(spell);
        }
    }
}
