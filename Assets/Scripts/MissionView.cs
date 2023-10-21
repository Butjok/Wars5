using System;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MissionView : MonoBehaviour {

    public Renderer[] renderers = { };

    public Material defaultMaterial;
    public Material hoveredMaterial;
    public Material unavailableMaterial;
    public UiLabel label;
    
    public Type MissionType {
        get {
            var type = Type.GetType(name);
            Assert.IsNotNull(type, name);
            return type;
        }
    }

    private Material material;
    private Material Material {
        set {
            material = value;
            foreach (var renderer in renderers)
                renderer.sharedMaterial = value;
        }
        get => material;
    }

    public bool IsAvailable {
        set => Material = value ? defaultMaterial : unavailableMaterial;
    }
    public bool Hovered {
        set {
            if (Material != unavailableMaterial)
                Material = value ? hoveredMaterial : defaultMaterial;
        }
    }

    public CinemachineVirtualCamera TryGetVirtualCamera => GetComponentInChildren<CinemachineVirtualCamera>();

    private void Reset() {
        renderers = GetComponentsInChildren<Renderer>();
    }
}