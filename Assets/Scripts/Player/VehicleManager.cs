using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class VehicleManager : MonoBehaviour
    {
        [SerializeField] private GameObject enterExitButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        public InteractableVehicle currentVehicle;
        public bool isInVehicle;
        
        private PlayerControllerHuman _player;
        private InputAction _interactAction;

        // Константи для UI
        private const string ENTER_TEXT = "Вхід";
        private const string EXIT_TEXT = "Вихід";
        private static readonly Vector3 ExitOffset = Vector3.up * 1.5f;

        private void Start()
        {
            InitializeComponents();
            InitializeUI();
        }

        private void InitializeComponents()
        {
            _player = FindFirstObjectByType<PlayerControllerHuman>();
            _interactAction = InputSystem.actions.FindAction("Interact");
            
            if (_player == null)
                Debug.LogError("PlayerControllerHuman не знайдено!");
            if (_interactAction == null)
                Debug.LogError("Interact action не знайдено!");
        }

        private void InitializeUI()
        {
            if (enterExitButton != null)
            {
                enterExitButton.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_interactAction.triggered) return;

            if (currentVehicle != null && !isInVehicle)
            {
                currentVehicle.EnterVehicle(_player);
            }
            else if (isInVehicle && currentVehicle != null)
            {
                currentVehicle.ExitVehicle(_player);
            }
        }

        public void SetInteractableVehicle(InteractableVehicle vehicle)
        {
            currentVehicle = vehicle;
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool showButton = currentVehicle != null && !isInVehicle;
            
            if (enterExitButton != null)
            {
                enterExitButton.SetActive(showButton || isInVehicle);
            }
            
            if (buttonText != null)
            {
                buttonText.text = isInVehicle ? EXIT_TEXT : ENTER_TEXT;
            }
        }

        public void EnterVehicle(InteractableVehicle vehicle, PlayerControllerHuman player)
        {
            if (vehicle == null || player == null) return;

            isInVehicle = true;
            currentVehicle = vehicle;
            
            // Деактивуємо контроль гравця та активуємо контроль машини
            player.DisableControls();
            var vehicleController = vehicle.GetVehicleController();
            if (vehicleController != null)
            {
                vehicleController.enabled = true;
            }
            
            // Переміщуємо гравця в позицію машини
            player.transform.position = vehicle.transform.position;
            
            UpdateUI();
        }

        public void ExitVehicle(PlayerControllerHuman player)
        {
            if (player == null || currentVehicle == null) return;

            isInVehicle = false;
            
            // Активуємо контроль гравця та деактивуємо контроль машини
            var vehicleController = currentVehicle.GetVehicleController();
            if (vehicleController != null)
            {
                vehicleController.enabled = false;
            }
            
            // Переміщуємо гравця поруч з машиною
            player.transform.position = currentVehicle.transform.position + ExitOffset;
            player.EnableControls();

            currentVehicle = null;
            UpdateUI();
        }
    }
}