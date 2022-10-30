using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public class ClientOptions : ZeroOptions
    {
        /// <summary>
        /// Ms delay to wait between disconnecting and connecting during a transfer
        /// </summary>
        public uint TransferDelayMs { get; set; } = 1000;
    }
}
