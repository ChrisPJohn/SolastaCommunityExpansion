﻿using SolastaUnfinishedBusiness.Api.Extensions;

namespace SolastaUnfinishedBusiness.CustomUI;

public class CustomGuiCharacterAction : GuiCharacterAction
{
    private readonly int attackModesToSkip;

    public CustomGuiCharacterAction(ActionDefinitions.Id actionId, int attackModesToSkip) : base(actionId)
    {
        this.attackModesToSkip = attackModesToSkip;
    }

    public void ApplySkip()
    {
        ActingCharacter.SetSkipAttackModes(attackModesToSkip);
    }

    public void RemoveSkip()
    {
        ActingCharacter.RemoveSkipAttackModes();
    }
}
