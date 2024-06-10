using System.Collections.Generic;
using SaveGame;
using UnityEngine;

public class Bridge2 : IMaterialized {

    public Level level;
    public int hp = 10;
    public List<Vector2Int> positions = new();

    [DontSave] public BridgeView2 view;
    [DontSave] public bool IsMaterialized { get; private set; }

    public void Materialize() { }
    public void Dematerialize() { }
}