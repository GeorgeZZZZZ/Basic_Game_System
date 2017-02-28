using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	0.3.0
public class Public_Functions : MonoBehaviour {

	// Smooth Angle transform function
	public static void SMO_ANG (float curAng, float tarAng, float smoTim, out float result) {
		float smoothV = 0f;	//calculation temp cache for Mathf.SmoothDamp

		result = Mathf.SmoothDampAngle (curAng, tarAng, ref smoothV, smoTim);
	}

	// Smooth Value transform function
	public static void SMO_VAL (float curVal, float tarVal, float smoTim, out float result) {
		float smoothV = 0f;	//calculation temp cache for Mathf.SmoothDamp

		result = Mathf.SmoothDamp (curVal, tarVal, ref smoothV, smoTim);
	}

	//	Smooth Value and give Offset
	//	cache: cache (current Value), target: target value, smoothDamp: smoothDamp
	//	out cacheOut: cache value output, out offset: offset result after calculation
	public static void SMO_VAL_OFFSET (float cache, float target, float smoothDamp, out float cacheOut, out float offset) {
		float oldDisOff = cache;	//	save old movement calculation cache value

		SMO_VAL (cache, target, smoothDamp, out cacheOut);	//	smooth movemeant
		offset = (cacheOut - oldDisOff);	//	calculate offset value
	}

	public static float Clear_Angle (float angleIn) {
		//	take away unnecessary number, limit angle in 360
		float newAng = angleIn - (360f * Mathf.Floor (angleIn / 360f));	

		if (newAng < -180f) {
			return (360f + newAng);
		} else if (newAng > 180f) {
			return (-360f + newAng);
		} else {
			return newAng;
		}
	}

	public static bool Mous_Click_Get_Pos_Dir (Camera cam,Transform curTrans,int maskIn,out Vector3 hitPos, out Quaternion tarRote) {
		
		Ray camRay = cam.ScreenPointToRay (Input.mousePosition);
		RaycastHit floorHit;

		if (Physics.Raycast (camRay, out floorHit, 100f, maskIn)) {
			hitPos = floorHit.point;
			Vector3 playerToMouse = floorHit.point - curTrans.position;
			playerToMouse.y = 0f;
			tarRote = Quaternion.LookRotation (playerToMouse);

			return true;
		} else {
			hitPos = Vector3.zero;
			tarRote = Quaternion.Euler (Vector3.zero);	//	has to return 0
			return false;
		}
	}
}
