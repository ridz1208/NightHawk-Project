using UnityEngine;
using System.Collections;

public class planeController : MonoBehaviour {

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

	public Rigidbody Missile;
	public GameObject launchPoint;

	private Vector3 missileOffset;
	private Vector3 missilePos;

	// Use this for initialization
	void Start()
	{
		frontCamera = (Camera)GameObject.Find("Front Camera").GetComponent<Camera>();
		rightCamera = (Camera)GameObject.Find("Right Camera").GetComponent<Camera>();
		leftCamera = (Camera)GameObject.Find("Left Camera").GetComponent<Camera>();
		mainCamera = (Camera)GameObject.Find("Main Camera").GetComponent<Camera>();

		/*missilePos = new Vector3 ();
		missilePos.Set(0f, -0.16f, 0.5f);
		missileOffset = missilePos - transform.position;*/

		rigid = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		//missilePos = transform.position + missileOffset;
		/*Speed Control with camera fade in or out*/
		mainCamera.enabled = true;
		frontCamera.enabled = false;
		leftCamera.enabled = false;
		rightCamera.enabled = false;

		if (Input.GetKey(KeyCode.Q))
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
			//print("Space pressed");
			Instantiate(Missile, launchPoint.transform.position, transform.rotation);
		}
	}

	void FixedUpdate()
	{
		//Simulate arcade style airplane controls
		Quaternion newRot = Quaternion.identity;
		float roll = 0f;
		float yaw = 0f;
		float pitch = 0f;

		float rotationx = transform.eulerAngles.x;
		float rotationy = transform.eulerAngles.y;
		float rotationz = transform.eulerAngles.z;

		float softTilt = getSoftTilt(transform.eulerAngles.z);
		//var dir = new Vector3 (Mathf.Cos (rotationy) * Mathf.Cos (rotationx), Mathf.Sin (rotationy) * Mathf.Cos (rotationx), Mathf.Sin (rotationx));
		//print (transform.forward);

		//roll = Input.GetAxis("Horizontal") * (Time.deltaTime * turnSpeed);
		roll = getPlaneRoll(rotationz, softTilt, Input.GetButton ("Horizontal"), Input.GetAxis("Horizontal"), Time.deltaTime);
		yaw = getPlaneYaw (Input.GetAxis ("Horizontal"), moveSpeed, maxSpeed, softTilt, Time.deltaTime);//Input.GetAxis("Horizontal") * (Time.deltaTime * turnSpeed);
		pitch = getPlanePitch (rotationx, Input.GetButton ("Vertical"), Input.GetAxis ("Vertical"), Time.deltaTime);

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

	float getSoftTilt(float rotationz){
		float rightleftsoft = 0f;
		if ((Input.GetAxis ("Horizontal")<=0)&&(rotationz >0)&&(rotationz <90)) rightleftsoft = rotationz*2.2f/100*-1;//linksrum || to the left
		if ((Input.GetAxis ("Horizontal")>=0)&&(rotationz >270)) rightleftsoft= (7.92f-rotationz*2.2f/100);//rechtsrum ||to the right

		if (rightleftsoft>1) rightleftsoft =1;
		if (rightleftsoft<-1) rightleftsoft =-1;
		
		if ((rightleftsoft>-0.01) && (rightleftsoft<0.01)) rightleftsoft=0;
		
		return rightleftsoft;
	}

	float getPlaneRoll(float rotationZ, float softTilt, bool hasHorizontlInput, float horizontalInput, float timeElapsed){
		float roll = 0f;
		float turnSpeed = 100f;
		float returnSpeed = 80f;

		// checks if plane is tilted in the direction opposite that of current horizontal manoeuvre
		if (hasHorizontlInput) {
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

	float getPlanePitch(float rotationX, bool hasVerticalInput, float verticalInput, float timeElapsed){
		float pitch = 0f;
		float turnSpeed = 80f;
		float returnSpeed = 50f;

		if (hasVerticalInput){
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
			if ((rotationX > 1) && (rotationX < 180)) pitch -= returnSpeed;
			if ((rotationX > 180) && (rotationX <359)) pitch += returnSpeed;
		}
		
		return pitch * timeElapsed;
	}
		

	float getPlaneYaw(float horizontalInput, float speed, float maxSpeed, float softTilt, float timeElapsed){
		float yaw = 0f;
		float turnSpeed = 80f;

		//float adjustedTurnSpeed = turnSpeed * (1.0f - 0.5f * speed / maxSpeed);
		float adjustedTurnSpeed = 80;
		yaw += horizontalInput * adjustedTurnSpeed;

		return yaw * timeElapsed;
	}
}
