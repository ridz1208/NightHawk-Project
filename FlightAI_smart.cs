﻿using UnityEngine;
using System.Collections;

public class FlightAI_smart: MonoBehaviour {

	Rigidbody rigid;

	Camera frontCamera;
	Camera rightCamera;
	Camera leftCamera;
	Camera mainCamera;

	bool accelerating = false;
	bool decelerating = false;

	public float moveSpeed = 500f;
	float turnSpeed = 75f;

	public Rigidbody Missile;
	public GameObject launchPoint;

	readonly float maxSpeed = 1000f;
	readonly float minSpeed = 100f;
	//for random target generation
	private bool resetRnd = true;
	Vector3 rndDir = new Vector3(0,0,0);

	// Use this for initialization
	void Start()
	{
		frontCamera = (Camera)GameObject.Find("Front Camera").GetComponent<Camera>();
		rightCamera = (Camera)GameObject.Find("Right Camera").GetComponent<Camera>();
		leftCamera = (Camera)GameObject.Find("Left Camera").GetComponent<Camera>();
		mainCamera = (Camera)GameObject.Find("Main Camera").GetComponent<Camera>();

		rigid = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		/*Speed Control with camera fade in or out*/
		mainCamera.enabled = true;
		frontCamera.enabled = false;
		leftCamera.enabled = false;
		rightCamera.enabled = false;


		if (moveSpeed < minSpeed)
			moveSpeed = minSpeed;
		else if (moveSpeed > maxSpeed)
			moveSpeed = maxSpeed;
	}

	Vector3 getRnd()
	{
		
		if (resetRnd == true) {
			//generate random point infront of AI
			float rndX = Random.Range (-500f, 500f);
			float rndY = Random.Range (-500f, 500f);
			float rndZ = Random.Range (0f, 500f);

			//convert random point to world coordinate
			rndDir = new Vector3(rndX, rndY,rndZ);
			//rndDir = transform.TransformPoint (rndDir);

			//return unit vector to random point
			//rndDir = rndDir / (Mathf.Sqrt (((rndZ - transform.position.z) * (rndZ - transform.position.z)) + (rndY - transform.position.y) * (rndY - transform.position.y)) + ((rndX - transform.position.x) * (rndX - transform.position.x))));
			resetRnd = false;
			return rndDir;

		} else {
			return rndDir;
		}

			
	}

	void FixedUpdate()
	{
		//Simulate arcade style airplane controls
		Quaternion newRot = Quaternion.identity;
		float roll = 0f;
		float yaw = 0f;
		float pitch = 0f;
		float horizontalInput = 0f;
		float verticalInput = 0f;
		float softTilt = 0f;


		float targetX = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.x;
		float targetY = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.y;
		float targetZ = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.z;

		float rotationx = transform.eulerAngles.x;
		float rotationy = transform.eulerAngles.y;
		float rotationz = transform.eulerAngles.z;


		//get unit vector between opponent and AI
		var tar = new Vector3 (targetX-transform.position.x, targetY-transform.position.y, targetZ-transform.position.z);
		float distance = (Mathf.Sqrt (((targetZ - transform.position.z) * (targetZ - transform.position.z)) + ((targetY - transform.position.y) * (targetY - transform.position.y)) + ((targetX - transform.position.x) * (targetX - transform.position.x)))); 
		tar = tar / distance;
		//get angle between opponent position from AI and AI direction
		float tarAngle = Vector3.Angle (transform.forward, tar);
		print (distance);
		//get opponent position in AI coordinates
		var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);





		//logic: if opponent in front, keep safe distance. if opponent behind do evasif maneuver
		if (worldToPlane.z > 0 && tarAngle < 15) {
			//determining input 
			if (worldToPlane.x > 1 ) {
				horizontalInput = 1.0f;
			} else if (worldToPlane.x < - 1){
				horizontalInput = -1.0f;
			}
			if (tar.y > transform.forward.y + 0.05) {
				verticalInput = -1.0f;
			} else if (tar.y < transform.forward.y -0.05){
				verticalInput = 1.0f;
			}

			//opponent in front
			print ("attacking opponent");
			if (distance < 50) {
				moveSpeed = moveSpeed * (distance / 50);
			} else {
				moveSpeed = moveSpeed + 5;
			}

			/*if (tarAngle < 5) {
				Instantiate (Missile, launchPoint.transform.position, transform.rotation);
			}*/

			/*softTilt = getSoftTilt(tar, transform.eulerAngles.z);
			roll = getPlaneRoll (tar, rotationz, softTilt, Time.deltaTime, tarAngle, worldToPlane);
			yaw = getPlaneYaw (tar, Time.deltaTime, tarAngle, worldToPlane);
			pitch = getPlanePitch (tar, rotationx, Time.deltaTime);*/

			//TODO :: check if oppenent heading towards AI
		} else if (worldToPlane.z < 0 && tarAngle > 170) {
			//opponent behind
			//moveSpeed = moveSpeed + 2;
			print ("evading opponent");
			//create random target in positive Z and follow it untill no longuer in front
			Vector3 target = getRnd();

			targetX = target.x;
			targetY = target.y;
			targetZ = target.z;

			tar = new Vector3 (targetX-transform.position.x, targetY-transform.position.y, targetZ-transform.position.z);
			tar = tar / (Mathf.Sqrt (((targetZ - transform.position.z) * (targetZ - transform.position.z)) + ((targetY - transform.position.y) * (targetY - transform.position.y)) + ((targetX - transform.position.x) * (targetX - transform.position.x))));

			worldToPlane = target;
			tarAngle = Vector3.Angle (transform.forward, tar);

			//determining input 
			if (worldToPlane.x > 1 ) {
				horizontalInput = 1.0f;
			} else if (worldToPlane.x < - 1){
				horizontalInput = -1.0f;
			}
			if (tar.y > transform.forward.y + 0.05) {
				verticalInput = -1.0f;
			} else if (tar.y < transform.forward.y -0.05){
				verticalInput = 1.0f;
			}

			/*softTilt = getSoftTilt(tar, transform.eulerAngles.z);
			roll = getPlaneRoll (tar, rotationz, softTilt, Time.deltaTime, tarAngle, worldToPlane);
			yaw = getPlaneYaw (tar, Time.deltaTime, tarAngle, worldToPlane);
			pitch = getPlanePitch (tar, rotationx, Time.deltaTime);*/

		} else {
			//determining input 
			if (worldToPlane.x > 1 ) {
				horizontalInput = 1.0f;
			} else if (worldToPlane.x < - 1){
				horizontalInput = -1.0f;
			}
			if (tar.y > transform.forward.y + 0.05) {
				verticalInput = -1.0f;
			} else if (tar.y < transform.forward.y -0.05){
				verticalInput = 1.0f;
			}

			if (!resetRnd) {resetRnd = true;}
			// follow the opponent.

			/*softTilt = getSoftTilt(tar, transform.eulerAngles.z);
			roll = getPlaneRoll (tar, rotationz, softTilt, Time.deltaTime, tarAngle, worldToPlane);
			yaw = getPlaneYaw (tar, Time.deltaTime, tarAngle, worldToPlane);
			pitch = getPlanePitch (tar, rotationx, Time.deltaTime);*/

		}

		//roll = 0;
		//yaw = 0;
		//pitch = 0;
		//newRot.eulerAngles = new Vector3(pitch, yaw, roll);
		//rigid.rotation *= newRot;
		float[] commands = new float[2];
		commands [0] = horizontalInput;
		commands [1] = verticalInput;

		rigid.rotation *= flightPhysics.getNewState(transform.eulerAngles, commands, Time.deltaTime);

		Vector3 newPos = Vector3.forward;
		newPos = rigid.rotation * newPos;
		rigid.velocity = newPos * (Time.deltaTime * moveSpeed);

	}


	/*float getSoftTilt(Vector3 tar, float rotationz){
		float rightleftsoft = 0f;
		float horizontalInput = 0f;
		//get angle between current direction and target direction
		//float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		//var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		//print (angle);
		//print (worldToPlane);

		//if target on left or right

		if ((horizontalInput<=0)&&(rotationz >0)&&(rotationz <90)) rightleftsoft = rotationz*2.2f/100*-1;//linksrum || to the left
		if ((horizontalInput>=0)&&(rotationz >270)) rightleftsoft= (7.92f-rotationz*2.2f/100);//rechtsrum ||to the right

		if (rightleftsoft>1) rightleftsoft =1;
		if (rightleftsoft<-1) rightleftsoft =-1;

		if ((rightleftsoft>-0.01) && (rightleftsoft<0.01)) rightleftsoft=0;

		return rightleftsoft;
	}

	float getPlaneRoll(Vector3 tar, float rotationZ, float softTilt, float timeElapsed, float angle, Vector3 worldToPlane){
		float roll = 0f;
		float horizontalInput=0f;
		float turnSpeed = 100f;
		float returnSpeed = 80f;

		//get angle between current direction and target direction
		//float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		//var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		//print (angle);
		//print (worldToPlane);

		//if target on left or right

		// checks if plane is tilted in the direction opposite that of current horizontal manoeuvre
		if (horizontalInput != 0) {
			if ((rotationZ > 1) && (rotationZ < 180) && (horizontalInput > 0))
				roll -= turnSpeed;
			if ((rotationZ > 180) && (rotationZ < 359) && (horizontalInput < 0))
				roll += turnSpeed;
			else roll += turnSpeed * -(1.0f - Mathf.Abs (softTilt)) * horizontalInput;

		} else {
			if ((rotationZ > 1) && (rotationZ < 135))
				roll -= returnSpeed;
			if ((rotationZ > 225) && (rotationZ < 359))
				roll += returnSpeed;
		}

		return roll * timeElapsed;


	}

	float getPlanePitch(Vector3 tar, float rotationX, float timeElapsed){

		float verticalInput = 0f;
		float pitch = 0f;
		float turnSpeed = 80f;
		float returnSpeed = 50f;

		//decides if rotation should be up or down
		if (tar.y > transform.forward.y + 0.05) {
			verticalInput = -1.0f;
		} else if (tar.y < transform.forward.y -0.05){
			verticalInput = 1.0f;
		}

		if (verticalInput!=0){
			if (verticalInput <= 0)
				pitch += verticalInput * 80;
			else {
				if (rotationX < 90)
					pitch += (1.0f - rotationX / 90.0f) * verticalInput * 80;
				else
					pitch += verticalInput * 80;
			}
		}

		else{
			if ((rotationX > 1) && (rotationX < 180)) pitch -= returnSpeed/4;
			if ((rotationX > 180) && (rotationX <359)) pitch += returnSpeed/4;
		}
		
		return pitch * timeElapsed;

	}

	float getPlaneYaw(Vector3 tar, float timeElapsed, float angle, Vector3 worldToPlane){
		float yaw = 0f;
		float turnSpeed = 80f;
		float horizontalInput = 0f;
		float adjustedTurnSpeed = 80;

		//get angle between current direction and target direction
		//float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		//var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		//print (angle);
		//print (worldToPlane);

		//if target on left or right
		if (worldToPlane.x > 1 ) {
			horizontalInput = 1.0f;
		} else if (worldToPlane.x < - 1){
			horizontalInput = -1.0f;
		}
			
		yaw += horizontalInput * adjustedTurnSpeed;
		return yaw * timeElapsed;

	}*/

	void OnTriggerEnter(Collider other) 
	{
		//print ("Collision occurs");
		if (other.gameObject.CompareTag("Missile"))
		{
			//Explode ();
			//var exp = GetComponent<ParticleSystem> ();
			var exp = GameObject.Find("Dadum");
			var explo = exp.GetComponent<ParticleSystem> ();
			explo.Play ();
			//exp.playOnAwake = false;
			Destroy(gameObject, 0.1f);
		}

	}
}