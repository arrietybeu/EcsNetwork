using arriety.ecs.components;
using arriety.login;

namespace TramQuyNetwork
{
    public partial class Form1 : Form
    {
        private LoginManager loginManager;
        private System.Windows.Forms.Timer statusTimer;
        
        public Form1()
        {
            InitializeComponent();
            InitializeEcsDemo();
        }
        
        private void InitializeEcsDemo()
        {
            // Initialize login manager with ECS system
            loginManager = new LoginManager();
            
            // Set up a timer to update the UI with ECS status
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1000; // Update every second
            statusTimer.Tick += UpdateStatus;
            statusTimer.Start();
            
            // Set form title
            this.Text = "TramQuy Network - ECS Architecture Demo";
            this.Size = new Size(600, 400);
            
            // Add status label
            var statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(550, 300);
            statusLabel.Location = new Point(25, 25);
            statusLabel.Text = "Initializing ECS Network System...";
            statusLabel.Font = new Font("Consolas", 10);
            this.Controls.Add(statusLabel);
        }
        
        private void UpdateStatus(object? sender, EventArgs e)
        {
            if (loginManager?.EcsNetwork == null) return;
            
            var statusLabel = this.Controls["statusLabel"] as Label;
            if (statusLabel == null) return;
            
            var status = $"""
                ECS Network System Status:
                ========================
                
                Connection Status: {(loginManager.IsConnected ? "Connected" : "Disconnected")}
                Login State: {loginManager.GetLoginState()}
                Session ID: {loginManager.GetSessionId()}
                
                ECS Architecture Components:
                - NetworkConnectionComponent: Manages TCP connection state
                - PacketBufferComponent: Handles packet queues and buffering
                - SessionComponent: Stores session data
                - LoginStateComponent: Tracks login progress
                - DeviceInfoComponent: Contains device information
                
                ECS Systems Running:
                - ConnectionSystem: Manages connection lifecycle
                - NetworkReceiveSystem: Handles incoming packets
                - NetworkSendSystem: Manages outgoing packets  
                - PacketDispatchSystem: Routes packets to handlers
                - LoginSystem: Orchestrates login flow
                
                Last Updated: {DateTime.Now:HH:mm:ss}
                """;
                
            statusLabel.Text = status;
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
            loginManager?.EcsNetwork?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
