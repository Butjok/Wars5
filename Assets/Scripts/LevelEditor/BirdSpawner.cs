using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class BirdSpawner : MonoBehaviour {

    public Bird prefab;
    public int count = 1;
    public List<Bird> birds = new();

    public void OnEnable() {
        if (prefab) {
            var boxCollider = GetComponent<BoxCollider>();
            var min = boxCollider.bounds.min;
            var max = boxCollider.bounds.max;
            Assert.IsTrue(boxCollider);
            for (var i = 0; i < count; i++) {
                var position = new Vector3(Random.Range(min.x, max.x), 0, Random.Range(min.z, max.z));
                var rotation = Quaternion.Euler(0, Random.value * 360, 0);
                var bird = Instantiate(prefab, position, rotation, transform);
                bird.bounds = boxCollider.bounds;
                birds.Add(bird);
            }
        }
    }

    public void OnDisable() {
        foreach (var bird in birds)
            Destroy(bird.gameObject);
        birds.Clear();
    }
}