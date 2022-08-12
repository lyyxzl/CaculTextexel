using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEditor.SceneView;

namespace CaculTexTexel.Editor {

    [InitializeOnLoad]
    public class CaculTextexelMode
    {
		private const string RendererIndexKey = "TextureTexelMode.RendererIndex";

		static CameraMode calTextelCameraMode = new CameraMode
		{
			drawMode = DrawCameraMode.UserDefined,
			name = "CaculTexTexel",
			section = "Miscellaneous",
		};

		static CaculTextexelMode()
		{
			EditorApplication.delayCall += SetUp;
		}

		private static bool IsDefinedCalTexCameraMode()
		{
			var field = typeof(SceneView).GetProperty("userDefinedModes", BindingFlags.Static | BindingFlags.NonPublic);
			var userDefinedModes = field.GetValue(null) as List<SceneView.CameraMode>;
			foreach (var cameraMode in userDefinedModes)
			{
				if (cameraMode.name.Equals(calTextelCameraMode.name) &&
					cameraMode.section.Equals(calTextelCameraMode.section))
				{
					return true;
				}
			}
			return false;
		}

		private static void SetUp()
		{
			if (!Utility.IsUseUniversalRenderPipeline())
			{
				return;
			}

			if (!IsDefinedCalTexCameraMode())
			{
				SceneView.AddCameraMode(calTextelCameraMode.name, calTextelCameraMode.section);
			}

			foreach (SceneView sceneView in SceneView.sceneViews)
			{
				sceneView.onCameraModeChanged += cameraMode =>
				{
					OnCameraModeChanged(sceneView);
				};

				if (sceneView.cameraMode.name.Equals(calTextelCameraMode.name) &&
					sceneView.cameraMode.section.Equals(calTextelCameraMode.section))
				{
					OnCameraModeChanged(sceneView);
				}
			}
		}
		private static void OnCameraModeChanged(SceneView sceneView)
		{
			SceneView.CameraMode cameraMode = sceneView.cameraMode;

			if (!Utility.IsExistRenderer())
			{
				if (cameraMode.name.Equals(calTextelCameraMode.name))
				{
					Debug.LogError("[CaculTexTexelRenderer] Not set CaculTexelRender_Renderer asset. Please add to UniversalRenderpipelineAsset.");
				}
				return;
			}

			Object asset = Utility.LoadUniversalRenderPipelineAsset();
			var so = new SerializedObject(asset);
			SerializedProperty prop = so.FindProperty("m_DefaultRendererIndex");
			int rendererIndex = Utility.GetRendererIndex(asset);

			if (cameraMode.name.Equals(calTextelCameraMode.name))
			{
				if (prop.intValue != rendererIndex)
				{
					EditorPrefs.SetInt(RendererIndexKey, prop.intValue);
				}

				prop.intValue = rendererIndex;
				so.ApplyModifiedProperties();
			}
			else
			{
				if (EditorPrefs.HasKey(RendererIndexKey) && prop.intValue == rendererIndex)
				{
					prop.intValue = EditorPrefs.GetInt(RendererIndexKey);
					so.ApplyModifiedProperties();
				}
			}
		}
	}
}


