using System.Collections;
using Drawing;
using UnityEngine;

public class BipedalWalker2 : MonoBehaviour {

    public const int left = 0, right = 1;

    public RaycastHit[] feetTarget = new RaycastHit[2];
    public float stepLength = .5f;
    public IEnumerator footMovementCoroutine = null;
    public float feetTargetMovementSpeed = 1;
    public int[] sign = new int[2];
    public int[] previousSign = new int[2];
    public float duration = .5f;
    public Color[] colors = { Color.red, Color.green };
    public Vector3[] hips = new Vector3[2];
    public Vector3? oldPosition;


    public void Update() {

        Draw.ingame.Arrow(transform.position, transform.position + transform.forward);

        for (var side = left; side <= right; side++) {
            var hipWorldPosition = transform.TransformPoint(hips[side]).ToVector2();
            if (Vector2.Distance(feetTarget[side].point.ToVector2(), hipWorldPosition) > stepLength + .001f)
                TryMoveFoot(side, 1);

            Draw.ingame.SolidCircleXZ(feetTarget[side].point, .1f, colors[side]);
            Draw.ingame.CircleXZ(hipWorldPosition.ToVector3(), stepLength, Color.white);
        }
        
        sign[left] = transform.InverseTransformPoint(feetTarget[left].point).z > 0.001f ? 1 : -1;
        sign[right] = transform.InverseTransformPoint(feetTarget[right].point).z > 0.001f ? 1 : -1;

        if (sign[left] == -1 && previousSign[left] == 1)
            TryMoveFoot(right, 1);
        if (sign[right] == -1 && previousSign[right] == 1)
            TryMoveFoot(left, 1);
        
        previousSign[left] = sign[left];
        previousSign[right] = sign[right];

        if (oldPosition is{} actualOldPosition && Vector3.Distance(transform.position, actualOldPosition)<.001f&&
            footMovementCoroutine == null) {
            var leftFootLocalPosition = transform.InverseTransformPoint(feetTarget[left].point);
            var rightFootLocalPosition = transform.InverseTransformPoint(feetTarget[right].point);
            if (leftFootLocalPosition.z * rightFootLocalPosition.z > .001f) {
                TryMoveFoot(left, 1);
                TryMoveFoot(right, -1);
            }
        }

        oldPosition = transform.position;
    }

    public bool TryMoveFoot(int side, int direction) {
        if (footMovementCoroutine != null)
            return false;
        var hipWorldPosition = transform.TransformPoint(hips[side]).ToVector2();
        var target = direction * transform.forward * stepLength + hipWorldPosition.ToVector3();
        if (Physics.Raycast(target + Vector3.up * 100, Vector3.down, out var hit, float.MaxValue, LayerMasks.Terrain | LayerMasks.Roads)) {
            footMovementCoroutine = MoveFoot(side, hit);
            StartCoroutine(footMovementCoroutine);
            return true;
        }
        return false;
    }
    
    public IEnumerator MoveFoot(int side, RaycastHit target) {
        var startTime = Time.time;
        var oldTarget = feetTarget[side];
        var duration = Vector2.Distance(feetTarget[side].point.ToVector2(), target.point.ToVector2()) / feetTargetMovementSpeed;
        while (Time.time - startTime < duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.InOutQuad(t);
            feetTarget[side].point = Vector3.Lerp(oldTarget.point, target.point, t);
            feetTarget[side].normal = Vector3.Lerp(oldTarget.normal, target.normal, t);
            yield return null;
        }
        feetTarget[side] = target;
        footMovementCoroutine = null;
    }
}