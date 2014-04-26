using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameStateManager : MonoBehaviour {
	
	private NetworkManager networkManager;
	private PlayerManager playerManager;
	private MessageDispatcher messageDispatcher;
	private MotorController motorController;

	private int curLogicTime;
	private string userID;
	private bool paused;

	void Start() {
		// Get all components
		InitNetworkManager();
		InitPlayerManager();
		InitMessageDispatcher();
		InitMotorController();

		// Initiate global values
		InitGlobalValues();

		// Send JOIN_GAME message
		SendJoinGameMessage();
	}

	void FixedUpdate() {
		if (!paused) {
			IncrementCurLogicTime();

			// Detect keyboard event and send message to its own playermanager
			float verticalDir = Input.GetAxisRaw("Vertical");   
			float horizontalDir = Input.GetAxisRaw("Horizontal");
			Dictionary<string, object> message = new Dictionary<string, object>();
			message["type"] = (object)MessageDispatcher.UPDATE_USER;
			message["userID"] = (object)userID;
			message["horizontalDir"] = (object)horizontalDir;
			message["verticalDir"] = (object)verticalDir;
			message["time"] = (object)curLogicTime;
			messageDispatcher.Dispatch(message);
		}

		// Deliver received messages
		Dictionary<string, object> receiveMessage = networkManager.receive();
		while (receiveMessage != null) {
			Debug.Log("Message from " + (receiveMessage["userID"] as string));
			messageDispatcher.Dispatch(receiveMessage);
			receiveMessage = networkManager.receive();
		}
	}

	public NetworkManager GetNetworkManager() {
		return networkManager;
	}

	public PlayerManager GetPlayerManager() {
		return playerManager;
	}

	public MessageDispatcher GetMessageDispatcher() {
		return messageDispatcher;
	}

	public MotorController GetMotorController() {
		return motorController;
	}

	public int GetCurLogicTime() {
		return curLogicTime;
	}

	public void IncrementCurLogicTime() {
		curLogicTime++;
	}

	public string GetUserID() {
		return userID;
	}

	private void InitNetworkManager() {
		// Get NetworkManager component
		networkManager = gameObject.GetComponent<NetworkManager>();
		if (networkManager == null) {
			Debug.Log("Cannot find NetworkManager");
			// TODO:Show something and then quit
			Application.LoadLevel(2);
		}
		Debug.Log("Get NetworkManager success!");
		
		// Initiate the connection
		networkManager.initialize();
		if (!networkManager.GetSocketState()) {
			Debug.Log("Set up connection failed!");
			// TODO:Show something and then quit
			Application.LoadLevel(2);
		}
		Debug.Log("Set up NetworkManager success!");
	}

	private void InitPlayerManager() {
		// Get PlayerManager component
		playerManager = gameObject.GetComponent<PlayerManager>();
		if (playerManager == null) {
			Debug.Log("Cannot find PlayerManager");	
			// TODO:Show something and then quit
			Application.LoadLevel(2);
		}
		playerManager.SetGameStateManager(this);
		Debug.Log("Get playerManager success");
	}

	private void InitMessageDispatcher() {
		// Get MessageDispatcher component
		messageDispatcher = gameObject.GetComponent<MessageDispatcher>();
		if (messageDispatcher == null) {
			Debug.Log("Cannot find the messageDispatcher");
			// TODO:Show something and then quit
			Application.LoadLevel(2);
		}
		messageDispatcher.SetGameStateManager(this);
		Debug.Log("Get MessageDispatcher success");
	}

	private void InitMotorController() {
		// Get MotorController component
		motorController = gameObject.GetComponent<MotorController>();
		if (motorController == null) {
			Debug.Log("Cannot find the motorController");
			// TODO:Show something and then quit
			Application.LoadLevel(2);
		}
		motorController.SetGameStateManager(this);
		Debug.Log("Get motorController success");
	}

	private void InitGlobalValues() {
		// Initiate logic time
		curLogicTime = 0;

		// Initiate user ID
		userID = networkManager.GetUserID();

		// Initiate pause state
		setPauseState(true);
	}

	private void SendJoinGameMessage() {
		// Send JOIN_GAME message
		Dictionary<string, object> message = new Dictionary<string, object>();
		message.Add("type", MessageDispatcher.JOIN_GAME);
		message.Add("userID", userID);
		message.Add("time", curLogicTime);
		networkManager.writeSocket(message);
		Debug.Log("Send JOIN_GAME message");

		// Set state to paused to wait for response
		setPauseState(true);
	}

	public void setPauseState(bool state) {
		paused = state;
		playerManager.setPauseState(state);
	}
}
