using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	0.1.0
public class Player_Attack_Behavior : MonoBehaviour {
	public float Attack_Frequency = 1f;
	public float Attack_Range;

	private Animator anime;
	private CameraFunctions camFun;
	private bool enemyInRange;
	private float timer = 0f;

	private bool hitSomeThing;

	// Use this for initialization
	void Start () {
		anime = GetComponent <Animator> ();
		Attack_Range = GetComponentInChildren <Weapon_Range> ().Range; 	//	looking for weapon in children and add attack range

		camFun = GetComponent <Player_Camera_Controller_RTS_RPG_AstarPathfing_Project> ()
			.Cam_Center_Point.GetComponent <CameraFunctions> ();	//	get camera component
	}

	// Update is called once per frame
	void Update () {
		
		if (camFun.followPlayerFlag) {	//	get camera follow flag to identify if is in RPG or RTS mode
			Attack_Manager ();			//	true means is in RPG mode then use manager attack
		}

		//Detect_Enemy ();

		Animating ();

		hitSomeThing = false;
	}

	/********************************
	 * --- Functions
	 ********************************/
	private void Attack_Manager () {
		bool leftMousClick = Input.GetMouseButtonDown (0);
		bool rightMousClick = Input.GetMouseButtonDown (1);

		if (leftMousClick) {
			enemyInRange = true;
		} else {
			enemyInRange = false;
		}
	}

	private void Detect_Enemy (){
		Collider[] inRangeCols = Physics.OverlapSphere(transform.position, Attack_Range);
		bool playerFoundInList = false;
		foreach (Collider col in inRangeCols) {
			if (col.transform.root == transform.root ||	//	if hit collider belong to this scipt obj
				col.gameObject.layer == 8 ||			//	if hit floor
				col.gameObject.layer == 10 )			//	if hit Obstacles
				continue;								//	then skip rest code for this time in this loop, unlike "break" it's not quitting the loop

			if (col.tag == "Player") {
				if (!col.GetComponent <HealthControl> ().Die) {
					enemyInRange = true;	//	player is in range then give true for animation
					playerFoundInList = true;	// if true when player is in collision list
				} else {
					enemyInRange = playerFoundInList = false;	// player is died, stop attacking
				}
			}
		}
		if (enemyInRange == true && playerFoundInList == false)	//	if player leave alert range
			enemyInRange = false;	//	stop attack animation
	} 

	//	MonoBehaviours, when any collider attached to this obj's rigidbody (including children) get collision will transfer information to this function
	void OnCollisionEnter (Collision info) {
		int hitCounts = 0;
		foreach (ContactPoint contPoint in info.contacts) {
			hitCounts++;
			if (GetComponent <Player_Attack_Behavior> ().enabled != true)	//	if script is not enabled
				break;	//	quit loop

			if (!this.anime.GetCurrentAnimatorStateInfo (0).IsName ("Attack")) {	//	if animator currentily not playing "Attack" animation
				continue;
			}

			if (contPoint.otherCollider.transform.root == transform.root) {	//	if hit collider belong to this scipt obj
				continue;
			}

			hitSomeThing = true;	//	hit something, quit current animation

			if (contPoint.otherCollider.gameObject.layer == 8 ||	//	if hit floor
				contPoint.otherCollider.gameObject.layer == 10) {	//	if hit Obstacles
				continue;
			}

			if (contPoint.thisCollider.tag == "Weapon" && contPoint.otherCollider.GetComponent ("HealthControl") != null) {
				contPoint.otherCollider.GetComponent <HealthControl> ()
					.Take_Damage (contPoint.thisCollider.GetComponent <Weapon_Damage> ().Damage);	//	give damage to hit object
			}
		}
	}

	void OnCollisionExit (Collision info) {

		//Debug.Log ("out:  " + info.gameObject.name);
	}

	private void Animating () {
		if (timer > 0)
			timer -= Time.deltaTime;

		if (enemyInRange && timer <= 0f) {
			timer = Attack_Frequency;
			anime.SetTrigger ("IsAttack");
		}

		if (hitSomeThing)
			anime.SetTrigger ("BackToWait");
	}
}
