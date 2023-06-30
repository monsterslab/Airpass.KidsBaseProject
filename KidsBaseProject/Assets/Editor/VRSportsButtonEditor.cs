using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AirpassUnity.VRSports
{
    [CustomEditor(typeof(VRSportsButton))]
    public class VRSportsButtonEditor : Editor
    {
        VRSportsButton self;
        GUIStyle style;
        bool foldOut_type = false;
        bool foldOut_basic = false;

        private void OnEnable()
        {
            self = (VRSportsButton)target;
            style = new GUIStyle();
        }

        void DrawHold()
        {
            EditorGUILayout.LabelField("Type Config", style);
            self.holdFlip = EditorGUILayout.Toggle("HoldFlip", self.holdFlip);
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("img_holding"), new GUIContent("Filling bar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sfxHolding"), new GUIContent("Holding Sfx"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fillMethod"), new GUIContent("Fill method"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fillClockWise"), new GUIContent("Fill clockwise"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("holdedMaxOffset"), new GUIContent("Fill offset max"));
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            self.interactable = EditorGUILayout.Toggle("Interactable", self.interactable);
            self.type = (ButtonType)EditorGUILayout.EnumPopup("Type", self.type);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interactedColor"), new GUIContent("interacted color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sfxInteracted"), new GUIContent("Interacted sfx"));
            serializedObject.ApplyModifiedProperties();
            switch (self.type)
            {
                case ButtonType.hold: DrawHold(); break;
            }
            if (foldOut_type = EditorGUILayout.Foldout(foldOut_type, "EventTriggers_Type"))
            {
                switch (self.type)
                {
                    case ButtonType.hold:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHoldBegin"), new GUIContent("Event while HoldBegin"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHolding"), new GUIContent("Event while Holding"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHolded"), new GUIContent("Event while Holded"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHoldLost"), new GUIContent("Event while HoldLost"));
                        break;
                    case ButtonType.enter:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEntered"), new GUIContent("Event while Enter"));
                        break;
                }
            }
            if (foldOut_basic = EditorGUILayout.Foldout(foldOut_basic, "EventTriggers_Basic"))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractEnter"), new GUIContent("Event while Enter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractDown"), new GUIContent("Event while Down"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractHolding"), new GUIContent("Event while Holding"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractUp"), new GUIContent("Event while Up"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractExit"), new GUIContent("Event while Exit"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInteractClick"), new GUIContent("Event while Click"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}