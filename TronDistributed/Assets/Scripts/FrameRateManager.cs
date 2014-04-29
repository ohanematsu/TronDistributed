using UnityEngine;
using System.Collections;

public class FrameRateManager : MonoBehaviour {

	public int targetFrameRate = 60;

	// Use this for initialization
	void Start () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
	}
}
