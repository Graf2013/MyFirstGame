using Player;
using UnityEngine;

namespace VFX
{
    class WheelTrailRenderingHandler : MonoBehaviour
    {
        private CarController _carController;
        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            _carController = GetComponentInParent<CarController>();
            _trailRenderer = GetComponent<TrailRenderer>();
        }
        private void Update()
        {
            //Якщо IsDrifting = true включаємо малювання сліду
            _trailRenderer.emitting = _carController.IsDrifting;
        }
    }
}