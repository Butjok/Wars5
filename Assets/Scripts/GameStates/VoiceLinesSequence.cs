using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voice Lines Sequence", fileName = "New Voice Lines Sequence")]
public class VoiceLinesSequence : ScriptableObject {
    public List<AudioClip> clips = new();
}