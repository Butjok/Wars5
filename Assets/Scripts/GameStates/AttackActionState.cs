using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static BattleConstants;
using static BattleView.Settings.SideSettings;

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

            if (PersistentData.Read().gameSettings.showBattleAnimation) {

                var attackerSide = attacker.Player.side;
                var targetSide = target.Player.side;
                if (attackerSide == targetSide) {
                    attackerSide = level.players.IndexOf(attacker.Player) < level.players.IndexOf(target.Player) ? Side.Left : Side.Right;
                    targetSide = attackerSide == Side.Left ? Side.Right : Side.Left;
                }

                var setup = new BattleView.Settings {
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
                var sides = new Dictionary<Side, BattleSideView>();

                using (var battle = new BattleView(setup))
                using (sides[attackerSide] = new BattleSideView(attackerSide, level.tiles[action.path[^1]]))
                using (sides[targetSide] = new BattleSideView(targetSide, level.tiles[action.targetUnit.NonNullPosition])) {

                    sides[Side.Left].Arrange(battle.units[Side.Left]);
                    sides[Side.Right].Arrange(battle.units[Side.Right]);

                    yield return StateChange.none;

                    level.view.cameraRig.camera.gameObject.SetActive(false);
                    foreach (var battleCamera in level.view.battleCameras)
                        battleCamera.gameObject.SetActive(true);
                    
                    if (level.view.unitUiRoot)
                        level.view.unitUiRoot.gameObject.SetActive(false);

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
                    foreach (var battleCamera in level.view.battleCameras)
                        battleCamera.gameObject.SetActive(false);
                    
                    if (level.view.unitUiRoot)
                        level.view.unitUiRoot.gameObject.SetActive(true);
                }
            }

            if (newTargetHp <= 0 && level.CurrentPlayer != level.localPlayer) {
                level.view.cameraRig.Jump(target.view.transform.position);
                var time = Time.time;
                while (Time.time < time + level.view.cameraRig.jumpDuration)
                    yield return StateChange.none;
            }
            target.SetHp(newTargetHp, true);

            if (attacker.Hp <= 0 && level.CurrentPlayer != level.localPlayer) {
                level.view.cameraRig.Jump(attacker.view.transform.position);
                var time = Time.time;
                while (Time.time < time + level.view.cameraRig.jumpDuration)
                    yield return StateChange.none;
            }
            else {
                attacker.Position = action.path.Last();
                attacker.SetAmmo(action.weaponName, attacker.GetAmmo(action.weaponName) - 1);
            }
            attacker.SetHp(newAttackerHp, true);
        }
    }
}