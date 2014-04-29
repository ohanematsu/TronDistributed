using UnityEngine;
using System.Collections;

public class InvisibleBoxLocator : MonoBehaviour {

	Vector3 lastWallWorldPos;
	Vector3 offset;
	private bool paused;

	// Use this for initialization
	void Start () {
		offset = new Vector3(0, 0, -2.0f);
		//lastWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		paused = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (paused) {
			return ;
		}
		PutCollider();
	}

	void PutCollider()
	{
		Vector3 newWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		if (newWallWorldPos == lastWallWorldPos) {
		}

		GameObject wall = new GameObject("TrailCollider");
		wall.transform.position = Vector3.Lerp(newWallWorldPos, lastWallWorldPos, 0.5f);
		//wall.transform.LookAt(newWallWorldPos); // Rotates the transform so the forward vector points at target's current position.

		Debug.Log ("Old position x: " + lastWallWorldPos.x + ", y: " + lastWallWorldPos.y + ", z: " + lastWallWorldPos.z);
		Debug.Log ("New position x: " + newWallWorldPos.x + ", y: " + newWallWorldPos.y + ", z: " + newWallWorldPos.z);

		BoxCollider boxCollider = wall.AddComponent("BoxCollider") as BoxCollider;
		if (newWallWorldPos.x == lastWallWorldPos.x) { // If motor don't change its horizontal direction
			boxCollider.size = new Vector3(0.02f, 3f, Vector3.Distance(newWallWorldPos, lastWallWorldPos));
		} else {
			boxCollider.size = new Vector3(Vector3.Distance(newWallWorldPos, lastWallWorldPos), 3f, 0.02f);
		}

		lastWallWorldPos = newWallWorldPos;
	}

	public void SetPauseState(bool state) {
		paused = state;
		if (state) {
			lastWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		}
	}
}
