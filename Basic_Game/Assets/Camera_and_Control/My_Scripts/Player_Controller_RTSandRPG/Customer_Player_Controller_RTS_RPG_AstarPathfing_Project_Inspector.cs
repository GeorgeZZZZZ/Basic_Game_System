using UnityEngine;
using UnityEditor;


//  vr: 0.2.0

[CustomEditor(typeof(Player_Controller_RTS_RPG_AstarPathfing_Project))]
public class Customer_Player_Controller_RTS_RPG_AstarPathfing_Project_Inspector : Editor
{
    Player_Controller_RTS_RPG_AstarPathfing_Project CPC;

    SerializedObject serOBJ;
    SerializedProperty PMB, PTB, camOBJ, selOBJ, pupBehav;
    
    private void OnEnable()
    {
        CPC = (Player_Controller_RTS_RPG_AstarPathfing_Project)target;

        serOBJ = new SerializedObject(target);
        PMB = serOBJ.FindProperty("PlayerMoveBehivior");    //  find serializable enum
        PTB = serOBJ.FindProperty("PlayerTurnBehivior");
        camOBJ = serOBJ.FindProperty("Cam_Center_Point");
        selOBJ = serOBJ.FindProperty("Select_Circle_Prefab");
        pupBehav = serOBJ.FindProperty("BehaviourPuppet");
    }

    public override void OnInspectorGUI()
    {
        serOBJ.Update();    //  update serializeable objs

        //  General Settings---------------------------
        GUILayout.Space(10);
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(camOBJ);
        EditorGUILayout.PropertyField(selOBJ);
        EditorGUILayout.PropertyField(pupBehav);

        CPC.Edge_Boundary = EditorGUILayout.IntField("Edge_Boundary", CPC.Edge_Boundary);
        CPC.Player_Normal_Speed = EditorGUILayout.FloatField("Player_Normal_Speed", CPC.Player_Normal_Speed);
        CPC.Player_Run_Speed = EditorGUILayout.FloatField("Player_Run_Speed", CPC.Player_Run_Speed);
        CPC.Player_Turnning_Speed = EditorGUILayout.FloatField("Player_Turnning_Speed", CPC.Player_Turnning_Speed);
        CPC.Jump_Speed = EditorGUILayout.FloatField("Jump_Speed", CPC.Jump_Speed);

        //  Character move type Settings---------------------------
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Settings Player Move Type: ", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(PMB);
        switch (PMB.enumValueIndex)
        {
            case 0:
                CPC.Move_Or_Turn_Player_According_To_Camera = true;
                CPC.Move_Player_towards_Character_Facing = false;
                CPC.Move_Player_Along_World_Axis = false;
                break;
            case 1:
                CPC.Move_Or_Turn_Player_According_To_Camera = false;
                CPC.Move_Player_towards_Character_Facing = true;
                CPC.Move_Player_Along_World_Axis = false;
                break;
            case 2:
                CPC.Move_Or_Turn_Player_According_To_Camera = false;
                CPC.Move_Player_towards_Character_Facing = false;
                CPC.Move_Player_Along_World_Axis = true;
                break;
        }

        if (!CPC.Move_Or_Turn_Player_According_To_Camera)
        {
            EditorGUILayout.PropertyField(PTB);
            switch (PTB.enumValueIndex)
            {
                case 0:
                    CPC.Turn_Player_by_Keyboard = true;
                    CPC.Turn_Player_by_Mouse_Point = false;
                    break;
                case 1:
                    CPC.Turn_Player_by_Keyboard = false;
                    CPC.Turn_Player_by_Mouse_Point = true;
                    break;
            }
        }
        else
        {
            CPC.Turn_Player_by_Keyboard = false;
            CPC.Turn_Player_by_Mouse_Point = false;
        }

        CPC.Force_RTS_Cam_View = EditorGUILayout.Toggle("Force_RTS_Cam_View", CPC.Force_RTS_Cam_View);
        
        serOBJ.ApplyModifiedProperties();   //  apply serializable objs change
    }

}