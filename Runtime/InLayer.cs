using UnityEngine;

namespace LayerFx
{
    [ExecuteAlways]
    public class InLayer : MonoBehaviour
    {
        public   Layer    _layer;
        internal Renderer _renderer;
        
        // =======================================================================
        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isEditor && Equals(_renderer, null) == false)
                return;
            
            if (Application.isEditor && _layer == null)
                return;
#endif
            
            _layer._list.Remove(_renderer);
        }

        private void OnWillRenderObject()
        {
#if UNITY_EDITOR
            if (Application.isEditor && Equals(_renderer, null) == false)
            {
                if (TryGetComponent<Renderer>(out _renderer) == false)
                    return;
            }
            
            if (Application.isEditor && _layer == null)
            {
                return;
            }
#endif
            
            _layer._list.Add(_renderer);
        }
    }
}
