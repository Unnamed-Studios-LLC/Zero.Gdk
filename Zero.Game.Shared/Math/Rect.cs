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

        public float Bottom => Y;
        public float Top => Y + Height;
        public float Left => X;
        public float Right => X + Width;

        /// <summary>
        /// If the given point is contained in this rectangle
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vec2 point)
        {
            return point.X >= X &&
                point.Y >= Y &&
                point.X <= X + Width &&
                point.Y <= Y + Height;
        }

        /// <summary>
        /// If the input rectangle in wholey contains within this rectangle
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Envelops(Rect other)
        {
            return Bottom <= other.Bottom && Left <= other.Left && Top >= other.Top && Right >= other.Right;
        }

        /// <summary>
        /// If the input rectange overlaps this rectangle
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(Rect other)
        {
            return Bottom < other.Top && Top > other.Bottom && Left < other.Right && Right > other.Left;
        }
    }
}
