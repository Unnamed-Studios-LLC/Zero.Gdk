using System;

namespace Zero.Game.Shared
{
    public struct Int2 : ISerializable
    {
        public int X;
        public int Y;

        public Int2(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public Int2 Add(Int2 other) => new Int2(X + other.X, Y + other.Y);
        public Int2 Add(int value) => new Int2(X + value, Y + value);
        public Int2 Clamp(Int2 min, Int2 max) => new Int2(Math.Min(Math.Max(X, min.X), max.X), Math.Min(Math.Max(Y, min.Y), max.Y));
        public Int2 Divide(Int2 other) => new Int2(X / other.X, Y / other.Y);
        public Int2 Divide(int value) => new Int2(X / value, Y / value);
        public Int2 Multiply(Int2 other) => new Int2(X * other.X, Y * other.Y);
        public Int2 Multiply(int value) => new Int2(X * value, Y * value);
        public Int2 Subtract(Int2 other) => new Int2(X - other.X, Y - other.Y);
        public Int2 Subtract(int value) => new Int2(X - value, Y - value);

        public bool Equals(Int2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
        {
            if (obj is Int2 other)
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

        public static Int2 Zero => new Int2(0, 0);
        public static Int2 One => new Int2(1, 1);

        public static bool operator ==(Int2 a, Int2 b) => a.Equals(b);
        public static bool operator !=(Int2 a, Int2 b) => !a.Equals(b);

        public static Int2 operator +(Int2 a, Int2 b) => a.Add(b);
        public static Int2 operator -(Int2 a, Int2 b) => a.Subtract(b);
        public static Int2 operator *(Int2 a, Int2 b) => a.Multiply(b);
        public static Int2 operator /(Int2 a, Int2 b) => a.Divide(b);

        public static Int2 operator +(Int2 a, int b) => a.Add(b);
        public static Int2 operator -(Int2 a, int b) => a.Subtract(b);
        public static Int2 operator *(Int2 a, int b) => a.Multiply(b);
        public static Int2 operator /(Int2 a, int b) => a.Divide(b);

        public static implicit operator Int2(int value) => new Int2(value, value);
    }
}
