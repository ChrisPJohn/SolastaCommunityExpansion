﻿using ModKit;
using SolastaCommunityExpansion.Spells;
using SolastaModApi.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SolastaCommunityExpansion.Models
{
    internal static class SpellsContext
    {
        internal class SpellRecord
        {
            public List<string> SuggestedSpellLists { get; set; }

            public bool IsFromOtherMod { get; set; }
        }

        internal static readonly Dictionary<SpellDefinition, SpellRecord> RegisteredSpells = new Dictionary<SpellDefinition, SpellRecord>();

        private static readonly List<SpellDefinition> RegisteredSpellsList = new List<SpellDefinition>();

        private static readonly SortedDictionary<string, SpellListDefinition> spellLists = new SortedDictionary<string, SpellListDefinition>();

        internal static SortedDictionary<string, SpellListDefinition> SpellLists
        {
            get
            {
                if (spellLists.Count != 0)
                {
                    return spellLists;
                }

                var dbCharacterClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
                var dbCharacterSubclassDefinition = DatabaseRepository.GetDatabase<CharacterSubclassDefinition>();

                foreach (var characterClass in dbCharacterClassDefinition)
                {
                    var title = characterClass.FormatTitle();

                    var featureDefinitionCastSpell = characterClass.FeatureUnlocks
                        .Select(x => x.FeatureDefinition)
                        .OfType<FeatureDefinitionCastSpell>()
                        .FirstOrDefault();

                    if (featureDefinitionCastSpell?.SpellListDefinition != null
                        && !spellLists.ContainsValue(featureDefinitionCastSpell.SpellListDefinition))
                    {
                        spellLists.Add(title, featureDefinitionCastSpell.SpellListDefinition);
                    }
                }

                foreach (var characterSubclass in dbCharacterSubclassDefinition)
                {
                    var title = characterSubclass.FormatTitle();

                    var featureDefinition = characterSubclass.FeatureUnlocks
                        .Select(x => x.FeatureDefinition)
                        .FirstOrDefault(x => x is FeatureDefinitionCastSpell || x is FeatureDefinitionMagicAffinity);

                    if (featureDefinition is FeatureDefinitionMagicAffinity featureDefinitionMagicAffinity
                        && featureDefinitionMagicAffinity.ExtendedSpellList != null
                        && !spellLists.ContainsValue(featureDefinitionMagicAffinity.ExtendedSpellList))
                    {
                        spellLists.Add(title.grey().italic(), featureDefinitionMagicAffinity.ExtendedSpellList);
                    }
                    else if (featureDefinition is FeatureDefinitionCastSpell featureDefinitionCastSpell
                        && featureDefinitionCastSpell.SpellListDefinition != null
                        && !spellLists.ContainsValue(featureDefinitionCastSpell.SpellListDefinition))
                    {
                        spellLists.Add(title.grey().italic(), featureDefinitionCastSpell.SpellListDefinition);
                    }
                }

                return spellLists;
            }
        }

        private static List<SpellDefinition> GetAllUnofficialSpells()
        {
            var officialSpellNames = typeof(SolastaModApi.DatabaseHelper.SpellDefinitions)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.PropertyType == typeof(SpellDefinition))
                .Select(f => f.Name).ToHashSet();

            return DatabaseRepository.GetDatabase<SpellDefinition>()
                .Where(f => !officialSpellNames.Contains(f.Name)).ToList();
        }

        private static void LoadAllUnofficialSpells()
        {
            var unofficialSpells = GetAllUnofficialSpells();

            foreach (var spellList in SpellLists.Values)
            {
                foreach (var unofficialSpell in unofficialSpells.Where(x => spellList.ContainsSpell(x)))
                {
                    RegisterSpell(unofficialSpell, isFromOtherMod: true, spellList.Name);
                }
            }
        }

        internal static void Load()
        {
            BazouSpells.Load();
            SrdSpells.Load();

            if (Main.Settings.AllowDisplayAllUnofficialContent)
            {
                LoadAllUnofficialSpells();
            }

            foreach (var registeredSpell in RegisteredSpells.Where(x => !Main.Settings.SpellSpellListEnabled.ContainsKey(x.Key.Name)))
            {
                Main.Settings.SpellSpellListEnabled.Add(registeredSpell.Key.Name, registeredSpell.Value.SuggestedSpellLists);
            }

            SwitchSpellList();
        }

        private static void SwitchSpell(SpellListDefinition spellListDefinition, SpellDefinition spellDefinition, bool enabled)
        {
            var spellsByLevel = spellListDefinition.SpellsByLevel;

            if (enabled && !spellListDefinition.ContainsSpell(spellDefinition))
            {
                if (!spellsByLevel.Any(x => x.Level == spellDefinition.SpellLevel))
                {
                    spellsByLevel.Add(new SpellListDefinition.SpellsByLevelDuplet
                    {
                        Level = spellDefinition.SpellLevel,
                        Spells = new List<SpellDefinition>()
                    });
                }

                spellListDefinition.SpellsByLevel.First(x => x.Level == spellDefinition.SpellLevel).Spells.Add(spellDefinition);
            }
            else if (!enabled && spellListDefinition.ContainsSpell(spellDefinition))
            {
                spellListDefinition.SpellsByLevel.First(x => x.Level == spellDefinition.SpellLevel).Spells.Remove(spellDefinition);
            }
        }

        internal static void SwitchSpellList(SpellDefinition spellDefinition = null, SpellListDefinition spellListDefinition = null)
        {
            if (spellDefinition == null)
            {
                RegisteredSpellsList.ForEach(x => SwitchSpellList(x, null));

                return;
            }

            if (spellListDefinition == null)
            {
                SpellLists.Values.ToList().ForEach(x => SwitchSpellList(spellDefinition, x));

                return;
            }

            var enabled = Main.Settings.SpellSpellListEnabled[spellDefinition.Name].Contains(spellListDefinition.Name);

            SwitchSpell(spellListDefinition, spellDefinition, enabled);
        }

        internal static void RegisterSpell(SpellDefinition spellDefinition, bool isFromOtherMod = false, params string[] suggestedSpellLists)
        {
            var dbSpellListDefinition = DatabaseRepository.GetDatabase<SpellListDefinition>();
            var validateSpellLists = suggestedSpellLists.Where(x => dbSpellListDefinition.TryGetElement(x, out _)).ToList();

            if (!RegisteredSpells.ContainsKey(spellDefinition))
            {
                if (spellDefinition.SpellLevel <= 6 || Main.Settings.EnableLevel20)
                {
                    RegisteredSpellsList.Add(spellDefinition);
                    RegisteredSpells.Add(spellDefinition, new SpellRecord 
                    { 
                        IsFromOtherMod = isFromOtherMod,
                        SuggestedSpellLists = validateSpellLists
                    });
                }
                else
                {
                    spellDefinition.GuiPresentation.SetHidden(true);
                }
            }
            else
            {
                RegisteredSpells[spellDefinition].SuggestedSpellLists.AddRange(validateSpellLists.Where(x => !RegisteredSpells[spellDefinition].SuggestedSpellLists.Contains(x)));
            }
        }

        internal static void SwitchAllSpellLists(bool select = true, SpellDefinition spellDefinition = null)
        {
            if (spellDefinition == null)
            {
                RegisteredSpellsList.ForEach(x => SwitchAllSpellLists(select, x));

                return;
            }

            Main.Settings.SpellSpellListEnabled[spellDefinition.Name].Clear();

            if (select)
            {
                Main.Settings.SpellSpellListEnabled[spellDefinition.Name].AddRange(SpellLists.Values.Select(x => x.Name));
            }

            SwitchSpellList(spellDefinition);
        }

        internal static void SwitchSuggestedSpellLists(bool select = true, SpellDefinition spellDefinition = null)
        {
            if (spellDefinition == null)
            {
                RegisteredSpellsList.ForEach(x => SwitchSuggestedSpellLists(select, x));

                return;
            }

            Main.Settings.SpellSpellListEnabled[spellDefinition.Name].Clear();

            if (select)
            {
                Main.Settings.SpellSpellListEnabled[spellDefinition.Name].AddRange(RegisteredSpells[spellDefinition].SuggestedSpellLists);
            }

            SwitchSpellList(spellDefinition);
        }

        internal static bool AreAllSpellListsSelected() => !RegisteredSpellsList.Any(x => !AreAllSpellListsSelected(x));

        internal static bool AreAllSpellListsSelected(SpellDefinition spellDefinition) => Main.Settings.SpellSpellListEnabled[spellDefinition.Name].Count == SpellsContext.SpellLists.Count;

        internal static bool AreSuggestedSpellListsSelected() => !RegisteredSpellsList.Any(x => !AreSuggestedSpellListsSelected(x));

        internal static bool AreSuggestedSpellListsSelected(SpellDefinition spellDefinition)
        {
            var suggestedSpellLists = RegisteredSpells[spellDefinition].SuggestedSpellLists;
            var selectedSpellLists = Main.Settings.SpellSpellListEnabled[spellDefinition.Name];

            if (suggestedSpellLists.Count != selectedSpellLists.Count || suggestedSpellLists.Count == 0 || selectedSpellLists.Count == 0)
            {
                return false;
            }

            return !suggestedSpellLists.Any(x => !selectedSpellLists.Contains(x));
        }

        public static string GenerateSpellsDescription()
        {
            var outString = new StringBuilder("[heading]Spells[/heading]");

            outString.Append("\n[list]");

            foreach (var spell in RegisteredSpellsList)
            {
                outString.Append("\n[*][b]");
                outString.Append(spell.FormatTitle());
                outString.Append("[/b]: ");
                outString.Append(spell.FormatDescription());
            }

            outString.Append("\n[/list]");

            return outString.ToString();
        }
    }
}
