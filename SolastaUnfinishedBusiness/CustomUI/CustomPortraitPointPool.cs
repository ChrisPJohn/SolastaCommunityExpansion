﻿using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SolastaUnfinishedBusiness.CustomUI;

public interface ICusomPortraitPointPoolProvider
{
    string Name { get; }
    string Tooltip { get; }
    AssetReferenceSprite Icon { get; }
    int GetPoints(RulesetCharacter character);
}

public class CustomPortraitPoolPower : ICusomPortraitPointPoolProvider
{
    private readonly FeatureDefinitionPower power;


    public CustomPortraitPoolPower(FeatureDefinitionPower power, string name = null, string tooltip = null,
        AssetReferenceSprite icon = null)
    {
        this.power = power;
        Name = name ?? power.Name;
        Tooltip = tooltip ?? $"Tooltip/&CustomPortraitPool{Name}";
        Icon = icon ?? power.GuiPresentation.SpriteReference;
    }

    public string Name { get; }
    public string Tooltip { get; }
    public AssetReferenceSprite Icon { get; }

    public int GetPoints(RulesetCharacter character)
    {
        return character.GetRemainingPowerUses(power);
    }
}

public class CustomPortraitPointPool : MonoBehaviour
{
    public static CustomPortraitPointPool Setup(ICusomPortraitPointPoolProvider provider, RulesetCharacter character,
        GameObject prefab, Transform parent)
    {
        CustomPortraitPointPool pool;

        var name = $"CustomPool({provider.Name})";
        var child = parent.Find(name);

        if (child != null)
        {
            pool = child.GetComponent<CustomPortraitPointPool>();
        }
        else
        {
            var obj = Instantiate(prefab, parent, false);
            obj.name = name;
            pool = obj.AddComponent<CustomPortraitPointPool>();

            pool.Setup(provider, character);
        }

        pool.UpdateState(provider, character);

        return pool;
    }

    private void Setup(ICusomPortraitPointPoolProvider provider, RulesetCharacter character)
    {
        var image = transform.Find("SorceryPointsImage").GetComponent<Image>();

        if (image == null)
        {
            return;
        }

        image.sprite = null;
        image.SetupSprite(provider.Icon);
    }

    private void UpdateState(ICusomPortraitPointPoolProvider provider, RulesetCharacter character)
    {
        gameObject.SetActive(true); //Do we need ability to set to inactive on update?

        var label = transform.Find("SorceyPointsLabel")?.GetComponent<GuiLabel>();
        if (label != null)
        {
            label.Text = $"{provider.GetPoints(character)}";
        }

        var tooltip = GetComponent<GuiTooltip>();
        if (tooltip != null)
        {
            tooltip.Content = provider.Tooltip;
        }
    }
}
