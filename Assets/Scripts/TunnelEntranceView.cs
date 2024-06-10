using UnityEngine;

public class TunnelEntranceView : MonoBehaviour {
    public Vector2Int Position {
        get => transform.position.ToVector2Int();
        set => transform.position = value.Raycasted();
    }
}