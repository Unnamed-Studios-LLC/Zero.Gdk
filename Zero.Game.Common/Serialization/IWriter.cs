namespace Zero.Game.Common
{
    public interface IWriter
    {
        void Write(uint bits, byte length);

        void Write(bool value);
        void Write(bool[] value);

        void Write(byte value);
        void Write(byte[] bytes);
        void Write(ushort value);
        void Write(ushort[] value);
        void Write(uint value);
        void Write(uint[] value);
        void Write(ulong value);
        void Write(ulong[] value);

        void Write(sbyte value);
        void Write(sbyte[] value);
        void Write(short value);
        void Write(short[] value);
        void Write(int value);
        void Write(int[] value);
        void Write(long value);
        void Write(long[] value);

        void Write(float value);
        void Write(float[] value);

        void Write(double value);
        void Write(double[] value);

        void Write(string str);
        void Write(string[] value);

        void WriteArrayLength(uint length);

        void WriteBytes(byte[] value);
    }
}
