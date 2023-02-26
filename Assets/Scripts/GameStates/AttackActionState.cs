using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static BattleConstants;
using static Battle.Setup.Side;

public static class AttackActionState {

    [Command]
    public static float pause = 2f;

    public static IEnumerator<StateChange> Run(Main main, UnitAction action) {

        var attacker = action.unit;
        var target = action.targetUnit;

        if (!TryGetDamage(attacker, target, action.weaponName, out var damagePercentageToTarget))
            throw new Exception();

        var newTargetHp = Mathf.RoundToInt(Mathf.Max(0, target.Hp - damagePercentageToTarget * MaxHp(target)));
        var newAttackerHp = attacker.Hp;

        var responseWeapons = GetWeaponNamesForResponseAttack(attacker, action.Path[^1], target, target.NonNullPosition)
            .Select(weaponName => (
                weaponName,
                damagePercentage: TryGetDamage(target, attacker, weaponName, out var damagePercentage) ? damagePercentage : -1))
            .Where(t => t.damagePercentage > -1)
            .ToList();

        var respond = responseWeapons.Count > 0;
        WeaponName responseWeaponName = default;
        if (respond) {
            var maxDamagePercentage = responseWeapons.Max(t => t.damagePercentage);
            var bestChoice = responseWeapons
                .Where(t => Mathf.Approximately(maxDamagePercentage, t.damagePercentage))
                .OrderByDescending(t => MaxAmmo(target, t.weaponName))
                .First();
            responseWeaponName = bestChoice.weaponName;
        }

        if (main.persistentData.gameSettings.showBattleAnimation) {

            var attackerPrefab = attacker.view.GetComponent<BattleAnimationPlayer>();
            var targetPrefab = target.view.GetComponent<BattleAnimationPlayer>();

            Assert.IsTrue(attackerPrefab);
            Assert.IsTrue(targetPrefab);

            var attackerSide = attacker.Player.side;
            var targetSide = target.Player.side;
            if (attackerSide == targetSide) {
                attackerSide = left;
                targetSide = right;
            }

            var setup = new Battle.Setup {
                [attackerSide] = new() {
                    unitViewPrefab = attackerPrefab,
                    count = Count(attacker.type, attacker.Hp, newAttackerHp),
                    color = attacker.Player.Color,
                },
                [targetSide] = new() {
                    unitViewPrefab = targetPrefab,
                    count = Count(target.type, target.Hp, newTargetHp),
                    color = target.Player.Color,
                }
            };
            var battleViews = new BattleView2[2];

            using (var battle = new Battle(setup))
            using (battleViews[attackerSide] = new BattleView2(attackerSide, main.tiles[action.Path[^1]]))
            using (battleViews[targetSide] = new BattleView2(targetSide, main.tiles[action.targetUnit.NonNullPosition])) {

                battleViews[left].Arrange(battle.units[left]);
                battleViews[right].Arrange(battle.units[right]);

                main.mainCamera.gameObject.SetActive(false);
                main.battleCameras[left].gameObject.SetActive(true);
                main.battleCameras[right].gameObject.SetActive(true);

                var attackAnimations = new List<BattleAnimation>();
                foreach (var unit in battle.units[attackerSide]) {
                    var animation = new BattleAnimation(unit);
                    attackAnimations.Add(animation);
                    var found = unit.inputs.TryGetValue(action.weaponName, out var inputs);
                    Assert.IsTrue(found, $"{unit}: cannot find battle animation input for weapon {action.weaponName}");
                    animation.Play(action.Path.Count > 1 ? inputs.moveAttack : inputs.attack, battle.GetTargets(unit));
                }

                while (attackAnimations.Any(aa => !aa.Completed))
                    yield return StateChange.none;

                if (respond) {

                    var responseAnimations = new List<BattleAnimation>();
                    foreach (var unit in battle.units[targetSide].Where(u => u.survives)) {
                        var animation = new BattleAnimation(unit);
                        responseAnimations.Add(animation);
                        var found = unit.inputs.TryGetValue(responseWeaponName, out var inputs);
                        Assert.IsTrue(found, $"{unit}: cannot find battle animation input for weapon {responseWeaponName}");
                        animation.Play(inputs.respond, battle.GetTargets(unit));
                    }

                    while (responseAnimations.Any(ra => !ra.Completed))
                        yield return StateChange.none;

                }

                var time = Time.time;
                while (Time.time < time + pause && !Input.anyKeyDown)
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