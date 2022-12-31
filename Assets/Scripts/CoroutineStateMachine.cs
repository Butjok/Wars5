using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoroutineStateMachine : MonoBehaviour {

    [SerializeField] private float linearSpeed = .5f;
    [SerializeField] private float steeringSpeed = 90;
    [SerializeField] private Vector2 straightMoveDurationRange = new(1, 2);
    [SerializeField] private Vector2 steeringDurationRange = new(.5f, 1);
    [SerializeField] private float turretSpinSpeed = 360;
    [SerializeField] private Vector2 turretSpinDurationRange = new(.5f, 1);
    [SerializeField] private Transform turret;
    [SerializeField] private Material lineMaterial;
    
    private float currentLinearSpeed;
    private float currentSteeringSpeed;
    private float currentTurretSpinSpeed;

    private void Start() {
        StartCoroutine(WanderAround());
    }

    private void Update() {

        // move forward with the current linear speed
        transform.position += transform.forward * Time.deltaTime * currentLinearSpeed;

        // rotate with the current steering speed
        {
            var angle = transform.rotation.eulerAngles.y;
            angle += currentSteeringSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        // spin the turret with the current turret spin speed
        {
            var angle = turret.localEulerAngles.y;
            angle += currentTurretSpinSpeed * Time.deltaTime;
            turret.localEulerAngles = new Vector3(0, angle, 0);
        }
    }

    private IEnumerator WanderAround() {

        var lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = .05f;
        lineRenderer.sharedMaterial = lineMaterial;
        
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, turret.position);
        
        currentLinearSpeed = linearSpeed;

        var iterations = Random.Range(1, 4);
        for (var i = 0; i < iterations; i++) {
            currentSteeringSpeed = Random.Range(-1,2) * steeringSpeed;
            var linearMoveDuration = Random.Range(straightMoveDurationRange[0], straightMoveDurationRange[1]);

            var startTime = Time.time;
            while (Time.time < startTime + linearMoveDuration) {
                yield return null;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, turret.position);
            }
            
            currentSteeringSpeed = 0;
        }

        currentLinearSpeed = 0;

        Destroy(lineRenderer);

        yield return SpinTurret();
    }

    private IEnumerator SpinTurret() {
        
        currentTurretSpinSpeed = turretSpinSpeed;
        var turretSpinDuration = Random.Range(turretSpinDurationRange[0], turretSpinDurationRange[1]);
        yield return new WaitForSeconds(turretSpinDuration);
        currentTurretSpinSpeed = 0;
        
        yield return WanderAround();
    }
}