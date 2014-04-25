using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player : MonoBehaviour {

	public GameObject motor;
	private float moveSpeed;

	private string userId;
	private float dirHorizontal;
	private float dirVertical;

	private int lastLogicTime;
	private float lastLocalTime;
	private Vector3 lastPosition;
	private Vector3 lastMovement;
	private Quaternion lastRotation;

	private List<GameObject> trailColliders;

	public void SetStartState(GameObject prefab, float speed, string id, float h, float v, Vector3 startPos, 
	                          int logicTime, float localTime) {
		motor = prefab;
		moveSpeed = speed;

		userId = id;
		dirHorizontal = h;
		dirVertical = v;

		lastPosition = startPos;
		lastLogicTime = logicTime;
		lastLocalTime = localTime;
		lastMovement = new Vector3 (0, 0, 0);

		trailColliders = new List<GameObject>();

		Vector3 moveDirection = new Vector3(dirHorizontal, 0, dirVertical);
		moveDirection = transform.TransformDirection(moveDirection);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);
		lastRotation = motor.transform.rotation;
	}
	
	public void UpdateLocally(float curLocalTime) {
		// Calculate movement
		Vector3 moveDirection = new Vector3(dirHorizontal, 0, dirVertical);
		moveDirection = transform.TransformDirection(moveDirection);
		Vector3 movement = moveDirection * moveSpeed * (curLocalTime - lastLocalTime);

		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Update player's roation
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Create invisible colliders in trail
		CreateTrailCollider(oldPos);

		// Update
		lastPosition = oldPos;
		lastMovement = movement;
		lastLocalTime = curLocalTime;
		lastRotation = motor.transform.rotation;
	}

	public void UpdateBasedOnNetwork(Vector3 pos, Vector3 movement, float h, float v,
	                                 Quaternion rotation, int curLogicTime, float curLocalTime) {
		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Update player's direction
		dirVertical = v;
		dirHorizontal = h;

		// Update player's rotation
		Vector3 moveDirection = new Vector3(dirHorizontal, 0, dirVertical);
		moveDirection = transform.TransformDirection(moveDirection);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Create invisible colliders in trail
		CreateTrailCollider(oldPos);

		// Update Logic Time
		lastLogicTime = curLogicTime;
		lastLocalTime = curLocalTime;
		lastMovement = movement;
		lastPosition = motor.transform.position;
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

