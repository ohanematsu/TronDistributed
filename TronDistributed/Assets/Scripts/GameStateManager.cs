using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameStateManager : MonoBehaviour {
	
	private NetworkManager networkManager;
	private PlayerManager playerManager;
	private MessageDispatcher messageDispatcher;
	private MotorController motorController;
	private InvisibleColliderFactory colliderFactory;
	private CollisionDetector collisionDetector;

	private int curLogicTime;
	private string userID;
	private bool paused = true;
	private bool receiveMsg;
	private float moveSpeed;
	private float timeInterval;

	private int state;
	public static int NORMAL = 0;
	public static int SENT_JOIN = 1;
	public static int RECEIVED_JOIN_ACK = 2;

	private float lastSentHonrizontalDir;
	private float lastSentVerticalDir;

	void Start() {
		// Get all components
		InitNetworkManager();
		InitPlayerManager();
		InitMessageDispatcher();
		InitMotorController();
		InvisibleColliderFactory();
		InitCollisionDetector();

		// Initiate global values
		InitGlobalValues();

		// Send JOIN_GAME message
		SendJoinGameMessage();

		// Enable receiving message
		receiveMsg = true;
	}

	void FixedUpdate() {
		if (!receiveMsg) {
			return ;
		}
		
		// Deliver received messages
		Dictionary<string, object> receiveMessage = networkManager.receive();
		while (receiveMessage != null) {
			Debug.Log("Receive " + receiveMessage["type"] + " message!");
			messageDispatcher.Dispatch(receiveMessage);
			receiveMessage = networkManager.receive();
		}
		
		if (!paused) {
			IncrementCurLogicTime ();
		}
	}

	void Update() {

		if (!receiveMsg) {
			return ;
		}
		/*
		// Deliver received messages
		Dictionary<string, object> receiveMessage = networkManager.receive();
		while (receiveMessage != null) {
			Debug.Log("Receive " + receiveMessage["type"] + " message!");
			messageDispatcher.Dispatch(receiveMessage);
			receiveMessage = networkManager.receive();
		}*/

		if (!paused) {
			//IncrementCurLogicTime();

			// Detect keyboard event and decide if need sending UPDATE message
			float verticalDir = Input.GetAxisRaw("Vertical");   
			float horizontalDir = Input.GetAxisRaw("Horizontal");
			if (verticalDir == 0.0f && horizontalDir == 0.0f) {
				return ;
			}
			if (verticalDir == motorController.GetVerticalDir() && horizontalDir == motorController.GetHorizontalDir()) {
				return ;
			}
			if (verticalDir == lastSentVerticalDir && horizontalDir == lastSentHonrizontalDir) {
				return ;
			}

			// Send UPDATE message
			Dictionary<string, object> message = new Dictionary<string, object>();
			message["type"] = (object)MessageDispatcher.UPDATE_USER;
			message["userID"] = (object)userID;
			message["horizontalDir"] = (object)horizontalDir;
			message["verticalDir"] = (object)verticalDir;
			message["time"] = (object)curLogicTime;
			networkManager.writeSocket(message);
			Debug.Log("Send local update message");

			// Update last sent direction
			lastSentHonrizontalDir = horizontalDir;
			lastSentVerticalDir = verticalDir;
		}
	}

	void OnDestroy() {
		if (networkManager.GetSocketState()) {
			Dictionary<string, object> message = new Dictionary<string, object>();
			message["type"] = MessageDispatcher.DELETE_USER;
			message["userID"] = userID;
			networkManager.writeSocket(message);
			networkManager.closeSocket();
			Debug.Log("Release! Socket closed");
		}
	}

	private void InitNetworkManager() {
		// Get NetworkManager component
		networkManager = gameObject.GetComponent<NetworkManager>();
		if (networkManager == null) {
			Debug.Log("Cannot find NetworkManager");
			// TODO:Show something and then quit
			Application.LoadLevel(3);
		}
		Debug.Log("Get NetworkManager success!");
		
		// Initiate the connection
		networkManager.initialize();
		if (!networkManager.GetSocketState()) {
			Debug.Log("Set up connection failed!");
			// TODO:Show something and then quit
			Application.LoadLevel(3);
		}
		Debug.Log("Set up NetworkManager success!");
	}

	private void InitPlayerManager() {
		// Get PlayerManager component
		playerManager = gameObject.GetComponent<PlayerManager>();
		if (playerManager == null) {
			Debug.Log("Cannot find PlayerManager");	
			// TODO:Show something and then quit
			Application.LoadLevel(3);
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
			Application.LoadLevel(3);
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
			Application.LoadLevel(3);
		}
		motorController.SetGameStateManager(this);
		Debug.Log("Get motorController success");
	}
	
	private void InvisibleColliderFactory() {
		// Get trail component
		GameObject trail = transform.GetChild(3).gameObject;
		if (trail == null) {
			Debug.Log("Cannot find trail");
		}

		// Get InvisibleColliderFactory
		//colliderFactory = gameObject.GetComponent<InvisibleColliderFactory>();
		colliderFactory = trail.GetComponent<InvisibleColliderFactory>();
		if (colliderFactory == null) {
			Debug.Log("Find InvisibleColliderFactory failed");
			Application.LoadLevel(3);
		}
		Debug.Log("Get InvisibleColliderFactory success!");
	}

	private void InitCollisionDetector() {
		// Get Collision Detector
		collisionDetector = gameObject.GetComponent<CollisionDetector>();
		if (collisionDetector == null) {
			Debug.Log("Find collision detector failed!");
			Application.LoadLevel(3);
		}
		collisionDetector.SetGameStateManager(this);
		Debug.Log("Get collision detector success");
	}

	private void InitGlobalValues() {
		receiveMsg = false;

		// Initiate logic time
		curLogicTime = 0;

		// Initiate user ID
		userID = networkManager.GetUserID();

		// Initiate moveSpeed 
		moveSpeed = 5.0f;

		state = NORMAL;

		timeInterval = 0.02f;

		// Initiate pause state
		SetPauseState(true);

		lastSentHonrizontalDir = 0.0f;
		lastSentVerticalDir = 0.0f;
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
		SetPauseState(true);

		state = SENT_JOIN;
	}

	public void SetPauseState(bool state) {
		paused = state;
		playerManager.setPauseState(state);
		colliderFactory.SetPauseState(state);
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

	public InvisibleColliderFactory GetColliderFactory() {
		return colliderFactory;
	}
	
	public int GetCurLogicTime() {
		return curLogicTime;
	}
	
	public void SetCurLogicTime(int time) {
		curLogicTime = time;
	}
	
	public void IncrementCurLogicTime() {
		curLogicTime++;
	}
	
	public string GetUserID() {
		return userID;
	}
	
	public int GetState() {
		return state;
	}
	
	public void SetState(int newState) {
		state = newState;
	}
	
	public float GetTimeInterval() {
		return timeInterval;
	}

	public float GetMoveSpeed() {
		return moveSpeed;
	}
}
