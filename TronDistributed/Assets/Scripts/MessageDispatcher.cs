using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class MessageDispatcher : MonoBehaviour {

	private GameStateManager gameStateManager;

	public static string JOIN_GAME =     "join_game";      // A new player tries to join the game
	public static string JOIN_GAME_ACK = "join_game_ack";  // Confirm the JOIN_GAME request
	public static string ADD_USER =      "add_user";       // A new player joins the game
	public static string UPDATE_USER =   "update";         // Update player
	public static string DELETE_USER =   "delete_user";    // Delete a user from the game
	public static string USER_CRASH =    "crash";          // A remote user crashes
	public static string ALIVE_LIST =    "alive_list";     // Super node die
	public static string PAUSE =         "pause_game";     // Pause the game
	public static string RESUME =        "resume_game";    // Resume the game
	public static string GAME_OVER =     "game_over";      // Game over         

	private delegate void messageHandler(Dictionary<string, object> message);
	private static Dictionary<string, messageHandler> messageHandlerList = new Dictionary<string, messageHandler>();

	private float initX = 10.0f;

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

		// Check message's receiver first.
		// If it receives a JOIN_GAME message sent from itself, discard it
		string targetUserID = message["userID"] as string;
		if (targetUserID == gameStateManager.GetUserID()) {
			return ;
		}

		// Generate ack message and send
		Dictionary<string, object> ackeMessage = gameStateManager.GetPlayerManager().GenerateACKMessage(targetUserID);
		gameStateManager.GetNetworkManager().writeSocket(ackeMessage);

		// Send ADD_USER message
		if (gameStateManager.GetState() == GameStateManager.SENT_JOIN) {
			// If the super node has not been added to the game, send a message to add itself
			SendAddUserMessage(gameStateManager.GetUserID());
			gameStateManager.SetState(GameStateManager.NORMAL);
		}
		SendAddUserMessage(targetUserID);

		// Enable Clock
		gameStateManager.SetPauseState(false);
	}

	private void SendAddUserMessage(string userID) {
		// Generate the initial position and direction

		/* For test*/
		Vector3 startPos = new Vector3(initX, 1.1f, 10.0f);
		float h, v;
		h = 0.0f;
		v = 1.0f;
		initX += 10.0f;

		//Vector3 startPos = new Vector3(UnityEngine.Random.Range(1.0f, 63.0f), 1.1f, UnityEngine.Random.Range(1.0f, 63.0f));
		//float h, v;
		/*float tmp = UnityEngine.Random.Range(0.0f, 300.0f);
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
			tmp = UnityEngine.Random.Range(0.0f, 200.0f);
			if (0.0f <= tmp && tmp < 100.0f) {
				v = -1.0f;
			} else {
				v = 1.0f;
			}
		}*/
		
		// Generate add new user message and send
		Dictionary<string, object> addUserMessage = new Dictionary<string, object>();
		addUserMessage.Add("type", ADD_USER);
		addUserMessage.Add("userID", userID);
		addUserMessage.Add("posX", startPos.x);
		addUserMessage.Add("posY", startPos.y);
		addUserMessage.Add("posZ", startPos.x);
		addUserMessage.Add("horizontalDir", h);
		addUserMessage.Add("verticalDir", v);
		addUserMessage.Add("time", gameStateManager.GetCurLogicTime());
		gameStateManager.GetNetworkManager().writeSocket(addUserMessage);
	}

	private void HandleJoinGameACKMessage(Dictionary<string, object> message) {
		Debug.Log("HandleJoinGameACKMessage");
		string targetUserID = message["userID"] as string;

		// If the user isn't the target user, discard this message
		if (targetUserID != gameStateManager.GetUserID ()) {
			return ;
		}
			
		// Sync local logic time to global logic time
		gameStateManager.SetCurLogicTime(Convert.ToInt32(message["time"]));
		Debug.Log("After receiving JOIN_ACK, time is " + gameStateManager.GetCurLogicTime());

		// Parse global state data
		Dictionary<string, object> passedAllUsersGlobalStates = message["globalState"] as Dictionary<string, object>;
		Debug.Log("Number of user: " + passedAllUsersGlobalStates.Count);

		// Enqueue all messages
		foreach (KeyValuePair<string, object> pair in passedAllUsersGlobalStates) {
			/* For debug */
			string userID = pair.Key as string;
			Debug.Log("Sync User: " + userID);

			List<object> messages = pair.Value as List<object>;
			int cc = 0;
			foreach (object msg in messages) {
				Dictionary<string, object> processedMessage = msg as Dictionary<string, object>;
				EnqueuePlayerManagerUnProcessedMessageQueue(processedMessage);

				/* dirty fix */
				if (cc == 0) {
					Dictionary<string, object> tmp = new Dictionary<string, object>();
					tmp["type"] = "dirtyfix";
					for (int i = 0; i < 8; i++) {
						EnqueuePlayerManagerUnProcessedMessageQueue(tmp);
					}
				}
				cc = 1;
			}
		}

		// Update state
		gameStateManager.SetState(GameStateManager.RECEIVED_JOIN_ACK);
		Debug.Log("After receiving JOIN_ACK, state is " + gameStateManager.GetState());

		// Enable Clock
		gameStateManager.SetPauseState(false);
	}

	private void HandleAddUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleAddUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
		string userID = message ["userID"] as String;
		if (userID == gameStateManager.GetUserID ()) {
			return ;
		}

		/* dirty fix */
		Dictionary<string, object> tmp = new Dictionary<string, object>();
		tmp["type"] = "dirtyfix";
		for (int i = 0; i < 8; i++) {
			EnqueuePlayerManagerUnProcessedMessageQueue(tmp);
		}
	}
		
	private void HandleUpdateUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUpdateUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleDeleteUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleDeleteUserMessage");
		gameStateManager.GetPlayerManager().RemovePlayer(message);
	}
		
	private void HandleUserCrashMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUserCrashMessage");
		gameStateManager.GetPlayerManager().RemovePlayer(message);
	}
		
	private void HandlePauseMessage(Dictionary<string, object> message) {
		Debug.Log("HandlePauseUserMessage");
		gameStateManager.SetPauseState(true);
	}
		
	private void HandleResumeMessage(Dictionary<string, object> message) {
		Debug.Log("HandleResumeUserMessage");
		gameStateManager.SetPauseState(false);
	}

	private void HandleGameOverMessage(Dictionary<string, object> message) {
		Debug.Log("HandleGameOverMessage");
		Application.LoadLevel(2);
	}

	private void HandleAliveListMessgae(Dictionary<string, object> message) {
		Debug.Log("HandleAliveListMessgae");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
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
