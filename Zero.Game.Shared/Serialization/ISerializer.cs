using System.Collections.Generic;

namespace Zero.Game.Shared
{
    public interface ISerializer
    {
        uint Value(uint input, byte bitLength);

        bool Value(bool input);
        bool[] Value(bool[] input);
        List<bool> Value(List<bool> input);

        byte Value(byte input);
        byte[] Value(byte[] input);
        List<byte> Value(List<byte> input);
        ushort Value(ushort input);
        ushort[] Value(ushort[] input);
        List<ushort> Value(List<ushort> input);
        uint Value(uint input);
        uint[] Value(uint[] input);
        List<uint> Value(List<uint> input);
        ulong Value(ulong input);
        ulong[] Value(ulong[] input);
        List<ulong> Value(List<ulong> input);

        sbyte Value(sbyte input);
        sbyte[] Value(sbyte[] input);
        List<sbyte> Value(List<sbyte> input);
        short Value(short input);
        short[] Value(short[] input);
        List<short> Value(List<short> input);
        int Value(int input);
        int[] Value(int[] input);
        List<int> Value(List<int> input);
        long Value(long input);
        long[] Value(long[] input);
        List<long> Value(List<long> input);

        float Value(float input);
        float[] Value(float[] input);
        List<float> Value(List<float> input);
        double Value(double input);
        double[] Value(double[] input);
        List<double> Value(List<double> input);

        string Value(string input);
        string[] Value(string[] input);
        List<string> Value(List<string> input);

        T Value<T>(T input) where T : ISerializable, new();
        T[] Value<T>(T[] input) where T : ISerializable, new();
        List<T> Value<T>(List<T> input) where T : ISerializable, new();

        byte[] Value(byte[] input, int byteLength);
    }
}
