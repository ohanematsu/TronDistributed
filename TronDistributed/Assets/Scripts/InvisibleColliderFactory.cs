﻿using UnityEngine;
using System.Collections;

public class InvisibleColliderFactory : MonoBehaviour {

	private Vector3 lastWallWorldPos;
	Vector3 offset;
	private bool paused;
	private Vector3 defaultSize;

	// Use this for initialization
	void Start () {
		offset = new Vector3(0, 0, -5.0f);
		lastWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		paused = true;
		defaultSize = new Vector3(0.2f, 5.0f, 0.2f);
	}
	
	// Update is called once per frame
	void Update () {
		if (paused) {
			return ;
		}
		//PutCollider();
	}


	public GameObject CreateCollider(Vector3 pos) {
		GameObject collider = new GameObject("TrailCollider");
		Debug.Log("Create a new collider, pos[" + pos.x + ", " + pos.y + ", " + pos.z + "]");
		//collider.transform.position = gameObject.transform.position;
		collider.transform.position = pos;
		BoxCollider boxCollider = collider.AddComponent<BoxCollider>();
		boxCollider.size = defaultSize;
		//collider.transform.LookAt(pos);
		return collider;
	}

	public void UpdateCollider(GameObject trailCollider, Vector3 initPos, Vector3 newPos,
	                           float horizontalDir, float verticalDir, float extension) {
		Vector3 oldPos = trailCollider.transform.position; 
		if (newPos == oldPos) {
			return ;
		}
		//Debug.Log ("Old position x: " + oldPos.x + ", y: " + oldPos.y + ", z: " + oldPos.z);
		//Debug.Log ("New position x: " + newPos.x + ", y: " + newPos.y + ", z: " + newPos.z);

		// Update position (the position should be in the middle of the original position and the new position)
		trailCollider.transform.position = Vector3.Lerp(initPos, newPos, 0.5f);
		//trailCollider.transform.LookAt(trailCollider.transform.position);

		// Update size
		BoxCollider collider = trailCollider.GetComponent<BoxCollider>();
		if (collider == null) {
			Debug.Log("Error! Cannot find BoxCollider in current trail collider!");
			return ;
		}
		if (horizontalDir != 0.0f) {
			collider.size += new Vector3(extension, 0.0f, 0.0f);
		} else if (verticalDir != 0.0f) {
			collider.size += new Vector3(0.0f, 0.0f, extension);
		} else {
			Debug.Log("No direction button is pressed. Won't update collider size");
		}
	}

	/*public void SetLastWallWorldPos(Vector3 pos) {
		lastWallWorldPos = pos;
	}*/

	void PutCollider()
	{
		Vector3 newWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		if (newWallWorldPos == lastWallWorldPos) {
			return ;
		}

		GameObject wall = new GameObject("TrailCollider");
		wall.transform.position = Vector3.Lerp(newWallWorldPos, lastWallWorldPos, 0.5f);
		wall.transform.LookAt(newWallWorldPos); // Rotates the transform so the forward vector points at target's current position.

		//Debug.Log ("Old position x: " + lastWallWorldPos.x + ", y: " + lastWallWorldPos.y + ", z: " + lastWallWorldPos.z);
		//Debug.Log ("New position x: " + newWallWorldPos.x + ", y: " + newWallWorldPos.y + ", z: " + newWallWorldPos.z);

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
		/*if (state) {
			lastWallWorldPos = transform.TransformPoint(gameObject.transform.localPosition + offset);
		}*/
	}
}
