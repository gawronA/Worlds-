using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseTextureCreator))]
public class NoiseTextureCreatorInspector : Editor
{
	private NoiseTextureCreator creator;

	private void OnEnable()
	{
		creator = target as NoiseTextureCreator;
		Undo.undoRedoPerformed += RefreshCreator;
	}

	private void OnDisable()
	{
		Undo.undoRedoPerformed -= RefreshCreator;
	}
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if(EditorGUI.EndChangeCheck() && Application.isPlaying)
		{
			RefreshCreator();
		}
	}

	private void RefreshCreator()
	{
		if(Application.isPlaying) creator.FillTexture();
	}
}
