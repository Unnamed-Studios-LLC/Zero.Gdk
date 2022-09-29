using System;
using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct Vec3
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Z;

        public Vec3(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Int2 Int2 => new Int2((int)X, (int)Y);
        public Int3 Int3 => new Int3((int)X, (int)Y, (int)Z);
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);
        public float SqrMagnitude => (float)(X * X) + (float)(Y * Y) + (float)(Z * Z);

        public Vec3 Add(Vec3 other) => new Vec3(X + other.X, Y + other.Y, Z + other.Z);
        public Vec3 Add(float value) => new Vec3(X + value, Y + value, Z + value);
        public Vec3 Clamp(Vec3 min, Vec3 max) => new Vec3(Math.Min(Math.Max(X, min.X), max.X), Math.Min(Math.Max(Y, min.Y), max.Y), Math.Min(Math.Max(Z, min.Z), max.Z));
        public Vec3 Divide(Vec3 other) => new Vec3(X / other.X, Y / other.Y, Z / other.Z);
        public Vec3 Divide(float value) => new Vec3(X / value, Y / value, Z / value);
        public float Dot(Vec3 other) => X * other.X + Y * other.Y + Z * other.Z;
        public Vec3 Multiply(Vec3 other) => new Vec3(X * other.X, Y * other.Y, Z * other.Z);
        public Vec3 Multiply(float value) => new Vec3(X * value, Y * value, Z * value);
        public Vec3 Project(Vec3 other) => other * (float)(Dot(other) / (float)other.Dot(other));
        public Vec3 Subtract(Vec3 other) => new Vec3(X - other.X, Y - other.Y, Z - other.Z);
        public Vec3 Subtract(float value) => new Vec3(X - value, Y - value, Z - value);

        public bool Equals(Vec3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj)
        {
            if (obj is Vec3 other)
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

        public static Vec3 One => new Vec3(1, 1, 1);
        public static Vec3 Half => new Vec3(0.5f, 0.5f, 0.5f);
        public static Vec3 Zero => new Vec3(0, 0, 0);

        public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);
        public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);

        public static Vec3 operator +(Vec3 a, Vec3 b) => a.Add(b);
        public static Vec3 operator -(Vec3 a, Vec3 b) => a.Subtract(b);
        public static Vec3 operator *(Vec3 a, Vec3 b) => a.Multiply(b);
        public static Vec3 operator /(Vec3 a, Vec3 b) => a.Divide(b);

        public static Vec3 operator +(Vec3 a, float b) => a.Add(b);
        public static Vec3 operator -(Vec3 a, float b) => a.Subtract(b);
        public static Vec3 operator *(Vec3 a, float b) => a.Multiply(b);
        public static Vec3 operator /(Vec3 a, float b) => a.Divide(b);

        public static implicit operator Vec3(float value) => new Vec3(value, value, value);
        public static implicit operator Vec3(Int2 int2) => new Vec3(int2.X, int2.Y, 0);
        public static implicit operator Vec3(Int3 int3) => new Vec3(int3.X, int3.Y, int3.Z);
        public static implicit operator Vec3(Vec2 vec2) => new Vec3(vec2.X, vec2.Y, 0);
    }
}
