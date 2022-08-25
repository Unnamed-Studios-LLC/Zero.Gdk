using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public class UpdateViewAction : ViewAction
    {
        public override ViewActionType ActionType => ViewActionType.Update;

        public ObjectType ObjectType { get; private set; }
        public uint Id { get; private set; }
        public List<IData> Data { get; private set; }

        internal UpdateViewAction()
        {

        }

        public override void Write(ISWriter writer)
        {
            writer.Write((uint)ObjectType, 1);
            writer.Write(Id);

            writer.WriteArrayLength((uint)Data.Count);
            for (int i = 0; i < Data.Count; i++)
            {
                var data = Data[i];
                writer.Write((byte)data.Type);
                data.Serialize(writer);
            }
        }

        protected override void Read(ISReader reader)
        {
            ObjectType = (ObjectType)reader.Read(1);
            Id = reader.ReadUInt32();

            var dataLength = reader.ReadArrayLength();
            Data = ListCache.GetDataList();
            for (int i = 0; i < dataLength; i++)
            {
                var type = reader.ReadUInt8();
                var data = CommonDomain.Schema.CreateData(type);
                data.Serialize(reader);
                Data.Add(data);
            }
        }

        protected override void ReturnItemsToCache()
        {
            ListCache.ReturnDataList(Data);
        }

        internal void Assign(ObjectType objectType, uint id, List<IData> data)
        {
            ObjectType = objectType;
            Id = id;
            Data = data;
        }

        internal void Assign(ISReader reader)
        {
            Read(reader);
        }
    }
}
