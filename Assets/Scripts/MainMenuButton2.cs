using System;
using UnityEngine;

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

    private bool? highlight;
    public bool turnGear;
    public float arrowTime;

    public void Reset() {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public bool? Highlight {
        set {
            highlight = value;
            materialPropertyBlock ??= new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_EmissionColor", value == true ? Color.white * 1.25f : Color.black);
            foreach (var meshRenderer in meshRenderers)
                meshRenderer.SetPropertyBlock(materialPropertyBlock);

            turnGear = (bool)value;
            moveArrow = (bool)value;
            arrowTime = 0;
        }
        get => highlight;
    }

    public void Start() {
        if (arrow)
            arrowStartPosition = arrow.localPosition.x;
    }

    public void Update() {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var newValue = Physics.Raycast(ray, out var hit, float.MaxValue) && hit.collider == boxCollider;
        if (newValue != Highlight)
            Highlight = newValue;
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
    }
}