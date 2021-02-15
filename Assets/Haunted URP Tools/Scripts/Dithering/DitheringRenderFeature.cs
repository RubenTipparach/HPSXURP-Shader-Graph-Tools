using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class DitheringRenderFeature : ScriptableRendererFeature
    {
        DitheringPass ditheringPass;

        public override void Create()
        {
            ditheringPass = new DitheringPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        //ScripstableRendererFeature is an abstract class, you need this method
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ditheringPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(ditheringPass);
        }
    }


    public class DitheringPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/Dithering";
        static readonly string k_RenderTag = "Render Dithering Effects";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetDithering");

        //PROPERTIES
        static readonly int PatternIndex = Shader.PropertyToID("_PatternIndex");
        static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");
        static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
        static readonly int DitherScale = Shader.PropertyToID("_DitherScale");


        //pixelation PROPERTIES
        static readonly int WidthPixelation = Shader.PropertyToID("_WidthPixelation");
        static readonly int HeightPixelation = Shader.PropertyToID("_HeightPixelation");
        static readonly int ColorPrecison = Shader.PropertyToID("_ColorPrecision");

        Dithering dithering;
        Material ditheringMaterial;
        RenderTargetIdentifier currentTarget;

        public DitheringPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            this.ditheringMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (this.ditheringMaterial == null)
            {
                Debug.LogError("Material not created.");
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled) return;

            var stack = VolumeManager.instance.stack;

            this.dithering = stack.GetComponent<Dithering>();
            if (this.dithering == null) { return; }
            if (!this.dithering.IsActive()) { return; }

            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            int destination = TempTargetId;

            //getting camera width and height 
            var w = cameraData.camera.scaledPixelWidth;
            var h = cameraData.camera.scaledPixelHeight;

            //setting parameters here 
            cameraData.camera.depthTextureMode = cameraData.camera.depthTextureMode | DepthTextureMode.Depth;
            this.ditheringMaterial.SetInt(PatternIndex, this.dithering.patternIndex.value);
            this.ditheringMaterial.SetFloat(DitherThreshold, this.dithering.ditherThreshold.value);
            this.ditheringMaterial.SetFloat(DitherStrength, this.dithering.ditherStrength.value);
            this.ditheringMaterial.SetFloat(DitherScale, this.dithering.ditherScale.value);

            this.ditheringMaterial.SetFloat(WidthPixelation, this.dithering.widthPixelation.value);
            this.ditheringMaterial.SetFloat(HeightPixelation, this.dithering.heightPixelation.value);
            this.ditheringMaterial.SetFloat(ColorPrecison, this.dithering.colorPrecision.value);

            int shaderPass = 0;
            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, this.ditheringMaterial, shaderPass);

            //var RTs = GetRTs(cameraData.camera);


            //depth and bayer size parameters
            float[] indexMatrix4x4 = new float[] { 0,  8,  2, 10,
                                              12,  4, 14,  6,
                                               3, 11,  1,  9,
                                              15,  7, 13,  5 };

            Vector4 colorAmounts = new Vector4();

            colorAmounts.x = 256 / Mathf.Pow(2, dithering.rBitDepth.value); // 0 to 256
            colorAmounts.y = 256 / Mathf.Pow(2, dithering.gBitDepth.value); // 0 to 256
            colorAmounts.z = 256 / Mathf.Pow(2, dithering.bBitDepth.value); // 0 to 256
            colorAmounts.w = -1;

            ditheringMaterial.SetFloatArray("iMatrix4x4", indexMatrix4x4);
            ditheringMaterial.SetInt("levels", dithering.ditherSeparation.value);
            ditheringMaterial.SetColor("pn_bitsPerChannel", colorAmounts);
            ditheringMaterial.SetFloat("_Intensity", dithering.intensity.value);
            ditheringMaterial.SetInt("_DownscaleFactor", dithering.downscaleFactor.value);

            //// Downsample
            //DrawFullScreen(cmd, m_Material, RTs.downsampled, prop, (int)Pass.Downsample);
            //var down = RTs.downsampled;
            //prop.SetTexture("_DownScaledTex", down);


            //// Render final image
            //DrawFullScreen(cmd, m_Material, destination, prop, (int)Pass.FinalImage);
        }


    }


}