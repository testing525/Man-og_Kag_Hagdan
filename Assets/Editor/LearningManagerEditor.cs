using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LearningManager))]
public class LearningManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LearningManager manager = (LearningManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("📤 Export Training Data"))
        {
            manager.ExportTrainingData();
        }
    }
}
