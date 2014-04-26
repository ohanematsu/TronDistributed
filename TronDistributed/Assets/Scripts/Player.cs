using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Player : MonoBehaviour {

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

		userId = addUserMessage["userID"] as string;
		initHorizontalDir = (float)(double)addUserMessage["horizontalDir"];
		initVerticalDir = (float)(double)addUserMessage["verticalDir"];
		initLogicTime = (int)addUserMessage["time"];

		float initPosX = (float)(double)addUserMessage["posX"];
		float initPosY = (float)(double)addUserMessage["posY"];
		float initPosZ = (float)(double)addUserMessage["posZ"];
		initPosition = new Vector3(initPosX, initPosY, initPosZ);

		curHorizontalDir = initHorizontalDir;
		curVerticalDir = initVerticalDir;

		trailColliders = new List<GameObject>();
		processedMessage = new List<Dictionary<string, object>>();
		processedMessage.Add(addUserMessage);

		// Set position and rotation
		motor.transform.position = initPosition;
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		moveDirection = transform.TransformDirection(moveDirection);
		motor.transform.rotation = Quaternion.LookRotation(moveDirection);
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

		// Store Message
		Dictionary<string, object> message = new Dictionary<string, object>();
		message["type"] = (object)MessageDispatcher.UPDATE_USER;
		message["verticalDir"] = (object)curVerticalDir;
		message["horizontalDir"] = (object)curHorizontalDir;
		message["time"] = (object)newLogicTime;
		processedMessage.Add(message);
	}

	public void UpdateBasedOnNetwork(Dictionary<string, object> message, float fixedDeltaTime) {
		// Parse message
		float newHorizontalDir = (float)(double)message["horizontalDir"];
		float newVerticalDir = (float)(double)message["verticalDir"];
		int newLogicTime = (int)message["time"];

		// Update player's direction
		curHorizontalDir = newHorizontalDir;
		curVerticalDir = newVerticalDir;
	
		// Update position and rotation
		// UpdateBasedOnPrediction(newLogicTime, fixedDeltaTime);
	}

	public List<Dictionary<string, object>> GetProcessedMessage() {
		return processedMessage;
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

