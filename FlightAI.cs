using UnityEngine;
using System.Collections;

public class FlightAI : MonoBehaviour {

	Rigidbody rigid;

	Camera frontCamera;
	Camera rightCamera;
	Camera leftCamera;
	Camera mainCamera;

	bool accelerating = false;
	bool decelerating = false;

	public float moveSpeed = 500f;
	float turnSpeed = 75f;

	readonly float maxSpeed = 100000f;
	readonly float minSpeed = 100f;

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

		/*if (Input.GetKey(KeyCode.Q))
		{
			moveSpeed += 5f;
			if (!accelerating && mainCamera.fieldOfView != 80 && moveSpeed < maxSpeed)
			{
				StartCoroutine(cameraFadeOut());
			}
		}
		if (Input.GetKeyUp(KeyCode.Q) || moveSpeed >= maxSpeed)
			StartCoroutine(cameraStabilize());//mainCamera.fieldOfView = 60f;
		
		if (Input.GetKey(KeyCode.Z))
		{
			moveSpeed -= 5f;
			if (!decelerating && mainCamera.fieldOfView != 40 && moveSpeed > minSpeed)
			{
				StartCoroutine(cameraFadeIn());
			}
		}
		if (Input.GetKeyUp(KeyCode.Z) || moveSpeed <= minSpeed)
			StartCoroutine(cameraStabilize());// mainCamera.fieldOfView = 60f;

		*/
		if (moveSpeed < minSpeed)
			moveSpeed = minSpeed;
		else if (moveSpeed > maxSpeed)
			moveSpeed = maxSpeed;


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

		if (Input.GetKey(KeyCode.Space))
		{
			
		}
	}

	void FixedUpdate()
	{
		//Simulate arcade style airplane controls
		Quaternion newRot = Quaternion.identity;
		float roll = 0f;
		float yaw = 0f;
		float pitch = 0f;

		float targetX = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.x;
		float targetY = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.y;
		float targetZ = GameObject.Find("MainMockPlane").GetComponent<Transform>().position.z;

		/*
		float targetX = 0;
		float targetY = 0;
		float targetZ = 0;
		*/
		float rotationx = transform.eulerAngles.x;
		float rotationy = transform.eulerAngles.y;
		float rotationz = transform.eulerAngles.z;

		//print (transform.forward);
		var tar = new Vector3 (targetX-transform.position.x, targetY-transform.position.y, targetZ-transform.position.z);
		tar = tar / (Mathf.Sqrt (((targetZ - transform.position.z) * (targetZ - transform.position.z)) + ((targetY - transform.position.y) * (targetY - transform.position.y)) + ((targetX - transform.position.x) * (targetX - transform.position.x))));
		//print (tar);


		float softTilt = getSoftTilt(tar, transform.eulerAngles.z,targetX);



		roll = getPlaneRoll(tar, targetX, targetY, targetZ, rotationz, softTilt, Time.deltaTime);
		yaw = getPlaneYaw (tar, targetX, targetY, targetZ, moveSpeed, maxSpeed, rotationy, Time.deltaTime);
		pitch = getPlanePitch (tar, targetX, targetY, targetZ, rotationx, Time.deltaTime);

		//roll = 0;
		//yaw = 0;
		//pitch = 0;
		newRot.eulerAngles = new Vector3(pitch, yaw, roll);
		rigid.rotation *= newRot;

		Vector3 newPos = Vector3.forward;
		newPos = rigid.rotation * newPos;
		rigid.velocity = newPos * (Time.deltaTime * moveSpeed);



	}
	/*Camera coroutines*/

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

	float getSoftTilt(Vector3 tar, float rotationz, float targetX){
		float rightleftsoft = 0f;
		float horizontalInput = 0f;
		//get angle between current direction and target direction
		float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		/*print (angle);
		print (worldToPlane);*/

		//if target on left or right
		if (worldToPlane.x > 1 ) {
			horizontalInput = 1.0f;
		} else if (worldToPlane.x < - 1){
			horizontalInput = -1.0f;
		}

		if ((horizontalInput<=0)&&(rotationz >0)&&(rotationz <90)) rightleftsoft = rotationz*2.2f/100*-1;//linksrum || to the left
		if ((horizontalInput>=0)&&(rotationz >270)) rightleftsoft= (7.92f-rotationz*2.2f/100);//rechtsrum ||to the right

		if (rightleftsoft>1) rightleftsoft =1;
		if (rightleftsoft<-1) rightleftsoft =-1;

		if ((rightleftsoft>-0.01) && (rightleftsoft<0.01)) rightleftsoft=0;

		return rightleftsoft;
	}

	float getPlaneRoll(Vector3 tar, float targetX, float targetY, float targetZ, float rotationZ, float softTilt, float timeElapsed){
		float roll = 0f;
		float horizontalInput=0f;
		float turnSpeed = 100f;
		float returnSpeed = 80f;

		//get angle between current direction and target direction
		float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		/*print (angle);
		print (worldToPlane);*/

		//if target on left or right
		if (worldToPlane.x > 1 ) {
			horizontalInput = 1.0f;
		} else if (worldToPlane.x < - 1){
			horizontalInput = -1.0f;
		}
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

	float getPlanePitch(Vector3 tar, float targetX, float targetY, float targetZ, float rotationX, float timeElapsed){
		
		float verticalInput = 0f;
		float pitch = 0f;
		float turnSpeed = 80f;
		float returnSpeed = 50f;
		/* DEPRECATED
		float hypX = Mathf.Sqrt (((targetZ - transform.position.z) * (targetZ - transform.position.z)) + ((targetY - transform.position.y) * (targetY - transform.position.y)));
		//absolute to avoid negative angles (backspin)
		float adjX = Mathf.Abs (targetZ - transform.position.z);
		float angleLimit = (Mathf.Acos (adjX / hypX))*180/Mathf.PI;

		//print(angleLimit);
	

		*/
		//decides if rotation should be up or down
		if (tar.y > transform.forward.y + 0.05) {
			verticalInput = -1.0f;
		} else if (tar.y < transform.forward.y -0.05){
			verticalInput = 1.0f;
		}

		//sets limit to rotation using the smalles value of angleLimit of 90deg
		if (verticalInput !=0){
			if (verticalInput < 0) {
				//print (rotationX);
				/*
				if (rotationX > (360-angleLimit))
					pitch += ((rotationX - (360-angleLimit)) / angleLimit) * verticalInput * 80;
				else */if (rotationX > 270)
					pitch += ((rotationX - 270) / 90.0f) * verticalInput * 80;
				else
					pitch += verticalInput * 80;
			}else {
				//print (rotationX);
				/*
				if (rotationX < 90 && rotationX < angleLimit)
					pitch += (1.0f - rotationX / angleLimit) * verticalInput * 80;
				else */if (rotationX < 90)
					pitch += (1.0f - rotationX / 90.0f) * verticalInput * 80;
				else
					pitch += verticalInput * 80;
			}
		}

		else{
			if ((rotationX > 1) && (rotationX < 180)) pitch -= returnSpeed;
			if ((rotationX > 180) && (rotationX <359)) pitch += returnSpeed;
		}
		return pitch * timeElapsed;
	}

	float getPlaneYaw(Vector3 tar, float targetX, float targetY, float targetZ, float speed, float maxSpeed, float rotationY, float timeElapsed){
		float yaw = 0f;
		float turnSpeed = 80f;
		float horizontalInput = 0f;
		float adjustedTurnSpeed = 80;

		//get angle between current direction and target direction
		float angle = Vector3.Angle (transform.forward, tar);
		//get target position in plane coordinates
		var worldToPlane = transform.InverseTransformPoint(GameObject.Find("MainMockPlane").GetComponent<Transform>().position);
		/*print (angle);
		print (worldToPlane);*/

		//if target on left or right
		if (worldToPlane.x > 1 ) {
			horizontalInput = 1.0f;
		} else if (worldToPlane.x < - 1){
			horizontalInput = -1.0f;
		}


		if (horizontalInput !=0){
			yaw += horizontalInput * adjustedTurnSpeed;
		}

		else{/*
			if ((rotationY > 1) && (rotationY < 180)) pitch -= returnSpeed;
			if ((rotationY > 180) && (rotationY <359)) pitch += returnSpeed;
		*/}
		return yaw * timeElapsed;

	}
}
