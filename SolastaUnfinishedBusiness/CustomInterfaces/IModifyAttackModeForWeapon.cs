namespace SolastaUnfinishedBusiness.CustomInterfaces;

internal interface IModifyAttackModeForWeapon
{
    void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode, RulesetItem weapon);
}
