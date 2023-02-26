using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static BattleConstants;

public static class AttackActionState {

    public static IEnumerator<StateChange> Run(Main main, UnitAction action) {

        var attacker = action.unit;
        var target = action.targetUnit;

        if (!TryGetDamage(attacker, target, action.weaponName, out var damagePercentageToTarget))
            throw new Exception();

        var newTargetHp = Mathf.RoundToInt(Mathf.Max(0, target.Hp - damagePercentageToTarget * MaxHp(target)));
        var newAttackerHp = attacker.Hp;

        float maxResponseDamagePercentage = 0;
        foreach (var weaponName in GetWeaponNames(target)) { }
        /*if (newTargetHp > 0 && Rules.CanAttackInResponse(target, attacker, out targetWeaponIndex)) {
            if (Rules.Damage(target, attacker, targetWeaponIndex, newTargetHp) is not { } damageToAttacker)
                throw new Exception();
            newAttackerHp = Mathf.Max(0, newAttackerHp - damageToAttacker);
        }*/

        if (main.persistentData.gameSettings.showBattleAnimation) {

            var leftPrefab = attacker.view.GetComponent<BattleAnimationPlayer>();
            var rightPrefab = target.view.GetComponent<BattleAnimationPlayer>();

            Assert.IsTrue(leftPrefab);
            Assert.IsTrue(rightPrefab);

            var setup = new Battle.Setup {
                left = new Battle.Setup.Side {
                    unitViewPrefab = leftPrefab,
                    count = new Vector2Int(attacker.Hp, newAttackerHp).Apply(Battle.Setup.Side.Count),
                    color = attacker.Player.Color,
                },
                right = new Battle.Setup.Side {
                    unitViewPrefab = rightPrefab,
                    count = new Vector2Int(target.Hp, newTargetHp).Apply(Battle.Setup.Side.Count),
                    color = target.Player.Color,
                }
            };
            var battleViews = new BattleView2[2];

            using (var battle = new Battle(setup))
            using (battleViews[left] = new BattleView2(left, main.tiles[action.Path[^1]]))
            using (battleViews[right] = new BattleView2(right, main.tiles[action.targetUnit.NonNullPosition])) {

                battleViews[left].Arrange(battle.units[left]);
                battleViews[right].Arrange(battle.units[right]);

                main.mainCamera.gameObject.SetActive(false);
                main.battleCameras[left].gameObject.SetActive(true);
                main.battleCameras[right].gameObject.SetActive(true);

                var attackAnimations = new List<BattleAnimation>();
                foreach (var unit in battle.units[left]) {
                    var animation = new BattleAnimation(unit);
                    attackAnimations.Add(animation);
                    animation.Play(action.Path.Count > 1 ? unit.moveAttack : unit.attack, battle.GetTargets(unit));
                }

                while (attackAnimations.Any(aa => !aa.Completed))
                    yield return StateChange.none;

                while (!Input.GetKeyDown(KeyCode.Space))
                    yield return StateChange.none;

                var responseAnimations = new List<BattleAnimation>();
                foreach (var unit in battle.units[right].Where(u => u && u.survives)) {
                    var animation = new BattleAnimation(unit);
                    responseAnimations.Add(animation);
                    animation.Play(unit.respond, battle.GetTargets(unit));
                }

                while (responseAnimations.Any(aa => !aa.Completed))
                    yield return StateChange.none;

                while (!Input.GetKeyDown(KeyCode.Space))
                    yield return StateChange.none;

                main.mainCamera.gameObject.SetActive(true);
                main.battleCameras[left].gameObject.SetActive(false);
                main.battleCameras[right].gameObject.SetActive(false);
            }
        }

        target.SetHp(newTargetHp, true);

        attacker.SetHp(newAttackerHp, true);
        if (attacker.Hp > 0) {
            attacker.Position = action.Path.Last();
            attacker.SetAmmo(action.weaponName, attacker.GetAmmo(action.weaponName) - 1);
        }
    }
}