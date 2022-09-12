﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ArmorCategoryDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;

namespace SolastaUnfinishedBusiness.Feats;

internal static class ArmorFeats
{
    public static void CreateArmorFeats([NotNull] List<FeatDefinition> feats)
    {
        var lightArmorProficiency = BuildProficiency("ProficiencyFeatLightArmor",
            ProficiencyType.Armor, EquipmentDefinitions.LightArmorCategory);
        var lightArmorFeat = BuildFeat("FeatLightArmor", lightArmorProficiency, AttributeModifierCreed_Of_Misaye);

        var mediumArmorProficiency = BuildProficiency("ProficiencyFeatMediumArmor",
            ProficiencyType.Armor, EquipmentDefinitions.MediumArmorCategory, EquipmentDefinitions.ShieldCategory);
        var mediumDexArmorFeat = BuildFeat("FeatMediumArmorDex", LightArmorCategory,
            mediumArmorProficiency, AttributeModifierCreed_Of_Misaye);
        var mediumStrArmorFeat = BuildFeat("FeatMediumArmorStr", LightArmorCategory,
            mediumArmorProficiency, AttributeModifierCreed_Of_Einar);

        var heavyArmorMasterFeat = BuildFeat("FeatHeavyArmorMaster", HeavyArmorCategory,
            DamageAffinityBludgeoningResistance, DamageAffinitySlashingResistance,
            DamageAffinityPiercingResistance);

        feats.AddRange(lightArmorFeat, mediumDexArmorFeat, mediumStrArmorFeat, heavyArmorMasterFeat);
    }

    private static FeatDefinition BuildFeat(string name, ArmorCategoryDefinition prerequisite,
        params FeatureDefinition[] features)
    {
        return FeatDefinitionBuilder
            .Create(name, DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(features)
            .SetArmorProficiencyPrerequisite(prerequisite)
            .AddToDB();
    }

    private static FeatDefinition BuildFeat(string name, params FeatureDefinition[] features)
    {
        return FeatDefinitionBuilder
            .Create(name, DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(features)
            .AddToDB();
    }

    private static FeatureDefinitionProficiency BuildProficiency(string name, ProficiencyType type,
        params string[] proficiencies)
    {
        return FeatureDefinitionProficiencyBuilder
            .Create(name, DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(type, proficiencies)
            .AddToDB();
    }
}
