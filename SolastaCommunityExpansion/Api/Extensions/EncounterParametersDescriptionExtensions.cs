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
    [TargetType(typeof(EncounterParametersDescription)), GeneratedCode("Community Expansion Extension Generator", "1.0.0")]
    public static partial class EncounterParametersDescriptionExtensions
    {
        public static T SetEncounterDefinition<T>(this T entity, EncounterDefinition value)
            where T : EncounterParametersDescription
        {
            entity.EncounterDefinition = value;
            return entity;
        }

        public static T SetLocationDefinition<T>(this T entity, LocationDefinition value)
            where T : EncounterParametersDescription
        {
            entity.LocationDefinition = value;
            return entity;
        }

        public static T SetPartyNavigating<T>(this T entity, System.Boolean value)
            where T : EncounterParametersDescription
        {
            entity.PartyNavigating = value;
            return entity;
        }

        public static T SetSurpriseOutcome<T>(this T entity, EncounterDefinitions.SurpriseOutcome value)
            where T : EncounterParametersDescription
        {
            entity.SurpriseOutcome = value;
            return entity;
        }
    }
}