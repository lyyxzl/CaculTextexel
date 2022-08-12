using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CaculTexTexel
{
    public class CaculTexTexelRenderer : ScriptableRendererFeature
    {
#if UNITY_EDITOR
		private CaculTexTexelPass caculTexTexelPass;

		[SerializeField] private Shader blitShader = null;
		[SerializeField] private Shader shader = null;
		[SerializeField] private Shader uiShader = null;
		[SerializeField] private Texture uiTexture;
#endif

		// Start is called before the first frame update
		public override void Create()
		{
#if UNITY_EDITOR
			if (!shader || !blitShader || !uiShader)
			{
				return;
			}

			caculTexTexelPass = new CaculTexTexelPass(shader, blitShader, uiShader, uiTexture);
			caculTexTexelPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
#endif
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
#if UNITY_EDITOR

			caculTexTexelPass.Setup(renderer.cameraColorTarget, renderer);
			renderer.EnqueuePass(caculTexTexelPass);

#endif
		}
	
	}
}