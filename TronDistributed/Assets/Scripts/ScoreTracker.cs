using UnityEngine;
using System.Collections;

public class ScoreTracker : MonoBehaviour {

	public GUIText textScore;
	public int score;

	// Use this for initialization
	void Start () {
		textScore = GameObject.Find("TextScoreGUI").GetComponent<GUIText>();
		score = 0;
		textScore.text = score.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateScore();
	}

	void UpdateScore() {
		score++;
		textScore.text = score.ToString();
	}
}
