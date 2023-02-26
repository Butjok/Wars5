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

    [TextArea(10, 20)] public string moveAttack = "";
    [TextArea(10, 20)] public string attack = "";
    [TextArea(10, 20)] public string respond = "";

    public float speed;
    public float acceleration;
    public List<BattleAnimationPlayer> debugTargets = new();
    public int spawnPointIndex;
    public int shuffledIndex;
    public int firedShots;
    public Projectile2View projectileViewPrefab;
    public bool survives;
    public Transform hitPoint;
    public bool triedToGetTurret;

    private List<Transform> hitPoints;
    private List<Transform> barrels;
    private List<ParticleSystem> shotParticleSystems;
    private List<AudioSource> shotAudioSources;
    private AudioSource hitAudioSource;
    private ParticleSystem hitParticleSystem;
    private List<BattleAnimationPlayer> targets;
    private Turret turret;

    [Command]
    public int ShotsCount => Tokenizer.Tokenize(attack).Count(token => token == "fire");

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

    public void Aim(IEnumerable<BattleAnimationPlayer> targets) {

        if (!turret && !triedToGetTurret) {
            triedToGetTurret = true;
            turret = GetComponentInChildren<Turret>();
        }

        this.targets = new List<BattleAnimationPlayer>(targets);
        Assert.AreNotEqual(0, this.targets.Count);
        var target = this.targets.Random();
        hitPoint = target.HitPoints.Count == 0 ? null : target.HitPoints.Random();
        Assert.IsTrue(hitPoint, $"{this}: target {target} has no impact points");

        if (turret) {
            turret.computer.Target = hitPoint;
            turret.aim = true;
        }
    }

    [Command]
    public Projectile2 Fire() {

        Assert.IsTrue(hitPoint, $"{this}: has no target hit point to shoot at");
        Assert.AreNotEqual(0, ShotsCount, $"{this}: no shot commands found in attack sequence");
        Assert.AreNotEqual(0, Barrels.Count, $"{this}: no barrels to shoot from");

        if (shotParticleSystems.Count > 0)
            shotParticleSystems[firedShots % shotParticleSystems.Count].Play();

        if (shotAudioSources.Count > 0) {
            var shotAudioSource = shotAudioSources[firedShots % shotAudioSources.Count];
            shotAudioSource.PlayOneShot(shotAudioSource.clip);
        }

        var projectile = new Projectile2(projectileViewPrefab, Barrels[firedShots % Barrels.Count], hitPoint, targets, firedShots % ShotsCount == 0);
        firedShots++;
        return projectile;
    }

    [Command]
    public void TakeHit(Projectile2 projectile, Transform hitPoint, bool isLastHit) {

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

        if (isLastHit && !survives) { }
    }

    [Command] [ContextMenu(nameof(PlayAttack))]
    public void PlayAttack() {
        new BattleAnimation(this).Play(attack, debugTargets);
    }
    [Command] [ContextMenu(nameof(PlayMoveAttack))]
    public void PlayMoveAttack() {
        new BattleAnimation(this).Play(moveAttack, debugTargets);
    }
    [Command] [ContextMenu(nameof(PlayRespond))]
    public void PlayRespond() {
        new BattleAnimation(this).Play(respond, debugTargets);
    }
}

public class BattleAnimation {

    public readonly BattleAnimationPlayer battleAnimationPlayer;
    public bool Completed { get; private set; }

    private DebugStack stack = new();

    public BattleAnimation(BattleAnimationPlayer battleAnimationPlayer) {
        this.battleAnimationPlayer = battleAnimationPlayer;
    }

    private IEnumerator Coroutine(string input) {
        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {

                case "set-speed":
                    battleAnimationPlayer.speed = stack.Pop<dynamic>();
                    break;

                case "break":
                    battleAnimationPlayer.acceleration = -Mathf.Sign(battleAnimationPlayer.speed) * stack.Pop<dynamic>();
                    yield return new WaitForSeconds(Mathf.Abs(battleAnimationPlayer.speed / battleAnimationPlayer.acceleration));
                    battleAnimationPlayer.acceleration = 0;
                    battleAnimationPlayer.speed = 0;
                    break;

                case "translate":
                    battleAnimationPlayer.transform.position += battleAnimationPlayer.transform.forward * stack.Pop<dynamic>();
                    break;

                case "wait":
                    yield return new WaitForSeconds(stack.Pop<dynamic>());
                    break;

                case "random":
                    var b = stack.Pop<dynamic>();
                    var a = stack.Pop<dynamic>();
                    stack.Push(Random.Range(a, b));
                    break;

                case "spawn-point-index":
                    stack.Push(battleAnimationPlayer.spawnPointIndex);
                    break;

                case "shuffled-index":
                    stack.Push(battleAnimationPlayer.shuffledIndex);
                    break;

                case "aim":
                    battleAnimationPlayer.Aim(stack.Pop<IEnumerable<BattleAnimationPlayer>>());
                    break;

                case "fire":
                    battleAnimationPlayer.Fire();
                    break;

                default:
                    stack.ExecuteToken(token);
                    break;
            }

        Completed = true;
    }

    public void Play(string input, IEnumerable<BattleAnimationPlayer> targets, bool stopAllCoroutines = true) {
        if (stopAllCoroutines)
            battleAnimationPlayer.StopAllCoroutines();
        stack.Push(targets);
        battleAnimationPlayer.StartCoroutine(Coroutine(input));
    }
}

public class Projectile2 : IDisposable {

    private Projectile2View view;
    private List<BattleAnimationPlayer> targets = new();
    public IReadOnlyList<BattleAnimationPlayer> Targets => targets;
    public readonly bool isLast;
    public readonly Transform hitPoint;

    public Projectile2(Projectile2View prefab, Transform barrel, Transform hitPoint, IEnumerable<BattleAnimationPlayer> targets, bool isLast) {

        Assert.IsTrue(prefab);
        Assert.IsTrue(barrel);
        view = Object.Instantiate(prefab, barrel.position, barrel.rotation);
        view.projectile = this;
        this.targets.AddRange(targets);
        this.isLast = isLast;
        this.hitPoint = hitPoint;
    }

    public void HitTargets() {

        foreach (var target in targets) {

            var hitPoints = target.HitPoints;
            if (hitPoints.Count == 0)
                continue;

            var hitPoint = target.HitPoints.Contains(this.hitPoint) ? this.hitPoint : hitPoints.Random();
            view.PlayImpact(hitPoint);
            target.TakeHit(this, hitPoint, isLast);
        }

        Dispose();
    }

    public void Dispose() {
        Assert.IsTrue(view);
        Object.Destroy(view.gameObject);
        view = null;
    }
}