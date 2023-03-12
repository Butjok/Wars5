using System.Collections.Generic;
using System.Linq;
// using RoslynCSharp;
using UnityEngine;
using UnityEngine.Assertions;


public class AiTest : MonoBehaviour {
    
    public Transform[] transforms = { };
    
    public string code = @"
using System.Collections.Generic;
using UnityEngine;

public class AiTestProcessing {
    public void Execute(Unit unit) {
         
    }
}";

    public Level level;
    
    private void Start() {
        level = Testing.CreateGame();
    }
    
    public void Execute() {
        // var unit = main.units.Values.First();
        // var domain = ScriptDomain.CreateDomain("AiTesting");
        // var scriptType = domain.CompileAndLoadMainSource(code);
        // Assert.IsNotNull(scriptType);
        // var proxy = scriptType.CreateInstance();
        // //proxy.Call("Execute", unit);
    }
}