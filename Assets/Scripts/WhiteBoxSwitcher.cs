using System;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoxSwitcher : MonoBehaviour {

    public List<UnitView> views = new();
    public int index = -1;

    public void Update() {
        
        if (views.Count > 0 && Input.GetKeyDown(KeyCode.Tab)) {
            if (index != -1)
                views[index].gameObject.SetActive(false);
            index = (index + 1) % views.Count;
            views[index].gameObject.SetActive(true);
            views[index].PlaceOnTerrain();
        }
    }
}