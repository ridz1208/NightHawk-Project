using UnityEngine;
using System.Collections;

public class Airplane : MonoBehaviour {
	
	Camera frontCamera;
	Camera rightCamera;
	Camera leftCamera;
	Camera mainCamera;

	bool accelerating = false;
	bool decelerating = false;

	private Rigidbody rigid;
	public Rigidbody Missile;
	public GameObject launchPoint;

	private Vector3 missileOffset;
	private Vector3 missilePos;
	private ParticleSystem exp;

	private float moveSpeed = 200f;
	private int health;

	void Start()
	{
		frontCamera = (Camera)GameObject.Find("Front Camera").GetComponent<Camera>();
		rightCamera = (Camera)GameObject.Find("Right Camera").GetComponent<Camera>();
		leftCamera = (Camera)GameObject.Find("Left Camera").GetComponent<Camera>();
		mainCamera = (Camera)GameObject.Find("Main Camera").GetComponent<Camera>();
		health = 100;

		rigid = GetComponent<Rigidbody>();
	}

	void Update()
	{
		/*Camera Control*/
		if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Keypad5))
		{
			mainCamera.enabled = false;
			leftCamera.enabled = false;
			rightCamera.enabled = false;

			frontCamera.enabled = true;
		}
		if (Input.GetKey(KeyCode.Keypad4))
		{
			mainCamera.enabled = false;
			frontCamera.enabled = false;
			rightCamera.enabled = false;

			leftCamera.enabled = true;
		}
		if (Input.GetKey(KeyCode.Keypad6))
		{
			mainCamera.enabled = false;
			frontCamera.enabled = false;
			leftCamera.enabled = false;

			rightCamera.enabled = true;
		}
	}


	public void releaseMissile (){
		Instantiate(Missile, launchPoint.transform.position, transform.rotation);
	}
	public void steerPlane (float[] commands ){
		rigid.rotation *= flightPhysics.getNewState(getDirection(), commands, Time.deltaTime);

		Vector3 forth = Vector3.forward;
		forth = rigid.rotation * forth;
		rigid.velocity = forth * (Time.deltaTime * moveSpeed);
	}
	public Vector3 getPosition (){
		return transform.position;
	}
	public Vector3 getDirection (){
		return transform.eulerAngles;
	}
	public float getSpeed (){
		return moveSpeed;
	}
	public void setSpeed(float s){
		if (s > 0) {
			
			if (!accelerating && mainCamera.fieldOfView != 80 && moveSpeed < flightPhysics.getMaxSpeed())
			{
				StartCoroutine(cameraFadeOut());
			}
			if (moveSpeed >= flightPhysics.getMaxSpeed ()) {
				StartCoroutine (cameraStabilize ());
			}
			moveSpeed = flightPhysics.accelerate (moveSpeed);
			
		} else if (s < 0) {
			if (!decelerating && mainCamera.fieldOfView != 40 && moveSpeed > flightPhysics.getMinSpeed())
			{
				StartCoroutine(cameraFadeIn());
			}
			if (moveSpeed <= flightPhysics.getMinSpeed ()) {
				StartCoroutine (cameraStabilize ());
			}
			moveSpeed = flightPhysics.decelerate (moveSpeed);
		}
	}

	public int getHealth(){
		return health;
	}
	public void setHealth(int h){
		health = h;
	}

	IEnumerator cameraStabilize()
	{
		while (mainCamera.fieldOfView != 60)
		{
			if (mainCamera.fieldOfView > 60)
			{
				mainCamera.fieldOfView -= 1;
			}
			else {
				mainCamera.fieldOfView += 1;
			}
			yield return new WaitForFixedUpdate();
		}

	}

	IEnumerator cameraFadeOut()
	{
		for (float i = 60f; i <= 80f; i += 2f)
		{
			accelerating = true;
			mainCamera.fieldOfView = i;
			yield return new WaitForFixedUpdate();
			accelerating = false;
		}
	}

	IEnumerator cameraFadeIn()
	{
		for (float i = 60f; i >= 40f; i -= 2f)
		{
			decelerating = true;
			mainCamera.fieldOfView = i;
			yield return new WaitForFixedUpdate();
			decelerating = false;
		}
	}

	void OnTriggerEnter(Collider other) 
	{
		//print ("Collision occurs");
		if (other.gameObject.CompareTag("Missile"))
		{
			if (health == 10) {
				exp = (ParticleSystem)gameObject.AddComponent <ParticleSystem> ();
				exp.Play ();
				//exp.playOnAwake = false;
				Destroy (gameObject);
			} else {
				setHealth (getHealth () - 10);
			}

		}
		if (other.gameObject.CompareTag ("FirstAid")) {
			if (getHealth () <= 50) {
				setHealth (getHealth () + 50);
			} else {
				setHealth (100);
			}
		}

	}
}
