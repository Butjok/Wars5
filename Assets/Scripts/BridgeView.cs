using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Formats.Alembic.Importer;

[RequireComponent(typeof(AlembicStreamPlayer))]
public class BridgeView : MonoBehaviour {

	public Bridge bridge;
	public AlembicStreamPlayer alembicStreamPlayer;

	public GameObject intact;
	public GameObject destruction;
	public GameObject destroyed;

	public int hp;
	public void SetHp(int value, bool animateDestruction = false) {
		
		hp = value;

		if (value > 0) {
			intact.SetActive(true);
			destroyed.SetActive(false);
			destruction.SetActive(false);
		}
		
		if (value <= 0) {
			
			// turn off everything
			intact.SetActive(false);
			destroyed.SetActive(false);
			destruction.SetActive(false);
			
			if (animateDestruction)
				StartCoroutine(DestructionAnimation());
			else
				destroyed.SetActive(true);
		}
	}
	
	private void Awake() {
		alembicStreamPlayer = GetComponent<AlembicStreamPlayer>();
		Assert.IsTrue(alembicStreamPlayer);
	}

	public IEnumerator DestructionAnimation() {
		destruction.SetActive(true);
		alembicStreamPlayer.CurrentTime = 0;
		while (alembicStreamPlayer.CurrentTime <= alembicStreamPlayer.EndTime) {
			yield return null;
			alembicStreamPlayer.CurrentTime += Time.deltaTime;
		}
		destruction.SetActive(false);
		destroyed.SetActive(true);
	}
}