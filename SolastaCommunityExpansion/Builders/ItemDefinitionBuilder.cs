﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Builders
{
    public class ItemDefinitionBuilder : DefinitionBuilder<ItemDefinition>
    {
        public ItemDefinitionBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        public ItemDefinitionBuilder(string name, Guid namespaceGuid)
            : base(name, namespaceGuid)
        {
        }

        public ItemDefinitionBuilder(ItemDefinition original, string name, string guid)
            : base(original, name, guid)
        {
        }

        private ItemDefinitionBuilder(ItemDefinition original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }

        public static ItemDefinitionBuilder Create(ItemDefinition original, string name, Guid namespaceGuid)
        {
            return new ItemDefinitionBuilder(original, name, namespaceGuid);
        }

        public ItemDefinitionBuilder SetDocumentInformation(RecipeDefinition recipeDefinition, params ContentFragmentDescription[] contentFragments)
        {
            SetDocumentInformation(recipeDefinition, contentFragments.AsEnumerable());
            return this;
        }

        public ItemDefinitionBuilder SetDocumentInformation(RecipeDefinition recipeDefinition, IEnumerable<ContentFragmentDescription> contentFragments)
        {
            if (Definition.DocumentDescription == null)
            {
                Definition.SetDocumentDescription(new DocumentDescription());
            }

            Definition.IsDocument = true;
            Definition.DocumentDescription.SetRecipeDefinition(recipeDefinition);
            Definition.DocumentDescription.SetLoreType(RuleDefinitions.LoreType.CraftingRecipe);
            Definition.DocumentDescription.SetDestroyAfterReading(true);
            Definition.DocumentDescription.SetLocationKnowledgeLevel(GameCampaignDefinitions.NodeKnowledge.Known);
            Definition.DocumentDescription.ContentFragments.SetRange(contentFragments);
            return this;
        }

        public ItemDefinitionBuilder SetGuiTitleAndDescription(string title, string description)
        {
            Definition.GuiPresentation.Title = title;
            Definition.GuiPresentation.Description = description;
            return this;
        }

        public ItemDefinitionBuilder SetGold(int gold)
        {
            Definition.SetCosts(new int[] { 0, gold, 0, 0, 0 });
            return this;
        }

        public ItemDefinitionBuilder SetCosts(int[] costs)
        {
            Definition.SetCosts(costs);
            return this;
        }

        public ItemDefinitionBuilder MakeMagical()
        {
            Definition.ItemTags.Remove("Standard");
            Definition.SetMagical(true);
            return this;
        }

        public ItemDefinitionBuilder SetStaticProperties(IEnumerable<ItemPropertyDescription> staticProperties)
        {
            Definition.StaticProperties.SetRange(staticProperties);
            return this;
        }

        public ItemDefinitionBuilder MergeStaticProperties(IEnumerable<ItemPropertyDescription> staticProperties)
        {
            Definition.StaticProperties.AddRange(staticProperties);
            return this;
        }

        public ItemDefinitionBuilder AddWeaponEffect(EffectForm effect)
        {
            Definition.WeaponDescription.EffectDescription.EffectForms.Add(effect);
            return this;
        }

        public ItemDefinitionBuilder SetUsableDeviceDescription(UsableDeviceDescription usableDescription)
        {
            Definition.IsUsableDevice = true;
            Definition.SetUsableDeviceDescription(usableDescription);
            return this;
        }

        public ItemDefinitionBuilder SetUsableDeviceDescription(params FeatureDefinitionPower[] functions)
        {
            return SetUsableDeviceDescription(functions.AsEnumerable());
        }

        public ItemDefinitionBuilder SetUsableDeviceDescription(IEnumerable<FeatureDefinitionPower> functions)
        {
            Definition.IsUsableDevice = true;
            Definition.SetUsableDeviceDescription(new UsableDeviceDescription());
            Definition.UsableDeviceDescription.DeviceFunctions.Clear();
            foreach (FeatureDefinitionPower power in functions)
            {
                DeviceFunctionDescription functionDescription = new DeviceFunctionDescription(DatabaseHelper.ItemDefinitions.Berry_Ration.UsableDeviceDescription.DeviceFunctions[0]);
                functionDescription.SetType(DeviceFunctionDescription.FunctionType.Power);
                functionDescription.SetFeatureDefinitionPower(power);
                Definition.UsableDeviceDescription.DeviceFunctions.Add(functionDescription);
            }
            return this;
        }
    }
}
