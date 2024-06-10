using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class BirdBox : MonoBehaviour {

    [Serializable]
    public class Bird {
        public Vector2 position;
        public Vector2 speed;
        public Vector2 speedLimits = new(.25f, .5f);
        public float avoidanceDistance = 1;
        public Transform view;
        public MeshRenderer renderer;
    }

    public BoxCollider boxCollider;
    public Vector2 min => boxCollider.bounds.min.ToVector2();
    public Vector2 max => boxCollider.bounds.max.ToVector2();

    public float attractionDistance = 2;
    public float avoidanceDistance = .1f;
    public Vector2Int flockSize = new(1, 5);

    public List<Bird> birds = new();
    public List<Bird> newBirds = new();

    public Transform prefab;

    public int minBirdsCount = 10;

    public void Reset() {
        boxCollider = GetComponent<BoxCollider>();
    }
    public void Update() {
        if (birds.Count < minBirdsCount)
            SpawnFlock();

        if (Input.GetKeyDown(KeyCode.Keypad5))
            SpawnFlock();

        foreach (var bird in birds) {
            var neighbors = birds.Where(b => (bird.position - b.position).magnitude < attractionDistance).ToList();
            var averagePosition = neighbors.Aggregate(Vector2.zero, (acc, b) => acc + b.position) / neighbors.Count;
            var averageSpeed = neighbors.Aggregate(Vector2.zero, (acc, b) => acc + b.speed) / neighbors.Count;
            Vector2 avoidance = default;
            foreach (var neighbor in neighbors) {
                var distance = Vector2.Distance(bird.position, neighbor.position);
                if (distance > bird.avoidanceDistance)
                    continue;
                var closeness = bird.avoidanceDistance - distance;
                avoidance += (bird.position - neighbor.position) * closeness;
            }

            avoidance /= neighbors.Count;

            bird.speed += Time.deltaTime * ((averagePosition - bird.position) * 10f / neighbors.Count
                                            + (averageSpeed - bird.speed) * 20f / neighbors.Count
                                            + avoidance * 50f);

            // fewer birds fly together slower they do
            var modulatedSpeed = Mathf.Lerp(bird.speedLimits[0], bird.speedLimits[1], Mathf.Clamp01((float)(neighbors.Count - 1) / 5));
            if (bird.speed.magnitude < modulatedSpeed)
                bird.speed = Vector2.Lerp(bird.speed, bird.speed.normalized * modulatedSpeed, Time.deltaTime / 2);
            if (bird.speed.magnitude > bird.speedLimits[1])
                bird.speed = bird.speed.normalized * bird.speedLimits[1];

            bird.position += bird.speed * Time.deltaTime;

            if (bird.position.x > min.x && bird.position.x < max.x && bird.position.y > min.y && bird.position.y < max.y)
                newBirds.Add(bird);
            else {
                Destroy(bird.view.gameObject);
                bird.view = null;
            }
        }

        (birds, newBirds) = (newBirds, birds);
        newBirds.Clear();

        foreach (var bird in birds) {
            //Draw.ingame.Cross();
            // Draw.ingame.SolidBox(bird.position.ToVector3(), .1f, Color.white);
            bird.view.position = bird.position.ToVector3();
            bird.view.rotation = Quaternion.LookRotation(bird.speed.ToVector3(), Vector3.up);
        }

        if (camera) {
            var height = camera.transform.position.y;
            foreach (var bird in birds) 
                bird.renderer.shadowCastingMode = height > heightThreshold ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    public Camera camera;
    public float heightThreshold = 10;

    [Command()]
    public void SpawnFlock() {
        SpawnFlock(Random.Range(flockSize[0], flockSize[1]));
    }
    public void SpawnFlock(int n) {
        Vector2 spawnPosition = default;
        var center = Vector2.Lerp(min, max, .5f);
        if (Random.Range(0, 2) == 0) {
            var randomY = Random.Range(min.y, max.y);
            spawnPosition = new Vector2(min.x + 1, randomY);
        }
        else {
            var randomX = Random.Range(min.x, max.x);
            spawnPosition = new Vector2(randomX, max.y - 1);
        }

        for (var i = 0; i < n; i++) {
            var bird = new Bird();
            bird.position = spawnPosition;
            bird.position += new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * jitter;
            bird.speed = (center - spawnPosition).normalized * bird.speedLimits[1] * Random.value;
            bird.speedLimits[1] *= Random.Range(.5f, 1f);
            bird.avoidanceDistance = Random.Range(.5f, 1.5f);
            bird.view = Instantiate(prefab, bird.position.ToVector3(), Quaternion.identity);
            bird.renderer = bird.view.GetComponentsInChildren<MeshRenderer>().Single(mr => mr.enabled);
            birds.Add(bird);
        }

    //        Debug.Log(birds.Count);
    }

    public float jitter = .1f;
}