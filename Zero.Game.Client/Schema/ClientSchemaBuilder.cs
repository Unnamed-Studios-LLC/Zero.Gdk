using Zero.Game.Common.Schema;

namespace Zero.Game.Client
{
    public class ClientSchemaBuilder : CommonSchemaBuilder
    {
        internal ClientSchema Build()
        {
            return new ClientSchema(GetDataDefinitions());
        }
    }
}
