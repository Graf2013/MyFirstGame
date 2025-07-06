using Player.PlayerWeapon;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class VehicleManager : MonoBehaviour
    {
        [SerializeField] private GameObject enterExitButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        //Посилання на транспорт яким володіє гравець
        public InteractableVehicle currentVehicle;
        //Перевірка чи гравець в машині
        public bool isInVehicle;
        
        //Посилання на гравця
        private PlayerControllerHuman _player;
        private InputAction _interactAction;
        
        private readonly  string _enterText = "Вхід";
        private readonly  string _exitText = "Вихід";
        
        
        

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerControllerHuman>();
            _interactAction = InputSystem.actions.FindAction("Interact");
            enterExitButton.SetActive(false);
        }
        
        private void Update()
        {
            //Якщо кнопка не нажата нічого не робимо
            if (!_interactAction.triggered) return;

            //Якщо кнопка нажата і герой не в транспорті заходимо в транспорт інакше виходимо
            if (currentVehicle != null && !isInVehicle)
            {
                currentVehicle.EnterVehicle(_player);
            }
            else if (isInVehicle && currentVehicle != null)
            {
                currentVehicle.ExitVehicle(_player);
            }
        }

        //Метод викликається коли гравець підходить до транспорту 
        public void SetInteractableVehicle(InteractableVehicle vehicle)
        {
            currentVehicle = vehicle;
            UpdateUI();
        }

        //Метод для показу кнопки
        private void UpdateUI()
        {
            //Перевіряє коли показувати кнопку 
            bool showButton = currentVehicle != null && !isInVehicle;
            
            if (enterExitButton != null)
            {
                enterExitButton.SetActive(showButton || isInVehicle);
            }
            
            if (buttonText != null)
            {
                buttonText.text = isInVehicle ? _exitText : _enterText;
            }
        }

        // Метод викликається коли гравець сідає в транспорт
        public void EnterVehicle(InteractableVehicle vehicle, PlayerControllerHuman player)
        {
            
            if (vehicle == null || player == null) return;

            isInVehicle = true;
            currentVehicle = vehicle;
            
            // Деактивуємо контроль гравця та активуємо контроль машини
            player.DisableControls();
            
            var controller = vehicle.GetVehicleController();
            controller.Enable();
        
            if (controller is CarController car)
            {
                car.SetDriver(player);
            }
            // Переміщуємо гравця в позицію машини
            player.transform.position = vehicle.transform.position;
            
            UpdateUI();

            vehicle.GetComponent<WeaponHandler>().isControlledByPlayer = true;
        }

        //Метод викликається коли гравець виходить з транспорту 
        public void ExitVehicle(PlayerControllerHuman player)
        {
            if (player == null || currentVehicle == null) return;

            isInVehicle = false;
            
            // Активуємо контроль гравця та деактивуємо контроль машини
            var controller = currentVehicle.GetVehicleController();
            controller.Disable();
            
            // Переміщуємо гравця поруч з машиною
            player.transform.position = currentVehicle.transform.position + Vector3.left;
            player.EnableControls();

            currentVehicle.GetComponent<WeaponHandler>().isControlledByPlayer = false;
            currentVehicle = null;
            UpdateUI();
            
        }
        
        public bool IsInVehicle()
        {
            return isInVehicle && currentVehicle != null;
        }

        public InteractableVehicle GetCurrentVehicle()
        {
            return currentVehicle;
        }
        
        public void OnEnterExitButtonClicked()
        {
            if (currentVehicle != null && !isInVehicle)
            {
                currentVehicle.EnterVehicle(_player);
            }
            else if (isInVehicle && currentVehicle != null)
            {
                currentVehicle.ExitVehicle(_player);
            }
        }

    }
}