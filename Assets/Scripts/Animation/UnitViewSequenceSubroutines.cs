using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitViewSequenceSubroutines : MonoBehaviour {
    [Serializable]
    public class Subroutine {
        public string name = "";
        [TextArea(5, 10)]
        public string text = "";
    }
    public Subroutine[] list = Array.Empty<Subroutine>();
}