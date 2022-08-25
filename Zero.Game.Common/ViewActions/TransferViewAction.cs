namespace Zero.Game.Common
{
    public class TransferViewAction : ViewAction
    {
        public override ViewActionType ActionType => ViewActionType.Transfer;

        public string Ip { get; private set; }
        public ushort Port { get; private set; }
        public string Key { get; private set; }

        public override void Write(ISWriter writer)
        {
            writer.Write(Ip);
            writer.Write(Port);
            writer.Write(Key);
        }

        protected override void Read(ISReader reader)
        {
            Ip = reader.ReadUtf();
            Port = reader.ReadUInt16();
            Key = reader.ReadUtf();
        }

        protected override void ReturnItemsToCache()
        {

        }

        public void Assign(string ip, ushort port, string key)
        {
            Ip = ip;
            Port = port;
            Key = key;
        }

        internal void Assign(ISReader reader)
        {
            Read(reader);
        }
    }
}
