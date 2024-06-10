using UnityEngine;
using UnityEngine.Assertions;

public class CrateView : MonoBehaviour {

    public MeshRenderer meshRenderer;

    public Vector2Int Position {
        set {
            if (value.TryRaycast(out var hit)) {
                transform.position = hit.point;
                transform.rotation = hit.normal.ToRotation(Random.value * 360f);
            }
            else {
                transform.position = value.ToVector3();
                transform.rotation = Quaternion.identity;
            }
        }
    }

    public void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsTrue(meshRenderer);
    }
}