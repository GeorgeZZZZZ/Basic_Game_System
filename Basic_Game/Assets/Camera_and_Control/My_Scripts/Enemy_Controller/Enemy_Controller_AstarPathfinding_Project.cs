using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

//	0.1.0
[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Seeker))]
[RequireComponent (typeof(SimpleSmoothModifier))]
[RequireComponent (typeof(RaycastModifier))]
public class Enemy_Controller_AstarPathfinding_Project : AIPath_For_Rigidbody_From_AstarPathfindingProject {

	public GameObject endOfPathEffect;
	public float Min_Stop_Range = 0.5f;
	public bool Use_Animation = true;

	private Animator anime;
	private Vector3 lastTarget;
	private Vector3 posOffset;
	private GameObject enemy;

	new void Start (){

		if (GetComponent ("Animator"))
			anime = GetComponent <Animator> ();

		//	modify and smooth path calculation
		GetComponent <SimpleSmoothModifier> ().maxSegmentLength = 1;
		GetComponent <SimpleSmoothModifier> ().iterations = 5;
		GetComponent <SimpleSmoothModifier> ().strength = 0.25f;

		enemy = GameObject.FindGameObjectWithTag ("Player");

		oldTar = new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);	//	just give a number to initialize

		base.Start ();
	}

	new void Update () {


		Detect_New_pos ();

		Move ();

		if (Use_Animation)
			Animating ();
	}

	/********************************
	 * --- Functions
	 ********************************/
	private void Move (){

		//	stop move after reach miniment range
		if (Vector3.Distance (transform.position, enemy.transform.position) > Min_Stop_Range)
			posOffset = Calculate_New_Pos_Dir (tr.position, false);
		else
			posOffset = Vector3.zero;

		rigid.MovePosition (transform.position + posOffset);	//	always facing on ememy

		if (targetDirection != Vector3.zero) {
			Quaternion newDir = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (targetDirection), turningSpeed * Time.deltaTime);
			rigid.MoveRotation (newDir);
		}
	}

	private void Detect_New_pos () {
		
		if (enemy != null) {
				newTar = enemy.transform.position;
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
		else
			walk = false;

		anime.SetBool ("IsWalking", walk);
	}
}
