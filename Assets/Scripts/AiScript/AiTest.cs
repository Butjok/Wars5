using NaughtyAttributes;
using UnityEngine;

public class AiTest : MonoBehaviour {
    
    public Transform[] transforms = { };
    
    [ResizableTextArea] public string code = @"
using UnityEngine;

public class AiTestProcessing {{
    public static const int pi = 4;
}}";
    
    [Button]
    public void Execute() {
        
    }
}