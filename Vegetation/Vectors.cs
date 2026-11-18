using System.Numerics;

namespace Vegetation;

// What the growth asks of a vector beyond what System.Numerics gives, a way that is never NaN, and
// the dot and cross products and the squared length written out a term at a time, rather than
// through System.Numerics' reductions, whose instructions the JIT picks by processor, so a seed
// grows the same tree on every machine.
internal static class Vectors
{
    public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    extension(Vector3 vector)
    {
        // The squared length, a product at a time.
        public float SquaredLength => vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;

        // The vector at unit length, or along Z where it has no length, so a stem whose ways cancel
        // turns along an axis rather than filling the skeleton with NaN, which a level would keep.
        public Vector3 Normalized
        {
            get
            {
                var length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
                return length > 0f ? vector * (1f / length) : Vector3.UnitZ;
            }
        }
    }
}
