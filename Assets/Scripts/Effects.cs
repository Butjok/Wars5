using UnityEngine;

public static class Effects {
    public static ParticleSystem SpawnExplosion(Vector3 position, Vector3 up, Transform parent = null, bool play = true) {
        var prefab = Resources.Load<ParticleSystem>("Explosion");
        if (!prefab)
            return null;
        var explosion = Object.Instantiate(prefab, position, Quaternion.LookRotation(up));
        explosion.transform.SetParent(parent, true);
        if (play)
            explosion.Play();
        return explosion;
    }
    public static ParticleSystem SpawnExplosion(Vector3 position, Transform parent = null, bool play = true) {
        return SpawnExplosion(position, Vector3.up, parent, play);
    }
}