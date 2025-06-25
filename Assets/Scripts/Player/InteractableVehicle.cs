using UnityEngine;

namespace Player
{
    public class InteractableVehicle : MonoBehaviour
    {
        [SerializeField] private CarController vehicleController;
        private VehicleManager _vehicleManager;

        private void Start()
        {
            _vehicleManager = FindFirstObjectByType<VehicleManager>();
            if (_vehicleManager == null)
            {
                Debug.LogError($"VehicleManager не знайдено у сцені для {gameObject.name}!");
            }

            if (vehicleController == null)
            {
                Debug.LogError($"PlayerController не прив’язаний до InteractableVehicle на {gameObject.name}!");
            }
            else
            {
                vehicleController.enabled = false;
            }
        }

        public void EnterVehicle(PlayerControllerHuman player)
        {
            if (_vehicleManager != null)
            {
                _vehicleManager.EnterVehicle(this, player);
            }
            else
            {
                Debug.LogError($"EnterVehicle не виконано: VehicleManager не ініціалізований для {gameObject.name}!");
            }
        }

        public void ExitVehicle(PlayerControllerHuman player)
        {
            if (_vehicleManager != null)
            {
                _vehicleManager.ExitVehicle(player);
            }
            else
            {
                Debug.LogError($"ExitVehicle не виконано: VehicleManager не ініціалізований для {gameObject.name}!");
            }
        }

        public CarController GetVehicleController()
        {
            return vehicleController;
        }
    }
}