using System;
using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitView2TreeParser : MonoBehaviour {

    public static readonly string[] playerMaterialShaders = { "Custom/wb_unit" };

    [Command] [ContextMenu(nameof(Parse))]
    public void Parse() {
        var view = GetComponent<UnitView2>();
        if (view)
            Parse(view);
    }

    public void Parse(UnitView2 view) {
        StartCoroutine(ParseAnimation(view));
    }
    public IEnumerator ParseAnimation(UnitView2 view) {

        foreach (var t in view.GetComponentsInChildren<Transform>()) {
            if (t == transform)
                continue;
            {
                var sp = new SerializedObject(t).FindProperty("m_LocalPosition");
                sp.prefabOverride = false;
                sp.serializedObject.ApplyModifiedProperties();
            }
            {
                var sp = new SerializedObject(t).FindProperty("m_LocalRotation");
                sp.prefabOverride = false;
                sp.serializedObject.ApplyModifiedProperties();
            }
        }

        view.hitPoints = view.transform.GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("HitPoint"))
            .ToList();

        view.body = view.transform.GetComponentsInChildren<Transform>()
            .SingleOrDefault(t => t.name == "Body");
        Assert.IsTrue(view.body, "no body");

        view.playerMaterialRenderers = view.transform.GetComponentsInChildren<Renderer>()
            .Where(r => r.sharedMaterials.Any(m => playerMaterialShaders.Contains(m.shader.name)))
            .ToList();

        view.wheels.Clear();
        foreach (var wheelTransform in view.transform.GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("Wheel"))) {

            var localPosition = view.transform.InverseTransformPointWithoutScale(wheelTransform.position);
            var renderer = wheelTransform.GetComponentInChildren<Renderer>();
            var radius = renderer ? Mathf.Max(renderer.bounds.extents.y, renderer.bounds.extents.z) : 1;

            var steeringGroup = UnitView2.Wheel.SteeringGroup.None;
            foreach (UnitView2.Wheel.SteeringGroup value in Enum.GetValues(typeof(UnitView2.Wheel.SteeringGroup)))
                if (wheelTransform.name.Contains(value.ToString()))
                    steeringGroup = value;

            var wheel = new UnitView2.Wheel {
                transform = wheelTransform,
                radius = radius,
                raycastOrigin = new Vector2(localPosition.x, localPosition.z),
                yOffset = localPosition.y,
                isFixed = wheelTransform.name.Contains("Fixed"),
                steeringGroup = steeringGroup,
            };
            view.wheels.Add(wheel);
        }

        /*
         * TODO: this will not work if the scaling is not 1
         */

        view.turrets.Clear();
        foreach (var turretTransform in view.transform.GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("Turret"))) {

            var turret = new UnitView2.Turret {
                transform = turretTransform,
                position = turretTransform.localPosition,
                name = turretTransform.name.Replace("Turret", ""),
                workMode = WorkMode.RotateToRest,
            };
            view.turrets.Add(turret);

            foreach (var barrelTransform in turretTransform.GetComponentsInChildren<Transform>()
                .Where(t => t.name.StartsWith("Barrel"))) {

                var barrel = new UnitView2.Turret.Barrel {
                    name = barrelTransform.name.Replace("Barrel", ""),
                    position = barrelTransform.localPosition,
                    transform = barrelTransform,
                    workMode = WorkMode.RotateToRest,
                };
                turret.barrels.Add(barrel);
            }
        }

        // while (!Input.anyKeyDown) {
        //     foreach (var hitPoint in hitPoints)
        //         Draw.ingame.Ray(hitPoint.position, hitPoint.forward);
        //     yield return null;
        // }
        // yield return null;

        // while (true) {
        //     foreach (var wheel in wheels) {
        //         var localRayOrigin = new Vector3(wheel.raycastOrigin.x, 0, wheel.raycastOrigin.y);
        //         var worldRayOrigin = transform.position + transform.rotation * localRayOrigin;
        //         Draw.ingame.Cross(worldRayOrigin, .5f);
        //     }
        //     yield return null;
        // }
        // yield return null;

        yield break;
    }
}