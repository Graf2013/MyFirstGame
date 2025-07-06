// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Player.PlayerUI
// {
//     public class HealthBarUI : MonoBehaviour
//     {
//         [SerializeField] private Image healthBar;
//         [SerializeField] private CarController carController;
//         void Start()
//         {
//             carController.currentHealth = carController.maxHealth;
//         }
//
//         void Update()
//         {
//             healthBar.fillAmount = carController.currentHealth / carController.maxHealth;
//         }
//     }
// }
