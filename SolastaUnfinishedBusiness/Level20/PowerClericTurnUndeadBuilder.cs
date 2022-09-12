﻿using SolastaUnfinishedBusiness.Builders.Features;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Level20;

internal sealed class PowerClericTurnUndeadBuilder : FeatureDefinitionPowerBuilder
{
    private const string PowerClericTurnUndead17Name = "PowerClericTurnUndead17";
    private const string PowerClericTurnUndead17Guid = "b0ef65ba1e784628b1c5b4af75d4f395";

    internal static readonly FeatureDefinitionPower PowerClericTurnUndead17 =
        CreateAndAddToDB(PowerClericTurnUndead17Name, PowerClericTurnUndead17Guid, 4);

    private PowerClericTurnUndeadBuilder(string name, string guid, int challengeRating) : base(
        PowerClericTurnUndead8, name, guid)
    {
        Definition.EffectDescription.EffectForms[0].KillForm.challengeRating = challengeRating;
    }

    private static FeatureDefinitionPower CreateAndAddToDB(string name, string guid, int challengeRating)
    {
        return new PowerClericTurnUndeadBuilder(name, guid, challengeRating).AddToDB();
    }
}
