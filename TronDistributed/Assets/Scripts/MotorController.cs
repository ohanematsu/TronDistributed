using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MotorController : MonoBehaviour {
	public AnimationClip idleAnimation;	
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip jumpPoseAnimation;

	public float walkMaxAnimationSpeed = 0.75F;
	public float trotMaxAnimationSpeed = 1F;
	public float runMaxAnimationSpeed = 1F;
	public float jumpAnimationSpeed = 1F;
	public float landAnimationSpeed =1F;

	private Animation _animation;

	enum CharacterState {
		Idle = 0,
		Walking = 1,		
		Trotting = 2,		
		Running = 3,		
		//Jumping = 4,    		
	}
	
	//private CharacterState _characterState;
	
	// The speed when walking	
	public float walkSpeed = 2.0F;
	
	// after trotAfterSeconds of walking we trot with trotSpeed	
	public float trotSpeed = 4.0F;
	
	// when pressing "Fire3" button (cmd) we start running	
	public float runSpeed = 6.0F;

	public float inAirControlAcceleration = 3.0F;

	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5F;

	// The gravity for the character
	public float gravity = 20.0F;
	
	// The gravity in controlled descent mode
	public float speedSmoothing = 10.0F;
	public float rotateSpeed = 500.0F;
	public float trotAfterSeconds = 3.0F;

	// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
	private float lockCameraTimer = 0.0F;

	// The current move direction in x-z
	//private Vector3 moveDirection = Vector3.zero;
	
	// The current vertical speed
	//private float verticalSpeed = 0.0F;
	
	// The current x-z move speed
	private float moveSpeed = 4.0f;

	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;

	private bool isControllable = true;

	private GameStateManager gameStateManager;

	private Camera mainCamera;
	private Vector3 cameraMotorDistance;

	private float curVerticalDir = 1;	
	private float curHorizontalDir = 0;

	private int lastProcessedTime = 0;

	private List<GameObject> trailColliders;
	private InvisibleColliderFactory colliderFactory;
	private Vector3 colliderPosOffset;
	private Vector3 lastColliderInitPos;

	// Use this for initialization
	void Awake (){
		//moveDirection = transform.TransformDirection(Vector3.forward);

		_animation = GetComponent<Animation>();	
		if (!_animation) {
			Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		}

		if(!idleAnimation) {
			_animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		
		if(!walkAnimation) {
			_animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		
		if(!runAnimation) {
			_animation = null;
			Debug.Log("No run animation found. Turning off animations.");
		}

		colliderPosOffset = new Vector3(0, 0, -2.0f);

		// Calculate the distance between camera and player
		mainCamera = Camera.main;
		cameraMotorDistance = mainCamera.transform.position - gameObject.transform.position;
	}

	public void SetGameStateManager(GameStateManager globalGameStateManager) {
		gameStateManager = globalGameStateManager;
	}

	public void SetInitParameters(Vector3 initPos, float initHorizontalDir, float initVerticalDir, int initLogicTime) {
		// Initilize position
		gameObject.transform.position = initPos;

		// Initilize direction and rotation
		curHorizontalDir = initHorizontalDir;
		curVerticalDir = initVerticalDir;
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		transform.rotation = Quaternion.LookRotation(moveDirection);

		// Reset camera position
		//mainCamera.transform.position = initPos + cameraMotorDistance;

		// Initialize collider container
		trailColliders = new List<GameObject>();
		colliderFactory = gameStateManager.GetColliderFactory();
		CreateTrailCollider();

		// Init last processed time
		lastProcessedTime = initLogicTime;
	}

	private void CreateTrailCollider() {
		Vector3 colliderPos = transform.TransformPoint(colliderPosOffset);
		GameObject newTrailCollider = colliderFactory.CreateCollider(colliderPos);
		trailColliders.Add(newTrailCollider);
		lastColliderInitPos = newTrailCollider.transform.position;
		Debug.Log("After adding a new collider, now this player has " + trailColliders.Count + " colliders");
	}

	private void UpdateLastCollider(Vector3 newColliderPos, float extendsion) {
		if (trailColliders.Count == 0) {
			return ;
		}
		
		GameObject trailCollider = trailColliders[trailColliders.Count - 1];
		if (trailCollider != null) {
			colliderFactory.UpdateCollider(trailCollider, lastColliderInitPos, 
				newColliderPos, curHorizontalDir, curVerticalDir, extendsion);
		}
	}

	public void UpdateDirection(float newHorizontalDir, float newVerticalDir, int newLogicTime, float fixedDeltaTime) {
		// Move the motor first
		UpdateMotor(newLogicTime, fixedDeltaTime);

		// Update direction
		if (newVerticalDir != 0.0f) {
			curVerticalDir = newVerticalDir;
			curHorizontalDir = 0.0f;
		}
		if (newHorizontalDir != 0.0f) {
			curHorizontalDir = newHorizontalDir;
			curVerticalDir = 0.0f;
		}

		// Create a new collider
		CreateTrailCollider();

		// Update lastProcessedTime
		lastProcessedTime = newLogicTime;
	}

	public void UpdateMotor(int newLogicTime, float fixedDeltaTime) {
		if (!isControllable) {
			// kill all inputs if not controllable.
			Input.ResetInputAxes();
		}
		
		// Calculate movement
		float deltaTime = (newLogicTime - lastProcessedTime) * fixedDeltaTime;
		Vector3 moveDirection = new Vector3(curHorizontalDir, 0, curVerticalDir);
		Vector3 movement = gameStateManager.GetMoveSpeed() * moveDirection * deltaTime;
		//Debug.Log("Local Movement: " + movement.x + "," + movement.y + "," + movement.z + ", speed = " + gameStateManager.GetMoveSpeed());
		
		// Move the controller
		CharacterController controller = GetComponent<CharacterController>();
		controller.Move(movement);

		// Validate Position
		if (gameObject.transform.position.x >= 128.5f || gameObject.transform.position.x < 0.0f) {
			if (gameStateManager.GetNetworkManager().GetSocketState()) {
				Dictionary<string, object> message = new Dictionary<string, object>();
				message["type"] = MessageDispatcher.DELETE_USER;
				message["userID"] = gameStateManager.GetUserID();
				gameStateManager.GetNetworkManager().writeSocket(message);
				gameStateManager.GetNetworkManager().closeSocket();
				Debug.Log("Run out of map! Quit");
				Application.LoadLevel(3);
			}
		}
		if (gameObject.transform.position.z >= 128.5f || gameObject.transform.position.z < 0.0f) {
			if (gameStateManager.GetNetworkManager().GetSocketState()) {
				Dictionary<string, object> message = new Dictionary<string, object>();
				message["type"] = MessageDispatcher.DELETE_USER;
				message["userID"] = gameStateManager.GetUserID();
				gameStateManager.GetNetworkManager().writeSocket(message);
				gameStateManager.GetNetworkManager().closeSocket();
				Debug.Log("Run out of map! Quit");
				Application.LoadLevel(3);
			}
		}
		
		// Set rotation to the move direction
		transform.rotation = Quaternion.LookRotation(moveDirection);

		// Update Camera Position
		//mainCamera.transform.position += movement;

		// Update collider
		Vector3 newColliderPos = transform.TransformPoint(colliderPosOffset);
		UpdateLastCollider(newColliderPos, movement.magnitude);

		// Update Time
		lastProcessedTime = newLogicTime;
	}

	private void UpdateLastProcessedTime(int newLogicTime) {
		lastProcessedTime = newLogicTime;
	}

	float GetSpeed() {
		return moveSpeed;
	}

	public bool IsJumping() {
		//return jumping;
		return false;
	}

	bool IsGrounded() {
		//return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
		return true;
	}

	public float GetHorizontalDir() {
		return curHorizontalDir;
	}

	public float GetVerticalDir() {
		return curVerticalDir;
	}

	bool IsMovingBackwards() {
		return movingBack;
	}

	public float GetLockCameraTimer() {
		return lockCameraTimer;
	}
	
	bool IsMoving() {
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
	}

	void Reset() {
		gameObject.tag = "Player";
	}
}
