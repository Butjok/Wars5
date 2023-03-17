using UnityEngine;

[ExecuteInEditMode]
public class HardFollow2 : MonoBehaviour {
    public Transform target;
    public void LateUpdate() {
        if (target)
            transform.position = target.position;
    }
}