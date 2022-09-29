using System;
using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct Int3
    {
        [FieldOffset(0)]
        public int X;
        [FieldOffset(4)]
        public int Y;
        [FieldOffset(8)]
        public int Z;

        public Int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Int3 Add(Int3 other) => new Int3(X + other.X, Y + other.Y, Z + other.Z);
        public Int3 Add(int value) => new Int3(X + value, Y + value, Z + value);
        public Int3 Clamp(Int3 min, Int3 max) => new Int3(Math.Min(Math.Max(X, min.X), max.X), Math.Min(Math.Max(Y, min.Y), max.Y), Math.Min(Math.Max(Z, min.Z), max.Z));
        public Int3 Divide(Int3 other) => new Int3(X / other.X, Y / other.Y, Z / other.Z);
        public Int3 Divide(int value) => new Int3(X / value, Y / value, Z / value);
        public Int3 Multiply(Int3 other) => new Int3(X * other.X, Y * other.Y, Z * other.Z);
        public Int3 Multiply(int value) => new Int3(X * value, Y * value, Z * value);
        public Int3 Subtract(Int3 other) => new Int3(X - other.X, Y - other.Y, Z - other.Z);
        public Int3 Subtract(int value) => new Int3(X - value, Y - value, Z - value);

        public bool Equals(Int3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj)
        {
            if (obj is Int3 other)
            {
                return Equals(other);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static Int3 Zero => new Int3(0, 0, 0);
        public static Int3 One => new Int3(1, 1, 1);

        public static bool operator ==(Int3 a, Int3 b) => a.Equals(b);
        public static bool operator !=(Int3 a, Int3 b) => !a.Equals(b);

        public static Int3 operator +(Int3 a, Int3 b) => a.Add(b);
        public static Int3 operator -(Int3 a, Int3 b) => a.Subtract(b);
        public static Int3 operator *(Int3 a, Int3 b) => a.Multiply(b);
        public static Int3 operator /(Int3 a, Int3 b) => a.Divide(b);

        public static Int3 operator +(Int3 a, int b) => a.Add(b);
        public static Int3 operator -(Int3 a, int b) => a.Subtract(b);
        public static Int3 operator *(Int3 a, int b) => a.Multiply(b);
        public static Int3 operator /(Int3 a, int b) => a.Divide(b);

        public static implicit operator Int3(int value) => new Int3(value, value, value);
        public static implicit operator Int3(Int2 int2) => new Int3(int2.X, int2.Y, 0);
    }
}
