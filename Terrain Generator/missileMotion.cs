using UnityEngine;
using System.Collections;

public class missileMotion : MonoBehaviour {
	//public GameObject launchpoint;
	public float moveSpeed;
	public float range;
	private Rigidbody rigid;
	private float distanceTraveled;

	// Use this for initialization
	void Start () {
		distanceTraveled = 0;
		rigid = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (distanceTraveled > range) {
			Destroy (gameObject);
		}
		Vector3 forward = transform.rotation * Vector3.forward;
		rigid.velocity = forward * (Time.deltaTime * moveSpeed);
		distanceTraveled += moveSpeed * Time.deltaTime;
		Debug.Log (distanceTraveled);
	}

	void OnCollisionEnter(Collision hit) 
	{
		print ("Collision occurs");
		if (hit.gameObject.CompareTag("Plane"))
		{
			hit.gameObject.SetActive(false);
		}

	}
}
