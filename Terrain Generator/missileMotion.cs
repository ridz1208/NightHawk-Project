using UnityEngine;
using System.Collections;

public class missileMotion : MonoBehaviour {
	//public GameObject launchpoint;
	public float moveSpeed;
	public float range;
	//private ParticleSystem exp;
	private Rigidbody rigid;
	private float distanceTraveled;

	// Use this for initialization
	void Start () {
		distanceTraveled = 0;
		rigid = GetComponent<Rigidbody> ();
		//exp = (ParticleSystem)gameObject.AddComponent <ParticleSystem>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (distanceTraveled > range) {
			Destroy (gameObject);
		}
		Vector3 forward = transform.rotation * Vector3.forward;
		rigid.velocity = forward * (Time.deltaTime * moveSpeed);
		distanceTraveled += moveSpeed * Time.deltaTime;
		//Debug.Log (distanceTraveled);
	}

	/*void OnCollisionEnter(Collision hit) 
	{
		print ("Collision occurs");
		if (hit.gameObject.CompareTag("Plane"))
		{
			hit.gameObject.SetActive(false);
		}

	}*/

	/*void OnTriggerEnter(Collider other) 
	{
		//print ("Collision occurs");
		if (other.gameObject.CompareTag("Plane"))
		{
			//Explode ();
			//var exp = GetComponent<ParticleSystem> ();
			exp.Play ();
			//exp.playOnAwake = false;
			Destroy(other.gameObject, exp.duration);
		}

	}*/
}
