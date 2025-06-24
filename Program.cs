using arriety.login;

namespace TramQuyNetwork
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Initialize the ECS-based login manager
            //var loginManager = new LoginManager();
            
            // Start the login process (this will run in background)
            // The LoginManager will use the ECS architecture internally
            
            Application.Run(new Form1());
        }
    }
}