using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[RequireComponent(typeof(MovePathWalker))]
public class UnitView : MonoBehaviour {

	public Unit unit;
	public Renderer[] renderers;
	public MaterialPropertyBlock propertyBlock;
	[FormerlySerializedAs("curve")] public AnimationCurve blinkCurve = new AnimationCurve();
	public Speedometer[] speedometers;
	public SteeringArm[] steeringArms;
	public Wheel[] wheels;
	public Piston[] wheelPistons;
	public MovePathWalker walker;
	public Turret turret;
	public Transform center;
	public Body body;
	public UnitView prefab;
	public TMP_Text hpText;

	public Color movedTint = Color.white / 2;

	public Vector2Int Position {
		get => transform.position.ToVector2().RoundToInt();
		set {
			transform.position = value.ToVector3Int();
			foreach (var speedometer in speedometers)
				speedometer.Clear();
			foreach (var steeringArm in steeringArms)
				steeringArm.transform.localRotation = Quaternion.identity;
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

	public ChangeTracker<bool> selected;

	public void Awake() {

		walker = GetComponentInChildren<MovePathWalker>();
		Assert.IsTrue(walker);
		turret = GetComponentInChildren<Turret>();

		speedometers = GetComponentsInChildren<Speedometer>();
		steeringArms = GetComponentsInChildren<SteeringArm>();

		selected = new ChangeTracker<bool>(_ => {
			if (selected.v)
				Blink();
		});

		propertyBlock = new MaterialPropertyBlock();
		renderers = GetComponentsInChildren<Renderer>();

		wheels = GetComponentsInChildren<Wheel>();
		wheelPistons = wheels.Select(wheel => wheel.GetComponent<Piston>()).Distinct().ToArray();
		body = GetComponentInChildren<Body>();
	}


	public Unit Carrier {
		set {

		}
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
		// TODO: move to shader code
		if (blinkCurve.length > 0)
			DOTween.To(t => {
				var value = blinkCurve.Evaluate(t);
				propertyBlock.SetFloat("_Selected", value);
				foreach (var renderer in renderers)
					renderer.SetPropertyBlock(propertyBlock);
			}, 0, blinkCurve[blinkCurve.length - 1].time, blinkDuration);
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
}