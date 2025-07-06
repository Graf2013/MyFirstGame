using Player.PlayerInterface;
using UnityEngine;
using UnityEngine.UI;

namespace Player.PlayerUI
{
    public class PlayerUiManager : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private Image energyBar;

        private IUIProvider currentProvider;

        public void SetUIProvider(IUIProvider provider)
        {
            currentProvider = provider;
        }

        private void Update()
        {
            if (currentProvider == null) return;

            float healthFill = currentProvider.GetCurrentHealth() / currentProvider.GetMaxHealth();
            float energyFill = currentProvider.GetCurrentEnergy() / currentProvider.GetMaxEnergy();
            
            healthBar.fillAmount = healthFill;
            energyBar.fillAmount = energyFill;
        }
    }
}