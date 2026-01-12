using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class SignalJammingFeature : ScriptableRendererFeature
    {
        class SignalJamPass : ScriptableRenderPass
        {
            private Material _material;
            private static readonly int IntensityID = Shader.PropertyToID("_SignalJamIntensity");
            
            public SignalJamPass(Material material)
            {
                _material = material;
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                if (_material == null)
                    return;

                float intensity = Shader.GetGlobalFloat(IntensityID);
                if (intensity <= 0.01f) // Re-enabled with small threshold
                    return;

                var resources = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                
                TextureHandle source = resources.activeColorTexture;

                // Create temporary texture
                var desc = renderGraph.GetTextureDesc(source);
                desc.name = "SignalJamTemp";
                desc.clearBuffer = false;
                TextureHandle temp = renderGraph.CreateTexture(desc);

                // Set all shader properties from the global intensity
                _material.SetFloat("_Intensity", intensity);

                // Blit from source to temp using our material
                RenderGraphUtils.BlitMaterialParameters blitParams = new RenderGraphUtils.BlitMaterialParameters(
                    source,
                    temp,
                    _material,
                    0);
                
                renderGraph.AddBlitPass(blitParams, "Signal Jam Effect");

                // Copy temp back to source (this is the camera's active color target)
                renderGraph.AddCopyPass(
                    temp, 
                    source,
                    "Signal Jam Copy Back");
            }
        }
        
        [SerializeField] private Material jamMaterial;
        private SignalJamPass _pass;

        public override void Create()
        {
            _pass = new SignalJamPass(jamMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
    }
}