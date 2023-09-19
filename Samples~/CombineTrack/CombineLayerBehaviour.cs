using System;
using UnityEngine;
using UnityEngine.Playables;

namespace LayerFx
{
    [Serializable]
    public class CombineLayerBehaviour : PlayableBehaviour
    {
        [Range(0, 1)]
        public float Weight = 1;

        public Combine.Layer _layer;
        public int           _layerIndex;

        // =======================================================================
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_layer == null)
                return;
            
            _layer._opacity = 0;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_layer == null)
            {
                var combine = playerData as Combine;
                if (combine != null)
                    _layer = combine._layers.Count > _layerIndex && _layerIndex >= 0 ? combine._layers[_layerIndex] : null;
            }
            
            if (_layer == null)
                return;
            
            _layer._opacity = Weight * info.weight;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (_layer == null)
                return;
            
            _layer._opacity = 0f;
        }
    }
}