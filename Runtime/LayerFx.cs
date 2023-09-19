using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LayerFx
{
    public class LayerFx : ScriptableRendererFeature
    {
        private const  string            k_ShaderName = "Hidden/LayerFx/Combine";
        private static List<ShaderTagId> k_ShaderTags;
        
        public  SoCollection<Combine> _combines      = new SoCollection<Combine>();
        public  SoCollection<Layer>   _layers        = new SoCollection<Layer>();
        private List<LayerPass>       _layerPasses   = new List<LayerPass>();
        private List<CombinePass>     _combinePasses = new List<CombinePass>();

        private Material _blit;
        [HideInInspector]
        public  Shader   _blitShader;
        
        // =======================================================================
        private class LayerPass : ScriptableRenderPass
        {
            public  LayerFx            _owner;
            public  Layer              _layer;
            private RenderTarget       _output;
            private RendererListParams _rlp;
            private ProfilingSampler   _profiler;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _layer._event;
                _output         = new RenderTarget().Allocate(_layer.GlobalTexName);
                _rlp            = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, _layer._mask.Value));
                _profiler       = new ProfilingSampler(_layer.name);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (_layer == null)
                    return;
#endif
                // allocate resources
                var cmd  = CommandBufferPool.Get(nameof(LayerFx));
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                
                if (_layer._format.Enabled)
                    desc.colorFormat = _layer._format.Value;
                
                _profiler.Begin(cmd);
                
                _output.Get(cmd, in desc);
                
#if UNITY_2022_1_OR_NEWER
                var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                    ? renderingData.cameraData.renderer.cameraColorTarget
                    : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                cmd.SetRenderTarget(_output.Id, depth);
                
                if (_layer._clear.Enabled)
                    cmd.ClearRenderTarget(RTClearFlags.Color, _layer._clear, 1f, 0);

                if (_layer._mask.Enabled)
                {
                    ref var cameraData = ref renderingData.cameraData;
                    var     camera     = cameraData.camera;
                    camera.TryGetCullingParameters(out var cullingParameters);

                    _rlp.cullingResults = context.Cull(ref cullingParameters);
                    _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                    
                    var rl = context.CreateRendererList(ref _rlp);
                    cmd.DrawRendererList(rl);
                }

                try
                {
                    foreach (var rnd in _layer._list)
                        cmd.DrawRenderer(rnd, rnd.sharedMaterial);
                }
                catch
                {
                    // ignored
                }

                _layer._list.Clear();

                _profiler.End(cmd);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                _output.Release(cmd);
            }
        }
        
        private class CombinePass : ScriptableRenderPass
        {
            private static readonly int             s_Weight     = Shader.PropertyToID("_Weight");
            public static           int             s_BlendTex    = Shader.PropertyToID("_BlendTex");
            private static          Combine.Layer[] s_LayersCahce = new Combine.Layer[64];
            
            public  LayerFx          _owner;
            public  Combine          _combine;
            private RenderTarget     _output;
            private RenderTargetFlip _process;
            private ProfilingSampler _profiler;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _combine._event;
                _output         = new RenderTarget().Allocate(_combine._output._globalTex);
                _process        = new RenderTargetFlip($"{_owner.name}_combine");
                _profiler       = new ProfilingSampler("Combine");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // collect layers
                var layersCount = 0;
                foreach (var lb in _combine._layers)
                {
                    if (lb._layer == null)
                        continue;
                    
                    if (string.IsNullOrEmpty(lb._layer._globalTex))
                        continue;
                    
                    if (lb._opacity == 0f)
                        continue;
                    
                    s_LayersCahce[layersCount] = lb;
                    layersCount ++;
                }
                
                if (layersCount == 0)
                    return;
                
                // allocate resources
                var cmd  = CommandBufferPool.Get(nameof(LayerFx));
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.colorFormat = RenderTextureFormat.ARGB32;
                
                _profiler.Begin(cmd);
                
                var output = _getOutput(ref renderingData);
                
                _process.Get(cmd, in desc);
                
                // first blit over output tex
                var layer = s_LayersCahce[0];
                cmd.SetGlobalFloat(s_Weight, layer._opacity);
                cmd.SetGlobalTexture(s_BlendTex, layer._layer.GlobalTexName);
                
                if (layer._blending._blending == Combine.Blending.Custom)
                    Utils.Blit(cmd, output, _process.From, layer._blending._material, layer._blending._pass);
                else
                    Utils.Blit(cmd, output, _process.From, _owner._blit, _blendingPass(layer._blending._blending));

                // invoke layers chain
                for (var n = 1; n < layersCount; n++)
                {
                    layer = s_LayersCahce[n];
                    cmd.SetGlobalFloat(s_Weight, layer._opacity);
                    cmd.SetGlobalTexture(s_BlendTex, layer._layer.GlobalTexName);
                    
                    if (layer._blending._blending == Combine.Blending.Custom)
                        Utils.Blit(cmd, _process.From, _process.To, layer._blending._material, layer._blending._pass);
                    else
                        Utils.Blit(cmd, _process.From, _process.To, _owner._blit, _blendingPass(layer._blending._blending));
                    
                    _process.Flip();
                }
                
                // final blit over the output with alpha
                cmd.SetGlobalFloat(s_Weight, _combine._opacity);
                Utils.Blit(cmd, _process.From, output, _owner._blit, 0);
                
                _profiler.End(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
                RTHandle _getOutput(ref RenderingData renderingData)
                {
                    switch (_combine._output._target)
                    {
                        case Combine.Output.Target.Camera:
                            return _getCameraOutput(ref renderingData);
                        
                        case Combine.Output.Target.GlobalTex:
                            var desc = renderingData.cameraData.cameraTargetDescriptor;
                            desc.colorFormat = RenderTextureFormat.ARGB32;
                            _output.Get(cmd, in desc);
                            if (_combine._output._clear.Enabled)
                            {
                                cmd.SetRenderTarget(_output.Id);
                                cmd.ClearRenderTarget(RTClearFlags.Color, _combine._output._clear.Value, 1f, 0);
                            }
                            return _output;
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                RTHandle _getCameraOutput(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                _output.Release(cmd);
                _process.Release(cmd);
            }
        }
        
        // =======================================================================
        public override void Create()
        {
            _layerPasses = _layers
                      .Values
                      .Select(n => new LayerPass() { _owner = this, _layer = n })
                      .Where(n => n._layer != null)
                      .ToList();
            
            _combinePasses = _combines
                      .Values
                      .Select(n => new CombinePass() { _owner = this, _combine = n })
                      .Where(n => n._combine != null)
                      .ToList();
            
#if UNITY_EDITOR
            if (_blitShader == null)
            {
                _blitShader = Shader.Find(k_ShaderName);
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            
            _blit = new Material(_blitShader);
            
            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
            
            foreach (var pass in _layerPasses)
                pass.Init();
            
            foreach (var pass in _combinePasses)
                pass.Init();
        }

        private void OnDestroy()
        {
            _layers.Destroy();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // in game or scene view only (ignore inspector draw)
            if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
                return;
            
            foreach (var pass in _layerPasses)
                renderer.EnqueuePass(pass);
            
            foreach (var pass in _combinePasses.Where(n => n._combine._opacity > 0f))
                renderer.EnqueuePass(pass);
        }
        
        // =======================================================================        
        private static int _blendingPass(Combine.Blending blending)
        {
            return blending switch {
                Combine.Blending.Normal     => 1,
                Combine.Blending.Multiply   => 2,
                Combine.Blending.Screen     => 2,
                Combine.Blending.Overlay    => 3,
                Combine.Blending.SoftLight  => 4,
                Combine.Blending.Difference => 6,
                Combine.Blending.Add        => 7,
                Combine.Blending.Subtract   => 8,
                Combine.Blending.Hue        => 9,
                Combine.Blending.Saturation => 10,
                Combine.Blending.Color      => 11,
                Combine.Blending.Luminosity => 12,
                _                         => throw new ArgumentOutOfRangeException(nameof(blending), blending, null)
            };
        }
    }
}
    