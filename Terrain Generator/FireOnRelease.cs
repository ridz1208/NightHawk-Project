using UnityEngine;
using System.Collections;

public class FireOnRelease : MonoBehaviour {
	Rigidbody rigid;

	public GameObject launchPoint;
	public float moveSpeed;
	public float range;
	public bool fired;

	private float distTraveled;
	private Vector3 offset;
	private Vector3 velocity;
	private Renderer rend;
	private Vector3 previousPosition;
	private Vector3 direction;
	//private Rigidbody planeRigid;

	void Start () { 
		fired = false;
		rigid = GetComponent<Rigidbody> ();
		offset = transform.position - launchPoint.transform.position;
		rend = GetComponent<Renderer>();
		rend.enabled = false;
		previousPosition = new Vector3(0,0,0);
		direction = Vector3.forward;
		//planeRigid = plane.GetComponent<Rigidbody> ();
	}

	void Update () {
		if (fired) {
			rend.enabled = true;
		}
	}
		
	void FixedUpdate () {
		if (fired) {
			//rigid.velocity = velocity * moveSpeed * Time.deltaTime;

			//transform.Translate (velocity * moveSpeed * Time.deltaTime);

			transform.position += moveSpeed * direction * Time.deltaTime;

			//rigid.velocity = direction * moveSpeed * Time.deltaTime;

			distTraveled += moveSpeed * Time.deltaTime;

		} else {
			direction = launchPoint.transform.position - previousPosition;
			Debug.Log (direction);
			transform.position = launchPoint.transform.position + offset;
			rigid.rotation = Quaternion.Euler(direction);
		}
		previousPosition = launchPoint.transform.position;
	}

	void release () {
		/*Rigidbody otherRigid = plane.GetComponent<Rigidbody> (); 
		Vector3 vel = Vector3.forward;
		vel = otherRigid.rotation * vel;
		Debug.Log (otherRigid.rotation);
		velocity = vel;*/
		fired = true;
	}
}
