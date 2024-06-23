using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
public class CharacterTest : MonoBehaviour {

    public MeshRenderer meshRenderer;

    public void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsTrue(meshRenderer);
    }

    [Command]
    public void LogInfo() {
        
    }
}