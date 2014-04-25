using UnityEngine;
using System.Collections;

//public class MessageParser : MonoBehaviour {
public class MessageParser {

	private PlayerManager playerManager;
	private MotorController motorController;

	private string JOIN_USER = "join_game"; // this type of message is only received by the ui of super node
	private string UPDATE_USER = "update";
	private string DELETE_USER = "delete_user";
	private string JOIN_USER_RESPONSE = "join_game_response"; // add new player
	private string USER_CRASH = "crash";
	private string PAUSE = "pause_game";
	private string RESUME = "resume_game";
	private string GAME_OVER = "game_over";
	
	public MessageParser(MotorController gameMotorController, PlayerManager gamePlayerManager) {
		motorController = gameMotorController;
		playerManager = gamePlayerManager;
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
		//playerManager.AddNewPlayer(message.getUserID(), message.getPosition(); message.get
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
