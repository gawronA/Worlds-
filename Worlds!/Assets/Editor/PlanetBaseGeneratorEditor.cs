using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading;

[CustomEditor(typeof(PlanetBaseGenerator))]
[CanEditMultipleObjects]
public class PlanetBaseGeneratorEditor : Editor
{
	List<Vector3> scaleVertices;
	//static float scale;
	
	SerializedProperty planetName;
	SerializedProperty m_PlanetToTilesSubdivides;
	SerializedProperty m_TileToPolygonsSubdivides;
	SerializedProperty m_Scale;
	private void OnEnable()
	{
		scaleVertices = new List<Vector3> { new Vector3(-1, 0, (1.0f + Mathf.Sqrt(5.0f)) / 2.0f).normalized,
											new Vector3(1, 0, (1.0f + Mathf.Sqrt(5.0f)) / 2.0f).normalized };

		planetName = serializedObject.FindProperty("planetName");
		m_PlanetToTilesSubdivides = serializedObject.FindProperty("m_PlanetToTilesSubdivides");
		m_TileToPolygonsSubdivides = serializedObject.FindProperty("m_TileToPolygonsSubdivides");
		m_Scale = serializedObject.FindProperty("m_Scale");
	}
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(planetName);

		EditorGUILayout.LabelField("Scale: " + m_Scale.floatValue.ToString());
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(m_PlanetToTilesSubdivides, new GUIContent("Planet to tiles div."));
		EditorGUILayout.IntSlider(m_TileToPolygonsSubdivides, 0, 3, new GUIContent("Tile to polygons div."));
		if(EditorGUI.EndChangeCheck())
		{
			if(m_PlanetToTilesSubdivides.intValue < 0) m_PlanetToTilesSubdivides.intValue = 0;
			Vector3 tmp = new Vector3(scaleVertices[1].x, scaleVertices[1].y, scaleVertices[1].z);
			for(int i = 0; i < m_PlanetToTilesSubdivides.intValue; i++) tmp = Vector3.Lerp(scaleVertices[0], tmp, 0.5f).normalized;
			for(int i = 0; i < m_TileToPolygonsSubdivides.intValue; i++) tmp = Vector3.Lerp(scaleVertices[0], tmp, 0.5f).normalized;
			m_Scale.floatValue = 1.0f / Vector3.Distance(scaleVertices[0], tmp);
		}
		serializedObject.ApplyModifiedProperties();
		//PlanetBaseGenerator myScript = (PlanetBaseGenerator)target;

		//EditorGUILayout.TextField()
		//EditorGUILayout.IntField(m_PlanetToTilesSubdivides.intValue);
		//EditorGUILayout.IntSlider(m_TileToPolygonsSubdivides, 0, 3, new GUIContent("Tile to polygons divides"));
		//myScript.m_TileToPolygonsSubdivides = EditorGUILayout.IntField("Tile to polygons divides", myScript.m_TileToPolygonsSubdivides);
		/*
		if(showAdditionalOptions = EditorGUILayout.Foldout(showAdditionalOptions, "Additional options"))
		{
			if(showPrefabExport = EditorGUILayout.Foldout(showPrefabExport, "Prefab export"))
			{
				EditorGUILayout.LabelField("Prefab folder path:");
				EditorGUILayout.SelectableLabel(myScript.prefabFolder);
				if(GUILayout.Button("Select folder"))
				{
					prefabFolder = EditorUtility.OpenFolderPanel("Select prefab folder", prefabFolder, "");
					prefabFolder = prefabFolder.Replace(Application.dataPath, "Assets") + "/";
					myScript.prefabFolder = prefabFolder;
				}

				EditorGUILayout.LabelField("Meshes folder path:");
				EditorGUILayout.SelectableLabel(myScript.meshFolder);
				if(GUILayout.Button("Select folder"))
				{
					meshesFolder = EditorUtility.OpenFolderPanel("Select meshes folder", meshesFolder, "");
					meshesFolder = meshesFolder.Replace(Application.dataPath, "Assets") + "/";
					myScript.meshFolder = meshesFolder;
				}
				if(GUILayout.Button("Create prefab"))
				{
					//new Thread(myScript.CreatePrefab());
				}
			}
		}
		else
		{
			showPrefabExport = false;
		}
		//TilesSubdivides = EditorGUILayout.IntField("Planet to tiles divides", TilesSubdivides);
		//PolygonsSubdivides = EditorGUILayout.IntField("Tile to polygons divides", PolygonsSubdivides);
		//myScript.m_PlanetToTilesSubdivides = TilesSubdivides;
		//myScript.m_TileToPolygonsSubdivides = PolygonsSubdivides;
		//base.OnInspectorGUI();*/
	}
}
