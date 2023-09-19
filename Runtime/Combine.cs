using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LayerFx
{
    [Serializable]
    public class Combine : ScriptableObject
    {
        [Range(0, 1)]
        public float _opacity = 1f;
        
        [Tooltip("When to combine layers")]
        public RenderPassEvent _event = RenderPassEvent.AfterRenderingTransparents;
        [Tooltip("Where to combine layers")]
        public Output _output = new Output();
        [Tooltip("Blend passes, if the layer opacity set to zero it will be ignored")]
        public List<Layer> _layers = new List<Layer>();
        
        public float Opacity
        {
            get => _opacity;
            set => _opacity = Mathf.Clamp01(value);
        }
        
        // =======================================================================
        [Serializable]
        public class Output
        {
            [Tooltip("Where to combine layers")]
            public Target   _target;
            [Tooltip("Name of the global texture to combine")]
            public string _globalTex = "_outputTex";
            [Tooltip("Clear color for global texture, if not set, cleaning will not be performed")]
            public Optional<Color> _clear = new Optional<Color>(Color.white, true);

            public enum Target
            {
                Camera,
                GlobalTex,
            }
        }
        
        [Serializable]
        public class Layer
        {
            [HideInInspector]
            public string _title;
            [Range(0, 1)]
            public float _opacity = 1f;
            public Blending              _blending = new Blending();
            public global::LayerFx.Layer _layer;

            [Serializable]
            public class Blending
            {
                public Combine.Blending _blending = Combine.Blending.Normal;
                public Material         _material;
                public int              _pass;
            }
        }

        public enum Blending
        {
            Normal = 0,
            // Dissolve     = 1,
            // Darken       = 2,
            Multiply = 3,
            // ColorBurn    = 4,
            // LinearBurn   = 5,
            // DarkenColor  = 6,
            // Lighten      = 7,
            Screen = 8,
            // ColorDodge   = 9,
            // LighterDodge = 10,
                
            [InspectorName(null)]
            Overlay = 11,
            SoftLight = 12,
            // HardLight   = 13,
            // VividLight  = 14,
            // LinearLight = 15,
            // PinLight    = 16,
            // HardMix     = 17,
                
            Difference = 18,
            // Exclusion  = 19,
            Add      = 20,
            Subtract = 21,
            // Divide     = 22,
            [InspectorName(null)]
            Hue = 23,
            Saturation = 24,
            Color      = 25,
            Luminosity = 26,
            
            Custom     = 77 
        }

        // =======================================================================
        private void OnValidate()
        {
            foreach (var cl in _layers.Where(n => n._layer != null))
                cl._title = cl._layer.name;
        }
    }
}