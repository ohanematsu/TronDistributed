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

	private Vector3 colliderPosOffset;
	private Vector3 lastColliderInitPos;

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

		colliderPosOffset = new Vector3(0, 0, -0.5f);

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
		Vector3 newColliderPos = motor.transform.TransformPoint(colliderPosOffset);

		// Update player's roation
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Create invisible colliders in trail
		//CreateTrailCollider(oldPos);
		UpdateLastCollider(newColliderPos, movement.magnitude);

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
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Store message
		processedMessage.Add(message);

		// Update last processed time
		lastProcessedLogicTime = newLogicTime;

		// Create a new collider
		CreateTrailCollider();
	}

	public int GetLastProcessedTime() {
		return lastProcessedLogicTime;
	}

	public List<Dictionary<string, object>> GetProcessedMessage() {
		return processedMessage;
	}

	public void UpdateLastCollider(Vector3 newColliderPos, float extendsion) {
		if (trailColliders.Count == 0) {
			return ;
		}

		GameObject trailCollider = trailColliders[trailColliders.Count - 1];
		if (trailCollider != null) {
			colliderFactory.UpdateCollider(trailCollider, lastColliderInitPos, 
				newColliderPos, curHorizontalDir, curVerticalDir, extendsion);
		}
	}

	public void CreateTrailCollider() {
		Vector3 colliderPos = motor.transform.TransformPoint(colliderPosOffset);
		GameObject newTrailCollider = colliderFactory.CreateCollider(colliderPos);
		trailColliders.Add(newTrailCollider);
		lastColliderInitPos = newTrailCollider.transform.position;
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

