using UnityEngine;
using UnityEditor;

namespace UnityNeRF.Editor
{
    [CustomEditor(typeof(VolumetricRenderFeature))]
    internal class VolumetricRenderFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty maxStepsProp;
        private SerializedProperty stepSizeProp;
        private SerializedProperty semitransparentShadowsProp;

        public override void OnInspectorGUI()
        {
            FindProperties();
            serializedObject.Update();

            // Rendering
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(maxStepsProp, Styles.MaxSteps);
            EditorGUILayout.PropertyField(stepSizeProp, Styles.StepSize);
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Separator();

            // Shadows
            EditorGUILayout.LabelField("Shadows", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(semitransparentShadowsProp);
            EditorGUI.indentLevel -= 1;
            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            SerializedProperty settingsProp = serializedObject.FindProperty("settings");
            maxStepsProp = settingsProp.FindPropertyRelative("MaxSteps");
            stepSizeProp = settingsProp.FindPropertyRelative("StepSize");
         
            SerializedProperty shadowSettingsProp = settingsProp.FindPropertyRelative("ShadowSettings");
            semitransparentShadowsProp = shadowSettingsProp.FindPropertyRelative("SemitransparentShadows");
        }

        private static class Styles
        {
            public static readonly GUIContent MaxSteps = EditorGUIUtility.TrTextContent("Max Steps");
            public static readonly GUIContent StepSize = EditorGUIUtility.TrTextContent("Step Size");
            public static readonly GUIContent SemitransparentShadows = EditorGUIUtility.TrTextContent("Semitransparent Shadows");
        }
    }
}
