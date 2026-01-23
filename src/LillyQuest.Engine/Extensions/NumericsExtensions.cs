using System.Numerics;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Extensions;

public static class NumericsExtensions
{
    public static Vector2 ToNumerics(this Vector2D<int> vector)
    {
        return new Vector2(vector.X, vector.Y);
    }

}
