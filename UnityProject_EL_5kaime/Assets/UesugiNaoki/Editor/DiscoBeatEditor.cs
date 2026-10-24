using UnityEditor;
using UnityEngine;

namespace Disco
{
    /// <summary>Play 中に Inspector から ON/OFF を試せるようにする。</summary>
    [CustomEditor(typeof(DiscoBeat))]
    public class DiscoBeatEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                var beat = (DiscoBeat)target;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("ON")) beat.SetEffect(true);
                if (GUILayout.Button("OFF")) beat.SetEffect(false);
                EditorGUILayout.EndHorizontal();
            }
            if (!Application.isPlaying) EditorGUILayout.HelpBox("Play 中に ON / OFF を試せます", MessageType.Info);
        }
    }
}
