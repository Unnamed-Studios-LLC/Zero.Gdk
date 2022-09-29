using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct Circle
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Radius;

        public Circle(float x, float y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public Circle(Vec2 vector, float radius) : this(vector.X, vector.Y, radius)
        {
        }

        public Rect BoundingRect => new Rect(X - Radius, Y - Radius, Radius + Radius, Radius + Radius);
        public Vec2 Coordinates
        {
            get => new Vec2(X, Y);
            set => (X, Y) = (value.X, value.Y);
        }

        public bool Contains(Vec2 point)
        {
            var width = point.X - X;
            var height = point.Y - Y;
            return width * width + height * height <= Radius * Radius;
        }

        public bool Overlaps(Circle other)
        {
            var vector = Coordinates - other.Coordinates;
            var combRadius = Radius + other.Radius;
            return vector.SqrMagnitude <= combRadius;
        }
    }
}
