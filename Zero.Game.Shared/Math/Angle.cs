using System;

namespace Zero.Game.Shared
{
    public static class Angle
    {
        public const float Deg2Rad = 0.01745329251f;
        public const float Rad2Deg = 57.2957795131f;
        public const float PI = 3.14159265359f;
        public const float PI2 = 6.28318530718f;
        public const float NPI = -3.14159265359f;

        public static float ToDegrees(float radians) => radians * Rad2Deg;

        public static float ToRadians(float degrees) => degrees * Deg2Rad;


        public static Vec2 Vec2(float radians)
        {
            return new Vec2
            {
                X = (float)Math.Cos(radians),
                Y = (float)Math.Sin(radians)
            };
        }

        public static float MinDifference(float angleA, float angleB)
        {
            var difference = (angleB - angleA + PI) % PI2 - PI;
            return difference < NPI ? difference + PI2 : difference;
        }
    }
}
