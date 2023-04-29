namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IModifyWeaponAttackAttribute
{
    void ModifyAttribute(
        RulesetCharacter character,
        RulesetAttackMode attackMode,
        RulesetItem weapon,
        bool canAddAbilityDamageBonus);
}
