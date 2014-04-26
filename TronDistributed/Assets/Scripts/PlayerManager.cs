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
		Dictionary<string, object> passedAllUsersGlobalStates = message["passedStates"] as Dictionary<string, object>;
		foreach (KeyValuePair<string, object> pair in passedAllUsersGlobalStates) {
			if (Players.ContainsKey(pair.Key)) {
				Players[pair.Key].SyncGlobalState(pair.Value as Dictionary<string, object>);
			}
		}
	}

	//public bool AddNewPlayer(string id, Vector3 startPos, float h, float v, Quaternion startRotation, int logicTime) {
	public void AddNewPlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		if (Players.ContainsKey(userID)) {
			return false;
		}

		// If the user is the one sending out JOIN_GAME message, initiate its position and direction
		if (userID == gameStateManager.GetUserID ()) {
			InitLocalPlayer(message);
		} else {
			InitRemotePlayer(userID, message);
		}
	}
	
	private void InitLocalPlayer(Dictionary<string, object> message) {
		// Parse message
		float startPosX = message["posX"] as float;
		float startPosY = message["posY"] as float;
		float startPosZ = message["posZ"] as float;
		Vector3 startPos = new Vector3(startPosX, startPosY, startPosZ);
		float startHorizontalDir = message["horizontalDir"] as float;
		float startVerticalDir = message["verticalDir"] as float;
		int startLogicTime = message["time"] as int;

		// Initiate start position and direction
		gameStateManager.GetMotorController().SetInitParameters(startPos, startHorizontalDir, startVerticalDir);

		// Enable update
		gameStateManager.setPauseState(false);
	}

	private void InitRemotePlayer(string userID, Dictionary<string, object> message) {
		// Parse message
		float startPosX = message["posX"] as float;
		float startPosY = message["posY"] as float;
		float startPosZ = message["posZ"] as float;
		Vector3 startPos = new Vector3(startPosX, startPosY, startPosZ);
		float startHorizontalDir = message["horizontalDir"] as float;
		float startVerticalDir = message["verticalDir"] as float;
		int startLogicTime = message["time"] as int;
		
		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, startPos, Quaternion.identity) as GameObject;
		//playerPrefab.transform.rotation = Quaternion.LookRotation(startDirection);
		playerPrefab.AddComponent<CapsuleCollider>();
		playerPrefab.AddComponent<TrailRenderer>();
		Debug.Log("Instantiate prefab for player " + userID + " complete");
		
		// Create player
		//float curTime = Time.time;
		Player newPlayer = new Player();
		newPlayer.SetStartState(playerPrefab, otherPlayerSpeed, userID, startHorizontalDir, startVerticalDir,
		                        startPos, startLogicTime, Time.fixedTime);
		Players.Add(userID, newPlayer);
		Debug.Log("Initate player " + userID + " complete");
	}

	//public bool updatePlayerBasedOnNetwork(string id, Vector3 pos, Vector3 movement,
	//                                       float h, float v, Quaternion rotation, int curLogicTime) {

	public void UpdatePlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		if (Players.ContainsKey(userID)) {
			return false;
		}
		
		// If this message is sent by the local player, update local player
		if (userID == gameStateManager.GetUserID()) {
			UpdateLocalPlayer(message);
		} else {
			UpdateRemotePlayer(message);
		}
	}

	public void UpdateLocalPlayer(Dictionary<string, object> message) {
		// Parse message
		float horizontalDir = message["horizontalDir"] as float;
		float verticalDir = message["verticalDir"] as float;
		int curLogicTime = message["time"] as int;
		
		// Update local player
		gameStateManager.GetMotorController().UpdateMotor(horizontalDir, verticalDir);
	}

	public void UpdateRemotePlayer(string userID, Dictionary<string, object> message) {
		// Parse message
		float horizontalDir = message["horizontalDir"] as float;
		float verticalDir = message["verticalDir"] as float;
		int curLogicTime = message["time"] as int;

		// Update remote player
		Players[userID].UpdateBasedOnNetwork(horizontalDir, verticalDir, curLogicTime);
	}

	public void RemovePlayer(Dictionary<string, object> message) {
		string userID = message["userID"] as string;
		if (Players.ContainsKey(userID)) {
			return false;
		}
	
		// Delete player
		Players[userID].DestroyAllGameObject();
		Players.Remove(userID);
		knownPlayerUnProcessedMsgList.Remove(userID);
		Debug.Log("Remove player " + userID + " complete");
	}

	// Initialization
	void Awake() {
		knownPlayerUnProcessedMsgList = Dictionary<string, List<Dictionary<string, object>>>();
		unknownPlayerUnProcessedMsgList = List<Dictionary<string, object>>();
		Players = new Dictionary<string, Player>();

		messageHandlerList.Add(MessageDispatcher.JOIN_GAME_ACK, SyncGlobalGameState);
		messageHandlerList.Add(MessageDispatcher.ADD_USER,      AddNewPlayer);
		//messageHandlerList.Add(MessageDispatcher.ADD_LOCAL,     InitLocalPlayer);
		messageHandlerList.Add(MessageDispatcher.UPDATE_USER,   UpdatePlayer);
		//messageHandlerList.Add(MessageDispatcher.UPDATE_LOCAL,  UpdateLocalPlayer);
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
			if (knownPlayerUnProcessedMsgList[pair.Key].Count != 0) {
				// If this player has unprocessed message, process it
				foreach (Dictionary<string, object> msg in knownPlayerUnProcessedMsgList[pair.Key]) {
					Dispatch(msg);
				}
			} else {
				// If this player doesn't have unprocessed message, do normal prediction
				pair.Value.UpdateBasedOnPrediction();
			}
		}
		
		// Process message for unknown players
		foreach (Dictionary<string, object> msg in unknownPlayerUnProcessedMsgList) {
			Dispatch(msg);
		}
	}
}
