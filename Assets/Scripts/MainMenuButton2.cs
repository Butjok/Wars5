using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class MainMenuButton2 : MonoBehaviour {

    public MeshRenderer[] meshRenderers = Array.Empty<MeshRenderer>();
    public MaterialPropertyBlock materialPropertyBlock;
    public Camera camera;
    public BoxCollider boxCollider;
    public Transform gear;

    public Transform arrow;
    public bool moveArrow;
    public float arrowStartPosition;
    public AnimationCurve arrowCurve = new();
    public float arrowSpeed = 1;

    private bool? highlighted;
    public bool turnGear;
    public float arrowTime;

    public Vector3 startLocalPosition;
    public MainMenuSelectionState2.Command command;
    public MainMenuView2 mainMenuView;
    
    public bool Interactable {
        set => boxCollider.enabled = value;
    }

    public void Reset() {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public bool? Highlighted {
        set {
            highlighted = value;
            materialPropertyBlock ??= new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_EmissionColor", value == true ? Color.white * 1 : Color.black);
            foreach (var meshRenderer in meshRenderers)
                meshRenderer.SetPropertyBlock(materialPropertyBlock);

            turnGear = (bool)value;
            moveArrow = (bool)value;
            arrowTime = 0;

            transform.localPosition = startLocalPosition;
            if (value == true)
                transform.localPosition += new Vector3(0, 0, .0025f);
        }
        get => highlighted;
    }

    public void Start() {
        if (arrow)
            arrowStartPosition = arrow.localPosition.x;
        startLocalPosition = transform.localPosition;
    }

    public void Update() {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var newValue = Physics.Raycast(ray, out var hit, float.MaxValue) && hit.collider == boxCollider;
        if (newValue != Highlighted)
            Highlighted = newValue;
        if (gear && turnGear)
            gear.Rotate(Vector3.up, 360 * Time.deltaTime, Space.Self);
        if (arrow) {
            var localPosition = arrow.localPosition;
            if (moveArrow) {
                localPosition.x = arrowStartPosition + arrowCurve.Evaluate((arrowTime * arrowSpeed) % 1f);
                arrowTime += Time.deltaTime;
            }
            else
                localPosition.x = arrowStartPosition;

            arrow.localPosition = localPosition;
        }

        if (Input.GetMouseButtonDown(Mouse.left) && Highlighted == true) {
            mainMenuView.select(command);
        }
    }
}