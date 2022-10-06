using UnityEngine;

public class UnitViewAnimationSequenceAutoPlay : MonoBehaviour {
    public void Start() {
        foreach (var sequence in FindObjectsOfType<UnitViewAnimationSequence>())
            sequence.Play();
    }
}