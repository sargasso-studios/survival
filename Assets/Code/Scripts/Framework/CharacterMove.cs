using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour {
	[Header("Input Names")]
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] private string runInputName;
	[SerializeField] private string crouchInputName;
	[SerializeField] private string jumpInputName;

	[Header("Movement")]
	[SerializeField] private float walkSpeed;
	[SerializeField] private float runSpeed;
	[SerializeField] private float swimSpeed;
	[SerializeField] private float crouchSpeed;
	[SerializeField] private float crouchCameraMove;
	[SerializeField] private AnimationCurve jumpFallOff;
	[SerializeField] private float jumpMultiplier;
	private float movementSpeed;
	private bool isJumping;
	private bool isCrouch;
	private bool isRun;

	[Header("GameObject")]
	[SerializeField] private GameObject camera;
	[SerializeField] private GameObject water;
	private CapsuleCollider charCollider;

	[Header("Physics")] // Two varialbes, ray length is the length of the ray shooting down to detect floor, slopForce is the downwards force applied to remove jitters
	[SerializeField] private float slopeForce;
	[SerializeField] private float slopeForceRayLength;
	[SerializeField] private Vector3 defaultGravity;
	[SerializeField] private Vector3 waterGravity;
	public static bool inWater;
	private Vector3 charGravity;

	// Called once after objects are initialized, used to initialize variables and get the Character Controller Component
	private void Awake() {
		charCollider = GetComponent<CapsuleCollider>();
		movementSpeed = walkSpeed;
		inWater = false;
		charGravity = defaultGravity;
	}

	// Called once per frame and calls the PlayerMovement function, which controls all player movement
	private void Update() {
		PlayerMovement();
	}

	// Called once per frame, first 5 lines deal with the keyboard movement, then stops slope jittering, then calls other functions
	private void PlayerMovement() {
		float vertInput = Input.GetAxis(verticalInputName);
		float horizInput = Input.GetAxis(horizontalInputName);

		Vector3 forwardMovement = transform.forward * vertInput;
		Vector3 rightMovement = transform.right * horizInput;

		// charCollider.transform.(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed * Time.deltaTime);
		// charCollider.Move(charGravity * Time.deltaTime);

		if((vertInput != 0 || horizInput != 0) && OnSlope()) {
			// charCollider.Move(Vector3.down * charCollider.height / 2 * slopeForce * Time.deltaTime);
		}

		// Swim();
		// Run();
		// Crouch();
		// JumpInput();
		// Debug.Log(movementSpeed);
	}

	// Checks to make sure player has pressed the jump key, and also is not already jumping
	private void JumpInput() {
		if(Input.GetButtonDown(jumpInputName) && !isJumping && !isCrouch) {
			isJumping = true;
			StartCoroutine(JumpEvent());
		}
	}

	// Called when player has jumped and isnt already jumping, and executes the jump
	private IEnumerator JumpEvent() {
		// charCollider.slopeLimit = 90.0f;
		float timeInAir = 0.0f;
		return null;

		// do {
		// 	float jumpForce = jumpFallOff.Evaluate(timeInAir);
		// 	// charCollider.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
		// 	timeInAir += Time.deltaTime;
		// 	yield return null;
		// } while (!charCollider.isGrounded && charCollider.collisionFlags != CollisionFlags.Above);

		// charCollider.slopeLimit = 45.0f;
		isJumping = false;
	}

	// Raycast used to detect the angle of the floor below the player, and if the player is on a slope applies a downwards force to stop jitters while walking down
	private bool OnSlope() {
		if (isJumping) {
			return false;
		}
		RaycastHit hit;

		if(Physics.Raycast(transform.position, Vector3.down, out hit, charCollider.height / 2 * slopeForceRayLength)) {
			if(hit.normal != Vector3.up) {
				return true;
			}
		}
		
		return false;
	}

	// Called when the player presses the run key, and increases the players movement while the key is held down
	private void Run() {
		if(Input.GetButtonDown(runInputName) && !isCrouch && !inWater) {
			movementSpeed = runSpeed;
			isRun = true;
		} else if(Input.GetButtonUp(runInputName) && !isCrouch && !inWater) {
			movementSpeed = walkSpeed;
			isRun = false;
		}
	}

	// Called when the player presses the crouch key, and lowers the camera while the key is held down
	private void Crouch() {
		if(Input.GetButtonDown(crouchInputName) && !isRun && !inWater && CrouchWaterDistance() && !isJumping) {
			camera.transform.Translate(Vector3.down * crouchCameraMove);
			movementSpeed = crouchSpeed;
			isCrouch = true;

		} else if(Input.GetButtonUp(crouchInputName) && !isRun && !inWater && isCrouch) {
			camera.transform.Translate(Vector3.up * crouchCameraMove);
			movementSpeed = walkSpeed;
			isCrouch = false;
		}
	}

	//
	private void Swim() {
		if(IsUnderwater() && !inWater) {
			if(isCrouch) camera.transform.Translate(Vector3.up * crouchCameraMove);
			movementSpeed = swimSpeed;
			isRun = false;
			isCrouch = false;
			inWater = true;
			charGravity = waterGravity;
			Debug.Log("Underwater");
		} else if(!IsUnderwater() && inWater) {
			movementSpeed = walkSpeed;
			inWater = false;
			charGravity = defaultGravity;
			Debug.Log("Above Water");
		}
		
	}

	// Checks if the player camera is below the y position of the water surface plane
	private bool IsUnderwater() {
		return camera.transform.position.y < water.transform.position.y;
	}

	// Checks if the difference between the camera position and water position is greater than the crouch movement, and thus prevents crouching below the water surface
	private bool CrouchWaterDistance() {
		return camera.transform.position.y - water.transform.position.y - 0.5 > crouchCameraMove;
	}
}