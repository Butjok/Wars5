using UnityEngine;

[CreateAssetMenu(menuName = nameof(DialogueSpeaker))]
public class DialogueSpeaker : ScriptableObject {
	public Sprite portrait;
	[Range(-1,1)]
	public int side = -1;
}