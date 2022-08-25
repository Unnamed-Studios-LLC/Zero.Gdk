using Zero.Game.Common;

namespace Zero.Game.Server
{
    public static class Mock
    {
        public static void AddMockConnection(StartConnectionRequest request)
        {
            ZeroServer.Node?.AddMockConnection(request);
        }
    }
}
