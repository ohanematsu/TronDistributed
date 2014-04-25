using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player : MonoBehaviour {
	public int userId;
	public GameObject motor;
	//public Vector3 curPosition;
	public Vector3 curDirection;

	private float lastTime;
	private float moveSpeed;
	private Vector3 lastPosition;
	private Vector3 lastDirection;
	private Vector3 lastMovement;
	private List<GameObject> trailColliders;

	public void SetStartState(int id, GameObject prefab, Vector3 startPos, Vector3 startDirection, float speed, float time) {
		userId = id;
		motor = prefab;
		//curPosition = startPos;
		lastTime = time;
		curDirection = startDirection;
		moveSpeed = speed;
		lastPosition = startPos;
		lastDirection = startDirection;
		lastMovement = new Vector3 (0, 0, 0);
		trailColliders = new List<GameObject>();
	}
	
	public void UpdateLocally(float curTime) {
		// Calculate movement
		Vector3 movement = curDirection * moveSpeed;
		movement *= (curTime - lastTime);

		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Create invisible colliders in trail
		CreateTrailCollider(oldPos);

		lastPosition = oldPos;
		lastMovement = movement;
		lastTime = curTime;
	}

	public void UpdateBasedOnNetwork(Vector3 direction, Vector3 movement, float curTime) {
		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Update player's rotation
		lastDirection = curDirection;
		motor.transform.rotation = Quaternion.LookRotation(direction);

		// Create invisible colliders in trail
		CreateTrailCollider(oldPos);

		lastPosition = oldPos;
		lastMovement = movement;
		curDirection = direction;
		lastTime = curTime;
	}

	public void DestroyAllGameObject() {
		DestroyPrefab();
		DestroyAllColliders();
	}

	private void CreateTrailCollider(Vector3 oldPos) {
		GameObject trailCollider = new GameObject("TrailCollider");
		Vector3 newPos = motor.transform.position;
		trailCollider.transform.position = Vector3.Lerp(oldPos, newPos, 0.5f);
		trailCollider.transform.LookAt(newPos); // Rotates the transform so the forward vector points at target's current position.
			
		BoxCollider boxCollider = trailCollider.AddComponent("BoxCollider") as BoxCollider;
		boxCollider.size = new Vector3(0.05f, 3f, Vector3.Distance(newPos, oldPos));

		trailColliders.Add(trailCollider);
	}

	private void DestroyAllColliders() {
		for (int i = 0; i < trailColliders.Count; i++) {
			Destroy(trailColliders[i]);
			Debug.Log("Destroy trail collider " + i + " in player " + userId);
		}
	}

	private void DestroyPrefab() {
		Destroy(motor);
		Debug.Log("Remove fab of player " + userId + " complete");
	}
}

