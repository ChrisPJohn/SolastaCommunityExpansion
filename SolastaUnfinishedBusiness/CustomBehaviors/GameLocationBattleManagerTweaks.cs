﻿using System.Collections;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.Extensions;
using UnityEngine;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal static class GameLocationBattleManagerTweaks
{
    /**
     * This method is almost completely original game source provided by TA (1.4.8)
     * All changes made by CE mod should be clearly marked for easy future updates
     */
    public static void ComputeAndNotifyAdditionalDamage(GameLocationBattleManager instance,
        GameLocationCharacter attacker, GameLocationCharacter defender, IAdditionalDamageProvider provider,
        List<EffectForm> actualEffectForms, CharacterActionParams reactionParams, RulesetAttackMode attackMode,
        bool criticalHit)
    {
        var additionalDamageForm = DamageForm.Get();
        var featureDefinition = provider as FeatureDefinition;

        /*
         * ######################################
         * [CE] EDIT START
         * Support for wild-shaped characters
         */

        //[CE] Store original RulesetCharacterHero for future use
        var hero = attacker.RulesetCharacter as RulesetCharacterHero ??
                   attacker.RulesetCharacter.OriginalFormCharacter as RulesetCharacterHero;

        /*
         * Support for wild-shaped characters
         * [CE] EDIT END
         * ######################################
         */

        // What is the method to determine the amount of damage?
        if (provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination.Die)
        {
            var diceNumber = provider.DamageDiceNumber;

            if (provider.DamageAdvancement == RuleDefinitions.AdditionalDamageAdvancement.ClassLevel)
            {
                // Find the character class which triggered this
                /*
                 * ######################################
                 * [CE] EDIT START
                 * Support for wild-shaped characters
                 */

                // [CE] comment-out this local variable, so that one declared above, which accounts for wild-shape, is used
                // RulesetCharacterHero hero = attacker.RulesetCharacter as RulesetCharacterHero;

                // [CE] commented-out original code
                //CharacterClassDefinition classDefinition = hero.FindClassHoldingFeature(featureDefinition);

                // Use null-coalescing operator to ward against possible `NullReferenceException`
                var classDefinition = hero?.FindClassHoldingFeature(featureDefinition);

                /*
                 * Support for wild-shaped characters
                 * [CE] EDIT END
                 * ######################################
                 */
                if (classDefinition != null)
                {
                    var classLevel = hero.ClassesAndLevels[classDefinition];
                    diceNumber = provider.GetDiceOfRank(classLevel);
                }
            }
            /*
             * ######################################
             * [CE] EDIT START
             * Support for `CharacterLevel` damage progression
             */
            else if ((ExtraAdditionalDamageAdvancement)provider.DamageAdvancement ==
                     ExtraAdditionalDamageAdvancement.CharacterLevel)
            {
                var rulesetCharacter = attacker.RulesetCharacter as RulesetCharacterHero ??
                                       attacker.RulesetCharacter.OriginalFormCharacter as RulesetCharacterHero;

                if (rulesetCharacter != null)
                {
                    var characterLevel =
                        rulesetCharacter.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
                    diceNumber = provider.GetDiceOfRank(characterLevel);
                }
            }
            /*
             * Support for `CharacterLevel` damage progression
             * [CE] EDIT END
             * ######################################
             */
            else if (provider.DamageAdvancement == RuleDefinitions.AdditionalDamageAdvancement.SlotLevel)
            {
                if (reactionParams != null)
                {
                    diceNumber = provider.GetDiceOfRank(reactionParams.IntParameter);
                }
                else
                {
                    var condition =
                        attacker.RulesetCharacter.FindFirstConditionHoldingFeature(provider as FeatureDefinition);
                    if (condition != null)
                    {
                        diceNumber = provider.GetDiceOfRank(condition.EffectLevel);
                    }
                }
            }

            // Some specific families may receive more dice (example paladin smiting undead/fiends)
            if (defender.RulesetCharacter != null && provider.FamiliesWithAdditionalDice.Count > 0 &&
                provider.FamiliesWithAdditionalDice.Contains(defender.RulesetCharacter.CharacterFamily))
            {
                diceNumber += provider.FamiliesDiceNumber;
            }

            additionalDamageForm.DieType = provider.DamageDieType;
            additionalDamageForm.DiceNumber = diceNumber;
        }
        /*
        * ######################################
        * [CE] EDIT START
        * Support for wild-shaped characters
        */

        //Commented out original check
        //else if (attacker.RulesetCharacter is RulesetCharacterHero &&

        //check previously saved hero variable to allow wild-shaped heroes to count for these bonuses
        else if (hero != null &&
                 /*
                  * Support for wild-shaped characters
                  * [CE] EDIT END
                  * ######################################
                  */
                 (provider.DamageValueDetermination ==
                  RuleDefinitions.AdditionalDamageValueDetermination.ProficiencyBonus
                  || provider.DamageValueDetermination ==
                  RuleDefinitions.AdditionalDamageValueDetermination.SpellcastingBonus
                  || provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination
                      .ProficiencyBonusAndSpellcastingBonus
                  || provider.DamageValueDetermination ==
                  RuleDefinitions.AdditionalDamageValueDetermination.RageDamage))
        {
            additionalDamageForm.DieType = RuleDefinitions.DieType.D1;
            additionalDamageForm.DiceNumber = 0;
            additionalDamageForm.BonusDamage = 0;

            if (provider.DamageValueDetermination ==
                RuleDefinitions.AdditionalDamageValueDetermination.ProficiencyBonus ||
                provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination
                    .ProficiencyBonusAndSpellcastingBonus)
            {
                /*
                 * ######################################
                 * [CE] EDIT START
                 * Support for wild-shaped characters
                 */

                //Commented out original check
                // additionalDamageForm.BonusDamage += (attacker.RulesetCharacter as RulesetCharacterHero).GetAttribute(AttributeDefinitions.ProficiencyBonus).CurrentValue;

                //use previously saved original RulesetCharacterHero
                additionalDamageForm.BonusDamage +=
                    hero.GetAttribute(AttributeDefinitions.ProficiencyBonus).CurrentValue;

                /*
                 * Support for wild-shaped characters
                 * [CE] EDIT END
                 * ######################################
                 */
            }

            if (provider.DamageValueDetermination ==
                RuleDefinitions.AdditionalDamageValueDetermination.SpellcastingBonus ||
                provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination
                    .ProficiencyBonusAndSpellcastingBonus)
            {
                // Look for the Spell Repertoire
                var spellBonus = 0;
                foreach (var spellRepertoire in attacker.RulesetCharacter.SpellRepertoires)
                {
                    spellBonus = AttributeDefinitions.ComputeAbilityScoreModifier(attacker.RulesetCharacter
                        .GetAttribute(spellRepertoire.SpellCastingAbility).CurrentValue);

                    // Stop if this is a class repertoire
                    if (spellRepertoire.SpellCastingFeature.SpellCastingOrigin ==
                        FeatureDefinitionCastSpell.CastingOrigin.Class)
                    {
                        break;
                    }
                }

                additionalDamageForm.BonusDamage += spellBonus;
            }

            if (provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination.RageDamage)
            {
                additionalDamageForm.BonusDamage =
                    attacker.RulesetCharacter.TryGetAttributeValue(AttributeDefinitions.RageDamage);
            }
        }
        else if (provider.DamageValueDetermination ==
                 RuleDefinitions.AdditionalDamageValueDetermination.ProficiencyBonusOfSource)
        {
            // Try to find the condition granting the provider
            var holdingCondition =
                attacker.RulesetCharacter.FindFirstConditionHoldingFeature(provider as FeatureDefinition);
            RulesetCharacter sourceCharacter = null;
            if (holdingCondition != null &&
                RulesetEntity.TryGetEntity(holdingCondition.SourceGuid, out sourceCharacter))
            {
                additionalDamageForm.DieType = RuleDefinitions.DieType.D1;
                additionalDamageForm.DiceNumber = 0;
                additionalDamageForm.BonusDamage =
                    sourceCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            }
        }
        /*
         * ######################################
         * [CE] EDIT START
         * Support for wild-shaped characters
         */

        //Commented out original check
        // else if (provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination.TargetKnowledgeLevel && attacker.RulesetCharacter is RulesetCharacterHero && defender.RulesetCharacter is RulesetCharacterMonster)

        // [CE] use previously saved hero variable to check if attacker is actually a  hero, this allows for wild-shaped charaters to count
        else if (provider.DamageValueDetermination ==
                 RuleDefinitions.AdditionalDamageValueDetermination.TargetKnowledgeLevel && hero != null &&
                 defender.RulesetCharacter is RulesetCharacterMonster)
            /*
             * Support for wild-shaped characters
             * [CE] EDIT END
             * ######################################
             */
        {
            additionalDamageForm.DieType = RuleDefinitions.DieType.D1;
            additionalDamageForm.DiceNumber = 0;
            additionalDamageForm.BonusDamage = ServiceRepository.GetService<IGameLoreService>()
                .GetCreatureKnowledgeLevel(defender.RulesetCharacter).AdditionalDamage;
        }
        else if (provider.DamageValueDetermination ==
                 RuleDefinitions.AdditionalDamageValueDetermination.BrutalCriticalDice)
        {
            var useVersatileDamage = attackMode != null && attackMode.UseVersatileDamage;
            var damageForm = EffectForm.GetFirstDamageForm(actualEffectForms);
            additionalDamageForm.DieType = useVersatileDamage ? damageForm.VersatileDieType : damageForm.DieType;
            additionalDamageForm.DiceNumber =
                attacker.RulesetCharacter.TryGetAttributeValue(AttributeDefinitions.BrutalCriticalDice);
            additionalDamageForm.BonusDamage = 0;
        }
        else if (provider.DamageValueDetermination ==
                 RuleDefinitions.AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
        {
            var useVersatileDamage = attackMode != null && attackMode.UseVersatileDamage;
            var damageForm = EffectForm.GetFirstDamageForm(actualEffectForms);
            additionalDamageForm.DieType = useVersatileDamage ? damageForm.VersatileDieType : damageForm.DieType;
            additionalDamageForm.DiceNumber = 1;
            additionalDamageForm.BonusDamage = 0;
        }
        else if (provider.DamageValueDetermination ==
                 RuleDefinitions.AdditionalDamageValueDetermination.HalfAbilityScoreBonus)
        {
            if (attackMode != null)
            {
                var abilityScore = attackMode.AbilityScore;
                var halfUp = Mathf.CeilToInt(0.5f *
                                             AttributeDefinitions.ComputeAbilityScoreModifier(
                                                 attacker.RulesetCharacter.TryGetAttributeValue(abilityScore)));
                if (halfUp > 0)
                {
                    additionalDamageForm.DieType = RuleDefinitions.DieType.D1;
                    additionalDamageForm.DiceNumber = 0;
                    additionalDamageForm.BonusDamage = halfUp;
                }
            }
        }
        else if (provider.DamageValueDetermination == RuleDefinitions.AdditionalDamageValueDetermination.None)
        {
            additionalDamageForm.DiceNumber = 0;
            additionalDamageForm.BonusDamage = 0;
        }

        additionalDamageForm.IgnoreCriticalDoubleDice = provider.IgnoreCriticalDoubleDice;
        additionalDamageForm.IgnoreSpellAdvancementDamageDice = true;

        // Account the use
        if (attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name))
        {
            attacker.UsedSpecialFeatures[featureDefinition.Name]++;
        }
        else
        {
            attacker.UsedSpecialFeatures[featureDefinition.Name] = 1;
        }

        if (additionalDamageForm.DiceNumber > 0 || additionalDamageForm.BonusDamage > 0)
        {
            // Add the new damage form
            switch (provider.AdditionalDamageType)
            {
                case RuleDefinitions.AdditionalDamageType.SameAsBaseDamage:
                    additionalDamageForm.DamageType = EffectForm.GetFirstDamageForm(actualEffectForms).DamageType;
                    break;

                case RuleDefinitions.AdditionalDamageType.Specific:
                    additionalDamageForm.DamageType = provider.SpecificDamageType;
                    break;

                case RuleDefinitions.AdditionalDamageType.AncestryDamageType:
                    attacker.RulesetCharacter.EnumerateFeaturesToBrowse<FeatureDefinitionAncestry>(
                        FeatureDefinitionAncestry.FeaturesToBrowse);

                    // Pick the first one
                    if (FeatureDefinitionAncestry.FeaturesToBrowse.Count > 0)
                    {
                        additionalDamageForm.DamageType =
                            (FeatureDefinitionAncestry.FeaturesToBrowse[0] as FeatureDefinitionAncestry).DamageType;
                    }

                    break;
            }

            // For ancestry damage, add to the existing / matching damage, instead of add a new effect form
            if (provider.AdditionalDamageType == RuleDefinitions.AdditionalDamageType.AncestryDamageType
                && provider.DamageValueDetermination ==
                RuleDefinitions.AdditionalDamageValueDetermination.SpellcastingBonus)
            {
                foreach (var effectForm in actualEffectForms)
                {
                    if (effectForm.FormType == EffectForm.EffectFormType.Damage &&
                        effectForm.DamageForm.DamageType == additionalDamageForm.DamageType)
                    {
                        effectForm.DamageForm.BonusDamage += additionalDamageForm.BonusDamage;
                    }
                }
            }
            else
            {
                // Add a new effect form
                var newEffectForm = EffectForm.GetFromDamageForm(additionalDamageForm);

                // Specific saving throw?
                if (provider.HasSavingThrow)
                {
                    // This additional damage will override the saving throw for the whole attack
                    newEffectForm.SavingThrowAffinity = provider.DamageSaveAffinity;
                    var rulesetImplementationService =
                        ServiceRepository.GetService<IRulesetImplementationService>();
                    var saveDC =
                        rulesetImplementationService.ComputeSavingThrowDC(attacker.RulesetCharacter, provider);
                    newEffectForm.OverrideSavingThrowInfo = new OverrideSavingThrowInfo(provider.SavingThrowAbility,
                        saveDC, provider.Name, RuleDefinitions.FeatureSourceType.ExplicitFeature);
                }

                actualEffectForms.Add(newEffectForm);
            }

            // Notify observers
            if (attacker.RulesetCharacter.AdditionalDamageGenerated != null)
            {
                // We want to include doubling the dice for a critical hit
                var diceNumber = additionalDamageForm.DiceNumber;
                if (additionalDamageForm.DieType != RuleDefinitions.DieType.D1 && criticalHit &&
                    !additionalDamageForm.IgnoreCriticalDoubleDice)
                {
                    diceNumber *= 2;
                }

                // Handle bardic inspiration die override
                if (additionalDamageForm.OverrideWithBardicInspirationDie &&
                    attacker.RulesetCharacter.GetBardicInspirationDieValue() != RuleDefinitions.DieType.D1)
                {
                    additionalDamageForm.DieType = attacker.RulesetCharacter.GetBardicInspirationDieValue();
                }

                attacker.RulesetCharacter.AdditionalDamageGenerated.Invoke(attacker.RulesetCharacter,
                    defender.RulesetActor, additionalDamageForm.DieType, diceNumber,
                    additionalDamageForm.BonusDamage, provider.NotificationTag);
            }
        }

        // Do I need to perform condition operations?
        if (provider.ConditionOperations.Count > 0)
        {
            foreach (var conditionOperation in provider.ConditionOperations)
            {
                var newEffectForm = new EffectForm();
                newEffectForm.FormType = EffectForm.EffectFormType.Condition;
                newEffectForm.ConditionForm = new ConditionForm();
                newEffectForm.ConditionForm.ConditionDefinition = conditionOperation.ConditionDefinition;
                newEffectForm.ConditionForm.Operation =
                    conditionOperation.Operation == ConditionOperationDescription.ConditionOperation.Add
                        ? ConditionForm.ConditionOperation.Add
                        : ConditionForm.ConditionOperation.Remove;
                newEffectForm.CanSaveToCancel = conditionOperation.CanSaveToCancel;
                newEffectForm.SaveOccurence = conditionOperation.SaveOccurence;

                if (conditionOperation.Operation == ConditionOperationDescription.ConditionOperation.Add &&
                    provider.HasSavingThrow)
                {
                    // This additional damage will override the saving throw for the whole attack
                    newEffectForm.SavingThrowAffinity = conditionOperation.SaveAffinity;
                    var rulesetImplementationService =
                        ServiceRepository.GetService<IRulesetImplementationService>();
                    var saveDC =
                        rulesetImplementationService.ComputeSavingThrowDC(attacker.RulesetCharacter, provider);
                    newEffectForm.OverrideSavingThrowInfo = new OverrideSavingThrowInfo(provider.SavingThrowAbility,
                        saveDC, provider.Name, RuleDefinitions.FeatureSourceType.ExplicitFeature);
                }

                actualEffectForms.Add(newEffectForm);
            }
        }

        // Do I need to add a light source?
        if (provider.AddLightSource && defender.RulesetCharacter != null &&
            defender.RulesetCharacter.PersonalLightSource == null)
        {
            var lightSourceForm = provider.LightSourceForm;

            var visibilityService = ServiceRepository.GetService<IGameLocationVisibilityService>();
            float brightRange = lightSourceForm.BrightRange;
            var dimRange = brightRange + lightSourceForm.DimAdditionalRange;
            defender.RulesetCharacter.PersonalLightSource = new RulesetLightSource(
                lightSourceForm.Color,
                brightRange,
                dimRange,
                lightSourceForm.GraphicsPrefabAssetGUID,
                lightSourceForm.LightSourceType,
                featureDefinition.Name,
                defender.RulesetCharacter.Guid);
            defender.RulesetCharacter.PersonalLightSource.Register(true);

            visibilityService.AddCharacterLightSource(defender, defender.RulesetCharacter.PersonalLightSource);

            var holdingCondition =
                attacker.RulesetCharacter.FindFirstConditionHoldingFeature(provider as FeatureDefinition);
            if (holdingCondition != null)
            {
                var effect = attacker.RulesetCharacter.FindEffectTrackingCondition(holdingCondition);
                effect.TrackLightSource(defender.RulesetCharacter, defender.Guid, string.Empty,
                    defender.RulesetCharacter.PersonalLightSource);
            }
        }

        //CHANGE: replaced `this` with `instance`
        instance.AdditionalDamageProviderActivated?.Invoke(attacker, defender, provider);
    }


    /**
     * This method is almost completely original game source provided by TA (1.4.8)
     * All changes made by CE mod should be clearly marked for easy future updates
     * This is for both physical and magical attacks
     */
    public static IEnumerator HandleAdditionalDamageOnCharacterAttackHitConfirmed(
        GameLocationBattleManager instance,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        ActionModifier attackModifier,
        RulesetAttackMode attackMode,
        bool rangedAttack,
        RuleDefinitions.AdvantageType advantageType,
        List<EffectForm> actualEffectForms,
        RulesetEffect rulesetEffect,
        bool criticalHit,
        bool firstTarget)
    {
        instance.triggeredAdditionalDamageTags.Clear();
        attacker.RulesetCharacter.EnumerateFeaturesToBrowse<IAdditionalDamageProvider>(
            instance.featuresToBrowseReaction);

        // Add item properties?
        if (attacker.RulesetCharacter.CharacterInventory != null)
        {
            if (attackMode?.SourceObject is RulesetItem weapon)
            {
                weapon.EnumerateFeaturesToBrowse<IAdditionalDamageProvider>(instance.featuresToBrowseItem);
                instance.featuresToBrowseReaction.AddRange(instance.featuresToBrowseItem);
                instance.featuresToBrowseItem.Clear();
            }
        }

        /*
         * ######################################
         * [CE] EDIT START
         * Support for extra types of Smite (like eldritch smite)
         */

        // store ruleset service for further use
        var rulesetImplementation = ServiceRepository.GetService<IRulesetImplementationService>();

        /*
         * Support for extra types of Smite (like eldritch smite)
         * [CE] EDIT END
         * ######################################
         */

        foreach (var featureDefinition in instance.featuresToBrowseReaction)
        {
            var provider = featureDefinition as IAdditionalDamageProvider;
            var additionalDamage = provider as FeatureDefinitionAdditionalDamage;

            // Some additional damage only work with attack modes (Hunter's Mark)
            if (provider.AttackModeOnly && attackMode == null)
            {
                continue;
            }

            // Trigger method
            var validTrigger = false;
            var validUses = true;
            if (provider.LimitedUsage != RuleDefinitions.FeatureLimitedUsage.None)
            {
                switch (provider.LimitedUsage)
                {
                    case RuleDefinitions.FeatureLimitedUsage.OnceInMyTurn
                        when attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name) ||
                             (instance.Battle != null && instance.Battle.ActiveContender != attacker):
                    case RuleDefinitions.FeatureLimitedUsage.OncePerTurn
                        when attacker.UsedSpecialFeatures.ContainsKey(featureDefinition.Name):
                        validUses = false;
                        break;

                    default:
                    {
                        if (attacker.UsedSpecialFeatures.Count > 0)
                        {
                            // Check if there is not already a used feature with the same tag (special sneak attack for Rogue Hoodlum / COTM-18228)
                            foreach (var kvp in attacker.UsedSpecialFeatures)
                            {
                                if (DatabaseRepository.GetDatabase<FeatureDefinitionAdditionalDamage>()
                                    .TryGetElement(kvp.Key, out var previousFeature))
                                {
                                    if (previousFeature.NotificationTag == provider.NotificationTag)
                                    {
                                        validUses = false;
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }

            if (additionalDamage != null
                && additionalDamage.OtherSimilarAdditionalDamages != null
                && additionalDamage.OtherSimilarAdditionalDamages.Count > 0
                && attacker.UsedSpecialFeatures.Count > 0)
            {
                // Check if there is not already a used feature of the same "family"
                foreach (var kvp in attacker.UsedSpecialFeatures)
                {
                    if (DatabaseRepository.GetDatabase<FeatureDefinitionAdditionalDamage>()
                        .TryGetElement(kvp.Key, out var previousFeature))
                    {
                        if (additionalDamage.OtherSimilarAdditionalDamages.Contains(previousFeature))
                        {
                            validUses = false;
                        }
                    }
                }
            }

            ItemDefinition itemDefinition = null;
            if (attackMode != null)
            {
                itemDefinition = DatabaseRepository.GetDatabase<ItemDefinition>()
                    .GetElement(attackMode.SourceDefinition.Name, true);
            }

            CharacterActionParams reactionParams = null;

            /*
             * ######################################
             * [CE] EDIT START
             * Support for extra types of Smite (like eldritch smite)
             */
            var validProperty = true;

            if (attackMode != null && validUses &&
                provider.RequiredProperty != RuleDefinitions.RestrictedContextRequiredProperty.None)
            {
                validProperty = rulesetImplementation.IsValidContextForRestrictedContextProvider(provider,
                    attacker.RulesetCharacter, itemDefinition, rangedAttack, attackMode, rulesetEffect);
            }

            //[CE] try checking triggers only if context is valid, to prevent SpendSpellSlot showing popup on incorrect context
            if (validUses && validProperty)
                //commented-out original code
                // if (validUses)
                /*
                 * Support for extra types of Smite (like eldritch smite)
                 * [CE] EDIT END
                 * ######################################
                 */
            {
                switch (provider.TriggerCondition)
                {
                    // Typical for Sneak Attack
                    case RuleDefinitions.AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly
                        when attackMode != null
                             || (rulesetEffect != null
                                 && provider.RequiredProperty == RuleDefinitions.RestrictedContextRequiredProperty
                                     .SpellWithAttackRoll):
                    {
                        if (advantageType == RuleDefinitions.AdvantageType.Advantage ||
                            (advantageType != RuleDefinitions.AdvantageType.Disadvantage &&
                             instance.IsConsciousCharacterOfSideNextToCharacter(defender, attacker.Side, attacker)))
                        {
                            validTrigger = true;
                        }

                        break;
                    }
                    /*
                     * ######################################
                     * [CE] EDIT START
                     * Support for extra types of Smite (like eldritch smite)
                     */

                    // [CE] remove melee check, so that other types of smites can be made
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpendSpellSlot:

                        // commented-out original code
                        // case RuleDefinitions.AdditionalDamageTriggerCondition.SpendSpellSlot
                        //     when attackModifier != null
                        //          && attackModifier.Proximity == RuleDefinitions.AttackProximity.Melee:

                        /*
                         * Support for extra types of Smite (like eldritch smite)
                         * [CE] EDIT END
                         * ######################################
                         */
                    {
                        //TODO: implement wild-shape, MC and warlock spell slot tweaks 
                        // This is used for Divine Smite
                        // Look for the spellcasting feature holding the smite
                        var hero = attacker.RulesetCharacter as RulesetCharacterHero;
                        if (hero == null && attacker.RulesetCharacter.OriginalFormCharacter != null)
                        {
                            hero = attacker.RulesetCharacter.OriginalFormCharacter as RulesetCharacterHero;
                        }

                        var classDefinition = hero.FindClassHoldingFeature(featureDefinition);
                        RulesetSpellRepertoire selectedSpellRepertoire = null;
                        foreach (var spellRepertoire in hero.SpellRepertoires)
                        {
                            if (spellRepertoire.SpellCastingClass != classDefinition)
                            {
                                continue;
                            }

                            var atLeastOneSpellSlotAvailable = false;
                            for (var spellLevel = 1;
                                 spellLevel <= spellRepertoire.MaxSpellLevelOfSpellCastingLevel;
                                 spellLevel++)
                            {
                                spellRepertoire.GetSlotsNumber(spellLevel, out var remaining, out var dummy);
                                if (remaining <= 0)
                                {
                                    continue;
                                }

                                selectedSpellRepertoire = spellRepertoire;
                                atLeastOneSpellSlotAvailable = true;
                                break;
                            }

                            if (!atLeastOneSpellSlotAvailable)
                            {
                                continue;
                            }

                            reactionParams =
                                new CharacterActionParams(attacker, ActionDefinitions.Id.SpendSpellSlot);
                            reactionParams.ActionModifiers.Add(new ActionModifier());
                            yield return instance.PrepareAndReactWithSpellUsingSpellSlot(attacker,
                                selectedSpellRepertoire, provider.NotificationTag, reactionParams);
                            validTrigger = reactionParams.ReactionValidated;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasConditionCreatedByMe:
                    {
                        if (defender.RulesetActor.HasConditionOfTypeAndSource(provider.RequiredTargetCondition,
                                attacker.Guid))
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasCondition:
                    {
                        if (defender == null)
                        {
                            break;
                        }

                        if (provider.RequiredTargetCondition == null)
                        {
                            Trace.LogError(
                                "Provider trigger condition is TargetHasCondition, but no condition given");
                            break;
                        }

                        if (defender.RulesetActor.HasConditionOfType(provider.RequiredTargetCondition.Name))
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetDoesNotHaveCondition:
                    {
                        if (defender == null)
                        {
                            break;
                        }

                        if (provider.RequiredTargetCondition == null)
                        {
                            Trace.LogError(
                                "Provider trigger condition is TargetDoesNotHaveCondition, but no condition given");
                            break;
                        }

                        if (!defender.RulesetActor.HasConditionOfType(provider.RequiredTargetCondition.Name))
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetIsWounded:
                    {
                        if (defender?.RulesetCharacter != null && defender.RulesetCharacter.CurrentHitPoints <
                            defender.RulesetCharacter.GetAttribute(AttributeDefinitions.HitPoints).CurrentValue)
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasSenseType:
                    {
                        if (defender?.RulesetCharacter != null &&
                            defender.RulesetCharacter.HasSenseType(provider.RequiredTargetSenseType))
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.TargetHasCreatureTag:
                    {
                        if (defender?.RulesetCharacter != null &&
                            defender.RulesetCharacter.HasTag(provider.RequiredTargetCreatureTag))
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.RangeAttackFromHigherGround
                        when attackMode != null:
                    {
                        if (defender == null)
                        {
                            break;
                        }

                        if (attacker.LocationPosition.y > defender.LocationPosition.y)
                        {
                            if (itemDefinition != null
                                && itemDefinition.IsWeapon)
                            {
                                var weaponTypeDefinition = DatabaseRepository.GetDatabase<WeaponTypeDefinition>()
                                    .GetElement(itemDefinition.WeaponDescription.WeaponType);
                                if (weaponTypeDefinition.WeaponProximity == RuleDefinitions.AttackProximity.Range)
                                {
                                    validTrigger = true;
                                }
                            }
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpecificCharacterFamily:
                    {
                        if (defender?.RulesetCharacter != null && defender.RulesetCharacter.CharacterFamily ==
                            provider.RequiredCharacterFamily.Name)
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.CriticalHit:
                        validTrigger = criticalHit;
                        break;
                    case RuleDefinitions.AdditionalDamageTriggerCondition.EvocationSpellDamage when firstTarget &&
                        rulesetEffect is RulesetEffectSpell &&
                        (rulesetEffect as RulesetEffectSpell).SpellDefinition.SchoolOfMagic ==
                        RuleDefinitions.SchoolEvocation:
                    case RuleDefinitions.AdditionalDamageTriggerCondition.EvocationSpellDamage when firstTarget &&
                        rulesetEffect is RulesetEffectPower &&
                        (rulesetEffect as RulesetEffectPower).PowerDefinition.SurrogateToSpell != null &&
                        (rulesetEffect as RulesetEffectPower).PowerDefinition.SurrogateToSpell.SchoolOfMagic ==
                        RuleDefinitions.SchoolEvocation:
                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamageMatchesSourceAncestry
                        when firstTarget && rulesetEffect is RulesetEffectSpell &&
                             attacker.RulesetCharacter.HasAncestryMatchingDamageType(actualEffectForms):
                        validTrigger = true;
                        break;

                    case RuleDefinitions.AdditionalDamageTriggerCondition.SpellDamagesTarget
                        when firstTarget && rulesetEffect is RulesetEffectSpell:
                    {
                        // This check is for Warlock / invocation / agonizing blast
                        if (provider.RequiredSpecificSpell == null || provider.RequiredSpecificSpell ==
                            (rulesetEffect as RulesetEffectSpell).SpellDefinition)
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.NotWearingHeavyArmor:
                    {
                        if (attacker.RulesetCharacter != null && !attacker.RulesetCharacter.IsWearingHeavyArmor())
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive:
                        validTrigger = true;
                        break;

                    case RuleDefinitions.AdditionalDamageTriggerCondition.RagingAndTargetIsSpellcaster
                        when defender?.RulesetCharacter != null:
                    {
                        if (attacker.RulesetCharacter.HasConditionOfType(RuleDefinitions.ConditionRaging) &&
                            defender.RulesetCharacter.SpellRepertoires.Count > 0)
                        {
                            validTrigger = true;
                        }

                        break;
                    }

                    case RuleDefinitions.AdditionalDamageTriggerCondition.Raging:
                    {
                        if (attacker.RulesetCharacter.HasConditionOfType(RuleDefinitions.ConditionRaging))
                        {
                            validTrigger = true;
                        }

                        break;
                    }
                }
            }

            /*
             * ######################################
             * [CE] EDIT START
             * Support for extra types of Smite (like eldritch smite)
             */

            //Commented-out original code. Actual check moved up, to make sure Reaction popups (like SpendSpellSlot) won't be shown if context is not valid.
            // // Check required properties for physical attacks if needed
            // IRulesetImplementationService rulesetImplementationService = ServiceRepository.GetService<IRulesetImplementationService>();
            //
            // bool validProperty = true;
            // if (attackMode != null && validTrigger && provider.RequiredProperty != RuleDefinitions.RestrictedContextRequiredProperty.None)
            // {
            //     validProperty = rulesetImplementationService.IsValidContextForRestrictedContextProvider(provider, attacker.RulesetCharacter, itemDefinition, rangedAttack, attackMode, rulesetEffect);
            // }

            /*
            * Support for extra types of Smite (like eldritch smite)
            * [CE] EDIT END
            * ######################################
            */

            if (validTrigger && validProperty)
            {
                instance.ComputeAndNotifyAdditionalDamage(attacker, defender, provider, actualEffectForms,
                    reactionParams, attackMode, criticalHit);
                instance.triggeredAdditionalDamageTags.Add(provider.NotificationTag);
            }
        }
    }
}
