namespace arriety.ecs
{
    public interface ISystem
    {
        void Update(World world, float deltaTime);
        void Initialize(World world);
        void Shutdown(World world);
    }
} 