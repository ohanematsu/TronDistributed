using UnityEngine;
using System.Collections;

public class PlayHandler : MonoBehaviour {

	void OnMouseEnter() {
		//Debug.Log("Play button enter!");
		renderer.material.color = Color.red;
		//Debug.Log("Play button enter xxxx!");
	}
	
	void OnMouseExit() {
		//Debug.Log("Play button exit!");
		renderer.material.color = Color.white;
		//Debug.Log("Play button exit xxxxx!");
	}
	
	void OnMouseUp() {
		// Load main level
		Application.LoadLevel(1);
	}
}
