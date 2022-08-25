namespace Zero.Game.Common
{
    public interface IReader
    {
        int Length { get; }

        uint Read(byte length);

        uint ReadArrayLength();

        bool ReadBool();
        bool[] ReadBoolArray();

        byte ReadUInt8();
        byte[] ReadUInt8Array();
        ushort ReadUInt16();
        ushort[] ReadUInt16Array();
        uint ReadUInt32();
        uint[] ReadUInt32Array();
        ulong ReadUInt64();
        ulong[] ReadUInt64Array();

        sbyte ReadInt8();
        sbyte[] ReadInt8Array();
        short ReadInt16();
        short[] ReadInt16Array();
        int ReadInt32();
        int[] ReadInt32Array();
        long ReadInt64();
        long[] ReadInt64Array();

        float ReadFloat();
        float[] ReadFloatArray();

        double ReadDouble();
        double[] ReadDoubleArray();

        string ReadUtf();
        string[] ReadUtfArray();

        byte[] ReadBytes(int length);
    }
}
