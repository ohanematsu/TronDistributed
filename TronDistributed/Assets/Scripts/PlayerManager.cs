using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// This class contains all players in the game, include itself
public class PlayerManager : MonoBehaviour{
	public GameObject otherPlayer; // Don't change it. It is set in the editor
	
	private Dictionary<string, Player> Players;
	private Dictionary<string, List<Dictionary<string, object>>> knownPlayerUnProcessedMsgList;
	private List<Dictionary<string, object>> unknownPlayerUnProcessedMsgList;

	private GameStateManager gameStateManager;

	private delegate void messageHandler(Dictionary<string, object> message);
	private static Dictionary<string, messageHandler> messageHandlerList = new Dictionary<string, messageHandler>();

	public float otherPlayerSpeed = 0.1f;
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
		if (Players.ContainsKey(userID)) {
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
		float initPosX = (float)(double)message["posX"];
		float initPosY = (float)(double)message["posY"];
		float initPosZ = (float)(double)message["posZ"];
		Vector3 initPosition = new Vector3(initPosX, initPosY, initPosZ);

		float initHorizontalDir = (float)(double)message["horizontalDir"];
		float initVerticalDir = (float)(double)message["verticalDir"];

		// Initiate start position and direction
		gameStateManager.GetMotorController().SetInitParameters(initPosition, initHorizontalDir, initVerticalDir);

		// Enable update
		gameStateManager.setPauseState(false);

		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, initPosition, Quaternion.identity) as GameObject;
		Player newPlayer = new Player();
		newPlayer.SetStartState(playerPrefab, otherPlayerSpeed, message);
		Players.Add(gameStateManager.GetUserID(), newPlayer);
	}

	private void InitRemotePlayer(string userID, Dictionary<string, object> message) {
		float initPosX = (float)(double)message["posX"];
		float initPosY = (float)(double)message["posY"];
		float initPosZ = (float)(double)message["posZ"];
		Vector3 initPosition = new Vector3(initPosX, initPosY, initPosZ);

		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, initPosition, Quaternion.identity) as GameObject;

		//playerPrefab.transform.rotation = Quaternion.LookRotation(startDirection);
		playerPrefab.AddComponent<CapsuleCollider>();
		playerPrefab.AddComponent<TrailRenderer>();
		Debug.Log("Instantiate prefab for player " + userID + " complete");
		
		// Create player
		//float curTime = Time.time;
		Player newPlayer = new Player();
		newPlayer.SetStartState(playerPrefab, otherPlayerSpeed, message);
		Players.Add(userID, newPlayer);
		Debug.Log("Initate player " + userID + " complete");
	}

	public void UpdatePlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
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
		// Parse message
		float horizontalDir = (float)(double)message["horizontalDir"];
		float verticalDir = (float)(double)message["verticalDir"];
		int curLogicTime = (int)message["time"];
		
		// Update local player
		gameStateManager.GetMotorController().UpdateMotor(horizontalDir, verticalDir, Time.fixedDeltaTime);

		// Update information in player manager
		Players[userID].GetProcessedMessage().Add(message);
	}

	public void UpdateRemotePlayer(string userID, Dictionary<string, object> message) {
		// Update remote player
		Players [userID].UpdateBasedOnNetwork(message, Time.fixedDeltaTime);
	}

	public void RemovePlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		if (!Players.ContainsKey(userID)) {
			return ;
		}
	
		// Delete player
		Players[userID].DestroyAllGameObject();
		Players.Remove(userID);
		knownPlayerUnProcessedMsgList.Remove(userID);
		Debug.Log("Remove player " + userID + " complete");
	}

	public Dictionary<string, object> GenerateACKMessage(string targetUserId) {
		Dictionary<string, object> globalStateMessage = new Dictionary<string, object>();
		foreach (KeyValuePair<string, Player> playerPair in Players) {
			globalStateMessage.Add(playerPair.Key, playerPair.Value.GetProcessedMessage());
		}

		Dictionary<string, object> ackMessage = new Dictionary<string, object>();
		ackMessage.Add("type", MessageDispatcher.JOIN_GAME_ACK);
		ackMessage.Add("userID", targetUserId);
		ackMessage.Add("globalState", globalStateMessage);
		ackMessage.Add("time", gameStateManager.GetCurLogicTime ());
		return ackMessage;
	}

	// Initialization
	void Awake() {
		knownPlayerUnProcessedMsgList = new Dictionary<string, List<Dictionary<string, object>>>();
		unknownPlayerUnProcessedMsgList = new List<Dictionary<string, object>>();
		Players = new Dictionary<string, Player>();

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
		foreach (KeyValuePair<string, Player> pair in Players) {
			// If this player has unprocessed messages, process them first
			if (knownPlayerUnProcessedMsgList[pair.Key].Count != 0) {
				foreach (Dictionary<string, object> msg in knownPlayerUnProcessedMsgList[pair.Key]) {
					Dispatch(msg);
				}
			}

			// Update the player
			if (pair.Key != gameStateManager.GetUserID()) {
				pair.Value.UpdateBasedOnPrediction(gameStateManager.GetCurLogicTime(), Time.fixedTime);
			}
		}
		
		// Process messages for unknown players
		foreach (Dictionary<string, object> msg in unknownPlayerUnProcessedMsgList) {
			Dispatch(msg);
		}
	}
}
