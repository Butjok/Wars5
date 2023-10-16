using Drawing;
using UnityEngine;

public class BushTester : MonoBehaviour {

    public SphereCollider[] sphereColliders;

    public void Reset() {
        sphereColliders = GetComponentsInChildren<SphereCollider>();
    }

    public bool IntersectsRoads() {
        foreach (var sphereCollider in sphereColliders) {
            var origin = sphereCollider.transform.position + Vector3.up * 100;
            var radius = sphereCollider.radius * transform.localScale.x;
            var ray = new Ray(origin, Vector3.down);
            if (Physics.SphereCast(ray, radius, out var hit, 10000, 1 << LayerMask.NameToLayer("Roads"))) {
                // using (Draw.ingame.WithDuration(5))
                    // Draw.ingame.WireSphere(ray.GetPoint(hit.distance), radius, Color.red);
                return true;
            }
        }
        return false;
    }
}