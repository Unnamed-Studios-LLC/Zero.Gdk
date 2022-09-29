using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Rect
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Width;
        [FieldOffset(12)]
        public float Height;

        public Rect(float x, float y, float width, float height) : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Vec2 Bl => new Vec2(X, Y);
        public Vec2 Br => new Vec2(X + Width, Y);

        public Vec2 Point
        {
            get => new Vec2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vec2 Size
        {
            get => new Vec2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vec2 Tl => new Vec2(X, Y + Height);
        public Vec2 Tr => new Vec2(X + Width, Y + Height);

        public bool Contains(Vec2 point)
        {
            return point.X >= X &&
                point.Y >= Y &&
                point.X <= X + Width &&
                point.Y <= Y + Height;
        }
    }
}
