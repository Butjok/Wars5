using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using static Rules;
using static BattleConstants;
using static BattleView.Setup.SideSettings;

public class AttackActionState : StateMachineState {

    [Command]
    public static float pause = 1.5f;

    public AttackActionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (level, action) = (stateMachine.Find<LevelSessionState>().level, stateMachine.Find<ActionSelectionState>().selectedAction);

            Assert.IsNotNull(level);
            Assert.IsNotNull(action);

            var attacker = action.unit;
            var target = action.targetUnit;

            if (!TryGetDamage(attacker, target, action.weaponName, out var damagePercentageToTarget))
                throw new Exception();

            var attackerSide = attacker.Player.side;
            var targetSide = target.Player.side;
            if (attackerSide == targetSide) {
                attackerSide = level.players.IndexOf(attacker.Player) < level.players.IndexOf(target.Player) ? Side.Left : Side.Right;
                targetSide = attackerSide == Side.Left ? Side.Right : Side.Left;
            }

            var newTargetHp = Mathf.RoundToInt(Mathf.Max(0, target.Hp - damagePercentageToTarget * MaxHp(target)));
            var newAttackerHp = attacker.Hp;

            WeaponName? responseWeaponName = default;
            if (newTargetHp > 0) {
                var responseWeapons = GetWeaponNamesForResponseAttack(attacker, action.path[^1], target, target.NonNullPosition)
                    .Select(weaponName => (
                        weaponName,
                        damagePercentage: TryGetDamage(target, attacker, weaponName, out var damagePercentage) ? damagePercentage : -1))
                    .Where(t => t.damagePercentage > -1)
                    .ToList();
                if (responseWeapons.Count > 0) {
                    var maxDamagePercentage = responseWeapons.Max(t => t.damagePercentage);
                    var bestChoice = responseWeapons
                        .Where(t => Mathf.Approximately(maxDamagePercentage, t.damagePercentage))
                        .OrderByDescending(t => MaxAmmo(target, t.weaponName))
                        .First();
                    responseWeaponName = bestChoice.weaponName;
                }
            }

            var setup = new BattleView.Setup {
                [attackerSide] = new() {
                    side = attackerSide,
                    unitViewPrefab = attacker.view.prefab,
                    count = CountBeforeAndAfter(attacker.type, attacker.Hp, newAttackerHp),
                    color = attacker.Player.Color,
                    weaponName = action.weaponName
                },
                [targetSide] = new() {
                    side = targetSide,
                    unitViewPrefab = target.view.prefab,
                    count = CountBeforeAndAfter(target.type, target.Hp, newTargetHp),
                    color = target.Player.Color,
                    weaponName = responseWeaponName
                }
            };
            setup.attacker = setup[attackerSide];
            setup.target = setup[targetSide];

            var battleViewSides = new Dictionary<Side, BattleViewSide>();
            using (var battleView = new BattleView(setup))
            using (battleViewSides[attackerSide] = new BattleViewSide(attackerSide, battleView, level.tiles[action.path[^1]]))
            using (battleViewSides[targetSide] = new BattleViewSide(targetSide, battleView, level.tiles[action.targetUnit.NonNullPosition])) {
                level.view.cameraRig.camera.gameObject.SetActive(false);
                foreach (var battleCamera in level.view.battleCameras)
                    battleCamera.gameObject.SetActive(true);

                if (level.view.unitUiRoot)
                    level.view.unitUiRoot.gameObject.SetActive(false);

                var attackAnimations = new List<Func<bool>>();
                foreach (var unit in battleView.unitViews[attackerSide])
                    attackAnimations.Add(action.path.Count > 1 ? unit.MoveAttack(action.weaponName) : unit.Attack(action.weaponName));

                while (attackAnimations.Any(attackAnimation => !attackAnimation()))
                    yield return StateChange.none;

                if (responseWeaponName is { } actualResponseWeaponName) {
                    var responseAnimations = new List<Func<bool>>();
                    foreach (var unit in battleView.unitViews[targetSide].Where(u => u.survives))
                        responseAnimations.Add(unit.Respond(actualResponseWeaponName));

                    while (responseAnimations.Any(responseAnimation => !responseAnimation()))
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

            // a short pause before one of the units die
            if (newTargetHp <= 0 || newAttackerHp <= 0) {
                var time = Time.time;
                while (Time.time < time + .25f)
                    yield return StateChange.none;
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