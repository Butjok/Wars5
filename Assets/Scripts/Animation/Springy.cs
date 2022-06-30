using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]//Diverts th
               //[exece selection to this object
               [ExecuteInEditMode]
public class Springy : MonoBehaviour
{
	public Transform springTarget;
	public float drag = 2.5f;//drag
	public float springForce = 80.0f;//Spring
	public Vector3 position;
	public Vector3 velocity;
	public float length = 1;
	
	public void Start() {
		position = springTarget.position;
	}
	public void Update() {
		position += velocity * Time.deltaTime;
		var to = springTarget.position - position;
		var force = to * springForce;
		force += -velocity * drag;
		velocity += force * Time.deltaTime;
		transform.rotation=Quaternion.LookRotation(position-transform.position);
	}
	[ContextMenu(nameof(Clear))]
	public void Clear() {
		position = springTarget.position;
		velocity=Vector3.zero;
	}
	private void OnDrawGizmos() {
		//Gizmos.DrawSphere(position,.05f);
		
		//Gizmos.DrawLine(transform.position,(position-transform.position).normalized*length+transform.position);
	}

#if false
	void Start()
	{
		SpringRB = springObj.GetComponent<Rigidbody>();//Find the RigidBody component
		springObj.transform.parent = null;//Take the spring out of the hierarchy
	} 
	
	

	void FixedUpdate()
	{
		//Sync the rotation 
		SpringRB.transform.rotation = this.transform.rotation;

		//Calculate the distance between the two points
		LocalDistance = springTarget.InverseTransformDirection(springTarget.position - springObj.position);
		SpringRB.AddRelativeForce((LocalDistance) * springForce);//Apply Spring

		//Calculate the local velocity of the springObj point
		LocalVelocity = (springObj.InverseTransformDirection(SpringRB.velocity));
		SpringRB.AddRelativeForce((-LocalVelocity) * drag);//Apply drag

		//Aim the visible geo at the spring target
		//GeoParent.transform.LookAt(springObj.position, new Vector3(0, 0, 1));
	}
#endif
}