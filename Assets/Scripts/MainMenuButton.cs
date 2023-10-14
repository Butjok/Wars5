using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour{

    public Renderer renderer;
    
    private MaterialPropertyBlock propertyBlock;
    public MaterialPropertyBlock PropertyBlock => propertyBlock ??= new MaterialPropertyBlock();
    
    public UnityEvent<MainMenuButton> onClick = new();
    public MainMenuSelectionState2.Command command;

    public bool Visible {
        set => gameObject.SetActive(value);
    }

    public void UpdateRenderer() {
        PropertyBlock.SetFloat("_Selected", highlightIntensity);
        PropertyBlock.SetFloat("_Active", interactable ? 1 : 0);
        renderer.SetPropertyBlock(PropertyBlock);
    }
    
    private float highlightIntensity;
    public float HighlightIntensity {
        set {
            highlightIntensity = value;
            UpdateRenderer();
        }
    }
    
    private bool interactable;
    public bool Interactable {
        get => interactable;
        set {
            interactable = value;
            UpdateRenderer();
        }
    }

    private void Reset() {
        renderer = GetComponent<Renderer>();
        Assert.IsTrue(renderer);
    }
}