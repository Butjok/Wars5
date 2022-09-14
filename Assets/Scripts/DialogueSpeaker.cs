using System;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(DialogueSpeaker))]
public class DialogueSpeaker : ScriptableObject {

    public enum Mood { Normal, Happy, Mad, Worried }

    private static DialogueSpeaker Load(string name) {
        var result = Resources.Load<DialogueSpeaker>(name);
        Assert.IsTrue(result);
        return result;
    }

    public static DialogueSpeaker Natalie => Load("NatalieDialogueSpeaker");
    public static DialogueSpeaker Vladan => Load("VladanDialogueSpeaker");

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