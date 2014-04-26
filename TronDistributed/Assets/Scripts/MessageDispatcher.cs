using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MessageDispatcher : MonoBehaviour {

	private GameStateManager gameStateManager;

	public static string JOIN_GAME = "join_game";                    // A new player tries to join the game
	public static string JOIN_USER_RESPONSE = "join_game_response";  //
	public static string JOIN_GAME_ACK = "join_game_ack";            // Confirm the JOIN_GAME request
	public static string ADD_USER = "add_user";                      // A new player joins the game
	//public static string ADD_LOCAL = "add_local";                     // Local user joins the game
	public static string UPDATE_USER = "update";                    // Update player
	//public static string UPDATE_LOCAL = "update_local";              // Update local player
	public static string DELETE_USER = "delete_user";                // Delete a user from the game
	public static string USER_CRASH = "crash";                       // A remote user crashes
	public static string PAUSE = "pause_game";                       // Pause the game
	public static string RESUME = "resume_game";                     // Resume the game
	public static string GAME_OVER = "game_over";                    // Game over         

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

		// Generate the initial position and direction
		Vector3 startPos = new Vector3(Random.Range(1.0f, 63.0f), 1, Random.Range(1.0f, 63.0f));
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

		// Generate response message and send
		Dictionary<string, object> responseMessage = new Dictionary<string, object>();
		responseMessage.Add("userID", gameStateManager.GetUserID());
		responseMessage.Add ("type", JOIN_USER_RESPONSE);
		responseMessage.Add ("posX", startPos.x);
		responseMessage.Add ("posY", startPos.y);
		responseMessage.Add ("posZ", startPos.x);
		responseMessage.Add ("honrizontalDir", h);
		responseMessage.Add ("verticalDir", v);
		gameStateManager.GetNetworkManager().writeSocket(responseMessage);

		// Generate ack message and send
		Dictionary<string, object> ackeMessage = gameStateManager.GetPlayerManager().GenerateACKMessage();
		gameStateManager.GetNetworkManager().writeSocket(ackeMessage);
	}

	private void HandleJoinGameACKMessage(Dictionary<string, object> message) {

	}

	private void HandleAddLocalMessage(Dictionary<string, object> message) {
	}

	private void HandleAddUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleAddUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleUpdateUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUpdateUserMessage");
		/*
		if (networkManager.GetUserID () == message.getUserID()) {
			return ;
		}
		playerManager.updatePlayerBasedOnNetwork(message.getUserID(), message.getPosition (), message.getMovement(),
		                                         message.getHorizontalDir(), message.getVerticalDir(),
		                                         message.getRotation(), message.getTime());*/
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}

	private void HandleUpdateLocalMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUpdateLocalMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
	}
		
	private void HandleDeleteUserMessage(Dictionary<string, object> message) {
		Debug.Log("HandleDeleteUserMessage");
		EnqueuePlayerManagerUnProcessedMessageQueue(message);
		/*
		playerManager.RemovePlayer(message.getUserID());
		*/
	}
		
	private void HandleUserCrashMessage(Dictionary<string, object> message) {
		Debug.Log("HandleUserCrashMessage");
		/*HandleDeleteUserMessage(message);*/
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
	}

	private void EnqueuePlayerManagerUnProcessedMessageQueue(Dictionary<string, object> message) {
		gameStateManager.GetPlayerManager().EnqueueUnProcessedMessageQueue(message);
	}

	void Awake() {
		messageHandlerList.Add(JOIN_GAME,     HandleJoinGameMessage);
		messageHandlerList.Add(JOIN_GAME_ACK, HandleJoinGameACKMessage);
		messageHandlerList.Add(ADD_USER,      HandleAddUserMessage);
		//messageHandlerList.Add(ADD_LOCAL,     HandleAddLocalMessage);
		messageHandlerList.Add(UPDATE_USER,   HandleUpdateUserMessage);
		//messageHandlerList.Add(UPDATE_LOCAL,  HandleUpdateLocalMessage);
		messageHandlerList.Add(DELETE_USER,   HandleDeleteUserMessage);
		messageHandlerList.Add(USER_CRASH,    HandleUserCrashMessage);
		messageHandlerList.Add(PAUSE,         HandlePauseMessage);
		messageHandlerList.Add(RESUME,        HandleResumeMessage);
		messageHandlerList.Add(GAME_OVER,     HandleGameOverMessage);
	}
}
