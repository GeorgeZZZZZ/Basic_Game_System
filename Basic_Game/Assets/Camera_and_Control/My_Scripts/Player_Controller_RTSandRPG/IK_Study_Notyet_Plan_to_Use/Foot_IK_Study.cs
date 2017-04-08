using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot_IK_Study : MonoBehaviour {

    public float IKWeight = 1f;

    public Transform leftIKTarget;
    public Transform rightIKTarget;

    private Animator anime;

    // Use this for initialization
    void Start () {
        anime = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("11111111111");

        anime.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IKWeight);
        anime.SetIKPositionWeight(AvatarIKGoal.RightFoot, IKWeight);

        anime.SetIKPosition(AvatarIKGoal.LeftFoot, leftIKTarget.position);
        anime.SetIKPosition(AvatarIKGoal.RightFoot, rightIKTarget.position);
    }
}
