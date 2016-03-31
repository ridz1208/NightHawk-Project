using UnityEngine;
using System.Collections;

public interface IPlayer{
	void fireMissile();
	Vector3 getPos();
	Vector3 getDir();
	float getSpeed();
	int getHealth();
	void setHealth (int h);
}