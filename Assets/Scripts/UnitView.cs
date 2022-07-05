using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class UnitView : MonoBehaviour {

	public Unit unit;
	public Lazy<Renderer[]> renderers;
	public Lazy<MaterialPropertyBlock> propertyBlock;
	[FormerlySerializedAs("curve")] public AnimationCurve blinkCurve = new AnimationCurve();
	public Speedometer[] speedometers;
	public SteeringArm[] steeringArms;
	public MovePathWalker walker;
	public Turret turret;
	public Transform center;

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
		set { }
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

	public bool Moved {
		set => UpdateColor();
	}
	public Color PlayerColor {
		set => UpdateColor();
	}
	public void UpdateColor() {
		Color = unit.player.color * (unit.moved.v ? movedTint : Color.white);
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

		propertyBlock = new Lazy<MaterialPropertyBlock>(() => new MaterialPropertyBlock());
		renderers = new Lazy<Renderer[]>(GetComponentsInChildren<Renderer>);
	}

	public static readonly int colorId = Shader.PropertyToID("_PlayerColor");
	private Color Color {
		set {
			propertyBlock.v.SetColor(colorId, value);
			foreach (var renderer in renderers.v)
				renderer.SetPropertyBlock(propertyBlock.v);
		}
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
		DOTween.To(t => {
			var value = blinkCurve.Evaluate(t);
			propertyBlock.v.SetFloat("_Selected", value);
			foreach (var renderer in renderers.v)
				renderer.SetPropertyBlock(propertyBlock.v);
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