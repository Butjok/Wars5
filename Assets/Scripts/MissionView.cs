using System;
using UnityEngine;
using UnityEngine.Assertions;

public class MissionView : MonoBehaviour {

    public Renderer[] renderers = { };

    public Material defaultMaterial;
    public Material hoveredMaterial;
    public Material unavailableMaterial;
    
    public MissionName MissionName {
        get {
            var parsed = Enum.TryParse(name, out MissionName missionName);
            Assert.IsTrue(parsed, name);
            return missionName;
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

    private void Reset() {
        renderers = GetComponentsInChildren<Renderer>();
    }
}