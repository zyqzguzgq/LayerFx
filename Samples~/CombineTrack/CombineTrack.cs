using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LayerFx
{
    [TrackColor(0.5979568f, 0.3724881f, 0.922f)]
    [TrackClipType(typeof(CombineLayerAsset))]
    [TrackBindingType(typeof(Combine))]
    public class CombineTrack : TrackAsset
    {
    }
}