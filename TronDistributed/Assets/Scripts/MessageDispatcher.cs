using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MessageDispatcher : MonoBehaviour {

	private GameStateManager gameStateManager;

	public static string JOIN_GAME =     "join_game";      // A new player tries to join the game
	public static string JOIN_GAME_ACK = "join_game_ack";  // Confirm the JOIN_GAME request
	public static string ADD_USER =      "add_user";       // A new player joins the game
	public static string UPDATE_USER =   "update";         // Update player
	public static string DELETE_USER =   "delete_user";    // Delete a user from the game
	public static string USER_CRASH =    "crash";          // A remote user crashes
	public static string PAUSE =         "pause_game";     // Pause the game
	public static string RESUME =        "resume_game";    // Resume the game
	public static string GAME_OVER =     "game_over";      // Game over         

	private delegate void messageHandler(Dictionary<string, object> message);
	private static Dictionary<string, messageHandler> messageHandlerList = new Dictionary<string, messageHandler>();

	public void Dispatch(Dictionary<string, object> message) {
		string type = message["type"] as string;
		if (!messageHandlerList.ContainsKey(type)) {
			// Unknown type, ignore this message
			Debug.Log("Unknow Type: " + type);
			return ;
		}
		messageHandlerList[type](message);
	}

	public void SetGameStateManager(GameStateManager globalGameStateManager) {
		gameStateManager = globalGameStateManager;
	}
		
	private void HandleJoinGameMessage(Dictionary<string, object> message) {
		Debug.Log("HandleJoinGameMessage");

		// Generate ack message and send
		string targetUserID = message["userID"] as string;
		Dictionary<string, object> ackeMessage = gameStateManager.GetPlayerManager().GenerateACKMessage(targetUserID);
		gameStateManager.GetNetworkManager().writeSocket(ackeMessage);

		// Generate the initial position and direction
		Vector3 startPos = new Vector3(Random.Range(1.0f, 63.0f), 1.1f, Random.Range(1.0f, 63.0f));
		float h, v;
		float tmp = Random.Range(0.0f, 300.0f);
		if (0.0f <= tmp && tmp < 100.0f) {
			h = -1.0f;
		} else if (100.0f <= tmp && tmp < 200.0f) {
			h = 0.0f;
		} else {
			h = 1.0f;
		}
		if (h != 0.0f) {
			v = 0.0f;
		} else {
			tmp = Random.Range(0.0f, 200.0f);
			if (0.0f <= tmp && tmp < 100.0f) {
				v = -1.0f;
			} else {
				v = 1.0f;
			}
		}

		// Generate add user message and send
		Dictionary<string, object> addUserMessage = new Dictionary<string, object>();
		addUserMessage.Add("type", ADD_USER);
		addUserMessage.Add("userID", targetUserID);
		addUserMessage.Add("posX", startPos.x);
		addUserMessage.Add("posY", startPos.y);
		addUserMessage.Add("posZ", startPos.x);
		addUserMessage.Add("honrizontalDir", h);
		addUserMessage.Add("verticalDir", v);
		addUserMessage.Add("time", gameStateManager.GetCurLogicTime());
		gameStateManager.GetNetworkManager().writeSocket(addUserMessage);
	}

	private void HandleJoinGameACKMessage(Dictionary<string, object> message) {
		Debug.Log("HandleAddUserMessage");
		string targetUserID = message["userID"] as string;
		if (targetUserID == gameStateManager.GetUserID ()) {
			EnqueuePlayerManagerUnProcessedMessageQueue(message);
		}
	}

	private void HandleAddUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleAddUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleUpdateUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUpdateUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleDeleteUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleDeleteUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleUserCrashMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUserCrashMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandlePauseMessage(Dictionary<string, object> message) {
		Debug.Log("HandlePauseUserMessage");
		gameStateManager.setPauseState(true);
	}
		
	private void HandleResumeMessage(Dictionary<string, object> message) {
		Debug.Log("HandleResumeUserMessage");
		gameStateManager.setPauseState(false);
	}

	private void HandleGameOverMessage(Dictionary<string, object> message) {
		Debug.Log("HandleGameOverMessage");
		//TODO
		Application.LoadLevel(2);
	}

	private void EnqueuePlayerManagerUnProcessedMessageQueue(Dictionary<string, object> message) {
		Debug.Log("Enqueue " + message["type"] as string + " message");
		gameStateManager.GetPlayerManager().EnqueueUnProcessedMessageQueue(message);
	}

	void Awake() {
		messageHandlerList.Add(JOIN_GAME,     HandleJoinGameMessage);
		messageHandlerList.Add(JOIN_GAME_ACK, HandleJoinGameACKMessage);
		messageHandlerList.Add(ADD_USER,      HandleAddUserMessage);
		messageHandlerList.Add(UPDATE_USER,   HandleUpdateUserMessage);
		messageHandlerList.Add(DELETE_USER,   HandleDeleteUserMessage);
		messageHandlerList.Add(USER_CRASH,    HandleUserCrashMessage);
		messageHandlerList.Add(PAUSE,         HandlePauseMessage);
		messageHandlerList.Add(RESUME,        HandleResumeMessage);
		messageHandlerList.Add(GAME_OVER,     HandleGameOverMessage);
	}
}
