using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUi : MonoBehaviour
{
    [SerializeField] private Image energyBar;
    [SerializeField] public float maxEnergy;
    [SerializeField] public float currentEnergy;
    [SerializeField] private PlayerController playerController;
    void Start()
    {
        maxEnergy = playerController.maxMoveSpeed;
    }

    void Update()
    {
        energyBar.fillAmount = currentEnergy / maxEnergy;
        if (currentEnergy < playerController.minMoveSpeed)
        {
            currentEnergy -= Time.deltaTime;
        }
    }
}
