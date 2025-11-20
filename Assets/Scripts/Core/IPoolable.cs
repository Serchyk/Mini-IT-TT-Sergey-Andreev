namespace MiniIT.CORE
{
    /// <summary>
    /// Interface for poolable objects.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}