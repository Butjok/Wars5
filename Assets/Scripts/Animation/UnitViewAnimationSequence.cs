using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UnitView))]
public class UnitViewAnimationSequence : MonoBehaviour {

    public enum ActionType { SetSpeed, Accelerate, Break, Aim, Fire, Steer, Wait, Translate }

    [Serializable]
    public struct Action {

        public ActionType type;

        [Space]
        public Vector2 durationRange;
        public Vector2 floatValueRange;
        [Range(0, 1)]
        public float boolValueProbability;
    }

    public List<Action> actions = new();
    public float speed;
    public float acceleration;
    public float steeringSpeed;
    public UnitView unitView;

    private bool initialized;
    private void EnsureInitialized() {

        if (initialized)
            return;
        initialized = true;

        unitView = GetComponent<UnitView>();
        Assert.IsTrue(unitView);
    }

    public void Play(List<ImpactPoint> impactPoints = null) {
        EnsureInitialized();
        StartCoroutine(Sequence(impactPoints));
    }

    private IEnumerator Sequence(List<ImpactPoint> impactPoints) {

        foreach (var action in actions) {

            var duration = Random.Range(action.durationRange[0], action.durationRange[1]);
            var floatValue = Random.Range(action.floatValueRange[0], action.floatValueRange[1]);
            var boolValue = Random.value <= action.boolValueProbability;

            switch (action.type) {

                case ActionType.SetSpeed:
                    speed = floatValue;
                    break;

                case ActionType.Accelerate:
                    acceleration = floatValue;
                    yield return new WaitForSeconds(duration);
                    acceleration = 0;
                    break;

                case ActionType.Break:
                    acceleration = -Mathf.Sign(speed) * floatValue;
                    yield return new WaitForSeconds(Mathf.Abs(speed) / floatValue);
                    acceleration = 0;
                    speed = 0;
                    break;

                case ActionType.Aim:
                    unitView.turret.aim = boolValue;
                    break;

                case ActionType.Fire:
                    if (boolValue)
                        unitView.turret.Fire(impactPoints);
                    break;

                case ActionType.Steer:
                    steeringSpeed = floatValue;
                    yield return new WaitForSeconds(duration);
                    steeringSpeed = 0;
                    break;

                case ActionType.Wait:
                    yield return new WaitForSeconds(duration);
                    break;
                
                case ActionType.Translate:
                    transform.position += transform.forward * floatValue;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Update() {
        speed += acceleration * Time.deltaTime;
        transform.position += transform.forward * speed * Time.deltaTime;
        var angles = transform.rotation.eulerAngles;
        angles.y += steeringSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(angles);
    }
}