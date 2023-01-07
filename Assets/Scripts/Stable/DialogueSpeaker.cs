using System;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(DialogueSpeaker))]
public class DialogueSpeaker : ScriptableObject {

    public enum Mood { Normal, Happy, Mad, Worried }

    public static DialogueSpeaker Natalie => "NatalieDialogueSpeaker".LoadAs<DialogueSpeaker>();
    public static DialogueSpeaker Vladan => "VladanDialogueSpeaker".LoadAs<DialogueSpeaker>();

    public MoodSpriteDictionary portraits = new() {
        [Mood.Normal] = null,
        [Mood.Happy] = null,
        [Mood.Mad] = null,
        [Mood.Worried] = null,
    };
    [Range(-1, 1)]
    public int side = -1;
}

[Serializable]
public class MoodSpriteDictionary : SerializableDictionary<DialogueSpeaker.Mood, Sprite> { }