﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Resources = SolastaUnfinishedBusiness.Properties.Resources;

namespace SolastaUnfinishedBusiness.CustomUI;

public static class Sprites
{
    #region Map Icons

    internal static AssetReferenceSprite Teleport =>
        GetSprite("Teleport", Resources.Teleport, 24);

    #endregion

    #region Misc

    internal static AssetReferenceSprite GambitResourceIcon =>
        GetSprite("GambitResourceIcon", Resources.GambitResourceIcon, 64);

    internal static AssetReferenceSprite GenericResourceIcon =>
        GetSprite("GenericResourceIcon", Resources.GenericResourceIcon, 64);

    internal static AssetReferenceSprite ChannelDivinityResourceIcon =>
        GetSprite("ChannelDivinityResourceIcon", Resources.ChannelDivinityResourceIcon, 64);

    #endregion

    #region UI

    private static AssetReferenceSprite CheckOn =>
        GetSprite("CheckOn", Resources.checkOn, 12);

    internal static Texture CheckOnTexture => GetSpriteByGuid(CheckOn.AssetGUID)?.texture;

    private static AssetReferenceSprite CheckOff =>
        GetSprite("CheckOff", Resources.checkOff, 12);

    internal static Texture CheckOffTexture => GetSpriteByGuid(CheckOff.AssetGUID)?.texture;

    private static AssetReferenceSprite Expanded =>
        GetSprite("Expanded", Resources.expanded, 12);

    internal static Texture ExpandedTexture => GetSpriteByGuid(Expanded.AssetGUID)?.texture;

    private static AssetReferenceSprite Collapsed =>
        GetSprite("Collapsed", Resources.collapsed, 12);

    internal static Texture CollapsedTexture => GetSpriteByGuid(Collapsed.AssetGUID)?.texture;

    #endregion

    #region CustomSprites

    private static readonly Dictionary<string, Sprite> SpritesByGuid = new();

    /// <summary>
    ///     Convert a bitmap stored as an embedded resource to a Sprite.
    /// </summary>
    [NotNull]
    internal static Sprite GetOrCreateSprite(
        string name,
        Byte[] bitmap,
        int size)
    {
        return GetOrCreateSprite(name, bitmap, size, size);
    }

    [NotNull]
    private static Sprite GetOrCreateSprite(
        string name,
        Byte[] bitmap,
        int sizeX,
        int sizeY)
    {
        var (id, guid) = GetSpriteId(name, sizeX, sizeY);

        if (SpritesByGuid.TryGetValue(guid, out var sprite))
        {
            return sprite;
        }

        var texture = new Texture2D(sizeX, sizeY, TextureFormat.DXT5, false);

        texture.LoadImage(bitmap);
        sprite = Sprite.Create(texture, new Rect(0, 0, sizeX, sizeY), new Vector2(0, 0));

        SpritesByGuid[guid] = sprite;
        sprite.name = id;

        return sprite;
    }

    [CanBeNull]
    internal static Sprite GetSpriteByGuid([NotNull] string guid)
    {
        return SpritesByGuid.TryGetValue(guid, out var sprite) ? sprite : null;
    }

    /// <summary>
    ///     Create a unique Id to serve as id of a sprite in our internal cache and as the guid for AssetReference
    /// </summary>
    /// <param name="name"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static (string id, string guid) GetSpriteId(string name, int x, int y)
    {
        var id = $"_CE_{name}[{x},{y}]";

        return (id, GetSpriteGuid(id));
    }

    [NotNull]
    private static string GetSpriteGuid([NotNull] string id)
    {
        return GuidHelper.Create(DefinitionBuilder.CeNamespaceGuid, id).ToString();
    }

    [NotNull]
    internal static AssetReferenceSprite GetSprite(
        string name,
        Byte[] bitmap,
        int size)
    {
        return GetSprite(name, bitmap, size, size);
    }

    [NotNull]
    internal static AssetReferenceSprite GetSprite(
        string name,
        Byte[] bitmap,
        int sizeX,
        int sizeY)
    {
        var sprite = GetOrCreateSprite(name, bitmap, sizeX, sizeY);

        return new AssetReferenceSprite(GetSpriteGuid(sprite.name));
    }

    #endregion

    #region Conditions

    internal static AssetReferenceSprite ConditionGuardian =>
        GetSprite("ConditionGuardian", Resources.ConditionGuardian, 32);

    internal static AssetReferenceSprite ConditionInfiltrate =>
        GetSprite("ConditionInfiltrate", Resources.ConditionInfiltrate, 32);

    internal static AssetReferenceSprite ConditionGambit =>
        GetSprite("ConditionGambit", Resources.ConditionGambit, 32);

    internal static AssetReferenceSprite ConditionTacticalSurge =>
        GetSprite("ConditionTacticalSurge", Resources.ConditionTacticalSurge, 32);

    #endregion

    #region Spells

    internal static AssetReferenceSprite FarStep =>
        GetSprite("FarStep", Resources.FarStep, 128);

    internal static AssetReferenceSprite SpellRaiseSkeleton =>
        GetSprite("SpellRaiseSkeleton", Resources.SpellRaiseSkeleton, 128);

    internal static AssetReferenceSprite SpellRaiseSkeletonArcher =>
        GetSprite("SpellRaiseSkeletonArcher", Resources.SpellRaiseSkeletonArcher, 128);

    internal static AssetReferenceSprite SpellRaiseGhoul =>
        GetSprite("SpellRaiseGhoul", Resources.SpellRaiseGhoul, 128);

    internal static AssetReferenceSprite SpellRaiseSkeletonEnforcer =>
        GetSprite("SpellRaiseSkeletonEnforcer", Resources.SpellRaiseSkeletonEnforcer, 128);

    internal static AssetReferenceSprite SpellRaiseSkeletonKnight =>
        GetSprite("SpellRaiseSkeletonKnight", Resources.SpellRaiseSkeletonKnight, 128);

    internal static AssetReferenceSprite SpellRaiseSkeletonMarksman =>
        GetSprite("SpellRaiseSkeletonMarksman", Resources.SpellRaiseSkeletonMarksman, 128);

    internal static AssetReferenceSprite SpellRaiseGhost =>
        GetSprite("SpellRaiseGhost", Resources.SpellRaiseGhost, 128);

    internal static AssetReferenceSprite SpellRaiseWight =>
        GetSprite("SpellRaiseWight", Resources.SpellRaiseWight, 128);

    internal static AssetReferenceSprite SpellRaiseWightLord =>
        GetSprite("SpellRaiseWightLord", Resources.SpellRaiseWightLord, 128);

    internal static AssetReferenceSprite FeatTelekinetic =>
        GetSprite("FeatTelekinetic", Resources.FeatTelekinetic, 128);

    #endregion

    #region Powers

    internal static AssetReferenceSprite PowerFarStep =>
        GetSprite("PowerFarStep", Resources.PowerFarStep, 256, 128);

    internal static AssetReferenceSprite PowerGuardianMode =>
        GetSprite("PowerGuardianMode", Resources.PowerGuardianMode, 256, 128);

    internal static AssetReferenceSprite PowerInfiltratorMode =>
        GetSprite("PowerInfiltratorMode", Resources.PowerInfiltratorMode, 256, 128);

    internal static AssetReferenceSprite PowerDefensiveField =>
        GetSprite("PowerDefensiveField", Resources.PowerDefensiveField, 256, 128);

    #endregion

    #region Items

    internal static AssetReferenceSprite ItemThunderGauntlet =>
        GetSprite("ItemThunderGauntlet", Resources.ItemThunderGauntlet, 128);

    internal static AssetReferenceSprite ItemGemLightning =>
        GetSprite("ItemGemLightning", Resources.ItemGemLightning, 128);

    #endregion

    #region Actions

    internal static AssetReferenceSprite ActionPlaneMagic =>
        GetSprite("ActionPlaneMagic", Resources.ActionPlaneMagic, 80);

    internal static AssetReferenceSprite ActionInfuse =>
        GetSprite("ActionInfuse", Resources.ActionInfuse, 80);

    internal static AssetReferenceSprite ActionGambit =>
        GetSprite("ActionGambit", Resources.ActionGambit, 80);

    #endregion
}
