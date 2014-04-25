using UnityEngine;
using System.Collections;

//public class MessageParser : MonoBehaviour {
public class MessageParser {

	private PlayerManager playerManager;
	private MotorController motorController;
	private NetworkManager networkManager;

	public static string JOIN_USER = "join_game"; // this type of message is only received by the ui of super node
	public static string UPDATE_USER = "update";
	public static string DELETE_USER = "delete_user";
	public static string JOIN_USER_RESPONSE = "join_game_response"; // add new player
	public static string USER_CRASH = "crash";
	public static string PAUSE = "pause_game";
	public static string RESUME = "resume_game";
	public static string GAME_OVER = "game_over";
	
	public MessageParser(MotorController gameMotorController, PlayerManager gamePlayerManager, 
	                     NetworkManager gameNetworkManager) {
		motorController = gameMotorController;
		playerManager = gamePlayerManager;
		networkManager = gameNetworkManager;
	}
	
	public void ParseMessage(Message message) {
		string type = message.getType();
		if (type == JOIN_USER) {
			HandleJoinMessage (message);
		} else if (type == UPDATE_USER) {
			HandleUpdateUserMessage (message);
		} else if (type == DELETE_USER) {
			HandleDeleteUserMessage (message);
		} else if (type == USER_CRASH) {
			HandleUserCrashMessage (message);
		} else if (type == PAUSE) {
			HandlePauseMessage (message);
		} else if (type == RESUME) {
			HandleResumeMessage (message);
		} else if (type == JOIN_USER_RESPONSE) {
			HandleJoinResponseMessage (message);
		} else if (type == GAME_OVER) {
			HandleGameOverMessage(message);
		} else {
			// Unknown type, ignore this message
			Debug.Log("Unknow Type: " + type);
		}
	}
		
	private void HandleJoinMessage(Message message) {
		Debug.Log("HandleAddUserMessage");

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

		// Generate response message
		Message responseMessage = new Message();
		responseMessage.setUserName(message.getUserID());
		responseMessage.setType("JOIN_USER_RESPONSE");
		responseMessage.setPosition(startPos);
		responseMessage.setHorizontalDir (h);
		responseMessage.setVerticalDir (v);

		networkManager.writeSocket (responseMessage.toJsonString ());
	}
		
	private void HandleUpdateUserMessage(Message message) {
		Debug.Log("HandleUpdateUserMessage");
		playerManager.updatePlayerBasedOnNetwork(message.getUserID(), message.getPosition (), message.getMovement(),
		                                         message.getHorizontalDir(), message.getVerticalDir(),
		                                         message.getRotation(), message.getTime());
	}
		
	private void HandleDeleteUserMessage(Message message) {
		Debug.Log("HandleDeleteUserMessage");
		playerManager.RemovePlayer(message.getUserID());
	}
		
	private void HandleUserCrashMessage(Message message) {
		Debug.Log("HandleUserCrashMessage");
		HandleDeleteUserMessage(message);
	}
		
	private void HandlePauseMessage(Message message) {
		Debug.Log("HandlePauseUserMessage");
		motorController.setPauseState(true);
		playerManager.setPauseState(true);
	}
		
	private void HandleResumeMessage(Message message) {
		Debug.Log("HandleResumeUserMessage");
		motorController.setPauseState(false);
		playerManager.setPauseState(false);
	}

	private void HandleJoinResponseMessage(Message message) {
		Debug.Log("HandleJoinResponseMessage");
		playerManager.AddNewPlayer(message.getUserID(), message.getPosition(), message.getHorizontalDir(), 
		                           message.getVerticalDir(), message.getRotation(), message.getTime());

	}

	private void HandleGameOverMessage(Message message) {
		
	}
}
