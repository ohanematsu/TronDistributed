using UnityEngine;
using System.Collections;

public class InvisibleBoxLocator : MonoBehaviour {

	Vector3 lastWallWorldPos;
	Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = new Vector3(0, 0, -2.0f);
		lastWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
	}
	
	// Update is called once per frame
	void Update () {
		MakeWall();
	}

	void MakeWall()
	{
		GameObject wall = new GameObject("Wall");

		Vector3 newWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		wall.transform.position = Vector3.Lerp(newWallWorldPos, lastWallWorldPos, 0.5f);
		wall.transform.LookAt(newWallWorldPos); // Rotates the transform so the forward vector points at target's current position.

		BoxCollider boxCollider = wall.AddComponent("BoxCollider") as BoxCollider;
		if (newWallWorldPos.x == lastWallWorldPos.x) { // If motor don't change its horizontal direction
			boxCollider.size = new Vector3(0.02f, 3f, Vector3.Distance(newWallWorldPos, lastWallWorldPos));
		} else {
			boxCollider.size = new Vector3(Vector3.Distance(newWallWorldPos, lastWallWorldPos), 3f, 0.02f);
		}

		lastWallWorldPos = newWallWorldPos;
	}
}
