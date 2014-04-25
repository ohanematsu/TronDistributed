using UnityEngine;
using System.Collections;

public class EscapeMenuControl : MonoBehaviour {

	public bool pause = false;
	public GUITexture pauseMenu;

	// Use this for initialization
	void Start () {
		pauseMenu.enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			if (!pause) {
				pause = true;
			} else {
				pause = false;
			}

			if (pause) {
				Time.timeScale = 0;
				pauseMenu.enabled = true;
			} else {
				Time.timeScale = 1;
				pauseMenu.enabled = false;
			}
		}
	}
}
