using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

//	0.2.0
public class Player_Attack_Behavior : MonoBehaviour {
	public float Attack_Frequency = 1f;
	public float Attack_Range;

    public PropRoot PropRootRight;   //  use to grib obj in right hand
    public PropRoot PropRootLeft;   //  use to grib obj in left hand

    private Animator anime;
	private Camera_Controller camFun;
	private bool enemyInRange;
	private float timer = 0f;

	private bool hitSomeThing;

    private int layerMaskFloor;
    private int layerMaskObstacles;
    private int layerMaskRagdollObstacles;

    // Use this for initialization
    void Start () {
		anime = GetComponent <Animator> ();
		//Attack_Range = GetComponentInChildren <Weapon_Range> ().Range; 	//	looking for weapon in children and add attack range, work as AI attack range

		camFun = GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ()
			.Cam_Center_Point.GetComponent <Camera_Controller> ();  //	get camera component
        
        layerMaskFloor = LayerMask.GetMask("Floor");
        layerMaskObstacles = LayerMask.GetMask("Obstacles");
        layerMaskRagdollObstacles = LayerMask.GetMask("Obstacles_Only_Affect_On_Ragdoll");
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
		Collider[] inRangeCols = Physics.OverlapSphere(transform.position, Attack_Range);   //  search attack range
		bool playerFoundInList = false;
		foreach (Collider col in inRangeCols) {
			if (col.transform.root == transform.root ||	//	if hit collider belong to this scipt obj
				col.gameObject.layer == layerMaskFloor ||			//	if hit floor
				col.gameObject.layer == layerMaskObstacles)			//	if hit Obstacles
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

    /* comment at vr 0.2.1
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

			if (contPoint.otherCollider.gameObject.layer == layerMaskFloor ||	//	if hit floor
				contPoint.otherCollider.gameObject.layer == layerMaskObstacles) {	//	if hit Obstacles
				continue;
			}

            if (contPoint.thisCollider.tag == "Weapon" && contPoint.otherCollider.GetComponent ("HealthControl") != null) {
				contPoint.otherCollider.GetComponent <HealthControl> ()
					.Take_Damage (contPoint.thisCollider.GetComponent <Weapon_Damage> ().Damage);	//	give damage to hit object
			}
		}
    }
    */

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
