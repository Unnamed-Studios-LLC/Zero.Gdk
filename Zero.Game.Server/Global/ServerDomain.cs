namespace Zero.Game.Server
{
    internal static class ServerDomain
    {
        public static IDeploymentProvider DeploymentProvider { get; private set; }

        public static void Setup(IDeploymentProvider deploymentProvider)
        {
            DeploymentProvider = deploymentProvider;
        }
    }
}
