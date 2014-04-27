using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player {

	public GameObject motor;
	//private bool isLocal;
	private float moveSpeed;

	private string userId;
	private int initLogicTime;
	private Vector3 initPosition;
	private float initHorizontalDir;
	private float initVerticalDir;

	private float curHorizontalDir;
	private float curVerticalDir;
	//private float lastLocalTime;
	//private Vector3 lastMovement;
	//private Quaternion lastRotation;

	private List<GameObject> trailColliders;
	private List<Dictionary<string, object>> processedMessage;

	public void SetStartState(GameObject prefab, float speed, Dictionary<string, object> addUserMessage) {
		motor = prefab;
		moveSpeed = speed;
		Debug.Log("Init remote speed = " + moveSpeed);

		userId = addUserMessage["userID"] as string;
		initHorizontalDir = Convert.ToSingle(addUserMessage["horizontalDir"]);
		initVerticalDir = Convert.ToSingle(addUserMessage["verticalDir"]);
		initLogicTime = Convert.ToInt32(addUserMessage["time"]);

		float initPosX = Convert.ToSingle(addUserMessage["posX"]);
		float initPosY = Convert.ToSingle(addUserMessage["posY"]);
		float initPosZ = Convert.ToSingle(addUserMessage["posZ"]);
		initPosition = new Vector3(initPosX, initPosY, initPosZ);

		curHorizontalDir = initHorizontalDir;
		curVerticalDir = initVerticalDir;

		trailColliders = new List<GameObject>();
		processedMessage = new List<Dictionary<string, object>>();
		processedMessage.Add(addUserMessage);

		// Set position and rotation
		motor.transform.position = initPosition;
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		//moveDirection = motor.transform.TransformDirection(moveDirection);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);
	}
	
	public void UpdateBasedOnPrediction(int newLogicTime, float fixedDeltaTime) {
		Debug.Log("Update remote user based on prediction");
		// Calculate movement
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		//moveDirection = moveDirection.normalized;
		//moveDirection = motor.transform.TransformDirection(moveDirection);
		Vector3 movement = moveDirection * moveSpeed * fixedDeltaTime;

		Debug.Log("Remote Movement: " + movement.x + "," + movement.y + "," + movement.z + ", speed = " + moveSpeed);

		// Update player's position
		Vector3 oldPos = motor.transform.position;
		motor.transform.position = oldPos + movement;

		// Update player's roation
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);

		// Create invisible colliders in trail
		CreateTrailCollider(oldPos);

		// Store Message
		/*Dictionary<string, object> message = new Dictionary<string, object>();
		message["type"] = (object)MessageDispatcher.UPDATE_USER;
		message["verticalDir"] = (object)curVerticalDir;
		message["horizontalDir"] = (object)curHorizontalDir;
		message["time"] = (object)newLogicTime;
		processedMessage.Add(message);*/
	}

	public void UpdateBasedOnNetwork(Dictionary<string, object> message, float fixedDeltaTime) {
		// Parse message
		float newHorizontalDir = Convert.ToSingle(message["horizontalDir"]);
		float newVerticalDir = Convert.ToSingle(message["verticalDir"]);
		int newLogicTime = Convert.ToInt32(message["time"]);

		// Update player's direction
		curHorizontalDir = newHorizontalDir;
		curVerticalDir = newVerticalDir;
	
		// Update position and rotation
		// UpdateBasedOnPrediction(newLogicTime, fixedDeltaTime);
	}

	public List<Dictionary<string, object>> GetProcessedMessage() {
		return processedMessage;
	}

	private void CreateTrailCollider(Vector3 oldPos) {
		GameObject trailCollider = new GameObject("TrailCollider");
		Vector3 newPos = motor.transform.position;
		trailCollider.transform.position = Vector3.Lerp(oldPos, newPos, 0.5f);
		trailCollider.transform.LookAt(newPos); // Rotates the transform so the forward vector points at target's current position.
			
		BoxCollider boxCollider = trailCollider.AddComponent("BoxCollider") as BoxCollider;
		if (newPos.x == oldPos.x) { // If motor don't change its horizontal direction
			boxCollider.size = new Vector3(0.02f, 3f, Vector3.Distance(newPos, oldPos));
		} else {
			boxCollider.size = new Vector3(Vector3.Distance(newPos, oldPos), 3f, 0.02f);
		}

		trailColliders.Add(trailCollider);
	}

	public List<GameObject> GetAllColliders() {
		return trailColliders;
	}

	public GameObject GetPrefab() {
		return motor;
	}
}

