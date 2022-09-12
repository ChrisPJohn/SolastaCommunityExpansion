﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using static FeatureDefinitionCastSpell;
using static SolastaUnfinishedBusiness.Models.SpellsSlotsContext;
using static SolastaUnfinishedBusiness.Models.IntegrationContext;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;

namespace SolastaUnfinishedBusiness.Models;

public enum CasterType
{
    None = 0,
    Full = 2,
    Half = 4,
    HalfRoundUp = 5,
    OneThird = 6
}

public static class SharedSpellsContext
{
    public static Dictionary<string, BaseDefinition> RecoverySlots { get; } = new()
    {
        { "PowerCircleLandNaturalRecovery", Druid },
        { "PowerWizardArcaneRecovery", Wizard },
        { "PowerSpellMasterBonusRecovery", Wizard }
        // added during load
        //{ "TinkererSpellStoringItem", TinkererClass },
        //{ "ArtificerInfusionSpellRefuelingRing", TinkererClass },
        //{ "PowerAlchemistSpellBonusRecovery", TinkererClass }
    };

    private static Dictionary<CharacterClassDefinition, CasterType> ClassCasterType { get; } = new()
    {
        { Bard, CasterType.Full },
        { Cleric, CasterType.Full },
        { Druid, CasterType.Full },
        { Sorcerer, CasterType.Full },
        { Wizard, CasterType.Full },
        { Paladin, CasterType.Half },
        { Ranger, CasterType.Half }
        // added during load
        //{ TinkererClass, CasterType.HalfRoundUp }
    };

    private static Dictionary<CharacterSubclassDefinition, CasterType> SubclassCasterType { get; } = new()
    {
        { MartialSpellblade, CasterType.OneThird },
        { RoguishShadowCaster, CasterType.OneThird },
        { TraditionLight, CasterType.OneThird }
        // added during load
        //{ ConArtistSubclass, CasterType.OneThird }, // ChrisJohnDigital
        //{ SpellShieldSubclass, CasterType.OneThird } // ChrisJohnDigital
    };

    private static CasterType GetCasterTypeForClassOrSubclass(
        [CanBeNull] CharacterClassDefinition characterClassDefinition,
        CharacterSubclassDefinition characterSubclassDefinition)
    {
        if (characterClassDefinition != null && ClassCasterType.ContainsKey(characterClassDefinition))
        {
            return ClassCasterType[characterClassDefinition];
        }

        if (characterSubclassDefinition != null && SubclassCasterType.ContainsKey(characterSubclassDefinition))
        {
            return SubclassCasterType[characterSubclassDefinition];
        }

        return CasterType.None;
    }

    public static RulesetCharacterHero GetHero(string name)
    {
        // try to get hero from game campaign
        var gameCampaign = Gui.GameCampaign;

        if (gameCampaign != null)
        {
            var gameCampaignCharacter =
                gameCampaign.Party.CharactersList.Find(x => x.RulesetCharacter.Name == name);

            if (gameCampaignCharacter is { RulesetCharacter: RulesetCharacterHero rulesetCharacterHero })
            {
                return rulesetCharacterHero;
            }
        }

        // otherwise gets hero from level up
        var hero = Global.ActiveLevelUpHero;

        // finally falls back to inspection [when browsing hero in char pool]
        return hero ?? Global.InspectedHero;
    }

    public static bool IsWarlock(CharacterClassDefinition characterClassDefinition)
    {
        return characterClassDefinition == Warlock;
    }

    // need the null check for companions who don't have repertoires
    public static bool IsMulticaster([CanBeNull] RulesetCharacterHero rulesetCharacterHero)
    {
        return rulesetCharacterHero != null
               && rulesetCharacterHero.SpellRepertoires
                   .Count(sr => sr.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race) > 1;
    }

    // need the null check for companions who don't have repertoires
    public static bool IsSharedcaster([CanBeNull] RulesetCharacterHero rulesetCharacterHero)
    {
        return rulesetCharacterHero != null
               && rulesetCharacterHero.SpellRepertoires
                   .Where(sr => sr.SpellCastingClass != Warlock)
                   .Count(sr => sr.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race) > 1;
    }

    // need the null check for companions who don't have repertoires
    internal static int GetWarlockCasterLevel([CanBeNull] RulesetCharacterHero rulesetCharacterHero)
    {
        if (rulesetCharacterHero == null)
        {
            return 0;
        }

        var warlockLevel = 0;
        var warlock = rulesetCharacterHero.ClassesAndLevels.Keys.FirstOrDefault(x => x == Warlock);

        if (warlock != null)
        {
            warlockLevel = rulesetCharacterHero.ClassesAndLevels[warlock];
        }

        return warlockLevel;
    }

    public static int GetWarlockSpellLevel(RulesetCharacterHero rulesetCharacterHero)
    {
        var warlockLevel = GetWarlockCasterLevel(rulesetCharacterHero);

        return warlockLevel > 0 ? WarlockCastingSlots[warlockLevel - 1].Slots.IndexOf(0) : 0;
    }

    public static int GetWarlockMaxSlots(RulesetCharacterHero rulesetCharacterHero)
    {
        var warlockLevel = GetWarlockCasterLevel(rulesetCharacterHero);

        return warlockLevel > 0 ? WarlockCastingSlots[warlockLevel - 1].Slots[0] : 0;
    }

    public static int GetWarlockUsedSlots([NotNull] RulesetCharacterHero rulesetCharacterHero)
    {
        var repertoire = GetWarlockSpellRepertoire(rulesetCharacterHero);

        if (repertoire == null)
        {
            return 0;
        }

        repertoire.usedSpellsSlots.TryGetValue(-1, out var warlockUsedSlots);

        return warlockUsedSlots;
    }

    [CanBeNull]
    public static RulesetSpellRepertoire GetWarlockSpellRepertoire([NotNull] RulesetCharacterHero rulesetCharacterHero)
    {
        return rulesetCharacterHero.SpellRepertoires.FirstOrDefault(x => IsWarlock(x.SpellCastingClass));
    }

    public static int GetSharedCasterLevel([CanBeNull] RulesetCharacterHero rulesetCharacterHero)
    {
        if (rulesetCharacterHero?.ClassesAndLevels == null)
        {
            return 0;
        }

        var casterLevelContext = new CasterLevelContext();

        foreach (var classAndLevel in rulesetCharacterHero.ClassesAndLevels)
        {
            var currentCharacterClassDefinition = classAndLevel.Key;

            rulesetCharacterHero.ClassesAndSubclasses.TryGetValue(currentCharacterClassDefinition,
                out var currentCharacterSubclassDefinition);

            var casterType = GetCasterTypeForClassOrSubclass(currentCharacterClassDefinition,
                currentCharacterSubclassDefinition);

            casterLevelContext.IncrementCasterLevel(casterType, classAndLevel.Value);
        }

        return casterLevelContext.GetCasterLevel();
    }

    public static int GetSharedSpellLevel(RulesetCharacterHero rulesetCharacterHero)
    {
        if (!IsSharedcaster(rulesetCharacterHero))
        {
            var repertoire = rulesetCharacterHero.SpellRepertoires
                .Find(x => x.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race &&
                           x.SpellCastingClass != Warlock);

            return GetClassSpellLevel(repertoire);
        }

        var sharedCasterLevel = GetSharedCasterLevel(rulesetCharacterHero);

        return sharedCasterLevel > 0 ? FullCastingSlots[sharedCasterLevel - 1].Slots.IndexOf(0) : 0;
    }

    public static int GetClassSpellLevel([CanBeNull] RulesetSpellRepertoire spellRepertoire)
    {
        if (spellRepertoire?.SpellCastingFeature.SlotsPerLevels == null || spellRepertoire.SpellCastingLevel <= 0)
        {
            return 0;
        }

        var slotsPerLevel =
            spellRepertoire.SpellCastingFeature.SlotsPerLevels[spellRepertoire.SpellCastingLevel - 1];

        return slotsPerLevel.Slots.IndexOf(0);
    }

    public static void Load()
    {
        // ClassCasterType.Add(TinkererClass, CasterType.HalfRoundUp);
        SubclassCasterType.Add(ConArtistSubclass, CasterType.OneThird);
        SubclassCasterType.Add(SpellShieldSubclass, CasterType.OneThird);
        // SubclassCasterType.Add(PathOfTheRageMageSubclass, CasterType.OneThird);
        // RecoverySlots.Add("TinkererSpellStoringItem", TinkererClass);
        // RecoverySlots.Add("ArtificerInfusionSpellRefuelingRing", TinkererClass);
        // RecoverySlots.Add("PowerAlchemistSpellBonusRecovery", TinkererClass);
    }

    private sealed class CasterLevelContext
    {
        private readonly Dictionary<CasterType, int> levels;

        public CasterLevelContext()
        {
            levels = new Dictionary<CasterType, int>
            {
                { CasterType.None, 0 },
                { CasterType.Full, 0 },
                { CasterType.Half, 0 },
                { CasterType.HalfRoundUp, 0 },
                { CasterType.OneThird, 0 }
            };
        }

        public void IncrementCasterLevel(CasterType casterType, int increment)
        {
            levels[casterType] += increment;
        }

        public int GetCasterLevel()
        {
            var casterLevel = 0;

            // Full Casters
            casterLevel += levels[CasterType.Full];

            // Tinkerer / ...
            if (levels[CasterType.HalfRoundUp] == 1)
            {
                casterLevel++;
            }
            // Half Casters
            else
            {
                casterLevel += (int)Math.Floor(levels[CasterType.HalfRoundUp] / 2.0);
            }

            casterLevel += (int)Math.Floor(levels[CasterType.Half] / 2.0);

            // Con Artist / ...
            casterLevel += (int)Math.Floor(levels[CasterType.OneThird] / 3.0);

            return casterLevel;
        }
    }
}
