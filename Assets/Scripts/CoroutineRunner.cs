using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour {
    private static CoroutineRunner instance;

    public static CoroutineRunner Instance {
        get {
            if (!instance) {
                var gameObject = new GameObject(nameof(CoroutineRunner));
                instance = gameObject.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(gameObject);
            }
            return instance;
        }
    }
}