namespace arriety.ecs.components
{
    public enum LoginState
    {
        Disconnected,
        Connecting,
        WaitingForInit,
        Authenticating,
        Authenticated,
        LoginFailed
    }
    
    public class LoginStateComponent : IComponent
    {
        public int EntityId { get; set; }
        
        public LoginState State { get; set; } = LoginState.Disconnected;
        public string? LoginFailMessage { get; set; }
        public DateTime LastStateChange { get; set; }
        public bool AuthSent { get; set; }
    }
} 