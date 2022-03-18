﻿using System;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders
{
    public class RecipeDefinitionBuilder : DefinitionBuilder<RecipeDefinition, RecipeDefinitionBuilder>
    {
        #region Constructors
        protected RecipeDefinitionBuilder(RecipeDefinition original) : base(original)
        {
        }

        protected RecipeDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected RecipeDefinitionBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected RecipeDefinitionBuilder(string name, bool createGuiPresentation = true) : base(name, createGuiPresentation)
        {
        }

        protected RecipeDefinitionBuilder(RecipeDefinition original, string name, bool createGuiPresentation = true) : base(original, name, createGuiPresentation)
        {
        }

        protected RecipeDefinitionBuilder(RecipeDefinition original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }
        #endregion

        protected RecipeDefinitionBuilder(RecipeDefinition original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }

        public RecipeDefinitionBuilder SetCraftedItem(ItemDefinition craftedItem)
        {
            Definition.SetCraftedItem(craftedItem);
            return this;
        }

        public RecipeDefinitionBuilder SetCraftedItem(ItemDefinition craftedItem, int stackCount)
        {
            Definition.SetCraftedItem(craftedItem);
            Definition.SetStackCount(stackCount);
            return this;
        }

        public RecipeDefinitionBuilder SetCraftingCheckData(int craftingHours, int craftingDC, ToolTypeDefinition toolType)
        {
            Definition.SetCraftingHours(craftingHours);
            Definition.SetCraftingDC(craftingDC);
            Definition.SetToolTypeDefinition(toolType);
            return this;
        }

        public RecipeDefinitionBuilder AddIngredient(IngredientOccurenceDescription ingredient)
        {
            Definition.Ingredients.Add(ingredient);
            return this;
        }

        public RecipeDefinitionBuilder AddIngredient(ItemDefinition ingredient)
        {
            IngredientOccurenceDescription description = new IngredientOccurenceDescription();
            description.SetItemDefinition(ingredient);
            description.SetAmount(1);
            Definition.Ingredients.Add(description);
            return this;
        }

        public RecipeDefinitionBuilder AddIngredient(ItemDefinition ingredient, int amount)
        {
            IngredientOccurenceDescription description = new IngredientOccurenceDescription();
            description.SetItemDefinition(ingredient);
            description.SetAmount(amount);
            Definition.Ingredients.Add(description);
            return this;
        }

        public RecipeDefinitionBuilder SetSpellDefinition(SpellDefinition spellDefinition)
        {
            Definition.SetSpellDefinition(spellDefinition);
            return this;
        }
    }
}
