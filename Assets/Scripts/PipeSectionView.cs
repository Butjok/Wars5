using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PipeSectionView : MonoBehaviour {

    public Mesh meshForEnd;
    public Mesh meshForI;
    public Mesh meshForL;

    public int rotationOffsetEnd;
    public int rotationOffsetI;
    public int rotationOffsetL;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    public void Reset() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsTrue(meshFilter);
        Assert.IsTrue(meshRenderer);
    }

    private PipeSectionKind kind;
    public PipeSectionKind Kind {
        set {
            kind = value;
            meshFilter.sharedMesh = value switch {
                PipeSectionKind.End => meshForEnd,
                PipeSectionKind.I => meshForI,
                PipeSectionKind.L => meshForL,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
            UpscaleBounds();
        }
    }
    public Vector2Int Position {
        set {
            transform.position = value.ToVector3();
            UpscaleBounds();
        }
    }
    public Vector2Int Forward {
        set {
            transform.rotation = Quaternion.LookRotation(value.Rotate(kind switch {
                PipeSectionKind.End => rotationOffsetEnd,
                PipeSectionKind.I => rotationOffsetI,
                PipeSectionKind.L => rotationOffsetL,
                _ => throw new ArgumentOutOfRangeException()
            }).ToVector3(), Vector3.up);
            UpscaleBounds();
        }
    }

    public void UpscaleBounds() {
        var bounds = meshRenderer.bounds;
        bounds.size *= 10;
        meshRenderer.bounds = bounds;
    }
}

public enum PipeSectionKind {
    End,
    I,
    L
}