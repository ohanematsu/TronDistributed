using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player {

	public GameObject motor;
	private float moveSpeed;
	
	private Vector3 initPosition;
	private float initHorizontalDir;
	private float initVerticalDir;

	private float curHorizontalDir;
	private float curVerticalDir;
	private int lastProcessedLogicTime;

	private List<GameObject> trailColliders;
	private List<Dictionary<string, object>> processedMessage;

	private InvisibleColliderFactory colliderFactory;

	public void SetStartState(GameObject prefab, float speed, Dictionary<string, object> addUserMessage,
	                          InvisibleColliderFactory invisibleColliderFactory) {
		motor = prefab;
		moveSpeed = speed;

		colliderFactory = invisibleColliderFactory;

		lastProcessedLogicTime = Convert.ToInt32(addUserMessage["time"]);

		// Initilize position
		float initPosX = Convert.ToSingle(addUserMessage["posX"]);
		float initPosY = Convert.ToSingle(addUserMessage["posY"]);
		float initPosZ = Convert.ToSingle(addUserMessage["posZ"]);
		initPosition = new Vector3(initPosX, initPosY, initPosZ);
		motor.transform.position = initPosition;

		// Initilize rotation
		initHorizontalDir = Convert.ToSingle(addUserMessage["horizontalDir"]);
		initVerticalDir = Convert.ToSingle(addUserMessage["verticalDir"]);
		curHorizontalDir = initHorizontalDir;
		curVerticalDir = initVerticalDir;
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		trailColliders = new List<GameObject>();

		processedMessage = new List<Dictionary<string, object>>();
		processedMessage.Add(addUserMessage);

		CreateTrailCollider();
	}
	
	public void UpdateBasedOnPrediction(int newLogicTime, float fixedDeltaTime) {
		//Debug.Log("Update remote user based on prediction");

		// Calculate movement
		float deltaTime = fixedDeltaTime * (newLogicTime - lastProcessedLogicTime);
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		Vector3 movement = moveSpeed * moveDirection * deltaTime;
		//Debug.Log("Remote Movement: " + movement.x + "," + movement.y + "," + movement.z + ", speed = " + moveSpeed);

		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Update player's roation
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Create invisible colliders in trail
		//CreateTrailCollider(oldPos);
		UpdateLastCollider(oldPos, motor.transform.position);

		// Update last processed time
		lastProcessedLogicTime = newLogicTime;
	}

	public void UpdateBasedOnNetwork(Dictionary<string, object> message, float fixedDeltaTime) {
		// Get new logic time and move the player
		int newLogicTime = Convert.ToInt32(message["time"]);
		UpdateBasedOnPrediction(newLogicTime, fixedDeltaTime);

		// Parse message
		float newHorizontalDir = Convert.ToSingle(message["horizontalDir"]);
		float newVerticalDir = Convert.ToSingle(message["verticalDir"]);

		// Update player's direction
		curHorizontalDir = newHorizontalDir;
		curVerticalDir = newVerticalDir;

		// Store message
		processedMessage.Add(message);

		// Update last processed time
		lastProcessedLogicTime = newLogicTime;

		/*
		if (msgTimeStamp > lastProcessedLogicTime) {
			lastProcessedLogicTime = msgTimeStamp;
		} else {
			Debug.Log("Timestamp of UPDATE message is less than current time, only update direction");
		}*/
	}

	public int GetLastProcessedTime() {
		return lastProcessedLogicTime;
	}

	public List<Dictionary<string, object>> GetProcessedMessage() {
		return processedMessage;
	}

	private void UpdateLastCollider(Vector3 oldPos, Vector3 newPos) {
		if (oldPos == newPos) {
			return ;
		}

		if (trailColliders.Count == 0) {
			return ;
		}

		GameObject trailCollider = trailColliders[trailColliders.Count - 1];
		if (trailCollider != null) {
			colliderFactory.UpdateCollider(trailCollider, oldPos, newPos, curHorizontalDir, curVerticalDir);
		}
	}

	private void CreateTrailCollider() {
		GameObject newTrailCollider = colliderFactory.CreateCollider(motor.transform.position);
		trailColliders.Add(newTrailCollider);
		Debug.Log("After adding a new collider, now this player has " + trailColliders.Count + " colliders");
	}

	/*
	private void CreateTrailCollider(Vector3 oldPos) {
		GameObject trailCollider = new GameObject("TrailCollider");
		Vector3 newPos = motor.transform.position;
		if (oldPos == newPos) {
			return ;
		}
		trailCollider.transform.position = Vector3.Lerp(oldPos, newPos, 0.5f);
		//trailCollider.transform.LookAt(newPos); // Rotates the transform so the forward vector points at target's current position.

		Debug.Log ("Old position x: " + oldPos.x + ", y: " + oldPos.y + ", z: " + oldPos.z);
		Debug.Log ("New position x: " + newPos.x + ", y: " + newPos.y + ", z: " + newPos.z);
			
		BoxCollider boxCollider = trailCollider.AddComponent("BoxCollider") as BoxCollider;
		if (newPos.x == oldPos.x) { // If motor don't change its horizontal direction
			boxCollider.size = new Vector3(0.02f, 3f, Vector3.Distance(newPos, oldPos));
		} else {
			boxCollider.size = new Vector3(Vector3.Distance(newPos, oldPos), 3f, 0.02f);
		}

		trailColliders.Add(trailCollider);
	}*/

	public List<GameObject> GetAllColliders() {
		return trailColliders;
	}

	public GameObject GetPrefab() {
		return motor;
	}
}

