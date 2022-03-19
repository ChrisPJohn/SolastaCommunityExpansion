using SolastaModApi.Infrastructure;
using AK.Wwise;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using TA.AI;
using TA;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using  static  ActionDefinitions ;
using  static  TA . AI . DecisionPackageDefinition ;
using  static  TA . AI . DecisionDefinition ;
using  static  RuleDefinitions ;
using  static  BanterDefinitions ;
using  static  Gui ;
using  static  GadgetDefinitions ;
using  static  BestiaryDefinitions ;
using  static  CursorDefinitions ;
using  static  AnimationDefinitions ;
using  static  FeatureDefinitionAutoPreparedSpells ;
using  static  FeatureDefinitionCraftingAffinity ;
using  static  CharacterClassDefinition ;
using  static  CreditsGroupDefinition ;
using  static  SoundbanksDefinition ;
using  static  CampaignDefinition ;
using  static  GraphicsCharacterDefinitions ;
using  static  GameCampaignDefinitions ;
using  static  FeatureDefinitionAbilityCheckAffinity ;
using  static  TooltipDefinitions ;
using  static  BaseBlueprint ;
using  static  MorphotypeElementDefinition ;

namespace SolastaModApi.Extensions
{
    /// <summary>
    /// This helper extensions class was automatically generated.
    /// If you find a problem please report at https://github.com/SolastaMods/SolastaModApi/issues.
    /// </summary>
    [TargetType(typeof(RulesetInventorySlot)), GeneratedCode("Community Expansion Extension Generator", "1.0.0")]
    public static partial class RulesetInventorySlotExtensions
    {
        public static T AddRestrictedItemTags<T>(this T entity,  params  System . String [ ]  value)
            where T : RulesetInventorySlot
        {
            AddRestrictedItemTags(entity, value.AsEnumerable());
            return entity;
        }

        public static T AddRestrictedItemTags<T>(this T entity, IEnumerable<System.String> value)
            where T : RulesetInventorySlot
        {
            entity.RestrictedItemTags.AddRange(value);
            return entity;
        }

        public static T ClearRestrictedItemTags<T>(this T entity)
            where T : RulesetInventorySlot
        {
            entity.RestrictedItemTags.Clear();
            return entity;
        }

        public static T SetConfigSlot<T>(this T entity, System.Boolean value)
            where T : RulesetInventorySlot
        {
            entity.ConfigSlot = value;
            return entity;
        }

        public static T SetDirtyForUI<T>(this T entity, System.Boolean value)
            where T : RulesetInventorySlot
        {
            entity.SetField("<DirtyForUI>k__BackingField", value);
            return entity;
        }

        public static T SetDisabled<T>(this T entity, System.Boolean value)
            where T : RulesetInventorySlot
        {
            entity.Disabled = value;
            return entity;
        }

        public static T SetDisabledReason<T>(this T entity, RulesetInventorySlot.DisableReason value)
            where T : RulesetInventorySlot
        {
            entity.DisabledReason = value;
            return entity;
        }

        public static T SetEquipedItem<T>(this T entity, RulesetItem value)
            where T : RulesetInventorySlot
        {
            entity.SetField("equipedItem", value);
            return entity;
        }

        public static T SetItemEquiped<T>(this T entity, RulesetInventorySlot.ItemEquipedHandler value)
            where T : RulesetInventorySlot
        {
            entity.SetField("<ItemEquiped>k__BackingField", value);
            return entity;
        }

        public static T SetItemReleased<T>(this T entity, RulesetInventorySlot.ItemReleasedHandler value)
            where T : RulesetInventorySlot
        {
            entity.SetField("<ItemReleased>k__BackingField", value);
            return entity;
        }

        public static T SetItemStacksChanged<T>(this T entity, RulesetInventorySlot.ItemStacksChangedHandler value)
            where T : RulesetInventorySlot
        {
            entity.SetField("<ItemStacksChanged>k__BackingField", value);
            return entity;
        }

        public static T SetItemUnequiped<T>(this T entity, RulesetInventorySlot.ItemUnequipedHandler value)
            where T : RulesetInventorySlot
        {
            entity.SetField("<ItemUnequiped>k__BackingField", value);
            return entity;
        }

        public static T SetName<T>(this T entity, System.String value)
            where T : RulesetInventorySlot
        {
            entity.Name = value;
            return entity;
        }

        public static T SetPersonalSlot<T>(this T entity, System.Boolean value)
            where T : RulesetInventorySlot
        {
            entity.PersonalSlot = value;
            return entity;
        }

        public static T SetProxySlot<T>(this T entity, System.Boolean value)
            where T : RulesetInventorySlot
        {
            entity.ProxySlot = value;
            return entity;
        }

        public static T SetRestrictedItemTags<T>(this T entity,  params  System . String [ ]  value)
            where T : RulesetInventorySlot
        {
            SetRestrictedItemTags(entity, value.AsEnumerable());
            return entity;
        }

        public static T SetRestrictedItemTags<T>(this T entity, IEnumerable<System.String> value)
            where T : RulesetInventorySlot
        {
            entity.RestrictedItemTags.SetRange(value);
            return entity;
        }

        public static T SetShadowedSlot<T>(this T entity, RulesetInventorySlot value)
            where T : RulesetInventorySlot
        {
            entity.ShadowedSlot = value;
            return entity;
        }

        public static T SetSlotTypeDefinition<T>(this T entity, SlotTypeDefinition value)
            where T : RulesetInventorySlot
        {
            entity.SlotTypeDefinition = value;
            return entity;
        }
    }
}