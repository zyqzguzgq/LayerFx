using System;
using UnityEngine;
using UnityEngine.Playables;

namespace LayerFx
{
    [Serializable]
    public class CombineOpacityBehaviour : PlayableBehaviour
    {
        [Range(0, 1)]
        public float Weight = 1;

        private Combine _combine;
        
        // =======================================================================
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_combine == null)
                return;
            
            _combine._opacity = 0;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_combine == null)
                _combine = playerData as Combine;

            if (_combine == null)
                return;
            
            _combine._opacity = Weight * info.weight;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (_combine == null)
                return;
            
            _combine._opacity = 0f;
        }
    }
}