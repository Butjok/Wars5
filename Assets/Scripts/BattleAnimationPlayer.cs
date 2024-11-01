using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class BattleAnimationPlayer : MonoBehaviour {

    [Serializable]
    public struct Record {
        public string name;
        [TextArea(5, 10)] public string input;
    }
    public List<Record> subroutines = new();

    public WeaponNameBattleAnimationInputsDictionary inputs = new();

    public float speed;
    public float acceleration;
    public int spawnPointIndex;
    public int shuffledIndex;
    public bool survives;
    public Transform hitPoint;
    public int incomingRoundsLeft;
    public List<BattleAnimationPlayer> targets;
    
    private List<Transform> hitPoints;
    private List<Transform> barrels;
    private List<ParticleSystem> shotParticleSystems;
    private List<AudioSource> shotAudioSources;
    private AudioSource hitAudioSource;
    private ParticleSystem hitParticleSystem;

    public IReadOnlyList<Transform> HitPoints => hitPoints;
    public IReadOnlyList<Transform> Barrels => barrels;

    private void Awake() {

        hitPoints = transform
            .GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("HitPoint"))
            .ToList();

        barrels = transform
            .GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("Barrel"))
            .ToList();

        shotParticleSystems = transform
            .GetComponentsInChildren<ParticleSystem>()
            .Where(ps => ps.name.StartsWith("ShotParticleSystem"))
            .ToList();

        shotAudioSources = transform
            .GetComponentsInChildren<AudioSource>()
            .Where(t => t.name.StartsWith("ShotAudioSource"))
            .ToList();

        hitAudioSource = transform
            .GetComponentsInChildren<AudioSource>()
            .SingleOrDefault(t => t.name.StartsWith("HitAudioSource"));

        hitParticleSystem = transform
            .GetComponentsInChildren<ParticleSystem>()
            .SingleOrDefault(t => t.name.StartsWith("HitParticleSystem"));

        Assert.AreNotEqual(0, hitPoints.Count, $"{this} has no hit points");
    }

    private void Update() {
        transform.position += transform.forward * speed * Time.deltaTime;
        speed += acceleration * Time.deltaTime;
    }

    public void SetAim(Turret2 turret, bool value) {
        if (!turret)
            return;
        turret.target = value ? hitPoint : null;
    }

    [Command]
    public Projectile2 Fire(int barrelIndex, Projectile2View projectileViewPrefab) {

        Assert.IsTrue(hitPoint, $"{this}: has no target hit point to shoot at");
        Assert.AreNotEqual(0, Barrels.Count, $"{this}: no barrels to shoot from");

        if (shotParticleSystems.Count > 0)
            shotParticleSystems[barrelIndex % shotParticleSystems.Count].Play();

        if (shotAudioSources.Count > 0) {
            var shotAudioSource = shotAudioSources[barrelIndex % shotAudioSources.Count];
            shotAudioSource.PlayOneShot(shotAudioSource.clip);
        }

        foreach (var target in targets)
            target.incomingRoundsLeft++;

        return new Projectile2(projectileViewPrefab, Barrels[barrelIndex % Barrels.Count], hitPoint, targets);
    }

    [Command]
    public void TakeHit(Projectile2 projectile, Transform hitPoint) {

        if (hitAudioSource) {
            if (hitAudioSource.transform != transform)
                hitAudioSource.transform.SetPositionAndRotation(hitPoint.position, hitPoint.rotation);
            hitAudioSource.PlayOneShot(hitAudioSource.clip);
        }

        if (hitParticleSystem) {
            if (hitParticleSystem.transform != transform)
                hitParticleSystem.transform.SetPositionAndRotation(hitPoint.position, hitPoint.rotation);
            hitParticleSystem.Play();
        }

        incomingRoundsLeft--;
        if (incomingRoundsLeft == 0 && !survives) {
            gameObject.SetActive(false);
        }
    }
}

public class BattleAnimation {

    [Command]
    public static bool overrideTranslateArgument;
    [Command]
    public static float translateArgumentOverrideValue;
    [Command]
    public static bool overrideSetSpeedArgument = true;
    [Command]
    public static float setSpeedArgumentOverrideValue = 1.5f;
    [Command]
    public static bool overrideBreakArgument = true;
    [Command]
    public static float breakArgumentOverrideValue = 3;
    
    public readonly BattleAnimationPlayer player;
    public bool Completed { get; private set; }

    private Stack stack = new();

    public BattleAnimation(BattleAnimationPlayer player) {
        this.player = player;
    }

    private IEnumerator Coroutine(string input, int level = 0) {
        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {

                case "set-speed": {
                    var value = (dynamic)stack.Pop();
                    if (overrideSetSpeedArgument)
                        value = setSpeedArgumentOverrideValue;
                    player.speed = value;
                    break;
                }

                case "break": {
                    var value = (dynamic)stack.Pop();
                    if (overrideBreakArgument)
                        value = breakArgumentOverrideValue;
                    player.acceleration = -Mathf.Sign(player.speed) * value;
                    yield return new WaitForSeconds(Mathf.Abs(player.speed / player.acceleration));
                    player.acceleration = 0;
                    player.speed = 0;
                    break;
                }

                case "translate": {
                    var value = (dynamic)stack.Pop();
                    if (overrideTranslateArgument)
                        value = translateArgumentOverrideValue;
                    player.transform.position += player.transform.forward * value;
                    break;
                }

                case "wait":
                    yield return new WaitForSeconds((dynamic)stack.Pop());
                    break;

                case "random": {
                    var b = (dynamic)stack.Pop();
                    var a = (dynamic)stack.Pop();
                    stack.Push(Random.Range(a, b));
                    break;
                }

                case "spawn-point-index":
                    stack.Push(player.spawnPointIndex);
                    break;

                case "shuffled-index":
                    stack.Push(player.shuffledIndex);
                    break;

                case "find-turret": {
                    var turretName = (string)stack.Pop();
                    var turret = player
                        .GetComponentsInChildren<Turret2>()
                        .SingleOrDefault(t => t.name == turretName);
                    stack.Push(turret);
                    break;
                }

                case "set-aim": {
                    var value = (bool)stack.Pop();
                    var turret = (Turret2)stack.Pop();
                    player.SetAim(turret, value);
                    break;
                }

                case "fire": {
                    var barrelIndex = (int)stack.Pop();
                    var projectileViewPrefab = (Projectile2View)stack.Pop();
                    player.Fire(barrelIndex, projectileViewPrefab);
                    break;
                }

                case "call": {
                    var name = (string)stack.Pop();
                    var record = player.subroutines.SingleOrDefault(r => r.name == name);
                    Assert.IsTrue(!string.IsNullOrEmpty(record.name), $"cannot find subroutine {name}");
                    yield return Coroutine(record.input, level + 1);
                    break;
                }

                default:
                    stack.ExecuteToken(token);
                    break;
            }

        if (level == 0)
            Completed = true;
    }

    public void Play(string input, IEnumerable<BattleAnimationPlayer> targets, bool stopAllCoroutines = true) {

        if (stopAllCoroutines)
            player.StopAllCoroutines();

        player.targets = targets.ToList();
        Assert.AreNotEqual(0, player.targets.Count);
        var target = player.targets.Random();
        player.hitPoint = target.HitPoints.Count == 0 ? null : target.HitPoints.Random();
        Assert.IsTrue(player.hitPoint, $"{this}: target {target} has no impact points");

        player.StartCoroutine(Coroutine(input));
    }
}

public class Projectile2 : IDisposable {

    private Projectile2View view;
    private List<BattleAnimationPlayer> targets = new();
    public IReadOnlyList<BattleAnimationPlayer> Targets => targets;
    public readonly Transform hitPoint;

    public Projectile2(Projectile2View prefab, Transform barrel, Transform hitPoint, IEnumerable<BattleAnimationPlayer> targets) {
        Assert.IsTrue(prefab);
        Assert.IsTrue(barrel);
        Assert.IsTrue(hitPoint);
        
        view = Object.Instantiate(prefab, barrel.position, barrel.rotation);
        view.projectile = this;
        this.targets.AddRange(targets);
        this.hitPoint = hitPoint;
    }

    public void HitTargets() {

        foreach (var target in targets) {

            var hitPoints = target.HitPoints;
            if (hitPoints.Count == 0)
                continue;

            var hitPoint = target.HitPoints.Contains(this.hitPoint) ? this.hitPoint : hitPoints.Random();
            view.PlayImpact(hitPoint);
            target.TakeHit(this, hitPoint);
        }

        Dispose();
    }

    public void Dispose() {
        Assert.IsTrue(view);
        Object.Destroy(view.gameObject);
        view = null;
    }
}

[Serializable]
public struct BattleAnimationInputs {
    public string moveAttack;
    public string attack;
    public string respond;
}
[Serializable]
public class WeaponNameBattleAnimationInputsDictionary : SerializableDictionary<WeaponName, BattleAnimationInputs> { }