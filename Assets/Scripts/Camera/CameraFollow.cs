using UnityEngine;
using Unity.Cinemachine;

namespace GameMamager
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform; // Трансформ гравця
        [SerializeField] private Player.VehicleManager vehicleManager; // VehicleManager
        [SerializeField] private CinemachineCamera virtualCamera; // Віртуальна камера Cinemachine

        private void Start()
        {
            if (playerTransform == null)
            {
                playerTransform = FindFirstObjectByType<Player.PlayerControllerHuman>().transform;
            }
            if (vehicleManager == null)
            {
                vehicleManager = FindFirstObjectByType<Player.VehicleManager>();
            }
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineCamera>();
            }

            // Встановлюємо початкову ціль (гравець)
            if (virtualCamera != null && playerTransform != null)
            {
                virtualCamera.Follow = playerTransform;
            }
        }

        private void Update()
        {
            if (virtualCamera == null || vehicleManager == null) return;

            // Перемикаємо ціль залежно від стану гравця
            if (vehicleManager.isInVehicle && vehicleManager.currentVehicle != null)
            {
                virtualCamera.Follow = vehicleManager.currentVehicle.transform;
            }
            else
            {
                virtualCamera.Follow = playerTransform;
            }
        }
    }
}