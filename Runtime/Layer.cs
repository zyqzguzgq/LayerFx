using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//  LayerFx © NullTale - https://twitter.com/NullTale/
namespace LayerFx
{
    public class Layer : ScriptableObject
    {
        [Tooltip("When draw renderers")]
        public RenderPassEvent               _event = RenderPassEvent.AfterRenderingOpaques;
        [Tooltip("Output format")] [HideInInspector]
        public Optional<RenderTextureFormat> _format = new Optional<RenderTextureFormat>(RenderTextureFormat.ARGB32, true);
        [Tooltip("Renderers collected by render mask, also renderers can be collected via InLayer script")]
        public Optional<LayerMask>           _mask  = new Optional<LayerMask>(true);
        [Tooltip("Clear color, if not set, cleaning will not be performed")]
        public Optional<Color>               _clear = new Optional<Color>(Color.clear, true);
        [Tooltip("Global texture name, if not set, the Layer Name will be used as global texture name")]
        public Optional<string>              _globalTex = new Optional<string>("_globaTex", false);
        
        [NonSerialized] 
        public List<Renderer>                _list       = new List<Renderer>();

        public Color Background
        {
            get => _clear.Value;
            set => _clear.Value = value;
        }
        
        public Texture Texture       => Shader.GetGlobalTexture(GlobalTexName);
        public string  GlobalTexName => _globalTex.Enabled ? _globalTex.value : name;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            
            var lrf =UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this)).OfType<LayerFx>().FirstOrDefault(n => n._layers.Contains(this));
            if (lrf != null)
                lrf.Create();
#endif
        }
    }
}