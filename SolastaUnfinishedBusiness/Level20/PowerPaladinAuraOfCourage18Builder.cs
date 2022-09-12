﻿using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders.Features;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Level20;

internal sealed class PowerPaladinAuraOfCourage18Builder : FeatureDefinitionPowerBuilder
{
    private const string PowerPaladinAuraOfCourage18Name = "PowerPaladinAuraOfCourage18";
    private const string PowerPaladinAuraOfCourage18Guid = "d68c46024ea8432b981506a13ae35ecd";
    private static FeatureDefinitionPower _instance;

    private PowerPaladinAuraOfCourage18Builder() : base(PowerPaladinAuraOfCourage, PowerPaladinAuraOfCourage18Name,
        PowerPaladinAuraOfCourage18Guid)
    {
        var ed = Definition.EffectDescription;

        ed.SetTargetParameter(6);
        ed.SetRangeParameter(0);
        ed.SetRequiresTargetProximity(false);

        Definition.overriddenPower = PowerPaladinAuraOfCourage;
        Definition.GuiPresentation.Description = "Feature/&PowerPaladinAuraOfCourage18Description";
        Definition.GuiPresentation.Title = "Feature/&PowerPaladinAuraOfCourage18Title";
    }

    internal static FeatureDefinitionPower Instance =>
        _instance ??= new PowerPaladinAuraOfCourage18Builder().AddToDB();
}
