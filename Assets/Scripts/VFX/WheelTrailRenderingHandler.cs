using Player;
using UnityEngine;

namespace VFX
{
    class WheelTrailRenderingHandler : MonoBehaviour
    {
        private PlayerController _playerController;
        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();

            _trailRenderer = GetComponent<TrailRenderer>();
        }
        private void Update()
        {
            _trailRenderer.emitting = _playerController.IsDrifting;
        }
    }
}