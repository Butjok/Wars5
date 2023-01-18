using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[ExecuteInEditMode]
public class ShapesTest :ImmediateModeShapeDrawer {
    
    public float velocity = 10;
    public float timeStep = .1f;
    public int stepsCount = 100;
    public ThicknessSpace thicknessSpace = ThicknessSpace.Pixels;
    public float thickness = 2;
    public float radius = 2;

    public Vector3 from = new Vector3(0, 0, 1);
    public Vector3 to = new Vector3(1, 1, 1);

    public TextElement textElement;
    public float fontSize = 12;
    
    public override void OnEnable() {
        base.OnEnable();
        textElement = new TextElement();
    }

    public override void OnDisable() {
        base.OnDisable();
        textElement.Dispose();
    }

    public override void DrawShapes(Camera cam) {
        base.DrawShapes(cam);
        
        using (Draw.Command(cam)) {
            // var curve = BallisticCurve.From(transform.position, transform.forward, velocity, Vector3.down * 10);
            // Draw.ThicknessSpace = thicknessSpace;
            // Draw.Color = Color.yellow;
            // Draw.Thickness = thickness;
            // var path = new PolylinePath();
            // for (var step = 0; step < stepsCount; step++)
            //     path.AddPoint(curve.Sample(step * timeStep));
            // Draw.Polyline(path);

            if (Mouse.TryGetPosition(out Vector2Int position)) {
                // Debug.DrawLine(position.ToVector3Int(), position.ToVector3Int()+Vector3Int.up);
                Draw.ThicknessSpace = thicknessSpace;
                // Draw.RadiusSpace = thicknessSpace;
                Draw.RectangleBorder(position.ToVector3Int(), Quaternion.Euler(90,0,0), new Rect(-Vector2.one/2, Vector2.one), thickness, radius,Color.white);

                // Draw.SizeSpace = ThicknessSpace.Pixels;
                Draw.Text(textElement,position.ToVector3Int(), Quaternion.Euler(90,0,0), position.ToString(), fontSize);
            }
        }
    }
}
