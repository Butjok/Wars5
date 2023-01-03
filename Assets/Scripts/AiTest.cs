using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using RoslynCSharp;
using UnityEngine;
using UnityEngine.Assertions;


public class AiTest : MonoBehaviour {
    
    public Transform[] transforms = { };
    
    [ResizableTextArea] public string code = @"
using System.Collections.Generic;
using UnityEngine;

public class AiTestProcessing {
    public void Execute(Unit unit) {
         
    }
}";

    public Main main;
    
    private void Start() {
        main = Testing.CreateGame();
    }
    
    [Button]
    public void Execute() {
        var unit = main.units.Values.First();
        var domain = ScriptDomain.CreateDomain("AiTesting");
        var scriptType = domain.CompileAndLoadMainSource(code);
        Assert.IsNotNull(scriptType);
        var proxy = scriptType.CreateInstance();
        proxy.Call("Execute", unit);
    }
}