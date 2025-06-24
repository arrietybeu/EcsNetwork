namespace arriety.ecs.components
{
    public class SessionComponent : IComponent
    {
        public int EntityId { get; set; }
        
        public int SessionId { get; set; }
        public bool IsSessionInitialized { get; set; }
        public DateTime SessionStartTime { get; set; }
    }
} 