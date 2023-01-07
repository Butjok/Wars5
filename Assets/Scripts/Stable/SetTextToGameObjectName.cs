using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class SetTextToGameObjectName : MonoBehaviour {
    private void Update() {
        if (Application.isEditor && !Application.isPlaying)
            GetComponentInChildren<TMP_Text>().text = gameObject.name;
    }
}