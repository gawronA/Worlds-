using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetGenerator))]
[CanEditMultipleObjects]
public class PlanetGeneratorEditor : Editor
{
    SerializedProperty m_radius;
    SerializedProperty m_est_border;
    SerializedProperty m_scale;
    SerializedProperty m_xyzResolution;
    SerializedProperty m_shaded;
    SerializedProperty m_position;
    SerializedProperty m_planetPrefab;
    SerializedProperty m_planetChunkPrefab;
    SerializedProperty m_planetMaterial;

    int m_border;

    private void OnEnable()
    {
        m_radius = serializedObject.FindProperty("m_radius");
        m_est_border = serializedObject.FindProperty("m_est_border");
        m_scale = serializedObject.FindProperty("m_scale");
        m_xyzResolution = serializedObject.FindProperty("m_xyzResolution");
        m_shaded = serializedObject.FindProperty("m_shaded");
        m_position = serializedObject.FindProperty("m_position");
        m_planetPrefab = serializedObject.FindProperty("m_planetPrefab");
        m_planetChunkPrefab = serializedObject.FindProperty("m_planetChunkPrefab");
        m_planetMaterial = serializedObject.FindProperty("m_planetMaterial");

        int length = 2 * m_est_border.intValue + 2 * m_radius.intValue;
        int m_chunk_count = Mathf.CeilToInt((float)length / m_xyzResolution.intValue);
        m_border = (m_chunk_count * m_xyzResolution.intValue) / 2 - m_radius.intValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Chunk info");
        EditorGUILayout.PropertyField(m_xyzResolution, new GUIContent("Voxel resolution"));
        EditorGUILayout.PropertyField(m_planetChunkPrefab, new GUIContent("Chunk prefab"));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Planet info");
        EditorGUILayout.PropertyField(m_radius, new GUIContent("Planet radius"));
        EditorGUILayout.PropertyField(m_est_border, new GUIContent("Estimated border"));
        if(EditorGUI.EndChangeCheck())
        {
            int length = 2 * m_est_border.intValue + 2 * m_radius.intValue;
            int m_chunk_count = Mathf.CeilToInt((float)length / m_xyzResolution.intValue);
            m_border = (m_chunk_count * m_xyzResolution.intValue) / 2 - m_radius.intValue;
        }
        EditorGUILayout.LabelField("Actual border: " + m_border.ToString());
        EditorGUILayout.PropertyField(m_scale, new GUIContent("Scale"));
        EditorGUILayout.PropertyField(m_shaded, new GUIContent("Smooth shading"));
        EditorGUILayout.PropertyField(m_position, new GUIContent("Planet position"));
        EditorGUILayout.PropertyField(m_planetPrefab, new GUIContent("Planet prefab"));
        EditorGUILayout.PropertyField(m_planetMaterial, new GUIContent("Planet material"));

        serializedObject.ApplyModifiedProperties();
    }
}
