namespace SolastaUnfinishedBusiness.CustomInterfaces;

internal interface IModifyAttackAttributeForWeapon
{
    void ModifyAttribute(RulesetCharacter character, RulesetAttackMode attackMode, RulesetItem weapon);
}
