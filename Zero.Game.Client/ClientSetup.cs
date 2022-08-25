namespace Zero.Game.Client
{
    public abstract class ClientSetup
    {
        public abstract void BuildClientSchema(ClientSchemaBuilder builder);

        public abstract bool NewClient(ZeroClientInstance client);

        public abstract ClientOptions GetOptions();
    }
}
