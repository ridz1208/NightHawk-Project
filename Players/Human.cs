using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour, IPlayer {
	private string name = "Human Player";
	public Airplane p;
	private int health;

	void Start()
	{
		health = 100;
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Q))
		{
			p.setSpeed (1);
		}
		if (Input.GetKey(KeyCode.Z))
		{
			p.setSpeed (-1);
		}
			
		if (Input.GetKey(KeyCode.Space))
		{
			fireMissile ();
		}
	}

	void FixedUpdate()
	{
		//Simulate arcade style airplane controls
		float[] commands = new float[2];
		commands[0] = Input.GetAxis("Horizontal");
		commands[1] = Input.GetAxis("Vertical");

		p.steerPlane (commands);
	}

	public string getName(){
		return name;
	}
	public void setName(string n){
		name =n;
	}
	public void fireMissile(){
		p.releaseMissile();
	}
	public Vector3 getPos(){
		
		return p.getPosition ();
	}
	public Vector3 getDir(){
		return p.getDirection();
	}
	public float getSpeed(){
		return p.getSpeed();
	}
	public int getHealth(){
		return health;
	}
	public void setHealth(int h){
		health = h;
	}


}