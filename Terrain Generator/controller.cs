using UnityEngine;
using System.Collections;

public class controller : MonoBehaviour {

	// Use this for initialization
	void Start () {
		AudioListener.volume = PlayerPrefs.GetFloat("volume");
	}
	
	// Update is called once per frame
	void Update () {

	}
}
