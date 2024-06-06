using Butjok.CommandLine;
using UnityEngine;

public static class ExplosionCrater {

    [Command] public static float offset = 0.02f;
    [Command] public static float lifeTime = 30;

    public static MeshRenderer SpawnDecal(Vector2 position, Transform parent=null) {
        if (position.TryRaycast(out var hit, LayerMasks.Terrain | LayerMasks.Roads)) {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "ExplosionCrater";
            quad.transform.SetParent(parent, true);
            quad.transform.position = hit.point + offset * hit.normal;
            quad.transform.rotation = Quaternion.LookRotation(hit.normal);
            quad.transform.Rotate(0, 0, 0, Space.Self);
            var meshRenderer = quad.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = "ExplosionCrater".LoadAs<Material>();
            var materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetFloat("_CreationTime", Time.timeSinceLevelLoad);
            materialPropertyBlock.SetFloat("_LifeTime", lifeTime);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
            var destroyAfter = quad.AddComponent<DestroyInSeconds>();
            destroyAfter.delay = lifeTime;
            return meshRenderer;
        }
        return null;
    }

    [Command]
    public static void SpawnDecal() {
        SpawnDecal(Camera.main.TryPhysicsRaycast(out Vector3 point) ? point.ToVector2() : Vector2.zero);
    }
}