using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

//  0.1.0

[RequireComponent(typeof(Player_Attack_Behavior))]
public class Weapon_Control : MonoBehaviour {

    private PropRoot propLeft;
    private PropRoot propRight;

    // Use this for initialization
    void Start () {
        propLeft = GetComponent<Player_Attack_Behavior>().PropRootLeft;
        propRight = GetComponent<Player_Attack_Behavior>().PropRootRight;
    }
	
	// Update is called once per frame
	void Update () {
        // Dropping
        if (Input.GetKeyDown(KeyCode.X))
        {
            // By setting the prop root's currentProp to null, the prop connected to it will be dropped.
            if (propRight != null)  //  if right hand has occupied then drop right hand first
            {
                propRight.currentProp = null;
            } else if (propLeft != null)    //  if right hand is empty but left hand has occupied then dorp left hand obj
            {
                propLeft.currentProp = null;
            }
        }
        /*  not yet ready to use
        // Switching prop roots.
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Dropping/Picking up normally works in the fixed update cycle where joints can be properly connected. Swapping within a single frame can be done by calling PropRoot.DropImmediate();
            connectTo.DropImmediate();

            // Switch hands
            right = !right;

            // Assign the prop to the other hand
            connectTo.currentProp = prop;
        }
        */
    }
}
