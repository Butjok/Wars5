using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(DialogueSpeaker))]
public class DialogueSpeaker : ScriptableObject {

    private static DialogueSpeaker Load(string name) {
        var result = Resources.Load<DialogueSpeaker>(name);
        Assert.IsTrue(result);
        return result;
    }
    
    public static DialogueSpeaker Natalie => Load("NatalieDialogueSpeaker");
    public static DialogueSpeaker Vladan => Load("VladanDialogueSpeaker");

    public Sprite portrait;
    [Range(-1, 1)]
    public int side = -1;
}