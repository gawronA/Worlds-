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
    SerializedProperty m_lod1, m_lod2, m_lod3;
    SerializedProperty m_planetPrefab;
    SerializedProperty m_planetChunkPrefab;
    SerializedProperty m_planetMaterial;

    int m_length;
    int m_chunk_count;
    int m_border;

    bool m_auto_lod = false;

    GeographicCoordinate coords;
    float m_phiDeg, m_thetaDeg;
    private void OnEnable()
    {
        m_radius = serializedObject.FindProperty("m_radius");
        m_est_border = serializedObject.FindProperty("m_est_border");
        m_scale = serializedObject.FindProperty("m_scale");
        m_xyzResolution = serializedObject.FindProperty("m_xyzResolution");
        m_shaded = serializedObject.FindProperty("m_shaded");
        m_position = serializedObject.FindProperty("m_position");
        m_lod1 = serializedObject.FindProperty("m_lod1");
        m_lod2 = serializedObject.FindProperty("m_lod2");
        m_lod3 = serializedObject.FindProperty("m_lod3");
        m_planetPrefab = serializedObject.FindProperty("m_planetPrefab");
        m_planetChunkPrefab = serializedObject.FindProperty("m_planetChunkPrefab");
        m_planetMaterial = serializedObject.FindProperty("m_planetMaterial");

        m_length = 2 * m_est_border.intValue + 2 * m_radius.intValue;
        m_chunk_count = Mathf.CeilToInt((float)m_length / m_xyzResolution.intValue);
        m_border = (m_chunk_count * m_xyzResolution.intValue) / 2 - m_radius.intValue;

        coords = new GeographicCoordinate();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Chunk info");
        EditorGUILayout.PropertyField(m_xyzResolution, new GUIContent("Voxel resolution"));
        EditorGUILayout.PropertyField(m_planetChunkPrefab, new GUIContent("Chunk prefab"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
       
        EditorGUILayout.LabelField("Planet info");
        EditorGUILayout.PropertyField(m_radius, new GUIContent("Planet radius"));
        EditorGUILayout.PropertyField(m_est_border, new GUIContent("Estimated border"));
        if(EditorGUI.EndChangeCheck())
        {
            m_length = 2 * m_est_border.intValue + 2 * m_radius.intValue;
            m_chunk_count = Mathf.CeilToInt((float)m_length / m_xyzResolution.intValue);
            m_border = (m_chunk_count * m_xyzResolution.intValue) / 2 - m_radius.intValue;
        }
        EditorGUILayout.LabelField("Actual border: " + m_border.ToString());
        EditorGUILayout.LabelField("N. of chunks: " + Mathf.Pow(m_chunk_count, 3).ToString());
        EditorGUILayout.PropertyField(m_scale, new GUIContent("Scale"));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField("Level Of Detail");
        m_auto_lod = EditorGUILayout.Toggle("Auto distance", m_auto_lod);
        if(m_auto_lod)
        {
            //m_lod1.floatValue = 
            EditorGUILayout.LabelField("LOD1 max: " + m_lod1.ToString());
            EditorGUILayout.LabelField("LOD2 max: " + m_lod2.ToString());
            EditorGUILayout.LabelField("LOD3 max: " + m_lod3.ToString());
        }
        else
        {
            EditorGUILayout.PropertyField(m_lod1, new GUIContent("LOD1 max: "));
            EditorGUILayout.PropertyField(m_lod2, new GUIContent("LOD2 max: "));
            EditorGUILayout.PropertyField(m_lod3, new GUIContent("LOD3 max: "));
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.PropertyField(m_shaded, new GUIContent("Smooth shading"));
        EditorGUILayout.PropertyField(m_position, new GUIContent("Planet position"));
        EditorGUILayout.PropertyField(m_planetPrefab, new GUIContent("Planet prefab"));
        EditorGUILayout.PropertyField(m_planetMaterial, new GUIContent("Planet material"));

        serializedObject.ApplyModifiedProperties();
    }
}
