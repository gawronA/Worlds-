using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
[CanEditMultipleObjects]
public class PlanetGeneratorEditor : Editor
{
	SerializedProperty voxelMap;
	SerializedProperty position;
	SerializedProperty rotation;
	SerializedProperty radius;
	SerializedProperty size;
	SerializedProperty chunkGridResolution;
	SerializedProperty chunksMultiplier;
	SerializedProperty isoMultiplier;

	private void OnEnable()
	{
		voxelMap = serializedObject.FindProperty("voxelMap");
		position = serializedObject.FindProperty("position");
		rotation = serializedObject.FindProperty("rotation");
		radius = serializedObject.FindProperty("radius");
		size = serializedObject.FindProperty("size");
		chunkGridResolution = serializedObject.FindProperty("chunkGridResolution");
		chunksMultiplier = serializedObject.FindProperty("chunksMultiplier");
		isoMultiplier = serializedObject.FindProperty("isoMultiplier");
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(voxelMap);
		EditorGUILayout.PropertyField(position);
		EditorGUILayout.PropertyField(rotation);
		EditorGUILayout.PropertyField(radius);
		EditorGUILayout.PropertyField(size);
		EditorGUILayout.PropertyField(chunkGridResolution);
		EditorGUILayout.PropertyField(chunksMultiplier);
		EditorGUILayout.PropertyField(isoMultiplier);

		if(GUILayout.Button("Instantiate cube grid"))
		{
			(target as PlanetGenerator).InstantiateCubes();
		}

		serializedObject.ApplyModifiedProperties();
	}
}
