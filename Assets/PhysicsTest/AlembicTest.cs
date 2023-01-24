using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Formats.Alembic.Importer;

public class AlembicTest : MonoBehaviour {

    public AlembicStreamPlayer alembicStreamPlayer;

    private void Awake() {
        alembicStreamPlayer = GetComponent<AlembicStreamPlayer>();
        Assert.IsTrue(alembicStreamPlayer);
    }

    private void Update() {
        alembicStreamPlayer.CurrentTime = Time.time;
    }
}