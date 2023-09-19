using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

namespace LayerFx
{
    [Serializable]
    public class CombineLayerAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;

        public int _layerIndex;
        
        // =======================================================================
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable  = ScriptPlayable<CombineLayerBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour._layerIndex = _layerIndex;

            return playable;
        }
    }
}