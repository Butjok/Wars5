using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BarrelAssembly : MonoBehaviour {
	
	public Barrel[] barrelOrder = Array.Empty<Barrel>();
	public int index = -1;
	public float cooldown = .25f;
	public float? lastShot;
	public IEnumerator volley;
	
	public bool Shoot() {
		if (barrelOrder.Length == 0||lastShot is {} time && time+cooldown>Time.time)
			return false;
		index = (index + 1) % barrelOrder.Length;
		barrelOrder[index].Shoot();
		lastShot = Time.time;
		return true;
	}
	public void Volley(bool stopPrevious=true) {
		if (volley != null) {
			StopCoroutine(volley);
			volley = null;
		}
		//volley=
		for (var i=0;i<barrelOrder.Length;i++)
			Shoot();
	}
	/*private IEnumerator VolleySequence() {
		
	}*/
	
	public void Update() {
		if (Input.GetKeyDown(KeyCode.Equals)) {
			Volley();
		}
	}
}