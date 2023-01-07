using System;
using System.Collections;
using UnityEngine;

public static class BattleAnimationState {

    public static IEnumerator Run(UnitAction action, bool skipAnimation = false) {

        var attacker = action.unit;
        var target = action.targetUnit;

        if (Rules.Damage(attacker, target, action.weaponIndex) is not { } damageToTarget)
            throw new Exception();

        var newTargetHp = Mathf.Max(0, target.hp.v - damageToTarget);
        var newAttackerHp = attacker.hp.v;
        var targetWeaponIndex = -1;
        if (newTargetHp > 0 && Rules.CanAttackInResponse(target, attacker, out targetWeaponIndex)) {
            if (Rules.Damage(target, attacker, targetWeaponIndex, newTargetHp) is not { } damageToAttacker)
                throw new Exception();
            newAttackerHp = Mathf.Max(0, newAttackerHp - damageToAttacker);
        }

        if (!skipAnimation)
            Debug.Log("BattleAnimationView");

        attacker.position.v = action.path.Destination;

        if (newTargetHp <= 0) {
            var animation = CameraRig.Instance.Jump(target.view.transform.position.ToVector2());
            while (animation.active)
                yield return null;
        }
        target.hp.v = newTargetHp;

        if (newTargetHp > 0 && targetWeaponIndex != -1)
            target.ammo[targetWeaponIndex]--;

        if (newAttackerHp <= 0) {
            var animation = CameraRig.Instance.Jump(attacker.view.transform.position.ToVector2());
            while (animation.active)
                yield return null;
        }
        attacker.hp.v = newAttackerHp;

        if (newAttackerHp > 0)
            attacker.ammo[action.weaponIndex]--;
    }
}