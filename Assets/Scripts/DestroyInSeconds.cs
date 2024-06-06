using UnityEngine;

public class DestroyInSeconds : MonoBehaviour {
    public float delay;
    public void Update() {
        delay -= Time.deltaTime;
        if (delay <= 0)
            Destroy(gameObject);
    }
}