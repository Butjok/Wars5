using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Formats.Alembic.Importer;

public class AlembicTest : MonoBehaviour {

    public AlembicStreamPlayer alembicStreamPlayer;

    private void OnEnable() {
        alembicStreamPlayer = GetComponent<AlembicStreamPlayer>();
        Assert.IsTrue(alembicStreamPlayer);
        alembicStreamPlayer.CurrentTime = 0;
    }

    private void Update() {
        alembicStreamPlayer.CurrentTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.P)) {
            Rewind();
        }
    }

    [ContextMenu(nameof(Rewind))]
    public void Rewind() {
        alembicStreamPlayer.CurrentTime = 0;
    }
}