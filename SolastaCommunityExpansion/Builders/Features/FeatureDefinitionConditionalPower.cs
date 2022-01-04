﻿using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public interface IConditionalPower
    {
        bool IsActive(RulesetCharacterHero character);
    }

    public delegate bool IsActiveConditionalPowerDelegate(RulesetCharacterHero character);

    public class FeatureDefinitionConditionalPower : FeatureDefinitionPower, IConditionalPower
    {
        private IsActiveConditionalPowerDelegate isActive;

        internal void SetIsActiveDelegate(IsActiveConditionalPowerDelegate del)
        {
            isActive = del;
        }

        public bool IsActive(RulesetCharacterHero character)
        {
            return isActive == null || isActive(character);
        }
    }

    public class FeatureDefinitionConditionalPowerBuilder : BaseDefinitionBuilder<FeatureDefinitionConditionalPower>
    {
        public FeatureDefinitionConditionalPowerBuilder(string name, string guid,
            GuiPresentation guiPresentation) : base(name, guid)
        {
            Definition.SetGuiPresentation(guiPresentation);
        }

        public FeatureDefinitionConditionalPowerBuilder SetEffect(EffectDescription effect)
        {
            Definition.SetEffectDescription(effect);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder DelegatedToAction()
        {
            Definition.SetDelegatedToAction(true);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SurrogateToSpell(SpellDefinition spell)
        {
            Definition.SetSurrogateToSpell(spell);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetActivation(RuleDefinitions.ActivationTime time, int costPerUse)
        {
            Definition.SetActivationTime(time);
            Definition.SetCostPerUse(costPerUse);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetReaction(RuleDefinitions.ReactionTriggerContext context, string name)
        {
            Definition.SetReactionContext(context);
            Definition.SetReactionName(name);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetCastingFailure(RuleDefinitions.CastingSuccessComputation computation)
        {
            Definition.SetHasCastingFailure(true);
            Definition.SetCastingSuccessComputation(computation);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder CanUseInDialog()
        {
            Definition.SetCanUseInDialog(true);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetDisablingCondition(ConditionDefinition condition)
        {
            Definition.SetDisableIfConditionIsOwned(condition);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetRecharge(RuleDefinitions.RechargeRate rate)
        {
            Definition.SetRechargeRate(rate);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetSpellCastingFeature(FeatureDefinitionCastSpell spellFeature)
        {
            Definition.SetSpellcastingFeature(spellFeature);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetUsesFixed(int fixedUses)
        {
            Definition.SetFixedUsesPerRecharge(fixedUses);
            Definition.SetUsesDetermination(RuleDefinitions.UsesDetermination.Fixed);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetUsesProficiency()
        {
            Definition.SetUsesDetermination(RuleDefinitions.UsesDetermination.ProficiencyBonus);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetUsesAbility(int fixedUses, string attribute)
        {
            Definition.SetFixedUsesPerRecharge(fixedUses);
            Definition.SetUsesAbilityScoreName(attribute);
            Definition.SetUsesDetermination(RuleDefinitions.UsesDetermination.AbilityBonusPlusFixed);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder UseSpellCastingModifier()
        {
            Definition.SetAbilityScoreDetermination(RuleDefinitions.AbilityScoreDetermination.SpellcastingAbility);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetAttackModifierAbility(bool ability, bool proficiency, string attribute)
        {
            Definition.SetAbilityScore(attribute);
            Definition.SetAbilityScoreBonusToAttack(ability);
            Definition.SetProficiencyBonusToAttack(proficiency);
            Definition.SetAttackHitComputation(RuleDefinitions.PowerAttackHitComputation.AbilityScore);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetAttackModifierStatic(int attackModifier)
        {
            Definition.SetFixedAttackHit(attackModifier);
            Definition.SetAttackHitComputation(RuleDefinitions.PowerAttackHitComputation.Fixed);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetUniqueInstance(bool uniqueInstance)
        {
            Definition.SetUniqueInstance(uniqueInstance);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetShowCasting(bool casting)
        {
            Definition.SetShowCasting(casting);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder AddOverriddenPower(FeatureDefinitionPower overridenPower)
        {
            Definition.SetOverriddenPower(overridenPower);
            return this;
        }

        public FeatureDefinitionConditionalPowerBuilder SetIsActive(IsActiveConditionalPowerDelegate del)
        {
            Definition.SetIsActiveDelegate(del);
            return this;
        }
    }
}
