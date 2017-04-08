using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//	0.1.0
public class HealthControl : MonoBehaviour {
	public int Starting_Health = 100;
	public int Current_Health;
	public Slider Health_Slider;
	public Image Damage_Image;
	public float Flash_Speed = 5f;
	[HideInInspector] public bool Die = false;	//	for other script to identify current state

	private Animator anim;
	private Color Flash_Colour = new Color (1f, 0f, 0f, 0.1f);
	private bool Damage;
	private float timer;

	// Use this for initialization
	void Start () {
		anim = GetComponent <Animator> ();
		Current_Health = Starting_Health;

	}
	
	// Update is called once per frame
	void Update () {
		if (Damage_Image != null) {
			if (Damage) {
				Damage_Image.color = Flash_Colour;
			} else {
				Damage_Image.color = Color.Lerp (Damage_Image.color, Color.clear, Flash_Speed * Time.deltaTime);
			}
		}

		Damage = false;

		if (Die)
			timer -= Time.deltaTime;
		if (Die && timer < 0)
			Destroy (gameObject);
	}

	/********************************
	 * --- Functions
	 ********************************/

	public void Take_Damage (int amount) {
		if (Die)
			return;
		
		Damage = true;

		Current_Health -= amount;

		if (Health_Slider != null)
			Health_Slider.value = Current_Health;

		anim.SetTrigger ("IsDammage");

		if (Current_Health <= 0 && !Die) {
			Death ();
		}
	}

	private void Death () {
		Die = true;
		timer = 5f;
		transform.GetChild (0).gameObject.SetActive (false);

		if (GetComponent ("Player_Controller_RTS_RPG_AstarPathfing_Project") != null) {
			if (GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().enabled != false)
				GetComponent <Player_Controller_RTS_RPG_AstarPathfing_Project> ().enabled = false;
		} else if (GetComponent ("Enemy_Controller_AstarPathfinding_Project") != null) {
			if (GetComponent <Enemy_Controller_AstarPathfinding_Project> ().enabled != false)
				GetComponent <Enemy_Controller_AstarPathfinding_Project> ().enabled = false;
		}

		if (GetComponent ("Selectable_Unit_Controller_AstarPathfing_Project") != null) {
			if (GetComponent <Selectable_Unit_Controller_AstarPathfing_Project> ().enabled != false)
				GetComponent <Selectable_Unit_Controller_AstarPathfing_Project> ().enabled = false;
		}

		if (GetComponent ("AI_Attack_Behavior") != null) {
			if (GetComponent <AI_Attack_Behavior> ().enabled != false)
				GetComponent <AI_Attack_Behavior> ().enabled = false;
		}

		anim.SetTrigger ("Die");
	}
}
