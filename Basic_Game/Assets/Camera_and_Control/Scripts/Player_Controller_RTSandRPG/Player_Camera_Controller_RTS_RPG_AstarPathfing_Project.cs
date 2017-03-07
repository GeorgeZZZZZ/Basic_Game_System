using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;
//using System.Linq;
using System.Text;

//	0.5.5
[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(CapsuleCollider))]
[RequireComponent (typeof(Selectable_Unit_Controller_AstarPathfing_Project))]
public class Player_Camera_Controller_RTS_RPG_AstarPathfing_Project : MonoBehaviour {

	public GameObject Cam_Center_Point;
	public GameObject Select_Circle_Prefab;

	public int Edge_Boundary = 1;	//	valuable use for detect limit movement which mouse move near screen edge, unit in pixel 
	public float Player_Normal_Speed=1f;
	public float Player_Run_Speed=2.5f;
	public float Player_Turnning_Speed=180f; //180 degree per second
	public float Cam_Move_Speed=1f;

	public bool Move_Player_towards_Character_Facing = false;	//	WASD control forward/ backward/ left shift/ right shift
	public bool Move_Player_Along_World_Axis = false;	//	WASD control forward/ backward/ left shift/ right shift
	public bool Turn_Player_by_Keyboard = false;	//	QE control turn left/ turn right
	public bool Turn_Player_by_Mouse_Point = false;	//	turn to mouse position

	public bool Move_Or_Turn_Player_According_To_Camera = false;	//	WASD control forward/ backward/ left shift or turn left by Camera behavior/ right shift or turn right by Camera behavior

	public bool Move_Camera_towards_cam_Facing = false;
	public bool Move_Camera_Along_World_Axis = false;
	public bool Move_Camera_at_Edge = false;

	public bool Force_RTS_Cam_View = false;	//	Force enter RTS view mode, not perfect yet
	[HideInInspector] public bool characterMovingFlag = false;	//	flag is true if get input for character move

	private Camera playerCam;
	private Vector3 moveCalculation;
	private Rigidbody playerRigidbody;
	private Animator anim;

	private float speed;
	private float turnSpeed;
	private float retreatDivisor = 2f; // when character go backward or sideward, he's run and walk speed will divide by this number

	private bool mouseAreaSelec = false;	//	mouse area selecting flag
	private Vector3 curMousPos;

	private bool camFollowFlag;

	private float newYAng;	//	y angle container, for MoTbKaCB () to calculate next character y angle depending on camera center

	// Use this for initialization
	void Start () {
		
		playerRigidbody = GetComponent <Rigidbody> ();
		playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;		//freeze rigidbody's rotation to prevent fall down to floor
		//playerRigidbody.drag = Mathf.Infinity;
		playerRigidbody.angularDrag = Mathf.Infinity;	//prevant character keep turn not stop after finish rotation
		anim = GetComponent <Animator> ();
		playerCam = Cam_Center_Point.GetComponent <CameraFunctions> ().Cam_Obj.GetComponent <Camera> ();

		newYAng = transform.eulerAngles.y;	//	initialize value in first run
	}

	// Update is called once per physics update
	void FixedUpdate () {

		DEBUG ();	//	Call debug functions

		float moveFBForAnimeRPG = 0f;
		float moveLRForAnimeRPG = 0f;

		//	get physics input
		float moveFB = Input.GetAxis ("Vertical");
		float moveLR = Input.GetAxis ("Horizontal");
		float turnLR = Input.GetAxisRaw ("Rotate");
		bool mousLefButt = Input.GetMouseButton (0);

		if (moveFB != 0f | moveLR != 0f)
			characterMovingFlag = true;
		else
			characterMovingFlag = false;
		
		//when push shift button, not only increase walking speed also increase turning speed
		if (Input.GetButton ("Run")){
			speed = Player_Run_Speed;
			turnSpeed = Player_Turnning_Speed * 2.5f;
		}else{
			speed = Player_Normal_Speed;
			turnSpeed = Player_Turnning_Speed;
		}

		if (Force_RTS_Cam_View) {
			RELEASE ();
		}
		camFollowFlag = Cam_Center_Point.GetComponent <CameraFunctions> ().followPlayerFlag;	//check if camera disconnect form player

		if (camFollowFlag) {	//	if camera sitll follow player, keyboard control character
			//	only call move block when player received movement command
			if (moveFB != 0f || moveLR != 0f) {
				if (Move_Player_Along_World_Axis) {
					//	Move Player along World axis	
					MPaW (moveFB, moveLR, speed, retreatDivisor);
				} else if (Move_Player_towards_Character_Facing) {
					//	Move Player towards Character Facing
					MPtCF (moveFB, moveLR, speed, retreatDivisor);
				}
				moveFBForAnimeRPG = moveFB;
				moveLRForAnimeRPG = moveLR;
			}

			if (Turn_Player_by_Mouse_Point) {
				//	turning by mouse point
				TPbMP ();
			} else if (Turn_Player_by_Keyboard) {
				//	turning by keyboard
				if (turnLR != 0f) {
					TPbKC (turnLR, turnSpeed);
				}
			}

			if (Move_Or_Turn_Player_According_To_Camera) {
				//	move or turn by keyboard according camera behavior
				MoTbKaCB (moveFB, moveLR, speed, turnSpeed, retreatDivisor);
			}

		} else {	//	if not follow then move camera center point directilly, keyboard now control camera

			if (Move_Camera_at_Edge) {
				float movXOff, movZOff;
				Edge_Move_Control (out movXOff, out movZOff);	//	give offset velue by detect if mouse move near screen edge

				//moveLR = turnLR;	//	make A/D key to control Camera left and right movement

				moveFB += movXOff;
				moveLR += movZOff;
			}
			//	only call MDCtCF() when camera received movement command
			if (moveFB != 0f || moveLR != 0f) {
				if (Move_Camera_Along_World_Axis) {
					// Move Disconnect Camera Along World Axis
					MDCaW (moveFB, moveLR, Cam_Move_Speed);
				} else if (Move_Camera_towards_cam_Facing) {
					// Move Disconnect Camera towards Camera Facing
					MDCtCF (moveFB, moveLR, Cam_Move_Speed);
				}
			}

			RTS_Point_Selec (mousLefButt);

			RTS_Area_Selec (mousLefButt);
		}

		if (camFollowFlag)
			Animating (moveFBForAnimeRPG, moveLRForAnimeRPG);	//	Animation management
	}

	void OnGUI() {

		//	RTS selection function, mouse click then start draw selection area
		if (mouseAreaSelec) {
			// Create a rect from both mouse positions
			var rect = Utils_RTS_Draw.GetScreenRect( curMousPos, Input.mousePosition );
			Utils_RTS_Draw.DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
			Utils_RTS_Draw.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
		}
	}

	/********************************
	 * --- Functions
	 ********************************/

	//	move or turn by keyboard according camera behavior, typical RPG control system
	//	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
	private void MoTbKaCB (float FB, float LR, float movSP, float turnSP, float RD) {
		float tarYAng = Cam_Center_Point.transform.eulerAngles.y;
		float curYAng = transform.eulerAngles.y;

		//debug
		/*
		if (Cam_Center_Point.GetComponent <CameraFunctions> ().turnCompleteFollowFlag) 
			Cam_Center_Point.GetComponent <CameraFunctions> ().turnCompleteFollowFlag = false;
		if (!Cam_Center_Point.GetComponent <CameraFunctions> ().turnRPGFollowFlag)
			Cam_Center_Point.GetComponent <CameraFunctions> ().turnRPGFollowFlag = true;
		*/
		if (LR > 0f) {	//	if character is shiftting right
			newYAng = tarYAng + 90f;
			if (FB > 0f)
				newYAng -= 45f;
			else if (FB < 0f)
				newYAng += 45f;
		} else if (LR < 0f) {	//	if character is shiftting left
			newYAng = tarYAng - 90f;
			if (FB > 0f)
				newYAng += 45;
			else if (FB < 0f)
				newYAng -= 45f;
		} else if (FB > 0f) {	//	if character is moving forward
			newYAng = tarYAng;
		} else if (FB < 0f) {	//	if character is moving backward

			//	prenvent character and camera stuck at certain point and making camera shake badly
			float curBackAng = curYAng + 180f;	//	safety value for judgement after character move pass 0 point
			float tarBackAng = tarYAng + 180f;	//	safety value for judgement after camera center move pass 0 point
			if (curBackAng > 360f)	//	make sure value is in 360
				curBackAng -= 360f;
			if (tarBackAng > 360f)
				tarBackAng -= 360f;
			if (tarYAng <= curYAng) {	
				if (tarBackAng > curBackAng & tarYAng < curBackAng)	
					newYAng = tarYAng - 179f;	//	if character direction is at right of the camera center direction
				else
					newYAng = tarYAng + 179f;
			} else if (tarYAng > curYAng) {
				if (tarBackAng <= curBackAng & tarYAng > curBackAng)
					newYAng = tarYAng + 179f;	//	if character direction is at left of the camera center direction
				else
					newYAng = tarYAng - 179f;
			} 
		}

		Quaternion tempQuat = Quaternion.Euler (new Vector3 (0f, newYAng, 0f));
		Quaternion newAng = Quaternion.RotateTowards (transform.rotation, tempQuat, turnSP * Time.deltaTime);

		if (transform.eulerAngles.y != newAng.eulerAngles.y)
			playerRigidbody.MoveRotation (newAng);

		if (FB != 0 | LR != 0) {
			//move forward on charactor faceing basis on the direction of Camera
			moveCalculation = (Cam_Center_Point.transform.forward * FB) + (Cam_Center_Point.transform.right * LR);
			playerRigidbody.MovePosition (transform.position + moveCalculation.normalized * movSP * Time.deltaTime);
		}
	}

	//	RTS style Point Selection
	private void RTS_Point_Selec (bool mousLB) {
		
		//	search obj which contain Selectable_Unit_Controller.cs
		if (mousLB & !mouseAreaSelec) {
			
			//	cast a ray from camera and go through mouse position
			Ray camMousRay = playerCam.ScreenPointToRay (Input.mousePosition);
			RaycastHit selectionHit;

			//	cast a ray from camera and go through mouse position
			foreach (var selectableObj in FindObjectsOfType <Selectable_Unit_Controller_AstarPathfing_Project>()) {

				/******************
				 * may need add code for selected obj in to a global list for AI control at here
				 ******************/

				if (Physics.Raycast (camMousRay, out selectionHit, 50f)) {
					if (selectableObj.GetComponent <Collider> ().bounds.Contains (selectionHit.point)) {
						if (selectableObj.selectionCircle == null) {
							selectableObj.selectionCircle = Instantiate (Select_Circle_Prefab);
							selectableObj.selectionCircle.transform.SetParent (selectableObj.transform, false);
							selectableObj.selectionCircle.transform.eulerAngles = new Vector3 (90, 0, 0);
						}
					} else {
						if (selectableObj.selectionCircle != null) {
							Destroy (selectableObj.selectionCircle.gameObject);
							selectableObj.selectionCircle = null;
						}
					}
				}
			}
		}
	}

	//	RTS style Area Selection
	private void RTS_Area_Selec (bool mousLB) {
		
		//	if press left mouse button start draw square
		if (mousLB & !mouseAreaSelec) {
			mouseAreaSelec = true;
			curMousPos = Input.mousePosition;
		}

		//	if release left mouse button stop draw square
		//	original detect mousLefButtUp, but some times can't get release signal
		if (!mousLB) {
			mouseAreaSelec = false;
		}

		//	use projector give a circle under selected unity
		if (mouseAreaSelec) {
			//	search obj which contain component Selectable_Unit_Controller.cs
			foreach (var selectableObj in FindObjectsOfType <Selectable_Unit_Controller_AstarPathfing_Project>()) {
				//	call judgement function and see if obj is in selection area
				if (IsWithinSelectionBounds (selectableObj.gameObject)) {
					if (selectableObj.selectionCircle == null) {
						selectableObj.selectionCircle = Instantiate (Select_Circle_Prefab);
						selectableObj.selectionCircle.transform.SetParent( selectableObj.transform, false );
						selectableObj.selectionCircle.transform.eulerAngles = new Vector3( 90, 0, 0 );
					}
				} 
			}
		}
	}


	//	RTS selection function, judgement for selectable obj in or not in selction area from camera view angle 
	public bool IsWithinSelectionBounds (GameObject gameObject) {
		
		if (!mouseAreaSelec)
			return false;
		
		var cam = playerCam;
		var viewportBounds =
			Utils_RTS_Draw.GetViewportBounds (cam, curMousPos, Input.mousePosition);
			
		return viewportBounds.Contains (
			cam.WorldToViewportPoint (gameObject.transform.position));	//	use bounds() search if obj is in selection area
	}

	//	get current mouse position on screen
	private void MousPos (out float mousXPos, out float mousYPos) {
		//	current mouse position on screen in pixels
		//	0 point is at left, down of game window
		mousXPos = Input.mousePosition.x;	
		mousYPos = Input.mousePosition.y;
	}

	//	Edge Boundary Movement Control
	//	out xMovOff: x Movement Offset, out yMovOff: y Movement Offset
	private void Edge_Move_Control (out float xMovOff, out float zMovOff) {
		float mousXPos, mousYPos;
		float newXOff = 0f, newZOff = 0f;
		float curScreWid = Screen.width;	//	read current game window width in pixels
		float curScreHei = Screen.height;	//	read current game window height in pixels

		MousPos (out mousXPos, out mousYPos);

		//	mouse movement on Y axis, change obj Z direction in world axis
		if (mousXPos > curScreWid - Edge_Boundary) {
			newZOff = 1f;
		} else if (mousXPos < 0 + Edge_Boundary) {
			newZOff = -1f;
		}

		//	mouse movement on Y axis, change obj X direction in world axis
		if (mousYPos > curScreHei - Edge_Boundary) {
			newXOff = 1f;
		} else if (mousYPos < 0 + Edge_Boundary) {
			newXOff = -1f;
		}

		xMovOff = newXOff;
		zMovOff = newZOff;
	}

	//	Move Player towards Character Facing
	//	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
	void MPtCF(float FB, float LR, float SP, float RD){
		
		//move basis on the character face on (character local axis x, y, z)
		moveCalculation = (transform.forward * FB) + (transform.right * LR);
		if (FB > 0f && LR == 0f) {
			playerRigidbody.MovePosition (transform.position + moveCalculation.normalized * SP * Time.deltaTime);
		} else {
			playerRigidbody.MovePosition (transform.position + moveCalculation.normalized * SP/RD * Time.deltaTime);
		}
	}

	//	Move Player along world axis x, y, z
	//	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
	void MPaW(float FB, float LR, float SP, float RD){

		moveCalculation.Set (LR, 0f, FB);		//package keyboard value into vector3 type for later calcuation

		//judgment if character is walk or run backward or sideward, speed will dive by divier
		if (FB > 0f && LR == 0f) {
			moveCalculation = moveCalculation.normalized * SP * Time.deltaTime;
		} else {
			moveCalculation = moveCalculation.normalized * SP/RD * Time.deltaTime;
		}
		playerRigidbody.MovePosition (transform.position + moveCalculation);
	}

	//	Move Disconnect Camera center towards Camera Facing
	//	FB: forward/backward, LR: Left/Right, SP: Speed
	void MDCtCF(float FB, float LR, float SP){

		Cam_Center_Point.transform.Translate(Vector3.forward * FB * SP * Time.deltaTime);
		// multiply -1 is because input need be turn over
		Cam_Center_Point.transform.Translate(Vector3.left * LR * -1 * SP * Time.deltaTime);
	}

	//	Move Disconnect Camera center along world axis x, y, z
	//	FB: forward/backward, LR: Left/Right, SP: Speed
	void MDCaW(float FB, float LR, float SP){

		Cam_Center_Point.transform.Translate(Vector3.forward * FB * SP * Time.deltaTime, Space.World);
		// multiply -1 is because input need be turn over
		Cam_Center_Point.transform.Translate(Vector3.left * LR * -1 * SP * Time.deltaTime, Space.World);
	}

	//	Turning Player by Keyboard Control
	//	LR: Left/Right, TS: Turn Speed
	void TPbKC(float LR, float TS){
		Vector3 playerToKeyboard= new Vector3(0f, LR*TS*Time.deltaTime, 0);	//Same if write: float playerToKeyboard = lr*TS*Time.deltaTime; 
		Quaternion tbkcRotation = Quaternion.Euler (playerToKeyboard);	//if use float then the code will be: Quaternion.Euler (0f, playerToKeyboard, 0f);

		playerRigidbody.MoveRotation (playerRigidbody.rotation*tbkcRotation);
	}

	//	Turning Player by Mouse Pointing
	void TPbMP(){
		Quaternion roteTo;
		Vector3 mousHitPos;

		if (Public_Functions.Mous_Click_Get_Pos_Dir (playerCam, transform, LayerMask.GetMask ("floor"), out mousHitPos, out roteTo)) {
			playerRigidbody.MoveRotation (roteTo);
		}
		
	}

	//	Animation management
	private void Animating (float FB, float LR){
		bool walking = FB != 0f || LR != 0f;

		anim.SetBool ("IsWalking", walking);
	}

	//	force camera center stop follow player
	private void RELEASE () {
		Cam_Center_Point.GetComponent <CameraFunctions> ().followPlayerFlag = false;
	}

	private void DEBUG () {
		
		//debug try disconnect camera follow
		if (Input.GetKey (KeyCode.T)) {
			RELEASE ();
		}
	}
}

//		Debug.Log ("GetButtonDown:  " + Input.GetButtonDown ("Run"));