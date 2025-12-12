#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MorvaridEssential.Editor
{
    [CustomEditor(typeof(Animalo))]
    public class MenuItemsAnimatorSODrivenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Open Animator Panel"))
            {
                var myTarget = (Animalo)target;
                SimpleMenuItemsTimelineWindow.ShowWindow();
                SimpleMenuItemsTimelineWindow.Instance.SetAnimator(myTarget);
            }
        }
    }
}

#endif