using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

// This class contains all players in the game, include itself
public class PlayerManager : MonoBehaviour{
	public GameObject otherPlayer; // Don't change it. It is set in the editor
	
	private Dictionary<string, Player> Players;
	private Dictionary<string, List<Dictionary<string, object>>> knownPlayerUnProcessedMsgList;
	private List<Dictionary<string, object>> unknownPlayerUnProcessedMsgList;
	private List<string> toDeletePlayer;

	private GameStateManager gameStateManager;

	private delegate void messageHandler(Dictionary<string, object> message);
	private static Dictionary<string, messageHandler> messageHandlerList = new Dictionary<string, messageHandler>();

	public float otherPlayerSpeed = 4.0f;
	private bool paused = false;

	public void SetGameStateManager(GameStateManager globalGameStateManager) {
		gameStateManager = globalGameStateManager;
	}

	public void EnqueueUnProcessedMessageQueue(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		if (knownPlayerUnProcessedMsgList.ContainsKey(userID)) {
			knownPlayerUnProcessedMsgList[userID].Add(message);
		} else {
			unknownPlayerUnProcessedMsgList.Add(message);
		}
	}

	public bool isPaused() {
		return paused;
	}
	
	public void setPauseState(bool state) {
		paused = state;
	}

	private void Dispatch(Dictionary<string, object> message) {
		string type = message["type"] as string;
		if (!messageHandlerList.ContainsKey(type)) {
			// Unknown type, ignore this message
			Debug.Log("Unknow Type: " + type);
			return ;
		}
		messageHandlerList[type](message);
	}

	public void SyncGlobalGameState(Dictionary<string, object> message) {
		Dictionary<string, object> passedAllUsersGlobalStates = message["globalState"] as Dictionary<string, object>;
		foreach (KeyValuePair<string, object> pair in passedAllUsersGlobalStates) {
			string userID = pair.Key as string;
			List<Dictionary<string, object>> messages = pair.Value as List<Dictionary<string, object>>;
			foreach (Dictionary<string, object> processedMessage in messages) {
				Dispatch(processedMessage);
			}
		}
	}

	//public bool AddNewPlayer(string id, Vector3 startPos, float h, float v, Quaternion startRotation, int logicTime) {
	public void AddNewPlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		Debug.Log("Prepare to add user " + userID);
		if (Players.ContainsKey(userID)) {
			Debug.Log("User " + userID + " already exists");
			return ;
		} 

		// If the user is the one sending out JOIN_GAME message, initiate its position and direction
		if (userID == gameStateManager.GetUserID()) {
			InitLocalPlayer(message);
		} else {
			InitRemotePlayer(userID, message);
		}

		List<Dictionary<string, object>> messageList = new List<Dictionary<string, object>>();
		knownPlayerUnProcessedMsgList.Add(userID, messageList);
	}
	
	private void InitLocalPlayer(Dictionary<string, object> message) {
		Debug.Log("Prepare to init local user");

		float initPosX = Convert.ToSingle(message["posX"]);
		float initPosY = Convert.ToSingle(message["posY"]);
		float initPosZ = Convert.ToSingle(message["posZ"]);
		Vector3 initPosition = new Vector3(initPosX, initPosY, initPosZ);

		float initHorizontalDir = Convert.ToSingle(message["horizontalDir"]);
		float initVerticalDir = Convert.ToSingle(message["verticalDir"]);

		// Initiate start position and direction
		gameStateManager.GetMotorController().SetInitParameters(initPosition, initHorizontalDir, initVerticalDir);
		Debug.Log("Init local user's position and direction complete");

		// Instantiate prefabs
		//GameObject playerPrefab = Instantiate(otherPlayer, initPosition, Quaternion.identity) as GameObject;
		Player newPlayer = new Player();
		newPlayer.SetStartState(new GameObject(), otherPlayerSpeed, message);
		Players.Add(gameStateManager.GetUserID(), newPlayer);
		Debug.Log("Init local user complete");

		// Enable update
		gameStateManager.setPauseState(false);
		Debug.Log("Enable update");
	}

	private void InitRemotePlayer(string userID, Dictionary<string, object> message) {
		Debug.Log("Prepare to init remote user");

		float initPosX = Convert.ToSingle(message["posX"]);
		float initPosY = Convert.ToSingle(message["posY"]);
		float initPosZ = Convert.ToSingle(message["posZ"]);
		Vector3 initPosition = new Vector3(initPosX, initPosY, initPosZ);

		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, initPosition, Quaternion.identity) as GameObject;

		//playerPrefab.transform.rotation = Quaternion.LookRotation(startDirection);
		playerPrefab.AddComponent<CapsuleCollider>();
		playerPrefab.AddComponent<TrailRenderer>();
		playerPrefab.GetComponent<TrailRenderer>().time = 3600;
		Debug.Log("Instantiate prefab for player " + userID + " complete");
		
		// Create player
		//float curTime = Time.time;
		Player newPlayer = new Player();
		newPlayer.SetStartState(playerPrefab, gameStateManager.getMoveSpeed(), message);
		Players.Add(userID, newPlayer);
		Debug.Log("Initate player " + userID + " complete");
	}

	public void UpdatePlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		Debug.Log("update user: " + userID);
		if (!Players.ContainsKey(userID)) {
			return ;
		}
		
		// If this message is sent by the local player, update local player
		if (userID == gameStateManager.GetUserID()) {
			UpdateLocalPlayer(userID, message);
		} else {
			UpdateRemotePlayer(userID, message);
		}
	}

	public void UpdateLocalPlayer(string userID, Dictionary<string, object> message) {
		Debug.Log("Prepare to update local player");

		// Parse message
		float horizontalDir = Convert.ToSingle(message["horizontalDir"]);
		float verticalDir = Convert.ToSingle(message["verticalDir"]);
		int curLogicTime = Convert.ToInt32(message["time"]);
		
		// Update local player
		gameStateManager.GetMotorController().UpdateDirection(horizontalDir, verticalDir);

		// Update information in player manager
		Players[userID].GetProcessedMessage().Add(message);

		Debug.Log("Update local player complete");
	}

	public void UpdateRemotePlayer(string userID, Dictionary<string, object> message) {
		Debug.Log("Prepare to update remote player");
		// Update remote player
		Players[userID].UpdateBasedOnNetwork(message, Time.fixedDeltaTime);
		Debug.Log("Update remote player complete");
	}

	public void RemovePlayer(Dictionary<string, object> message) {
		Debug.Log("Prepare to remove player");
		string userID = message["userID"] as string;
		if (!Players.ContainsKey(userID)) {
			Debug.Log("Player " + userID + "doesn't exsit");
			return ;
		}
	
		toDeletePlayer.Add(userID);
		Debug.Log("Add to todelete player list");
	}

	private void DestroyPlayers() {
		foreach (string userID in toDeletePlayer) {
			// Delete player
			List<GameObject> trailColliders = Players[userID].GetAllColliders();
			foreach (GameObject collider in trailColliders) {
				Destroy(collider);
			}
			Destroy(Players[userID].GetPrefab());
			Players.Remove(userID);
			knownPlayerUnProcessedMsgList.Remove(userID);
			Debug.Log("Remove player " + userID + " complete");
		}

		toDeletePlayer.Clear();
	}

	public Dictionary<string, object> GenerateACKMessage(string targetUserId) {
		Debug.Log("Prepare to compare generate ack message");
		Dictionary<string, object> globalStateMessage = new Dictionary<string, object>();
		foreach (KeyValuePair<string, Player> playerPair in Players) {
			globalStateMessage.Add(playerPair.Key, playerPair.Value.GetProcessedMessage());
			Debug.Log("Add state of user " + playerPair.Key);
		}

		Dictionary<string, object> ackMessage = new Dictionary<string, object>();
		ackMessage.Add("type", MessageDispatcher.JOIN_GAME_ACK);
		ackMessage.Add("userID", targetUserId);
		ackMessage.Add("globalState", globalStateMessage);
		ackMessage.Add("time", gameStateManager.GetCurLogicTime ());
		Debug.Log("Generate ack message complete");
		return ackMessage;
	}

	// Initialization
	void Awake() {
		knownPlayerUnProcessedMsgList = new Dictionary<string, List<Dictionary<string, object>>>();
		unknownPlayerUnProcessedMsgList = new List<Dictionary<string, object>>();
		Players = new Dictionary<string, Player>();
		toDeletePlayer = new List<string>();

		messageHandlerList.Add(MessageDispatcher.JOIN_GAME_ACK, SyncGlobalGameState);
		messageHandlerList.Add(MessageDispatcher.ADD_USER,      AddNewPlayer);
		messageHandlerList.Add(MessageDispatcher.UPDATE_USER,   UpdatePlayer);
		messageHandlerList.Add(MessageDispatcher.DELETE_USER,   RemovePlayer);
		messageHandlerList.Add(MessageDispatcher.USER_CRASH,    RemovePlayer);
	}
	
	// Called every fixed framerate frame, if the MonoBehaviour is enabled.
	void FixedUpdate () {
		/*
		if (paused) {
			return ;
		}*/
		
		// Process message of known players
		foreach(KeyValuePair<string, Player> pair in Players) {
			// If this player has unprocessed messages, process them first
			if (knownPlayerUnProcessedMsgList[pair.Key].Count != 0) {
				Debug.Log("Dispatch message for user" + pair.Key);
				foreach (Dictionary<string, object> msg in knownPlayerUnProcessedMsgList[pair.Key]) {
					Dispatch(msg);
					//knownPlayerUnProcessedMsgList[pair.Key].Remove(msg);
				}
				knownPlayerUnProcessedMsgList[pair.Key].Clear();
			}

			// Update the player
			Debug.Log("user id: " + pair.Key);
			if (pair.Key != gameStateManager.GetUserID()) {
				pair.Value.UpdateBasedOnPrediction(gameStateManager.GetCurLogicTime(), Time.fixedDeltaTime);
			} else {
				gameStateManager.GetMotorController().UpdateMotor(Time.fixedDeltaTime);
			}
		}

		List<Dictionary<string, object>> cloneUnknownPlayerUnProcessedMsgList = 
			new List<Dictionary<string, object>>(unknownPlayerUnProcessedMsgList);
		// Process messages for unknown players
		foreach (Dictionary<string, object> msg in cloneUnknownPlayerUnProcessedMsgList) {
			Dispatch(msg);
		}
		unknownPlayerUnProcessedMsgList.Clear();

		DestroyPlayers();
	}
}
