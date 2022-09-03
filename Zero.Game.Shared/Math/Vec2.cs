using System;

namespace Zero.Game.Shared
{
    public struct Vec2 : ISerializable
    {
        public float X;
        public float Y;

        public Vec2(float x, float y) : this()
        {
            X = x;
            Y = y;
        }

        public float Angle => (float)Math.Atan2(Y, X);
        public Int2 Int2 => new Int2((int)X, (int)Y);
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);
        public float SqrMagnitude => (float)(X * X) + (float)(Y * Y);

        public static Vec2 SetMagnitude(Vec2 value, float currentMagnitude, float targetMagnitude)
        {
            if (value.X == 0 && value.Y == 0)
            {
                return new Vec2(1, 0);
            }

            return value * (float)(targetMagnitude / currentMagnitude);
        }

        public Vec2 Add(Vec2 other) => new Vec2(X + other.X, Y + other.Y);
        public Vec2 Add(float value) => new Vec2(X + value, Y + value);
        public Vec2 Clamp(Vec2 min, Vec2 max) => new Vec2(Math.Min(Math.Max(X, min.X), max.X), Math.Min(Math.Max(Y, min.Y), max.Y));
        public Vec2 Divide(Vec2 other) => new Vec2(X / other.X, Y / other.Y);
        public Vec2 Divide(float value) => new Vec2(X / value, Y / value);
        public float Dot(Vec2 other) => (float)(X * other.X) + (float)(Y * other.Y);
        public Vec2 Multiply(Vec2 other) => new Vec2(X * other.X, Y * other.Y);
        public Vec2 Multiply(float value) => new Vec2(X * value, Y * value);
        public Vec2 Project(Vec2 other) => other * (float)(Dot(other) / (float)other.SqrMagnitude);
        public Vec2 Subtract(Vec2 other) => new Vec2(X - other.X, Y - other.Y);
        public Vec2 Subtract(float value) => new Vec2(X - value, Y - value);

        public Vec2 SetMagnitude(float targetMagnitude)
        {
            return SetMagnitude(this, Magnitude, targetMagnitude);
        }

        public bool Equals(Vec2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
        {
            if (obj is Vec2 other)
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
            return $"({X}, {Y})";
        }

        public void Serialize(ISerializer serializer)
        {
            X = serializer.Value(X);
            Y = serializer.Value(Y);
        }

        public static Vec2 One => new Vec2(1, 1);
        public static Vec2 Half => new Vec2(0.5f, 0.5f);
        public static Vec2 Zero => new Vec2(0, 0);

        public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
        public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);

        public static Vec2 operator +(Vec2 a, Vec2 b) => a.Add(b);
        public static Vec2 operator -(Vec2 a, Vec2 b) => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, Vec2 b) => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, Vec2 b) => a.Divide(b);

        public static Vec2 operator +(Vec2 a, float b) => a.Add(b);
        public static Vec2 operator -(Vec2 a, float b) => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, float b) => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, float b) => a.Divide(b);

        public static implicit operator Vec2(float value) => new Vec2(value, value);
        public static implicit operator Vec2(Int2 int2) => new Vec2(int2.X, int2.Y);
        public static implicit operator Vec2(Int3 int3) => new Vec2(int3.X, int3.Y);
        public static implicit operator Vec2(Vec3 vec3) => new Vec2(vec3.X, vec3.Y);
    }
}
