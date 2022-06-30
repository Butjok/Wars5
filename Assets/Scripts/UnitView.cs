using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PathWalker))]
public class UnitView : MonoBehaviour {

	public Unit unit;
	public ChangeTracker<Vector2Int> position;
	public ChangeTracker<Vector2Int> rotation;
	public ChangeTracker<int> hp;
	public ChangeTracker<bool> visible;
	public ChangeTracker<bool> moved;
	public ChangeTracker<Color> playerColor;
	public ChangeTracker<bool> selected;
	public Lazy<Renderer[]> renderers;
	public Lazy<MaterialPropertyBlock> propertyBlock;
	public PathWalker pathWalker;
	[FormerlySerializedAs("curve")] public AnimationCurve blinkCurve = new AnimationCurve();
	public Speedometer[] speedometers;
	public SteeringArm[] steeringArms;

	public Color movedTint = Color.white / 2;

	public void Awake() {

		speedometers = GetComponentsInChildren<Speedometer>();
		steeringArms = GetComponentsInChildren<SteeringArm>();
		
		position = new ChangeTracker<Vector2Int>(_ => {
			transform.position = position.v.ToVector3Int();
			foreach (var speedometer in speedometers)
				speedometer.Clear();
			foreach  (var steeringArm in steeringArms)
				steeringArm.transform.localRotation=Quaternion.identity;
		});
		rotation = new ChangeTracker<Vector2Int>(_ => transform.rotation = Quaternion.LookRotation(rotation.v.ToVector3Int()));
		hp = new ChangeTracker<int>(_ => { });

		visible = new ChangeTracker<bool>(old => {

			// show
			if (!old && visible.v && unit.position.v is { } position) {
				gameObject.SetActive(true);
				this.position.v = position;
				rotation.v = unit.rotation.v;
				playerColor.v = unit.player.color;
				moved.v = unit.moved.v;
				hp.v = unit.hp.v;
			}

			// hide
			if (old && !visible.v) {
				gameObject.SetActive(false);
			}
		});

		moved = new ChangeTracker<bool>(_ => Color = playerColor.v * (unit.moved.v ? movedTint : Color.white));
		playerColor = new ChangeTracker<Color>(_ => Color = playerColor.v * (unit.moved.v ? movedTint : Color.white));
		//selected = new ChangeTracker<bool>(_ => );

		propertyBlock = new Lazy<MaterialPropertyBlock>(() => new MaterialPropertyBlock());
		renderers = new Lazy<Renderer[]>(GetComponentsInChildren<Renderer>);

		pathWalker = GetComponent<PathWalker>();
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
		unit.rotation.v = nextRotation;
		unit.moved.v = nextMoved;
	}
	public Vector2Int nextPosition;
	public Vector2Int nextRotation;
	public bool nextMoved;
	
	public void Blink(float duration) {
		// TODO: move to shader code
		DOTween.To(t=> {
			var value = blinkCurve.Evaluate(t);
			propertyBlock.v.SetFloat("_Selected", value);
			foreach(var renderer in renderers.v)
				renderer.SetPropertyBlock(propertyBlock.v);
		}, 0, 1, duration);
	}
	public float blinkDuration = 1;

	public void Start() {
		pathWalker.onComplete += () => {
			Walk();
		};
	}
	
	private void Update() {

		if (Input.GetKeyDown(KeyCode.Return)) {
			Blink(blinkDuration);
		}
		
		if (Input.GetKeyDown(KeyCode.Space)) {
			
			// unit.position.v = new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3));
			// unit.rotation.v = MathUtils.offsets.Random();
			// unit.moved.v = Random.Range(0, 10) > 5;

			if (!pathWalker.walking) {
				Walk();
			}
			else
				pathWalker.time.v = float.MaxValue;
		}
	}
	private void Walk() {
		while (true) {
			var points = new List<Vector2> {
				transform.position.ToVector2().RoundToInt()
			};
			points.Add(points[0] + transform.forward.ToVector2());
			for (var i = 0; i < 2; i++)
				points.Add(new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3)));

			pathWalker.points.v = points;

			if (pathWalker.path.Moves.All(m => m.type != MovePath.MoveType.Back)) {
				pathWalker.Walk();
				break;
			}
		}
	}
}