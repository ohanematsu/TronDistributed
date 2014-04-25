using UnityEngine;
using System.Collections;

public class MainMenuControl : MonoBehaviour {

	public bool isQuitButton = false;

	// called when the mouse entered the GUIElement or Collider.
	void OnMouseEnter() {
		renderer.material.color = Color.red;
	}

	void OnMouseExit() {
		renderer.material.color = Color.white;
	}

	void OnMouseUp() {
		if (isQuitButton) { 
			// Quit game
			Application.Quit();
		} else {
			// Load main level
			Application.LoadLevel(1);
		}
	}
}
