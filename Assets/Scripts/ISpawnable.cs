public interface ISpawnable {
    bool IsSpawned { get; }
    void Spawn();
    void Despawn();
}