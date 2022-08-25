namespace Zero.Game.Shared
{
    public struct Circle
    {

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
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }

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
