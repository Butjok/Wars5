using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[SelectionBase]
[RequireComponent(typeof(MovePathWalker))]
public class UnitView : MonoBehaviour {

    public static UnitView DefaultPrefab => "light-tank".LoadAs<UnitView>();
    
    public Unit unit;
    public Renderer[] renderers;
    public MaterialPropertyBlock propertyBlock;
    public SteeringArm[] steeringArms;
    public Wheel[] wheels;
    public Piston[] wheelPistons;
    public MovePathWalker walker;
    public Turret turret;
    public Body body;
    public UnitView prefab;
    public TMP_Text hpText;
    public ImpactPoint[] impactPoints = Array.Empty<ImpactPoint>();
    public BodyTorque bodyTorque;
    public UnitViewSequencePlayer moveAndAttack;
    public UnitViewSequencePlayer attack;
    public UnitViewSequencePlayer respond;

    public Vector2Int Position {
        get => transform.position.ToVector2().RoundToInt();
        set {
            transform.position = value.ToVector3Int();
            PlaceOnTerrain();
            ResetSteeringArms();
        }
    }
    public Vector2Int Forward {
        get => transform.forward.ToVector2().RoundToInt();
        set => transform.rotation = Quaternion.LookRotation(value.ToVector3Int(), Vector3.up);
    }

    public int Hp {
        set {
            if (!hpText)
                return;
            hpText.enabled = value != Rules.MaxHp(unit);
            if (hpText.enabled)
                // TODO: remove GC
                hpText.text = value.ToString();
        }
    }
    public bool Visible {
        set {
            // show
            if (value && unit.position.v is { } position) {
                gameObject.SetActive(true);
                Position = position;
            }
            else
                gameObject.SetActive(false);
        }
    }

    public bool LowAmmo {
        set { }
    }
    public bool HasCargo {
        set { }
    }
    public int Fuel {
        set { }
    }

    public bool Selected {
        set {
            if (value)
                Blink();
        }
    }

    private bool initialized;
    public void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        
        walker = GetComponentInChildren<MovePathWalker>();
        Assert.IsTrue(walker);
        
        turret = GetComponentInChildren<Turret>();

        steeringArms = GetComponentsInChildren<SteeringArm>();
        
        renderers = GetComponentsInChildren<Renderer>();

        wheels = GetComponentsInChildren<Wheel>();
        foreach (var wheel in  wheels)
            wheel.EnsureInitialized();
        
        wheelPistons = wheels.Select(wheel => wheel.GetComponent<Piston>()).Distinct().Where(piston => piston).ToArray();
        body = GetComponentInChildren<Body>();

        impactPoints = GetComponentsInChildren<ImpactPoint>();
    }
    
    public void Awake() {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void PlaceOnTerrain(bool resetPistons = false) {
        
        EnsureInitialized();
        
        foreach (var wheel in wheels)
            wheel.Update();
        if (resetPistons)
            foreach (var wheelPiston in wheelPistons)
                wheelPiston.Reset();
        body.Update();
    }

    public void ResetSteeringArms() {
        foreach (var steeringArm in steeringArms) {
            steeringArm.speedometer.Clear();
            steeringArm.transform.localRotation = Quaternion.identity;
        }
    }

    public Unit Carrier {
        set { }
    }

    public Color32 PlayerColor {
        set {
            propertyBlock.SetColor("_PlayerColor", value);
            UpdateRenderers();
        }
    }
    public bool Moved {
        set {
            propertyBlock.SetFloat("_Moved", value ? 1 : 0);
            UpdateRenderers();
        }
    }
    
    public void UpdateRenderers() {
        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(propertyBlock);
    }

    [ContextMenu(nameof(Move))]
    public void Move() {
        unit.position.v = nextPosition;
        unit.moved.v = nextMoved;
    }
    public Vector2Int nextPosition;
    public Vector2Int nextRotation;
    public bool nextMoved;

    public void Blink() {
    }
    public float blinkDuration = 1;

    private void Update() {

        if (Input.GetKeyDown(KeyCode.Return)) {
            Blink();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {

            // unit.position.v = new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3));
            // unit.rotation.v = MathUtils.offsets.Random();
            // unit.moved.v = Random.Range(0, 10) > 5;

            /*if (!pathWalker.walking) {
                Walk();
            }
            else
                pathWalker.time.v = float.MaxValue;*/
        }
    }

    public void TakeDamage(Projectile projectile,ImpactPoint impactPoint) {
        EnsureInitialized();
        bodyTorque.AddWorldForceTorque(impactPoint.transform.position, -impactPoint.transform.forward * projectile.impactForce);
    }
    
    public void Die(Projectile projectile, ImpactPoint impactPoint) {
        EnsureInitialized();
        Destroy(gameObject);
    }
    
    public void Die() {
        EnsureInitialized();
        Destroy(gameObject);
    }
}