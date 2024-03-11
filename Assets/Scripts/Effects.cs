using UnityEngine;

public static class Effects {
    public static ParticleSystem SpawnExplosion(Vector3 position, Vector3 up, bool play=true) {
        var prefab = Resources.Load<ParticleSystem>("Explosion");
        if (!prefab)
            return null;
        var explosion = Object.Instantiate(prefab, position, Quaternion.LookRotation(up));
        if (play)
            explosion.Play();
        return explosion;
    }
    public static ParticleSystem SpawnExplosion(Vector3 position, bool play=true) {
        return SpawnExplosion(position, Vector3.up, play);
    }
}