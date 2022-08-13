using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CaculTexTexel
{
	public class MaterialPool
    {
		Material material;
		public MaterialPool(Material mat)
        {
			this.material = mat;
		}

		Stack<Material> cached = new Stack<Material>();

		public Material Get()
        {
			Material ret = null;

			while (true)
			{
				if (cached.Count == 0)
				{
					break;
				}
				ret = cached.Pop();
				if (ret != null)
				{
					break;
				}
			}

			if (ret == null)
			{
				ret = GameObject.Instantiate(material);
			}
			return ret;
		}

		public void Release(Material mat)
        {
			cached.Push(mat);
		}

		public int GetCount()
        {
			return cached.Count;
        }
	}

    public class CaculTexTexelPass : ScriptableRenderPass
    {
#if UNITY_2020_2_OR_NEWER
        private ProfilingSampler sampler = new ProfilingSampler("CaculTexTexel");
#else
		private ProfilingSampler profilingSampler;
#endif
        private Material material;
		private Material materialBlit;
		private Material materialUI;

		List<Material> materials = new List<Material>();
		private ScriptableRenderer renderer;
		private RenderTargetIdentifier source;
		static readonly int tempCopy = Shader.PropertyToID("_TempCopy");

		private MaterialPool pool;

		
		public CaculTexTexelPass(Shader shader, Shader shaderBlit, Shader shaderUI, Texture texture)
        {
			material = CoreUtils.CreateEngineMaterial(shader);
			materialBlit = CoreUtils.CreateEngineMaterial(shaderBlit);

			pool = new MaterialPool(material);

			materialUI = CoreUtils.CreateEngineMaterial(shaderUI);
			materialUI.mainTexture = texture;
		}

		public void Setup(RenderTargetIdentifier source, ScriptableRenderer renderer)
		{
			this.source = source;
			this.renderer = renderer;
		}
		

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
		

#if UNITY_2020_2_OR_NEWER
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, sampler))
#else
			CommandBuffer cmd = CommandBufferPool.Get("CaculTexTexel");
			using (new ProfilingScope(cmd, profilingSampler))
#endif
			{
				var camera = renderingData.cameraData.camera;

				cmd.GetTemporaryRT(tempCopy, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear);
				cmd.Blit(source, tempCopy, materialBlit);
				
				cmd.Blit(tempCopy, source);

				var gos = Selection.gameObjects;
				foreach (var go in gos)
				{
					if (go is GameObject)
					{
						var rs = go.GetComponentsInChildren<Renderer>();
						foreach (var r in rs)
						{
							var mat = pool.Get();
							if (r.sharedMaterial == null)
                            {
								continue;
                            }
							mat.mainTexture = r.sharedMaterial.mainTexture;

							var scale = r.sharedMaterial.mainTextureScale;
							var offset = r.sharedMaterial.mainTextureOffset;
							mat.SetVector("_MainTex_ST", new Vector4(scale.x, scale.y, offset.x, offset.y));
							
							cmd.DrawRenderer(r, mat, 0, 0);
							materials.Add(mat);
						}
					}
				}

				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, materialUI);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			if (materials.Count <= 0)
			{
				return;
			}

			foreach(var mat in materials)
            {
				pool.Release(mat);
			}
			materials.Clear();
		}
	}
}