using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitViewAnimationSequenceAutoPlay : MonoBehaviour {
    public bool shuffle = true;
    private void Start() {
        var sequences = FindObjectsOfType<UnitViewAnimationSequence>();
        var shuffled = shuffle ? sequences.OrderBy(_ => Random.value).ToArray() : sequences;
        for (var i = 0; i < sequences.Length; i++)
            sequences[i].Play(i, Array.IndexOf(shuffled, sequences[i]));
    }
}