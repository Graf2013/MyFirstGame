namespace Player.PlayerInterface
{
    public interface IUIProvider
    {
        float GetCurrentHealth();
        float GetMaxHealth();
        float GetCurrentEnergy();
        float GetMaxEnergy();
    }
}