using System.Collections;
using UnityEngine;

public class SequencePlayer : MonoBehaviour {

    public static SequencePlayer player;

    public static IEnumerator Animation(string input,Transform transform,
        int spawnPointIndex, int shuffledIndex) {
        
        var speed=0f;
        var acceleration = 0f;
        
        var stack = new DebugStack();
        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {
                
                case "spawn-point-index":
                    stack.Push(spawnPointIndex);
                    break;
                    
                case "shuffled-index":
                    stack.Push(shuffledIndex);
                    break;
                    
                case "random":
                    var b = (dynamic)stack.Pop();
                    var a = (dynamic)stack.Pop();
                    stack.Push(Random.Range(a, b));
                    break;
                    
                case "set-speed":
                    speed = (dynamic)stack.Pop();
                    break;
                
                case "accelerate":
                    acceleration = (dynamic)stack.Pop();
                    yield return new WaitForSeconds((dynamic)stack.Pop());
                    acceleration = 0;
                    break;
                
                case "break":
                    acceleration = -Mathf.Sign(speed) * (dynamic)stack.Pop();
                    yield return new WaitForSeconds(Mathf.Abs(speed / acceleration));
                    acceleration = 0;
                    speed = 0;
                    break;
                
                case "set-aim":
                    
                    break;
            }
        yield break;
    }

    public static void Play(string input,Transform transform) {
        if (!player) {
            var gameObject = new GameObject(nameof(SequencePlayer));
            Object.DontDestroyOnLoad(gameObject);
            player = gameObject.AddComponent<SequencePlayer>();
        }
        player.StartCoroutine(Animation(input,transform,0,0));
    }
}