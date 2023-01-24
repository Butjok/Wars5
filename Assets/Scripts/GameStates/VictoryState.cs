using System.Collections;
using UnityEngine;

public static class VictoryState {
    public static IEnumerator Run(Main main, UnitAction winningAction) {

        Debug.Log("Victory!");
        if (CursorView.TryFind(out var cursor))
            cursor.Visible = false;
        PlayerView.globalVisibility = false;
        yield return null;
        // GameUiView.Instance.Victory = true;
    }
}