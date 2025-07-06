using Player;
using Player.PlayerInterface;
using UnityEngine;

namespace Test
{
    public class TestDamage : MonoBehaviour
    {
        public float damage = 20f;
        public PlayerControllerHuman player;
        public VehicleManager vehicleManager;

        public void DealDamage()
        {
            IDamageable target = null;

            if (vehicleManager.IsInVehicle())
            {
                var vehicle = vehicleManager.GetCurrentVehicle();
                target = vehicle.GetComponent<IDamageable>();
            }
            else
            {
                target = player.GetComponent<IDamageable>();
            }

            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"Завдано {damage} урону!");
            }
            else
            {
                Debug.LogWarning("Ціль не реалізує IDamageable");
            }
        }
    }
}