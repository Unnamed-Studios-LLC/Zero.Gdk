namespace Zero.Game.Server
{
    public class StartConnectionResponse
    {
        public StartConnectionResponse()
        {

        }

        public StartConnectionResponse(string workerIp, int port, string key)
        {
            WorkerIp = workerIp;
            Port = port;
            Key = key;
        }

        public StartConnectionResponse(ConnectionFailReason? failReason)
        {
            FailReason = failReason;
        }

        public bool Started => !FailReason.HasValue;
        public ConnectionFailReason? FailReason { get; set; }
        public string WorkerIp { get; set; }
        public int Port { get; set; }
        public string Key { get; set; }

        public static implicit operator StartConnectionResponse(ConnectionFailReason failReason) => new StartConnectionResponse(failReason);
    }
}
