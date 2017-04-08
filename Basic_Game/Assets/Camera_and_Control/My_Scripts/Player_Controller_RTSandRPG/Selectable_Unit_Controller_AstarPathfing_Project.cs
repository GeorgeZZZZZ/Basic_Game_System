using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

//0.4.4
[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Seeker))]
[RequireComponent (typeof(SimpleSmoothModifier))]
[RequireComponent (typeof(RaycastModifier))]
public class Selectable_Unit_Controller_AstarPathfing_Project : AIPath_For_Rigidbody_From_AstarPathfindingProject {
	
	public GameObject selectionCircle;	//	use for add circle above obj after been selected	
	public GameObject endOfPathEffect;

	public bool Use_Animation = true;

	private Animator anime;
	private Vector3 lastTarget;
	private Vector3 posOffset;

	private bool mousRBTiggerOnceFlag;
	private bool mousRBTiggerOnce;

	private float timer = 0f;
	private bool isRunning;
	new void Start (){

		if (GetComponent ("Animator"))
			anime = GetComponent <Animator> ();

		//	rewrite move speed with master control script if their is palyer controll script attached
		if (GetComponent ("Player_Controller_RTS_RPG_AstarPathfing_Project") != null) {
			speed = GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().Player_Normal_Speed;
			Run_Speed = GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().Player_Run_Speed;
			turningSpeed = GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().Player_Turnning_Speed;
		}

		//	modify and smooth path calculation
		GetComponent <SimpleSmoothModifier> ().maxSegmentLength = 1;
		GetComponent <SimpleSmoothModifier> ().iterations = 5;
		GetComponent <SimpleSmoothModifier> ().strength = 0.25f;

		base.Start ();
	}

	new void Update () {
		bool camFollowing = false;

		//	if their is palyer controll script attached then read camera follow state
		if (GetComponent ("Player_Controller_RTS_RPG_AstarPathfing_Project") != null) {
			camFollowing = GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().Cam_Center_Point.GetComponent <Camera_Controller> ().followPlayerFlag;
		}

		//	only start pathfind when player get in to RTS mode
		if (!camFollowing) {
			if (canMove == false) 
				canMove = true;
			//	if obj has been selected
			if (selectionCircle != null) {
				Detect_New_pos ();
			}

			Move ();
		} else {
			if (selectionCircle != null) {
				Destroy (selectionCircle.gameObject);
				selectionCircle = null;
			}
			if (canMove != false)
				canMove = false;
			
			newTar = transform.position;
		}

		if (!camFollowing && Use_Animation)
			Animating ();

		if (timer > 0) {
			timer -= Time.deltaTime;
		}
	}

	/********************************
	 * --- Functions
	 ********************************/
	private void Move (){
		
		posOffset = Calculate_New_Pos_Dir (tr.position, isRunning);
		rigid.MovePosition (transform.position + posOffset);

		if (targetDirection != Vector3.zero) {
			Quaternion newDir = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (targetDirection), turningSpeed * Time.deltaTime);
			rigid.MoveRotation (newDir);
		}
	}

	private void Detect_New_pos () {
		
		bool mousRightButton = Input.GetMouseButton (1);

		//	set a bool vaule and judge left mouse button
		//	only triger one cycle for each time left mouse button has been pusshed down
		//	do this is because sometimes Input.GetMouseButtonUp (0) miss value from mouse button
		if (mousRightButton & !mousRBTiggerOnceFlag) {
			mousRBTiggerOnce = true;
			mousRBTiggerOnceFlag = true;
		} else if (mousRightButton & mousRBTiggerOnceFlag)
			mousRBTiggerOnce = false;
		else if (!mousRightButton)
			mousRBTiggerOnceFlag = false;
		
		//	if right mouse button has been click then give mouse clcik postion to calculate path
		if (mousRBTiggerOnce) {
			//	double click judgement
			if (timer <= 0f) {
				isRunning = false;
				timer = 0.3f;
			} else if (timer > 0) {
				isRunning = true;
			}

			Vector3 mousHitPos;
			Quaternion roteTo;
			if (Public_Functions.Mous_Click_Get_Pos_Dir (Camera.main, transform, LayerMask.GetMask ("Floor"), out mousHitPos, out roteTo)) {
				newTar = mousHitPos;
			}
		}
	}

	public override void OnTargetReached () {
		if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1) {
			GameObject.Instantiate(endOfPathEffect, tr.position, tr.rotation);
			lastTarget = tr.position;
		}
	}

	//	Animation management
	private void Animating (){
		bool walk = false;

		if (posOffset != Vector3.zero)
			walk = true;
		else {
			walk = false;
			isRunning = false;
		}
		
		anime.SetBool ("IsWalking", walk);
		anime.SetBool ("IsRunning", isRunning);
	}
}
