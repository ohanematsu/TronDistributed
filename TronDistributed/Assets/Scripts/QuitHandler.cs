using UnityEngine;
using System.Collections;

public class QuitHandler : MonoBehaviour {

	void OnMouseEnter() {
		//Debug.Log("Quit button enter!");
		renderer.material.color = Color.blue;
		//Debug.Log("Quit button enter xxxxx!");
	}
	
	void OnMouseExit() {
		//Debug.Log("Quit button out!");
		renderer.material.color = Color.white;
		//Debug.Log("Quit button out xxxxx!");
	}
	
	void OnMouseUp() {
		// Quit game
		//Debug.Log("Quit button pressed!");
		Application.Quit();
		//Debug.Log("Quit button pressed! yyy");
	}
}
