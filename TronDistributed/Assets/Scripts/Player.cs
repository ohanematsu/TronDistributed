using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player : MonoBehaviour {

	public GameObject motor;
	private bool isLocal;
	private float moveSpeed;

	private string userId;
	private float curHorizontalDir;
	private float curVerticalDir;

	private int lastLogicTime;
	private float lastLocalTime;
	private Vector3 lastPosition;
	private Vector3 lastMovement;
	private Quaternion lastRotation;

	private List<GameObject> trailColliders;

	private List<OccuredEvent> processedMessage;

	public void SetStartState(GameObject prefab, float speed, bool isLocalPlayer, string id, float h, float v, 
	                          Vector3 startPos, int logicTime, float localTime) {
		motor = prefab;
		moveSpeed = speed;
		isLocal = isLocalPlayer;

		userId = id;
		curHorizontalDir = h;
		curVerticalDir = v;

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
	
	public void UpdateBasedOnPrediction(int newLogicTime, float fixedDeltaTime) {
		// Calculate movement
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		moveDirection = transform.TransformDirection(moveDirection);
		Vector3 movement = moveDirection * moveSpeed * fixedDeltaTime;

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
		lastRotation = motor.transform.rotation;
	}

	//public void UpdateBasedOnNetwork(Vector3 pos, Vector3 movement, float h, float v,
	//                                 Quaternion rotation, int curLogicTime, float curLocalTime) {
	public void UpdateBasedOnNetwork(float newHorizontalDir, float newVerticalDir, int newLogicTime, float fixedDeltaTime) {
		// Update player's direction
		curHorizontalDir = newHorizontalDir;
		curVerticalDir = newVerticalDir;
	
		UpdateBasedOnPrediction(newLogicTime, fixedDeltaTime);
	}

	public void SyncGlobalState(List<Dictionary<string, object>> passedGlobalStates) {
	}

	public void DestroyAllGameObject() {
		DestroyPrefab();
		DestroyAllColliders();
	}

	public bool isLocalPlayer() {
		return isLocal;
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

