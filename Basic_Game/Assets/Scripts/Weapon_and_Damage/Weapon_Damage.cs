using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;
using RootMotion.Demos;

//	0.2.0
public class Weapon_Damage : MonoBehaviour {

	public int Damage;


    private Prop prop;
    private int layerMaskFloor;
    private int layerMaskObstacles;
    private int layerMaskRagdollObstacles;

    void Start()
    {
        layerMaskFloor = LayerMask.GetMask("Floor");
        layerMaskObstacles = LayerMask.GetMask("Obstacles");
        layerMaskRagdollObstacles = LayerMask.GetMask("Obstacles_Only_Affect_On_Ragdoll");
        
        prop = GetComponentInChildren<PropMelee>();
    }

    void OnCollisionEnter(Collision info)
    {
        int hitCounts = 0;
        foreach (ContactPoint contPoint in info.contacts)
        {
            hitCounts++;
            
            if (prop.isPickedUp != true) //	if weapon is not been equip
                break;  //	quit loop

            if (contPoint.otherCollider.transform.root == transform.root)
            {   //	if hit collider belong to this scipt obj
                continue;
            }

            if (contPoint.otherCollider.transform.root == prop.propRoot.transform.root)
            {   //	if hit collider belong to obj equiped this scipt obj
                continue;
            }

            if (contPoint.otherCollider.GetComponent("HealthControl") == null)  //  if hit obj is undestrutable
            {
                continue;
            }

            contPoint.otherCollider.GetComponent<HealthControl>().Take_Damage(Damage);  //	give damage to hit object
        }
    }
}
