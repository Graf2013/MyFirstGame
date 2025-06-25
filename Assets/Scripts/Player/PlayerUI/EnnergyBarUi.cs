using UnityEngine;
using UnityEngine.UI;

namespace Player.PlayerUI
{
    public class EnergyBarUi : MonoBehaviour
    {
        [SerializeField] private Image energyBar;
        [SerializeField] public float maxEnergy;
        [SerializeField] public float currentEnergy;
        [SerializeField] private CarController carController;
        void Start()
        {
            maxEnergy = carController.maxMoveSpeed;
        }

        void Update()
        {
            energyBar.fillAmount = currentEnergy / maxEnergy;
            if (currentEnergy < carController.minMoveSpeed)
            {
                currentEnergy -= Time.deltaTime;
            }
        }
    }
}
