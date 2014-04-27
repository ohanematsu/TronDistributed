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
	private Vector3 moveDirection = Vector3.zero;
	
	// The current vertical speed
	//private float verticalSpeed = 0.0F;
	
	// The current x-z move speed
	private float moveSpeed = 0.0F;

	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
	
	// When did the user start walking (Used for going into trot after a while)
	private float walkTimeStart = 0.0F;

	private bool isControllable = true;

	private float curVerticalDir = 1;	
	private float curHorizontalDir = 0;

	private GameStateManager gameStateManager;

	//private bool paused = true;

	private bool directionChanged = false;

	private Vector3 invisiblePlace = new Vector3(32.0f, -10.0f, 32.0f);

	// Use this for initialization
	void Awake (){
		moveDirection = transform.TransformDirection(Vector3.forward);

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
	
		// Set initial position is invisible to the player
		//gameObject.transform.position = new Vector3(Random.Range(1.0f, 63.0f), 1.1f, Random.Range(1.0f, 63.0f));

		// Set initial state as paused
		//paused = true;
	}

	public void SetGameStateManager(GameStateManager globalGameStateManager) {
		gameStateManager = globalGameStateManager;
		//gameStateManager.setPauseState(false);
	}

	public void SetInitParameters(Vector3 initPos, float initHorizontalDir, float initVerticalDir) {
		gameObject.transform.position = initPos;
		UpdateSmoothedMovementDirection(initHorizontalDir, initVerticalDir);
		transform.rotation = Quaternion.LookRotation(moveDirection);
		directionChanged = false; // Reset
	}

	public void UpdateMotor(float newHorizontalDir, float newVerticalDir, float fixedDeltaTime) {
		if (!isControllable) {
			// kill all inputs if not controllable.
			Input.ResetInputAxes();
		}

		// Calculate move direction
		UpdateSmoothedMovementDirection(newHorizontalDir, newVerticalDir);
		
		// Calculate actual action
		Vector3 movement = moveDirection * moveSpeed;
		movement *= fixedDeltaTime;
		
		// Move the controller
		CharacterController controller = GetComponent<CharacterController>();
		//collisionFlags = controller.Move(movement);
		controller.Move(movement);
		
		// Set rotation to the move direction
		transform.rotation = Quaternion.LookRotation(moveDirection);
		
		// If direction changed, send UPDATE_USER message
		if (directionChanged) {
			Dictionary<string, object> message = new Dictionary<string, object>();
			message["honrizontalDir"] = curHorizontalDir;
			message["verticalDir"] = curVerticalDir;
			message["type"] = MessageDispatcher.UPDATE_USER;
			message["time"] = gameStateManager.GetCurLogicTime();
			gameStateManager.GetNetworkManager().writeSocket(message);
			Debug.Log ("Sent Message");
			directionChanged = false; //Reset
		}
	}

	private void UpdateSmoothedMovementDirection(float newHorizontalDir, float newVerticalDir) {
		Transform cameraTransform = Camera.main.transform;

		// Forward vector relative to the camera along the x-z plane    
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward); //Vectoe3.forward == Vector3(0, 0, 1).
		forward.y = 0;
		forward = forward.normalized; //normalized this vector with a magnitude of 1 

		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);

		if (newVerticalDir != 0) {
			if (newVerticalDir != curVerticalDir) {
				directionChanged = true;
			}
			curVerticalDir = newVerticalDir;
			curHorizontalDir = 0;
		}
		if (newHorizontalDir != 0) {
			if (newHorizontalDir != curHorizontalDir) {
				directionChanged = true;
			}
			curHorizontalDir = newHorizontalDir;
			curVerticalDir = 0;
		}

		// Are we moving backwards or looking backwards
		if (curVerticalDir < -0.2f) {
			movingBack = true;
		} else {	
			movingBack = false;
		}
			
		// Target direction relative to the camera
		Vector3 targetDirection= curHorizontalDir * right + curVerticalDir * forward;

		// Lock camera for short period when transitioning moving & standing still
		lockCameraTimer += Time.deltaTime;
	
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction	
		// moveDirection is always normalized, and we only update it if there is user input.	
		if (targetDirection != Vector3.zero) {
			// If we are really slow, just snap to the target direction
			if (moveSpeed < walkSpeed * 0.9f) {// && grounded) {
				moveDirection = targetDirection.normalized;
			} else { // Otherwise smoothly turn towards it
				moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				moveDirection = moveDirection.normalized;
			}
		}

		// Smooth the speed based on the current target direction
		float curSmooth = speedSmoothing * Time.deltaTime;

		// Choose target speed
		//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways		
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

		if (Time.time - trotAfterSeconds > walkTimeStart) {
			targetSpeed *= trotSpeed;
		} else {
			targetSpeed *= walkSpeed;
		}

		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);

		// Reset walk time start when we slow down
		if (moveSpeed < walkSpeed * 0.3f) {
			walkTimeStart = Time.time;
		}		
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
	
	Vector3 GetDirection() {
		return moveDirection;
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
