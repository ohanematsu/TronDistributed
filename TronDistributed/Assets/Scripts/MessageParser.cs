using UnityEngine;
using System.Collections;

//public class MessageParser : MonoBehaviour {
public class MessageParser {

	private PlayerManager playerManager;

	private string ADD_USER = "AddUser";
	private string UPDATE_USER = "update";
	private string DELETE_USER = "DeleteUser";
	private string USER_CRASH = "UserCrash";
	private string PAUSE = "Pause";
	private string RESUME = "Resume";
	
	public MessageParser(PlayerManager gamePlayerManager) {
		playerManager = gamePlayerManager;
	}
	
	public void ParseMessage(Message message) {
		string type = message.getType();
		if (type == ADD_USER) {
			HandleAddUserMessage(message);
		} else if (type == UPDATE_USER) {
			HandleUpdateUserMessage(message);
		} else if (type == DELETE_USER) {
			HandleDeleteUserMessage(message);
		} else if (type == USER_CRASH) {
			HandleUserCrashMessage(message);
		} else if (type == PAUSE) {
			HandlePauseMessage(message);
		} else if (type == RESUME) {
			HandleResumeMessage(message);
		} else {
			// Unknown type, ignore this message
			Debug.Log("Unknow Type: " + type);
		}
	}
		
	private void HandleAddUserMessage(Message message) {
		Debug.Log("HandleAddUserMessage");
		//playerManager.AddNewPlayer(message.getUserID(), message.getPosition(); message.get
	}
		
	private void HandleUpdateUserMessage(Message message) {
		Debug.Log("HandleUpdateUserMessage");
	}
		
	private void HandleDeleteUserMessage(Message message) {
		Debug.Log("HandleDeleteUserMessage");
	}
		
	private void HandleUserCrashMessage(Message message) {
		Debug.Log("HandleUserCrashMessage");
	}
		
	private void HandlePauseMessage(Message message) {
		Debug.Log("HandlePauseUserMessage");
	}
		
	private void HandleResumeMessage(Message message) {
		Debug.Log("HandleResumeUserMessage");
	}
}
