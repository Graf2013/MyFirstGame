using Player.PlayerInterface;
using Player.PlayerUI;
using UnityEngine;

namespace Player
{
    public class InteractableVehicle : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour controller;
        private IVehicleController  _vehicleController;
        private VehicleManager _vehicleManager;
        [SerializeField] PlayerUiManager  playerUiManager;

        private void Start()
        {
            // Приводимо збережений MonoBehaviour до інтерфейсу IVehicleController
            _vehicleController = controller as IVehicleController;
            // Знаходимо об'єкт VehicleManager у сцені, щоб передавати йому команди
            _vehicleManager = FindFirstObjectByType<VehicleManager>();
            
            playerUiManager = FindFirstObjectByType<PlayerUiManager>();
        }

        // Метод викликається, коли гравець сідає в цей транспорт
        public void EnterVehicle(PlayerControllerHuman player)
        {
            _vehicleManager.EnterVehicle(this, player);
            var uiProvider = controller as IUIProvider;
            playerUiManager.SetUIProvider(uiProvider);

        }

        // Метод викликається, коли гравець виходить з цього транспорту
        public void ExitVehicle(PlayerControllerHuman player)
        {
            _vehicleManager.ExitVehicle(player);
            var playerProvider = player.GetComponentInChildren<IUIProvider>();
            if (playerProvider != null)
            {
                playerUiManager.SetUIProvider(playerProvider);
            }
        }

        // Повертає інтерфейс контролера транспорту, щоб VehicleManager міг вмикати чи вимикати його
        public IVehicleController GetVehicleController()
        {
            return _vehicleController;
        }
    }
}