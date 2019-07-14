using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurfaceTextureCreator))]
public class SurfaceTextureCreatorInspector : Editor
{
	SerializedProperty resolution;
	SerializedProperty z;
	SerializedProperty drawVoxels;
	SerializedProperty voxelsOverlay;
	SerializedProperty drawContour;
	private void OnEnable()
	{
		resolution = serializedObject.FindProperty("resolution");
		z = serializedObject.FindProperty("z");
		drawVoxels = serializedObject.FindProperty("drawVoxels");
		voxelsOverlay = serializedObject.FindProperty("voxelsOverlay");
		drawContour = serializedObject.FindProperty("drawContour");
	}
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
			drawVoxels.boolValue = EditorGUILayout.Toggle("Draw Voxels", drawVoxels.boolValue);
			EditorGUI.BeginDisabledGroup(!drawVoxels.boolValue);
				voxelsOverlay.boolValue = EditorGUILayout.Toggle("Voxels overlay", voxelsOverlay.boolValue);
			EditorGUI.EndDisabledGroup();
			drawContour.boolValue = EditorGUILayout.Toggle("Draw contour", drawContour.boolValue);
			z.intValue = EditorGUILayout.IntSlider("z", z.intValue, 0, resolution.intValue - 1);
		if(EditorGUI.EndChangeCheck())
		{
			(target as SurfaceTextureCreator).drawVoxels = drawVoxels.boolValue;
			(target as SurfaceTextureCreator).voxelsOverlay = voxelsOverlay.boolValue;
			(target as SurfaceTextureCreator).drawContour = drawContour.boolValue;
			(target as SurfaceTextureCreator).FillTexture(z.intValue);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
