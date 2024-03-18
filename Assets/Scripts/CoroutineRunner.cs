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

    private List<CancellableCoroutine> cancellableCoroutines = new List<CancellableCoroutine>();
    public void StartCoroutine(CancellableCoroutine cancellableCoroutine) {
        cancellableCoroutines.Add(cancellableCoroutine);
    }
    public void CancelCoroutine(CancellableCoroutine cancellableCoroutine) {
        cancellableCoroutines.Remove(cancellableCoroutine);
        cancellableCoroutine.Cancel();
    }

    public void Update() {
        for (var i = cancellableCoroutines.Count - 1; i >= 0; i--)
            if (!cancellableCoroutines[i].Run().MoveNext())
                cancellableCoroutines.RemoveAt(i);
    }
}

public abstract class CancellableCoroutine {
    public abstract IEnumerator Run();
    public abstract void Cancel();
}