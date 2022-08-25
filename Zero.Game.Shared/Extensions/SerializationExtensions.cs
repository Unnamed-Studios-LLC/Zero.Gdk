namespace Zero.Game.Shared
{
    public static class SerializationExtensions
    {
        public static Int2 Value(this ISerializer serializer, Int2 value)
        {
            value.X = serializer.Value(value.X);
            value.Y = serializer.Value(value.Y);
            return value;
        }

        public static Int3 Value(this ISerializer serializer, Int3 value)
        {
            value.X = serializer.Value(value.X);
            value.Y = serializer.Value(value.Y);
            value.Z = serializer.Value(value.Z);
            return value;
        }

        public static Vec2 Value(this ISerializer serializer, Vec2 value)
        {
            value.X = serializer.Value(value.X);
            value.Y = serializer.Value(value.Y);
            return value;
        }

        public static Vec3 Value(this ISerializer serializer, Vec3 value)
        {
            value.X = serializer.Value(value.X);
            value.Y = serializer.Value(value.Y);
            value.Z = serializer.Value(value.Z);
            return value;
        }
    }
}
