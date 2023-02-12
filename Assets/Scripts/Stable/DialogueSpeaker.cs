using System;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(DialogueSpeaker))]
public class DialogueSpeaker : ScriptableObject {

    public enum Mood { Normal, Happy, Sad, Mad, Worried, Shocked, Crying, Laughing, Intimate }

    public static DialogueSpeaker Natalie => "NatalieDialogueSpeaker".LoadAs<DialogueSpeaker>();
    public static DialogueSpeaker Vladan => "VladanDialogueSpeaker".LoadAs<DialogueSpeaker>();

    public Sprite portraitNormal;
    public Sprite portraitHappy;
    public Sprite portraitSad;
    public Sprite portraitMad;
    public Sprite portraitWorried;
    public Sprite portraitShocked;
    public Sprite portraitCrying;
    public Sprite portraitLaughing;
    public Sprite portraitIntimate;

    public bool TryGetPortrait(Mood mood, out Sprite sprite) {
        sprite = mood switch {
            Mood.Normal => portraitNormal,
            Mood.Happy => portraitHappy,
            Mood.Sad => portraitSad,
            Mood.Mad => portraitMad,
            Mood.Worried => portraitWorried,
            Mood.Shocked => portraitShocked,
            Mood.Crying => portraitCrying,
            Mood.Laughing => portraitLaughing,
            Mood.Intimate => portraitIntimate,
            _ => throw new ArgumentOutOfRangeException(mood.ToString())
        };
        return sprite;
    }

    [Range(-1, 1)]
    public int side = -1;
}