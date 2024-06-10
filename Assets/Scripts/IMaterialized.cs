public interface IMaterialized {
    bool IsMaterialized { get; }
    void Materialize();
    void Dematerialize();
}