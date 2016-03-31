using UnityEngine;
using System.Collections;

public abstract class AI : MonoBehaviour, IPlayer {
	private string ID = "AI Player";
	private Airplane p;
	IPlayer opponent;

	void Start()
	{
		opponent = GameObject.Find ("Human");
	}

	public abstract void FixedUpdate ();

	public string getID(){
		return ID;
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

	public Vector3 getOpponentPosition(){
		return opponent.getPos ();
	}
	public float getOpponentSpeed(){
		return opponent.getSpeed ();
	}
	public Vector3 getOpponentDirection(){
		return opponent.getDir ();
	}

}
