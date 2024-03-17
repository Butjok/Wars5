using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Canvas))]
public class UiWindowManager : MonoBehaviour {

    private static UiWindowManager instance;
    public static UiWindowManager Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<UiWindowManager>();
                Assert.IsTrue(instance);
            }
            return instance;
        }
    }

    public Canvas canvas;
    public List<Window> windows = new();

    public class Window : IDisposable {
        public UiWindowManager manager;
        public UiWindow window;
        public Action onDispose;
        public Window(UiWindowManager manager, UiWindow prefab, Action onDispose = null) {
            this.manager = manager;
            this.onDispose = onDispose;
            Assert.IsTrue(prefab);
            window = Instantiate(prefab, manager.canvas.transform);
            manager.windows.Add(this);
            window.close = Dispose;
        }
        public void Dispose() {
            onDispose?.Invoke();
            manager.windows.Remove(this);
            Destroy(window.gameObject);
        }
    }

    [Command]
    public static Window CreateWindow(UiWindow prefab = null) {
        if (!prefab)
            prefab = "DefaultWindow".LoadAs<UiWindow>();
        Assert.IsTrue(prefab);
        return new Window(Instance, prefab);
    }
}