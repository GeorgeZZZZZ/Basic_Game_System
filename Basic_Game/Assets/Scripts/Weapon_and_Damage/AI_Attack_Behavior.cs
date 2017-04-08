using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	0.1.1
public class AI_Attack_Behavior : MonoBehaviour {
	public float Attack_Frequency = 2f;
	public float Attack_Range;

	private GameObject weaponSlot;
	private Animator anime;
	private bool enemyInRange;
	private float timer = 0f;

	private bool hitSomeThing;
	private bool attacking;

    private int floorMask;
    private int obstacleMask;
    private int obstacleRagdollMask;

    // Use this for initialization
    void Start () {
		anime = GetComponent <Animator> ();
		Attack_Range = GetComponentInChildren <Weapon_Range> ().Range; 	//	looking for weapon in children and add attack range

		timer = Attack_Frequency;
        floorMask = LayerMask.NameToLayer("Floor"); //  get number of Floor layer
        obstacleMask = LayerMask.NameToLayer("Obstacles");
        obstacleRagdollMask = LayerMask.NameToLayer("Obstacles_Only_Affect_On_Ragdoll");
    }
	
	// Update is called once per frame
	void Update () {
		Detect_Enemy ();

		Animating ();

		hitSomeThing = false;
	}

	/********************************
	 * --- Functions
	 ********************************/

	private void Detect_Enemy (){
		Collider[] inRangeCols = Physics.OverlapSphere(transform.position, Attack_Range);
		bool playerFoundInList = false;
		foreach (Collider col in inRangeCols) {
			if (col.transform.root == transform.root ||	//	if hit collider belong to this scipt obj
				col.gameObject.layer == floorMask ||			//	if hit floor
				col.gameObject.layer == obstacleMask ||           //	if hit Obstacles
                col.gameObject.layer == obstacleRagdollMask)			//	if hit Obstacles
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

			if (GetComponent <AI_Attack_Behavior> ().enabled != true)	//	if script is not enabled
				break;	//	quit loop

			if (!this.anime.GetCurrentAnimatorStateInfo (0).IsName ("Attack")) {	//	if animator currentily not playing "Attack" animation
				continue;
			}

			if (contPoint.otherCollider.transform.root == transform.root) {	//	if hit collider belong to this scipt obj
				continue;
			}

			hitSomeThing = true;	//	hit something, quit current animation

			if (contPoint.otherCollider.gameObject.layer == floorMask ||    //	if hit floor
                contPoint.otherCollider.gameObject.layer == obstacleMask ||   //	if hit Obstacles
                contPoint.otherCollider.gameObject.layer == obstacleRagdollMask)   //	if hit Obstacles
                continue;

			if (contPoint.thisCollider.tag == "Weapon"								//	if is weapon collide on somthing
				&& contPoint.otherCollider.GetComponent ("HealthControl") != null	//	if collide obj has health control
				&& attacking == true) {												//	if not attack yet
				attacking = false;													//	attacked, igron next collision until next attack movement
				contPoint.otherCollider.GetComponent <HealthControl> ()
					.Take_Damage (contPoint.thisCollider.GetComponent <Weapon_Damage> ().Damage);	//	give damage to hit object
			}
		}
	}

	void OnCollisionExit (Collision info) {

		//Debug.Log ("out:  " + info.gameObject.name);
	}

	private void Animating () {
		if (timer > 0 && enemyInRange)
			timer -= Time.deltaTime;
		else
			timer = Attack_Frequency;
		
		if (enemyInRange && timer <= 0f) {
			timer = Attack_Frequency;
			attacking = true;
			anime.SetTrigger ("IsAttack");
		}
			
		if (hitSomeThing)
			anime.SetTrigger ("BackToWait");
	}
}
