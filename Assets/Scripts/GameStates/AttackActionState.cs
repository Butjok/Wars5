using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static BattleConstants;
using static Battle.Setup.Side;

public class AttackActionState : StateMachineState {

    [Command]
    public static float pause = 1.5f;

    public AttackActionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (level, action) = (FindState<LevelSessionState>().level, FindState<ActionSelectionState>().selectedAction);

            Assert.IsNotNull(level);
            Assert.IsNotNull(action);

            var attacker = action.unit;
            var target = action.targetUnit;

            if (!TryGetDamage(attacker, target, action.weaponName, out var damagePercentageToTarget))
                throw new Exception();

            var newTargetHp = Mathf.RoundToInt(Mathf.Max(0, target.Hp - damagePercentageToTarget * MaxHp(target)));
            var newAttackerHp = attacker.Hp;

            var responseWeapons = GetWeaponNamesForResponseAttack(attacker, action.path[^1], target, target.NonNullPosition)
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

            if (PersistentData.Loaded.gameSettings.showBattleAnimation) {

                var attackerSide = attacker.Player.side;
                var targetSide = target.Player.side;
                if (attackerSide == targetSide) {
                    attackerSide = level.players.IndexOf(attacker.Player) < level.players.IndexOf(target.Player) ? left : right;
                    targetSide = attackerSide == left ? right : left;
                }

                var setup = new Battle.Setup {
                    [attackerSide] = new() {
                        unitViewPrefab = attacker.view.prefab,
                        count = Count(attacker.type, attacker.Hp, newAttackerHp),
                        color = attacker.Player.Color
                    },
                    [targetSide] = new() {
                        unitViewPrefab = target.view.prefab,
                        count = Count(target.type, target.Hp, newTargetHp),
                        color = target.Player.Color
                    }
                };
                var battleViews = new BattleView2[2];

                using (var battle = new Battle(setup))
                using (battleViews[attackerSide] = new BattleView2(attackerSide, level.tiles[action.path[^1]]))
                using (battleViews[targetSide] = new BattleView2(targetSide, level.tiles[action.targetUnit.NonNullPosition])) {

                    battleViews[left].Arrange(battle.units[left]);
                    battleViews[right].Arrange(battle.units[right]);

                    level.view.cameraRig.camera.gameObject.SetActive(false);
                    level.view.battleCameras[left].gameObject.SetActive(true);
                    level.view.battleCameras[right].gameObject.SetActive(true);

                    var attackAnimations = new List<Func<bool>>();
                    foreach (var unit in battle.units[attackerSide])
                        attackAnimations.Add(action.path.Count > 1 ? unit.MoveAttack(action.weaponName) : unit.Attack(action.weaponName));

                    while (attackAnimations.Any(aa => !aa()))
                        yield return StateChange.none;

                    if (respond) {
                        var responseAnimations = new List<Func<bool>>();
                        foreach (var unit in battle.units[targetSide].Where(u => u.survives))
                            responseAnimations.Add(unit.Respond(responseWeaponName));

                        while (responseAnimations.Any(ra => !ra()))
                            yield return StateChange.none;
                    }

                    var time = Time.time;
                    while (Time.time < time + pause && !Input.anyKeyDown)
                        yield return StateChange.none;
                    yield return StateChange.none;

                    level.view.cameraRig.camera.gameObject.SetActive(true);
                    level.view.battleCameras[left].gameObject.SetActive(false);
                    level.view.battleCameras[right].gameObject.SetActive(false);
                }
            }

            target.SetHp(newTargetHp, true);

            attacker.SetHp(newAttackerHp, true);
            if (attacker.Hp > 0) {
                attacker.Position = action.path.Last();
                attacker.SetAmmo(action.weaponName, attacker.GetAmmo(action.weaponName) - 1);
            }
        }
    }
}