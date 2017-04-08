using UnityEngine;
using UnityEditor;

//  vr: 0.1.1

[CustomEditor(typeof(Camera_Controller))]
public class Customer_Camera_Inspector : Editor {
    Camera_Controller CC;
    bool foldRPGMode, foldRTSMode;

    SerializedObject serOBJ;
    SerializedProperty MCCTinRPG, CFPB, CMTinRTS, MCCTinRTS, playerOBJ, xRoteOBJ, camOBJ;

    private void OnEnable()
    {
        CC = (Camera_Controller) target;

        serOBJ = new SerializedObject(target);
        MCCTinRPG = serOBJ.FindProperty("MouseControlCamTypesInRPGMode");    //  find serializable enum
        CFPB = serOBJ.FindProperty("CameraFollowPlayerBehavior");    //  find serializable enum
        CMTinRTS = serOBJ.FindProperty("CameraMovementTypes");    //  find serializable enum
        MCCTinRTS = serOBJ.FindProperty("MouseControlCamTypeseInRTSMode");    //  find serializable enum
        playerOBJ = serOBJ.FindProperty("Player_Obj");
        xRoteOBJ = serOBJ.FindProperty("X_Rote_Cent");
        camOBJ = serOBJ.FindProperty("Cam_Obj");
    }
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();    //  If this code is uncomment then all original values will appear in inspector

        serOBJ.Update();    //  update serializeable objs

        //  General Settings---------------------------
        GUILayout.Space(10);
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerOBJ);
        EditorGUILayout.PropertyField(xRoteOBJ);
        EditorGUILayout.PropertyField(camOBJ);

        CC.Height_Offset = EditorGUILayout.FloatField("Height_Offset", CC.Height_Offset);
        CC.Look_Sensitivity = EditorGUILayout.FloatField("Look_Sensitivity", CC.Look_Sensitivity);
        CC.Look_SmoothDamp = EditorGUILayout.FloatField("Look_SmoothDamp", CC.Look_SmoothDamp);
        CC.Mouse_Scroll_Sensitivity = EditorGUILayout.FloatField("Mouse_Scroll_Sensitivity", CC.Mouse_Scroll_Sensitivity);
        CC.Mouse_Scroll_SmoothDamp = EditorGUILayout.FloatField("Mouse_Scroll_SmoothDamp", CC.Mouse_Scroll_SmoothDamp);
        CC.Max_X_Rotation_Angle = EditorGUILayout.FloatField("Max_X_Rotation_Angle", CC.Max_X_Rotation_Angle);
        CC.Min_X_Rotation_Angle = EditorGUILayout.FloatField("Min_X_Rotation_Angle", CC.Min_X_Rotation_Angle);

        if (CC.Player_Obj != null) 
        {
            //  RPG Mode settings---------------------------
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Settings in RPG Mode: ", EditorStyles.boldLabel);

            foldRPGMode = EditorGUILayout.Foldout(foldRPGMode, "");
            if (foldRPGMode)
            {
                CC.Player_Follow_SmoothDamp = EditorGUILayout.FloatField("Player_Follow_SmoothDamp", CC.Player_Follow_SmoothDamp);
                CC.Max_Cam_Distance = EditorGUILayout.FloatField("Max_Cam_Distance", CC.Max_Cam_Distance);
                CC.Min_Cam_Distance = EditorGUILayout.FloatField("Min_Cam_Distance", CC.Min_Cam_Distance);
                CC.RPG_Min_X_Rotation_Angle = EditorGUILayout.FloatField("RPG_Min_X_Rotation_Angle", CC.RPG_Min_X_Rotation_Angle);
                CC.Distance_Change_Sensitivity = EditorGUILayout.FloatField("Distance_Change_Sensitivity", CC.Distance_Change_Sensitivity);
                CC.Distance_Change_SmoothDamp = EditorGUILayout.FloatField("Distance_Change_SmoothDamp", CC.Distance_Change_SmoothDamp);
                CC.Angle_Change_Sensitivity = EditorGUILayout.FloatField("Angle_Change_Sensitivity", CC.Angle_Change_Sensitivity);

                EditorGUILayout.PropertyField(MCCTinRPG);
                switch (MCCTinRPG.enumValueIndex)
                {
                    case 0:
                        CC.RPG_Dir_Rote_Cam = true;
                        CC.RPG_Mid_Mous_Rote_Cam = false;
                        break;
                    case 1:
                        CC.RPG_Dir_Rote_Cam = false;
                        CC.RPG_Mid_Mous_Rote_Cam = true;
                        break;
                    case 2:
                        CC.RPG_Dir_Rote_Cam = false;
                        CC.RPG_Mid_Mous_Rote_Cam = false;
                        break;
                }

                if (CC.RPG_Mid_Mous_Rote_Cam)
                    CC.RPG_Edge_Rote_Cam = EditorGUILayout.Toggle("RPG_Edge_Rote_Cam", CC.RPG_Edge_Rote_Cam);

                EditorGUILayout.PropertyField(CFPB);
                switch (CFPB.enumValueIndex)
                {
                    case 0:
                        CC.RPG_Classic_Cam_Follow = true;
                        CC.RPG_Complet_Cam_Follow = false;
                        break;
                    case 1:
                        CC.RPG_Classic_Cam_Follow = false;
                        CC.RPG_Complet_Cam_Follow = true;
                        break;
                }
            }
        }
        //  RTS Mode settings---------------------------
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Settings in RTS Mode: ", EditorStyles.boldLabel);

        foldRTSMode = EditorGUILayout.Foldout(foldRTSMode, "");
        if (foldRTSMode)
        {
            CC.RTS_Plan_Fir_View_Flag = EditorGUILayout.Toggle("RTS_Plan_Fir_View_Flag", CC.RTS_Plan_Fir_View_Flag);
            if (CC.RTS_Plan_Fir_View_Flag)
            {
                CC.RTS_Fir_Cam_Distance = EditorGUILayout.FloatField("RTS_Fir_Cam_Distance", CC.RTS_Fir_Cam_Distance);
                CC.RTS_Plan_Sec_View_Flag = EditorGUILayout.Toggle("RTS_Plan_Sec_View_Flag", CC.RTS_Plan_Sec_View_Flag);
                if (CC.RTS_Plan_Sec_View_Flag)
                {
                    CC.RTS_Sec_Cam_Distance = EditorGUILayout.FloatField("RTS_Sec_Cam_Distance", CC.RTS_Sec_Cam_Distance);
                }
            }

            EditorGUILayout.PropertyField(MCCTinRTS);
            switch (MCCTinRTS.enumValueIndex)
            {
                case 0:
                    CC.RTS_Mid_Mous_Rote_Cam = true;
                    break;
            }

            CC.Cam_Move_Speed = EditorGUILayout.FloatField("Cam_Move_Speed", CC.Cam_Move_Speed);

            EditorGUILayout.PropertyField(CMTinRTS);
            switch (CMTinRTS.enumValueIndex)
            {
                case 0:
                    CC.Move_Camera_towards_cam_Facing = true;
                    CC.Move_Camera_Along_World_Axis = false;
                    break;
                case 1:
                    CC.Move_Camera_towards_cam_Facing = false;
                    CC.Move_Camera_Along_World_Axis = true;
                    break;
            }
            
            CC.Move_Camera_at_Edge = EditorGUILayout.Toggle("Move_Camera_at_Edge", CC.Move_Camera_at_Edge);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Debug: ", EditorStyles.boldLabel);
        CC.Move_Debug = EditorGUILayout.Toggle("Move_Debug", CC.Move_Debug);

        serOBJ.ApplyModifiedProperties();   //  apply serializable objs change
    }
}
