using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VictoryState {
    public static IEnumerator<StateChange> Run(Main main, UnitAction winningAction) {

        winningAction.Dispose();
        
        Debug.Log("Victory!");
        if (CursorView.TryFind(out var cursor))
            cursor.show = false;
        PlayerView.globalVisibility = false;
        yield return StateChange.none;
        // GameUiView.Instance.Victory = true;
    }
}