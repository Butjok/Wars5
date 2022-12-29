using System;
using NaughtyAttributes;
using RoslynCSharp;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(UnitView))]
public class UnitBrain : MonoBehaviour {

    [ResizableTextArea] public string code = @"
using System.Collections.Generic;
using UnityEngine;

public static class AiTestProcessing {
    public static UnitAction FindAction(Unit unit) {
         return null;
    }
}";

    public ScriptDomain domain;

    private void Start() {
        domain = ScriptDomain.CreateDomain("AiTest");
    }

    public UnitAction FindAction() {
        var scriptType = domain.CompileAndLoadMainSource(code);
        Assert.IsNotNull(scriptType);
        return (UnitAction)scriptType.CallStatic("FindAction", GetComponent<UnitView>().unit);
    }

    [Button]
    public void Trigger() {
        var action = FindAction();
    }

    public bool debugDraw;
    private void OnDrawGizmos() {
        if (debugDraw)
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
    }
}