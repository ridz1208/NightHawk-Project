using UnityEngine;
using System.Collections;

public static class flightPhysics {

	readonly static float maxSpeed = 1000f;
	readonly static float minSpeed = 100f;

	public static float getMaxSpeed (){
		return maxSpeed;
	}

	public static float getMinSpeed (){
		return minSpeed;
	}

	public static float accelerate(float speed){
		if (speed < maxSpeed - 5) {
			return speed + 5;
		} else {
			return speed;
		}
	}

	public static float decelerate(float speed){
		if (speed > maxSpeed + 5) {
			return speed - 5;
		} else {
			return speed;
		}
	}

	public static Quaternion getNewState(Vector3 ang, float[] commands, float timeElapsed){
		Quaternion newRot = Quaternion.identity;
		float roll = 0f;
		float yaw = 0f;
		float pitch = 0f;

		float softTilt = getSoftTilt(ang.z);
		// horizontal --> float[0], vertical --> commands[1], hasVerticalInput and hasHorizontalInput is replaced by check for 
		// horizontalInput and verticalInput == 0, Time.deltatime multiplication is removed altogether
		roll = getPlaneRoll(ang.z, softTilt, commands[0]);
		yaw = getPlaneYaw (commands[0], softTilt);
		pitch = getPlanePitch (ang.x, commands[1]);

		newRot.eulerAngles = new Vector3(pitch*timeElapsed, yaw*timeElapsed, roll*timeElapsed);

		return newRot;
	}

	private static float getSoftTilt(float rotationz){
		float rightleftsoft = 0f;
		if ((Input.GetAxis ("Horizontal")<=0)&&(rotationz >0)&&(rotationz <90)) rightleftsoft = rotationz*2.2f/100*-1;//linksrum || to the left
		if ((Input.GetAxis ("Horizontal")>=0)&&(rotationz >270)) rightleftsoft= (7.92f-rotationz*2.2f/100);//rechtsrum ||to the right

		if (rightleftsoft>1) rightleftsoft =1;
		if (rightleftsoft<-1) rightleftsoft =-1;
		
		if ((rightleftsoft>-0.01) && (rightleftsoft<0.01)) rightleftsoft=0;
		
		return rightleftsoft;
	}

	private static float getPlaneRoll(float rotationZ, float softTilt, float horizontalInput){
		float roll = 0f;
		float turnSpeed = 100f;
		float returnSpeed = 80f;

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

		return roll;
	}

	private static float getPlanePitch(float rotationX, float verticalInput){
		float pitch = 0f;
		float turnSpeed = 80f;
		float returnSpeed = 50f;

		if (verticalInput != 0){
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
		
		return pitch;
	}

	private static float getPlaneYaw(float horizontalInput, float softTilt){
		float yaw = 0f;
		float turnSpeed = 80f;

		//float adjustedTurnSpeed = turnSpeed * (1.0f - 0.5f * speed / maxSpeed);
		float adjustedTurnSpeed = 80;
		yaw += horizontalInput * adjustedTurnSpeed;

		return yaw;
	}

}