using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace CaculTexTexel.Editor
{
	public class Utility
	{
		public const string RendererFeatureName = "CaculTexTexelRenderer";
		public static readonly string GraphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";

		public static SerializedProperty LoadCustomRenderPipeline()
		{
			GraphicsSettings graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>(GraphicsSettingsPath);
			var so = new SerializedObject(graphicsSettings);
			return so.FindProperty("m_CustomRenderPipeline");
		}

		public static bool IsUseUniversalRenderPipeline()
		{
			SerializedProperty prop = LoadCustomRenderPipeline();
			return prop != null && prop.objectReferenceValue != null &&
				prop.objectReferenceValue is UniversalRenderPipelineAsset;
		}

		public static UniversalRenderPipelineAsset LoadUniversalRenderPipelineAsset()
		{
			SerializedProperty prop = LoadCustomRenderPipeline();
			return prop.objectReferenceValue as UniversalRenderPipelineAsset;
		}

		public static int GetRendererIndex()
		{
			return GetRendererIndex(LoadUniversalRenderPipelineAsset());
		}

		public static int GetRendererIndex(Object asset)
		{
			if (!asset) { return -1; }
			var so = new SerializedObject(asset);
			SerializedProperty propArray = so.FindProperty("m_RendererDataList");
			for (int i = 0; i < propArray.arraySize; ++i)
			{
				SerializedProperty prop = propArray.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue is ForwardRendererData renderer)
				{
					if (renderer.rendererFeatures.Count > 0 &&
						renderer.rendererFeatures[0].name.Equals(RendererFeatureName))
					{
						return i;
					}
				}
			}
			return -1;
		}

		public static bool IsExistRenderer()
		{
			return GetRendererIndex() >= 0;
		}
	}
}
