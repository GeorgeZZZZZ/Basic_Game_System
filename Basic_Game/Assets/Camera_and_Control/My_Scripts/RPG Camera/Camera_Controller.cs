using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	vr: 0.6.12

public enum Mouse_Control_Cam_Types_In_RPG_Mode
{
    RPG_Dir_Rote_Cam,
    RPG_Mid_Mous_Rote_Cam
}

public enum Camera_Follow_Player_Behavior
{
    RPG_Classic_Cam_Follow,
    RPG_Complet_Cam_Follow
}

public enum Camera_Movement_Types_In_RTS_Mode
{
    Move_Camera_towards_cam_Facing,
    Move_Camera_Along_World_Axis
}

public enum Mouse_Control_Cam_Types_In_RTS_Mode
{
    RTS_Mid_Mous_Rote_Cam
}

public class Camera_Controller : MonoBehaviour
{
    //  drop-manu
    public Mouse_Control_Cam_Types_In_RPG_Mode MouseControlCamTypesInRPGMode; //  declar serializable enum for customer inspector script to look for
    public Camera_Follow_Player_Behavior CameraFollowPlayerBehavior;
    public Camera_Movement_Types_In_RTS_Mode CameraMovementTypes;
    public Mouse_Control_Cam_Types_In_RTS_Mode MouseControlCamTypeseInRTSMode;

    //
    //[Header("Obj for camera to follow")]
    public GameObject Player_Obj;
    //[Header("Camera Parts")]
    public GameObject X_Rote_Cent;
    public GameObject Cam_Obj;

    [Header("General Settings")]
    public float Height_Offset = 0f;
    [HideInInspector] public float Max_Field_View = 80f;    //	not inuse
    [HideInInspector] public float Min_Field_View = 18f;    //	not inuse
    [HideInInspector] public float Cam_Field_View_Sensitivity = 2f;	//	not inuse
    public float Look_Sensitivity = 50f;
    public float Mouse_Scroll_Sensitivity = 80f;
    public float Mouse_Scroll_SmoothDamp = 0.08f;
    public float Look_SmoothDamp = 0.05f;   //	This value lager makes smooth slower
    public float Max_X_Rotation_Angle = 80f;
    public float Min_X_Rotation_Angle = 0f;

    [Header("General Settings Affect in RPG Mode")]
    public float Player_Follow_SmoothDamp = 0.2f;
    public float Max_Cam_Distance = 6f;     //	Max camera distance in RPG view
    public float Min_Cam_Distance = 0.8f;   //	Min camera distance in RPG view
    public float RPG_Min_X_Rotation_Angle = -80f;
    public float Distance_Change_Sensitivity = 2f;
    public float Distance_Change_SmoothDamp = 0.05f;
    public float Angle_Change_Sensitivity = 2f;
    public int Edge_Boundary = 1;   //	valuable use for detect limit movement which mouse move near screen edge, unit in pixel 

    public bool RPG_Mid_Mous_Rote_Cam;  //	Use to identify if use mid mouse button plus mouse XY axis movement to rotate camera in RPG mode
    public bool RPG_Edge_Rote_Cam;      //	Use to identify if rotate camera when mouse move near screen edge in RPG mode
    public bool RPG_Dir_Rote_Cam;       //	Use to identify if not wait for mid mouse button but directilly record mouse XY axis movement to rotate camera in RPG mode
    public bool RPG_Complet_Cam_Follow = false; //	wheter camera center completily follow player turnning in RPG Mode
    public bool RPG_Classic_Cam_Follow = true;  //	 Follow player turnning with classic RPG style in RPG mode

    [Header("General Settings Affect in RTS Mode")]
    //[HideInInspector] public float Cam_Independent_Distance_Value = 6f; //	not inuse, Distance judgement value for cam to free and get into RTS view by it self
    public float RTS_Fir_Cam_Distance = 10f;
    public float RTS_Sec_Cam_Distance = 15f;

    public bool RTS_Plan_Fir_View_Flag = false;  //	A flag to determine if or not use RTS view
    public bool RTS_Plan_Sec_View_Flag = false;  //	A flag to determine if get into seconed level of RTS view

    public bool RTS_Mid_Mous_Rote_Cam;   //	Use to identify if use mid mouse button plus mouse XY axis movement to rotate camera in RTS mode

    public float Cam_Move_Speed = 4f;
    public bool Move_Camera_towards_cam_Facing = true;
    public bool Move_Camera_Along_World_Axis = false;
    public bool Move_Camera_at_Edge = false;

    public bool Move_Debug = false;

    [HideInInspector] public bool followPlayerFlag = true;  //	Identification for RPG mode, a bool flag for plaer control script use to control camera follow or not follow player.
    [HideInInspector] public bool mousMoveFlag; //	whether mouse is moving or not, for play control script to read

    //  private field

    private float camMaxDis = 0f;
    private float camMinDis = 0f;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private float smoXRotCach = 0f; //	smooth Cache and offset. Math.Smooth result store valuable,  
    private float smoYRotOff = 0f;  //	if claim in function block will cause incorrect result
    private float smoYFolOff = 0f;  //	which present as camera view vibration violently
    private float smoDisCach = 0f;  //	smooth Distance Cache
    private float smoMousCach = 0f; //	smooth Mouse Cache
    private float smoMousTarg = 0f; //	smooth Mouse Target
    private float smoCentHigh = 0f; //	smooth Center auto Height cache
    private float smoHighTarg = Mathf.Infinity; //	smooth center auto Height Target

    private float camMousRPGOff = 0f;   //	offset to save current camera center Y value befor following mouse in RPG follow mode

    private Transform XRoteCent;
    private Camera PlayerCam;
    private float Cam_Rotate_Distance_Factor;
    private float autoDisSave;
    //  private float autoAngSave;  //  not yet inuse
    private bool followPlayerFlagInternal = true;
    private float oldDiff = 0f;
    private float oldAng = 0f;

    private int stateRTSFPS = 0;
    private int stateCamAutoDis = 0;
    private int stateCamRTS = 0;
    //	private int stateCamFPS = 0;	//not yet inuse
    private int stateCamRPG = 0;    //	RPG follow mode state machine

    private bool lockandHideMousFlag = false;   //	a flag to identify whether mouse/cursor should be hide and lock in center

    private float mousTurnTimer;    //	a timer cache

    private int layerMaskPlayer;
    private int layerMaskCharacterRagdoll;
    private int layerMaskHeightAdjust;

    // Use this for initialization
    void Start()
    {
        PlayerCam = Cam_Obj.GetComponent<Camera>();
        XRoteCent = X_Rote_Cent.GetComponent<Transform>();
        Cam_Rotate_Distance_Factor = (Max_Cam_Distance - Min_Cam_Distance) / Max_X_Rotation_Angle;
        xRotation = transform.rotation.eulerAngles.x;   //	give initial angle keep same in sence
        camMaxDis = Max_Cam_Distance;
        camMinDis = Min_Cam_Distance;

        //	fouce false going to RTS mode when Player_Obj is null
        if (Player_Obj == null)
            followPlayerFlag = followPlayerFlagInternal = false;

        layerMaskPlayer = LayerMask.GetMask("Player");
        layerMaskCharacterRagdoll = LayerMask.GetMask("CharacterRagdoll");
        layerMaskHeightAdjust = LayerMask.GetMask("HeightAdjust");
        //camINITIALIZE ();
    }

    void FixedUpdate()
    {
        if (!followPlayerFlagInternal)
        {    //  run in RTS mode
             //	get physics input
            float moveFB = Input.GetAxis("Vertical");
            float moveLR = Input.GetAxis("Horizontal");

            if (Move_Camera_at_Edge)
            {
                float movXOff, movZOff;
                Edge_Move_Control(out movXOff, out movZOff);    //	give offset velue by detect if mouse move near screen edge

                //moveLR = turnLR;	//	make A/D key to control Camera left and right movement

                moveFB += movXOff;
                moveLR += movZOff;
            }
            //	only call MDCtCF() when camera received movement command
            if (moveFB != 0f || moveLR != 0f)
            {
                if (Move_Camera_Along_World_Axis)
                {
                    // Move Disconnect Camera Along World Axis
                    MDCaW(moveFB, moveLR, Cam_Move_Speed);
                }
                else if (Move_Camera_towards_cam_Facing)
                {
                    // Move Disconnect Camera towards Camera Facing
                    MDCtCF(moveFB, moveLR, Cam_Move_Speed);
                }
            }
        }
    }

    // Update is called once after every frame updated
    void LateUpdate()
    {
        float xMaxRote = Max_X_Rotation_Angle;
        float xMinRote = Min_X_Rotation_Angle;
        bool onlyMousCamFollow = false;
        bool completeCamFollow = false;
        bool classicCamFollow = false;
        DEBUG();    //	call debug functions

        //	Debug, fouce false going to RTS mode when followPlayerFlag has been closed
        if (followPlayerFlag == false)
            followPlayerFlagInternal = false;

        //	identify player character is moving or not by reading flag form control script
        bool charIsMovingFlag;
        if (Player_Obj != null && Player_Obj.GetComponent("Player_Controller_RTS_RPG_AstarPathfing_Project") != null)
        {
            charIsMovingFlag = Player_Obj.GetComponent<Player_Controller_RTS_RPG_AstarPathfing_Project>().characterMovingFlag;
        }

        float rawMousMidTrc = Input.GetAxisRaw("Mouse ScrollWheel");    //	record mid mouse scroll wheel movement

        float smoMousMidTrc = SMO_MousTrack(rawMousMidTrc); //	smooth mouse scroll wheel value

        //  manage camera different movement modes in RPG and RTS
        if (followPlayerFlagInternal)
        {   //	Only execuate in RPG mode, means camera follow player
            if (RPG_Mid_Mous_Rote_Cam)
            {
                if (Input.GetMouseButton(2))
                {   //	if mid mouse button has been pushed down
                    lockandHideMousFlag = true;
                    Mous_Turn_Cam_Control();    //	turn camera angle by reading mouse axis
                }
                else
                {
                    mousMoveFlag = false;   //  mouse fouce to stay in center of screen, is not moving. Reset timer 
                    lockandHideMousFlag = false;
                }
            }
            else if (RPG_Dir_Rote_Cam)
            {
                lockandHideMousFlag = true;
                Mous_Turn_Cam_Control();   //	read mouse axis and give to global value for rotation
            }

            if (RPG_Edge_Rote_Cam)
                Edge_Turn_Cam_Control();    //	call block to detect mouse position and rotate camera center when mouse get near screen edge

            if (RPG_Complet_Cam_Follow)
            {
                completeCamFollow = true;
            }
            else if (RPG_Classic_Cam_Follow)
            {
                classicCamFollow = true;
            }
        }
        else
        {    //  in RTS mode
            if (RTS_Mid_Mous_Rote_Cam)
            {
                if (Input.GetMouseButton(2))   //	if mid mouse button has been pushed down
                {
                    lockandHideMousFlag = true; //  hide the mouse
                    Mous_Turn_Cam_Control();    //	read mouse axis and give to global value for rotation
                }
                else
                {
                    lockandHideMousFlag = false;
                }

                onlyMousCamFollow = true;   //  because the camera is going to follow mouse movement anyway
            }
        }

        //	call function block for movement
        if (followPlayerFlagInternal)
        {
            FOLLOW();  //  follow player postion
            xMinRote = RPG_Min_X_Rotation_Angle;
        }
        else
        {
            INDEPENDENT();
        }

        /* 
		 * detect midmouse movement
		 * call function block to move camera far and near
		 * change rotation at same time
		 * use Cam_Rotate_Distance_Factor to sync center rotate angle and camera move distance
		 * if cam default distance is 1.5f, max distance is 10f then move distance is 8.5f
		 * max move angle is 60f then Cam_Rotate_Distance_Factor = 8.5f / 60f
		 */
        CAM_DIS_MANAGER(rawMousMidTrc, smoMousMidTrc, Mouse_Scroll_Sensitivity);

        if (!followPlayerFlagInternal)
        {
            AutoHight();    //	auto change hight when camera mode get into RTS
        }

        if (lockandHideMousFlag)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /*---------------
		 * Angle limition
		 ---------------*/
        xRotation = Mathf.Clamp(xRotation, xMinRote, xMaxRote); //	Mathf.Clamp (valuable, min, max); use to limit value between MAX and MIN
                                                                //	clean yRotation value provent lagre value and make sure this value is same as transform.eulerAngles.y
        yRotation = Public_Functions.Clear_Angle(yRotation);
        //---------------

        ROTATE(xRotation, yRotation, onlyMousCamFollow, completeCamFollow, classicCamFollow);   //	call function block for rotation
    }

    /********************************
	 * --- Functions
	 ********************************/
    //	Move Disconnect Camera center towards Camera Facing
    //	FB: forward/backward, LR: Left/Right, SP: Speed
    void MDCtCF(float FB, float LR, float SP)
    {

        transform.Translate(Vector3.forward * FB * SP * Time.deltaTime);
        // multiply -1 is because input need be turn over
        transform.Translate(Vector3.left * LR * -1 * SP * Time.deltaTime);
    }

    //	Move Disconnect Camera center along world axis x, y, z
    //	FB: forward/backward, LR: Left/Right, SP: Speed
    void MDCaW(float FB, float LR, float SP)
    {

        transform.Translate(Vector3.forward * FB * SP * Time.deltaTime, Space.World);
        // multiply -1 is because input need be turn over
        transform.Translate(Vector3.left * LR * -1 * SP * Time.deltaTime, Space.World);
    }

    //	Edge Boundary Movement Control
    //	out xMovOff: x Movement Offset, out yMovOff: y Movement Offset
    private void Edge_Move_Control(out float xMovOff, out float zMovOff)
    {
        float mousXPos, mousYPos;
        float newXOff = 0f, newZOff = 0f;
        float curScreWid = Screen.width;    //	read current game window width in pixels
        float curScreHei = Screen.height;   //	read current game window height in pixels

        MousPos(out mousXPos, out mousYPos);

        //	mouse movement on Y axis, change obj Z direction in world axis
        if (mousXPos > curScreWid - Edge_Boundary)
        {
            newZOff = 1f;
        }
        else if (mousXPos < 0 + Edge_Boundary)
        {
            newZOff = -1f;
        }

        //	mouse movement on Y axis, change obj X direction in world axis
        if (mousYPos > curScreHei - Edge_Boundary)
        {
            newXOff = 1f;
        }
        else if (mousYPos < 0 + Edge_Boundary)
        {
            newXOff = -1f;
        }

        xMovOff = newXOff;
        zMovOff = newZOff;
    }

    private void Get_Mous_Axis(out float mousX, out float mousY, out bool mousIsMoving)
    {
        mousX = Input.GetAxis("Mouse X") * Look_Sensitivity * Time.deltaTime;   //	record mouse movement on X axis
        mousY = Input.GetAxis("Mouse Y") * Look_Sensitivity * Time.deltaTime;   //	record mouse movement on Y axis
        if (mousX != 0f | mousY != 0f)
            mousIsMoving = true;
        else
            mousIsMoving = false;
    }

    //  read mouse movement and give number to global value
    private void Mous_Turn_Cam_Control()
    {
        float mousX, mousY;
        Get_Mous_Axis(out mousX, out mousY, out mousMoveFlag);
        xRotation -= mousY; //	mouse Y axis change obj X angle in world axis
        yRotation += mousX; //	mouse X axis change obj Y angle in world axis
    }

    //	get current mouse position on screen
    private void MousPos(out float mousXPos, out float mousYPos)
    {
        //	current mouse position on screen in pixels
        //	0 point is at left, down of game window
        mousXPos = Input.mousePosition.x;
        mousYPos = Input.mousePosition.y;
    }

    //	Edge Boundary Movement Control
    //	out xRotOff: x Rotation Offset, out yRotOff: y Rotation Offset
    private void Edge_Turn_Cam_Control()
    {
        float mousXPos, mousYPos;
        float newXOff = 0f, newYOff = 0f;
        float curScreWid = Screen.width;    //	read current game window width in pixels
        float curScreHei = Screen.height;   //	read current game window height in pixels

        MousPos(out mousXPos, out mousYPos);

        //	mouse movement on X axis, change obj Y angle in world axis
        if (mousXPos > curScreWid - Edge_Boundary)
        {
            newYOff = Look_Sensitivity * Time.deltaTime;
        }
        else if (mousXPos < 0 + Edge_Boundary)
        {
            newYOff = -Look_Sensitivity * Time.deltaTime;
        }

        //	mouse movement on Y axis, change obj X angle in world axis
        if (mousYPos > curScreHei - Edge_Boundary)
        {
            newXOff = Look_Sensitivity * Time.deltaTime;
        }
        else if (mousYPos < 0 + Edge_Boundary)
        {
            newXOff = -Look_Sensitivity * Time.deltaTime;
        }

        xRotation += newXOff;
        yRotation += newYOff;
    }

    //	movement when follow player
    //	HF: Hight Offset
    private void FOLLOW()
    {

        Vector3 tempPos = new Vector3(Player_Obj.transform.position.x, Player_Obj.transform.position.y + Height_Offset, Player_Obj.transform.position.z);
        transform.position = tempPos;
    }

    //	movement when not follow player
    private void INDEPENDENT()
    {

        //Vector3 temprPosOffset = new Vector3 (0f,0f,0f);
        //temprPosOffset = transform.position - Player_Obj.transform.position;
        //transform.position = Player_Obj.transform.position + temprPosOffset; //may not correct
    }

    //	rotate center point angle
    //	xR: Mouse xRotation, yR: Mouse yRotation	
    private void ROTATE(float xR, float yR, bool onlyFollowMous, bool completeFollow, bool classicRPG)
    {
        float yAng = 0f;
        //	must use Mathf.SmoothDampAngle otherwise camera will spine 360 when rotate player angle pass 0
        //	mouse follow value
        Public_Functions.SMO_ANG(smoXRotCach, xR, Look_SmoothDamp, out smoXRotCach);

        if (onlyFollowMous)
        {
            //	mouse follow value
            Public_Functions.SMO_ANG(smoYRotOff, yR, Look_SmoothDamp, out smoYRotOff);
            yAng = smoYRotOff;

        }
        else if (completeFollow)
        {
            //	mouse follow value
            Public_Functions.SMO_ANG(smoYRotOff, yR, Look_SmoothDamp, out smoYRotOff);
            smoYRotOff = Public_Functions.Clear_Angle(smoYRotOff);

            float playerY = 0f;
            if (Player_Obj != null)
                playerY = Player_Obj.transform.eulerAngles.y;
            //	follow player character
            Public_Functions.SMO_ANG(smoYFolOff, playerY, Look_SmoothDamp, out smoYFolOff);
            smoYFolOff = Public_Functions.Clear_Angle(smoYFolOff);

            yAng = smoYRotOff + smoYFolOff;

        }
        else if (classicRPG)
        {
            //	every time when mouse moved, timer start
            if (mousMoveFlag)
            {
                mousTurnTimer = 2f;
            }
            else
            {
                if (mousTurnTimer > 0f)
                    mousTurnTimer -= Time.deltaTime;
            }

            switch (stateCamRPG)
            {
                case 0:
                    float playerY = 0f;
                    if (Player_Obj != null)
                        playerY = Player_Obj.transform.eulerAngles.y;
                    //	follow player character
                    Public_Functions.SMO_ANG(transform.eulerAngles.y, playerY, Player_Follow_SmoothDamp, out smoYFolOff);
                    yAng = smoYFolOff;
                    if (mousTurnTimer > 0f)
                    {
                        camMousRPGOff = transform.eulerAngles.y - smoYRotOff;
                        stateCamRPG = 10;
                    }
                    break;
                case 10:
                    //	mouse follow value
                    Public_Functions.SMO_ANG(smoYRotOff, yR, Look_SmoothDamp, out smoYRotOff);
                    //	follow mouse movement
                    yAng = Public_Functions.Clear_Angle(smoYRotOff) + camMousRPGOff;

                    if (mousTurnTimer <= 0f)
                        stateCamRPG = 0;
                    break;
            }
        }

        transform.rotation = Quaternion.Euler(0f, yAng, 0f);
        XRoteCent.rotation = Quaternion.Euler(smoXRotCach, yAng, 0f);
    }

    //	Change Fieldview
    //	mSV: mouse Track Value, CFVS: Cam Field View Sensitivity
    private void CF(float mTV, float CFVS)
    {
        float maxFV = Max_Field_View;
        float minFV = Min_Field_View;
        float curFV = PlayerCam.fieldOfView;
        float newFV = 0f;

        if (mTV > 0f)
        {
            if (curFV > minFV)
            {
                newFV = curFV - CFVS;
            }
        }
        else if (mTV < 0f)
        {
            if (curFV < maxFV)
            {
                newFV = curFV + CFVS;
            }
        }

        //	if field view allow to be change, newFV remain 0f only if change value out of max and min range
        if (newFV != 0f)
        {
            PlayerCam.fieldOfView = newFV;
        }
    }

    //	Camera Distance Management, also manage switch whit FPS\RPG\RTS mode
    //	rMTV: raw Mouse Track Value, sMTV: smooth Mouse Track Value, sens: cam distance change Sensitivity
    //	out rotateX: rotation X
    private void CAM_DIS_MANAGER(float rMTV, float sMTV, float sens)
    {
        float curDis = Vector3.Distance(transform.position, PlayerCam.transform.position);
        float newDisOffset = 0f;
        float mouseTV = 0f; //	mouse mid track value apply with time.deltatime

        /*
		 * (sens * MTV * Time.deltaTime) is offset for new distance
		 * and need to multiply with -1 in order to use on real new distance calculation
		 * this valuable is use to calculate how far away for camera to move with every mid-mouse scroll scale
		 * 
		 * --- not finished yet
		 * this may not best solution, should add a function to smooth mid button track value
		 */
        if (sMTV != 0f)
        {
            mouseTV = sens * sMTV * Time.deltaTime;
            //	rotate center point angle when changing distance
            //if (xRotation <= xMaxRote) {
            //	xRotation -= (MTV * Look_Sensitivity * Time.deltaTime) / Cam_Rotate_Distance_Factor;
            //}
        }

        switch (stateRTSFPS)
        {
            case 0:
                if (curDis < Min_Cam_Distance + 0.01f & rMTV > 0f)
                {   //	switch to FPS view, not yet start
                    //stateRTSFPS = 0;
                    //newDisOffset = FPSDaA (MTV, curDis);
                }
                else if (curDis > Max_Cam_Distance - 0.001f & rMTV < 0f & RTS_Plan_Fir_View_Flag)
                {   //	Switch to RTS View, check rMTV is to reduce sensivity of RPG camera and RTS camera changement
                    oldAng = xRotation;
                    stateCamRTS = 0;
                    //planRTS = true;	
                    if (Player_Obj != null && Player_Obj.GetComponent("Player_Controller_RTS_RPG_AstarPathfing_Project") != null)
                    {
                        followPlayerFlag = false;   //	tell other scripts stop RPG mode
                        followPlayerFlagInternal = false;   //	disconnect with player, cancle cam distance limitation
                    }
                    stateRTSFPS = 10;
                }
                else
                {   //	In RPG mode
                    float autoOffset = ACCD(sMTV, curDis);  //	auto change cam distance if cam view has been block
                    if (autoOffset == 0f)
                    {
                        newDisOffset = autoOffset - mouseTV;    //	mouse track only has effect when auto change function allow
                    }
                    else
                    {
                        newDisOffset = autoOffset;
                    }
                }
                break;
            case 10:    //	switch into RTS view
                int curState;
                RTSDaA(rMTV, curDis,
                    out newDisOffset, out curState);    //	call block to manage RTS camera 

                if (rMTV > 0 & curDis < camMaxDis + 0.5f & curState == 30)
                {   //	if mid mouse track keep forwad and view almost return
                    //planRTS = false;	//	disconnect with player, cancle cam distance limitation
                    stateRTSFPS = 20;
                }
                else if (curDis <= camMaxDis + 0.01f & curState == 30)
                {   //	if view almost return
                    stateRTSFPS = 20;
                }
                else if (Input.GetKey(KeyCode.Backspace) && Player_Obj != null)
                {   //	go back to RPG mode only there is player character attached
                    stateCamRTS = 40;
                    stateRTSFPS = 30;
                }
                break;
            case 20:    //	switch to sub-RTS view

                newDisOffset -= mouseTV;    //	change camera distance with mid mouse track
                if (curDis > Max_Cam_Distance - 0.001f & rMTV < 0f & RTS_Plan_Fir_View_Flag)
                {   //	Switch back to RTS View
                    oldAng = xRotation;
                    stateCamRTS = 0;
                    stateRTSFPS = 10;
                }
                else if (Input.GetKey(KeyCode.Backspace) && Player_Obj != null)
                {   //	go back to RPG mode only there is player character attached
                    stateCamRTS = 50;
                    stateRTSFPS = 30;
                }
                break;
            case 30:    //	prepare goint back to RPG mode

                followPlayerFlagInternal = true;    //	start follow player and limitation

                RTSDaA(rMTV, curDis,
                                out newDisOffset, out curState);    //	call block to manage RTS camera 

                Move_Towards_Player();  //	move back to player position

                Vector3 tarPos = new Vector3(Player_Obj.transform.position.x, Player_Obj.transform.position.y + Height_Offset, Player_Obj.transform.position.z);
                float disDiff = Vector3.Distance(transform.position, tarPos);
                if (disDiff < 0.01f)
                {   //	if camera center near player
                    followPlayerFlag = true;    //	tell other scripts RPG mode is OK to start
                    stateRTSFPS = 0;
                }
                break;
        }

        // range limitation
        float newDis = curDis + newDisOffset;   //	calculate new position should be after apply offset
        if (newDis < camMinDis & followPlayerFlagInternal)
        {
            newDisOffset = -(curDis - camMinDis);
        }
        else if (newDis > camMaxDis & followPlayerFlagInternal)
        {
            newDisOffset = (camMaxDis - curDis);
        }

        CDFRoPtC(newDisOffset, curDis); //	call block to change the distance
    }

    //	move camera center towards player before going back to RPG mode
    private void Move_Towards_Player()
    {
        Vector3 tarPos = new Vector3(Player_Obj.transform.position.x, Player_Obj.transform.position.y + Height_Offset, Player_Obj.transform.position.z);
        transform.position = Vector3.Lerp(transform.position, tarPos, 5f * Time.deltaTime);
    }

    //	FPS	Distance and Angle
    //	curDis: current Distance, mouseTV: mouse Track Value apply with time.deltatime
    private float FPSDaA(float mTV, float curDis)
    {
        float aa = 0f;
        return aa;
    }

    //	RTS Distance and Angle
    //	rMTV: raw Mouse Track Value, curDis: current Distance, mouseTV: mouse Track Value apply with time.deltatime
    //	out newPosOffset: new Position Offset, out curState: current State (stateCamRTS)
    private void RTSDaA(float rMTV, float curDis, out float newPosOffset, out int curState)
    {

        switch (stateCamRTS)
        {
            case 0: //	go to first distance state
                oldDiff = RTS_Fir_Cam_Distance - curDis;
                smoDisCach = 0f;
                stateCamRTS = 10;
                break;
            case 10:    //	in first distance
                xRotation = Max_X_Rotation_Angle;   // if camera is in plan view then lock camera angle
                if (rMTV < 0 & curDis > RTS_Fir_Cam_Distance - 1f & curDis < RTS_Fir_Cam_Distance + 1f & RTS_Plan_Sec_View_Flag)
                {   //	mouse track backward, check curDis and rMTV is to reduce mid mouse truck sensitivity
                    oldDiff = RTS_Sec_Cam_Distance - curDis;
                    smoDisCach = 0f;
                    stateCamRTS = 20;
                }
                else if (rMTV > 0 & curDis > RTS_Fir_Cam_Distance - 1f & curDis < RTS_Fir_Cam_Distance + 1f)
                {   //	mouse track forward
                    oldDiff = camMaxDis - curDis;
                    xRotation = oldAng;
                    smoDisCach = 0f;
                    stateCamRTS = 30;
                }
                break;
            case 20:    //	in second distance
                xRotation = Max_X_Rotation_Angle;   // if camera is in plan view then lock camera angle
                if (rMTV > 0 & curDis > RTS_Sec_Cam_Distance - 1f)
                {   //	mouse track forward
                    oldDiff = RTS_Fir_Cam_Distance - curDis;
                    smoDisCach = 0f;
                    stateCamRTS = 10;
                }
                break;
            case 30:    //	quitting
                if (rMTV < 0 & curDis < camMaxDis + 1f)
                {   //	if mouse track backward during quitting procedure
                    oldAng = xRotation; //	remember current cam x angle
                    stateCamRTS = 0;
                }
                break;
            case 40:    //	prepare going back to RPG mode
                if (camMaxDis < curDis)
                {
                    oldDiff = camMaxDis - curDis;
                    xRotation = oldAng;
                    smoDisCach = 0f;
                }
                stateCamRTS = 50;
                break;
            case 50:
                break;
        }
        Public_Functions.SMO_VAL_OFFSET(smoDisCach, oldDiff, Distance_Change_SmoothDamp * 2,
            out smoDisCach, out newPosOffset);  //	smooth value and get out put
        curState = stateCamRTS;
    }

    // Auto change Hight if terrain changes height
    private void AutoHight()
    {
        if (smoHighTarg == Mathf.Infinity)
        {
            smoHighTarg = transform.position.y; //	initialize value
        }

        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, -transform.up, out rayHit, 100f, layerMaskHeightAdjust))
        {
            float newDis = Height_Offset + rayHit.point.y + 50f;    //	use defult height plus the obj height below camera center 
            float diff = newDis - smoCentHigh;  //	get difference between new position and old position

            if (diff != 0f)
            {   //	if obj below camera center do change height
                smoHighTarg = newDis;
            }
        }

        Public_Functions.SMO_VAL(smoCentHigh, smoHighTarg, 0.08f, out smoCentHigh); //	smooth value
        transform.position = new Vector3(transform.position.x, smoCentHigh, transform.position.z);  //	move camera to new position
    }

    //	Ray From Camera to Center
    private void RayCamCent(float curDis, out float rayDisOut)
    {
        float newRayDis = Mathf.Infinity;

        //	Method from Unity Manual "Direction and Distance from One Object to Another"
        Vector3 camHeading = transform.position - PlayerCam.transform.position;
        //float tempDis = tempHeading.magnitude;	//	use to calculation distance, same result as Vector3.Distance
        Vector3 camDir = camHeading / curDis;

        float maxDis = camMaxDis;
        if (camMaxDis < curDis)
        {
            maxDis = curDis;
        }
        RaycastHit objHit;  //	object detect ray between player and max distance
        if (Physics.Raycast(PlayerCam.transform.position, camDir, out objHit, maxDis))
        {
            newRayDis = objHit.distance;
        }
        rayDisOut = newRayDis;
    }

    //	Ray From Center to Camera
    private void RayCentCam(float curDis, out float rayDisOut)
    {
        float newRayDis = Mathf.Infinity;

        //	Method from Unity Manual "Direction and Distance from One Object to Another"
        Vector3 camHeading = PlayerCam.transform.position - transform.position;
        //float tempDis = tempHeading.magnitude;	//	use to calculation distance, same result as Vector3.Distance
        Vector3 camDir = camHeading / curDis;

        float maxDis = camMaxDis;
        if (camMaxDis < curDis)
        {
            maxDis = curDis;
        }
        RaycastHit objHit;  //	object detect ray between player and max distance
        if (!Physics.Raycast(transform.position, camDir, out objHit, maxDis, layerMaskPlayer)  //  if hit obj is not player
            && !Physics.Raycast(transform.position, camDir, out objHit, maxDis, layerMaskCharacterRagdoll)  //  if hit obj is not ragdoll
            && Physics.Raycast(transform.position, camDir, out objHit, maxDis))
        {  //  and ray dose hit something, if not add this it always hit in 0 position
            newRayDis = objHit.distance;
        }

        rayDisOut = newRayDis;
    }

    //	Auto Change Camera Distance 
    //	rawMTV: raw mid mouse track value, curDis: current Distance
    private float ACCD(float rawMTV, float curDis)
    {
        //float rayDis = Mathf.Infinity;
        float rayDis;
        float smoDisOff = 0f;   //	smooth distance offset
        float newDiff = 0f;

        RayCentCam(curDis, out rayDis);    //  call function shoot ray between center and camera and detect if there is obj block sight

        /*
		 * ---	not finished yet
		 * if mouse scroll then quit auto adjust distance
		 * this function may not the best solution
		 * another way is to change final target, function only quit if finial target is less than current distance
		 */

        if (rawMTV > 0f & stateCamAutoDis == 10)
        {   //	only quit in forward movement when there is block behind cam, this prevent bad cam movement
            smoDisCach = 0f;
            oldDiff = 0f;
            stateCamAutoDis = 0;
        }

        /*
		 * get difference between camera distance and ray collide
		 * multiply with minus 1 makes camera movement offset calculation easier
		 */
        float diff = -(curDis - rayDis);    //	target difference offset value

        switch (stateCamAutoDis)
        {
            case 0: //	normal
                if (diff < 0)
                {   //	if there is something block between camera and player
                    autoDisSave = curDis;   //	save current position as calculation original point
                                            //AutoAngSave = xRotation;
                    oldDiff = 0f;   //	initialize cache
                    smoDisCach = 0f;    //	initialize cache
                    stateCamAutoDis = 10;
                }
                break;
            case 10:    //	auto adjust distance
                newDiff = -(autoDisSave - rayDis);  //	calcuate for new difference base on original point

                if (curDis + newDiff < autoDisSave)
                {
                    oldDiff = newDiff;  //	if different distance changes lower than original point then save as new target
                }
                else
                {
                    oldDiff = autoDisSave - curDis; //	if different distance larger than original point then target original point
                    smoDisCach = 0f;    //	clear calculation cache, otherwise result will be greater than expect in other state
                    stateCamAutoDis = 20;
                }

                Public_Functions.SMO_VAL_OFFSET(smoDisCach, oldDiff, Distance_Change_SmoothDamp,
                    out smoDisCach, out smoDisOff); //	smooth value and get out put

                break;
            case 20:
                newDiff = -(autoDisSave - rayDis);
                if (curDis + newDiff < autoDisSave)
                {
                    smoDisCach = 0f;    //	clear calculation cache, otherwise result will be greater than expect in other state
                    stateCamAutoDis = 10;
                }

                Public_Functions.SMO_VAL_OFFSET(smoDisCach, oldDiff, Distance_Change_SmoothDamp,
                    out smoDisCach, out smoDisOff); //	smooth value and get out put

                if (smoDisCach >= oldDiff - 0.01f)
                {   //	cam move back to original point, claer all smooth calculation cache 
                    smoDisOff = oldDiff - smoDisCach;
                    smoDisCach = 0f;
                    oldDiff = 0f;
                    stateCamAutoDis = 0;
                }

                break;
        }
        return smoDisOff;
    }

    //	Change Distance From Ray of Player to Camera
    //	disPos: distance Position, curDis:	current Distance
    private void CDFRoPtC(float disOffset, float curDis)
    {

        //	Method from Unity Manual "Direction and Distance from One Object to Another"
        Vector3 camHeading = PlayerCam.transform.position - transform.position;
        Vector3 camDir = camHeading / curDis;
        Ray camRay = new Ray(transform.position, camDir);
        PlayerCam.transform.Translate(camRay.direction * disOffset, Space.World);
    }

    //	Camera Look at Center Point
    private void CLaCP()
    {

        //	Rotate Camera look at center point
        Vector3 tempRote = transform.position - PlayerCam.transform.position;
        Quaternion tempQuat = Quaternion.LookRotation(tempRote);
        PlayerCam.transform.rotation = tempQuat;
    }

    //	Initialize camera view and center angle
    private void camINITIALIZE()
    {

        //CLaCP ();	//	Camera Look at Center Point
        //FOLLOW ();	//	Initialize camera center position by follow player character
    }

    //	Smooth mid Mouse Track value
    private float SMO_MousTrack(float mouseTrackInput)
    {
        float oldDisOff = smoMousCach;

        smoMousTarg += mouseTrackInput;
        Public_Functions.SMO_VAL(smoMousCach, smoMousTarg, Mouse_Scroll_SmoothDamp, out smoMousCach);
        float smoothOutPut = smoMousCach - oldDisOff;

        if (smoMousCach == smoMousTarg)
        {
            smoMousCach = smoMousTarg = 0f; //	clear 0, keep cache clean
        }

        return smoothOutPut;
    }

    //	debug functions, test camera movement by keyboard
    private void DEBUG()
    {

        //	Use keyboard to debug if mouse function is not working well
        if (Move_Debug)
        {
            if (Input.GetKey(KeyCode.O))
            {
                xRotation += 1f * Look_Sensitivity * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.P))
            {
                xRotation -= 1f * Look_Sensitivity * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.K))
            {
                yRotation += 1f * Look_Sensitivity * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.L))
            {
                yRotation -= 1f * Look_Sensitivity * Time.deltaTime;
            }
        }
    }
}
//Debug.Log (" rayDis?: " + rayDis);