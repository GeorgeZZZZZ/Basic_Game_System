using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

//  0.1.0
namespace RootMotion.Demos
{

    public class PropPickUpTrigger_001 : MonoBehaviour
    {

        public Prop prop;
        public LayerMask characterLayers;

        private Player_Attack_Behavior PAB;

        void OnTriggerEnter(Collider collider)
        {
            if (prop.isPickedUp) return;

            if (!LayerMaskExtensions.Contains(characterLayers, collider.gameObject.layer)) return;
            
            PAB = collider.GetComponent<Player_Attack_Behavior>();
            
            if (PAB == null) return;
            
            //if (PAB.puppet.state != BehaviourPuppet.State.Puppet) return; // don't know what is this for

            if (PAB.PropRootLeft == null || PAB.PropRootRight == null) return;  //  if PropRoot did not assign prop script
            
            if (PAB.PropRootLeft.currentProp != null && PAB.PropRootRight.currentProp != null) return;  //  if both hand had been occupied

            if (PAB.PropRootRight.currentProp == null)  //  if right hand is free
            {
                PAB.PropRootRight.currentProp = prop;
            }
            else if (PAB.PropRootLeft.currentProp == null)  //  if left hand is free
            {
                PAB.PropRootLeft.currentProp = prop;
            }
        }
    }
}
